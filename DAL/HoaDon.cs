using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhaSachKimDong.DTO;

namespace QuanLyNhaSachKimDong.DAL
{
    public class HoaDonDAL
    {
        /// <summary>Lấy danh sách sách còn tồn để đưa vào ComboBox bán hàng.</summary>
        public DataTable LaySachDeBan()
        {
            string sql = @"SELECT s.MaSach, s.TenSach, s.GiaBia, k.SoLuongTon
                           FROM SACH s
                           INNER JOIN KHO k ON s.MaSach = k.MaSach
                           WHERE k.SoLuongTon > 0
                           ORDER BY s.TenSach";
            return SqlHelper.ExecuteQuery(sql);
        }

        /// <summary>
        /// Lập hóa đơn trong MỘT Transaction:
        /// 1) Thêm HOADON  2) Thêm CHITIETHOADON  3) Trừ tồn kho.
        /// Bất kỳ bước nào lỗi -> Rollback toàn bộ. Trả về mã hóa đơn mới.
        /// </summary>
        public int LapHoaDon(HoaDonDTO hd, List<ChiTietHoaDonDTO> dsChiTiet)
        {
            using (SqlConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Thêm hóa đơn, lấy MaHD vừa sinh ra (IDENTITY)
                        string sqlHD = @"INSERT INTO HOADON (NgayLap, TenKhach, TenDangNhap, TongTien)
                                         VALUES (@NgayLap, @TenKhach, @TenDangNhap, @TongTien);
                                         SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        int maHD;
                        using (SqlCommand cmd = new SqlCommand(sqlHD, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@NgayLap", hd.NgayLap);
                            cmd.Parameters.AddWithValue("@TenKhach", (object)hd.TenKhach ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TenDangNhap", hd.TenDangNhap);
                            cmd.Parameters.AddWithValue("@TongTien", hd.TongTien);
                            maHD = (int)cmd.ExecuteScalar();
                        }

                        foreach (ChiTietHoaDonDTO ct in dsChiTiet)
                        {
                            // 2. Thêm chi tiết. DonGiaMua lấy từ SACH tại thời điểm bán (lưu giá vốn)
                            string sqlCT = @"INSERT INTO CHITIETHOADON (MaHD, MaSach, SoLuong, DonGiaBan, DonGiaMua)
                                             SELECT @MaHD, MaSach, @SoLuong, @DonGiaBan, GiaMua
                                             FROM SACH WHERE MaSach = @MaSach";
                            using (SqlCommand cmd = new SqlCommand(sqlCT, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@MaHD", maHD);
                                cmd.Parameters.AddWithValue("@MaSach", ct.MaSach);
                                cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                                cmd.Parameters.AddWithValue("@DonGiaBan", ct.DonGiaBan);
                                if (cmd.ExecuteNonQuery() == 0)
                                    throw new InvalidOperationException("Không tìm thấy sách: " + ct.TenSach);
                            }

                            // 3. Trừ tồn kho. Điều kiện SoLuongTon >= @SoLuong đảm bảo không bị âm kho,
                            //    kể cả khi 2 máy cùng bán một lúc.
                            string sqlKho = @"UPDATE KHO SET SoLuongTon = SoLuongTon - @SoLuong
                                              WHERE MaSach = @MaSach AND SoLuongTon >= @SoLuong";
                            using (SqlCommand cmd = new SqlCommand(sqlKho, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@MaSach", ct.MaSach);
                                cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                                if (cmd.ExecuteNonQuery() == 0)
                                    throw new InvalidOperationException("Sách \"" + ct.TenSach + "\" không đủ tồn kho.");
                            }
                        }

                        tran.Commit();   // Mọi bước thành công -> ghi vĩnh viễn
                        return maHD;
                    }
                    catch
                    {
                        tran.Rollback(); // Có lỗi -> hoàn tác tất cả
                        throw;           // Ném lỗi lên BLL để báo người dùng
                    }
                }
            }
        }

        /// <summary>Lấy hóa đơn trong khoảng [tuNgay, denNgay] (gồm cả ngày cuối).</summary>
        public DataTable LayHoaDonTheoThoiGian(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"SELECT MaHD, NgayLap, TenKhach, TenDangNhap, TongTien
                           FROM HOADON
                           WHERE NgayLap >= @Tu AND NgayLap < @Den
                           ORDER BY NgayLap DESC";
            return SqlHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date.AddDays(1))); // < ngày kế tiếp = hết ngày cuối
        }

        /// <summary>Lấy chi tiết của một hóa đơn.</summary>
        public DataTable LayChiTietHoaDon(int maHD)
        {
            string sql = @"SELECT ct.MaSach, s.TenSach, ct.SoLuong, ct.DonGiaBan,
                                  ct.SoLuong * ct.DonGiaBan AS ThanhTien
                           FROM CHITIETHOADON ct
                           INNER JOIN SACH s ON ct.MaSach = s.MaSach
                           WHERE ct.MaHD = @MaHD";
            return SqlHelper.ExecuteQuery(sql, new SqlParameter("@MaHD", maHD));
        }
    }
}