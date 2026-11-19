using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using QuanLyNhaSachKimDong.DAL;
using QuanLyNhaSachKimDong.DTO;

namespace QuanLyNhaSachKimDong.BLL
{
    public class HoaDonBLL
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public DataTable LaySachDeBan() { return dal.LaySachDeBan(); }

        /// <summary>Thêm sách vào giỏ. Nếu sách đã có trong giỏ thì cộng dồn số lượng.</summary>
        public bool ThemVaoGio(BindingList<ChiTietHoaDonDTO> gio, string maSach, string tenSach,
                               decimal donGia, int tonKho, int soLuong, out string thongBao)
        {
            thongBao = "";
            if (string.IsNullOrWhiteSpace(maSach)) { thongBao = "Vui lòng chọn sách."; return false; }
            if (soLuong <= 0) { thongBao = "Số lượng phải lớn hơn 0."; return false; }

            ChiTietHoaDonDTO daCo = gio.FirstOrDefault(x => x.MaSach == maSach);
            int tongSauKhiThem = soLuong + (daCo != null ? daCo.SoLuong : 0);

            // Kiểm tra tồn kho (DAL còn kiểm tra lần nữa lúc lưu để chắc chắn)
            if (tongSauKhiThem > tonKho)
            {
                thongBao = "Không đủ tồn kho. Tồn hiện tại: " + tonKho + ", trong giỏ đã có: "
                           + (daCo != null ? daCo.SoLuong : 0);
                return false;
            }

            if (daCo != null)
            {
                daCo.SoLuong = tongSauKhiThem;
                gio.ResetBindings(); // Báo cho DataGridView vẽ lại
            }
            else
            {
                gio.Add(new ChiTietHoaDonDTO
                {
                    MaSach = maSach,
                    TenSach = tenSach,
                    SoLuong = soLuong,
                    DonGiaBan = donGia
                });
            }
            return true;
        }

        public decimal TinhTongTien(IEnumerable<ChiTietHoaDonDTO> gio)
        {
            return gio.Sum(x => x.ThanhTien);
        }

        /// <summary>Kiểm tra nghiệp vụ rồi lập hóa đơn (DAL xử lý Transaction).</summary>
        public bool LapHoaDon(string tenKhach, DateTime ngayLap, string tenDangNhap,
                              List<ChiTietHoaDonDTO> dsChiTiet, out int maHD, out string thongBao)
        {
            maHD = 0;
            thongBao = "";
            if (dsChiTiet == null || dsChiTiet.Count == 0)
            {
                thongBao = "Hóa đơn chưa có sách nào.";
                return false;
            }
            try
            {
                HoaDonDTO hd = new HoaDonDTO
                {
                    NgayLap = ngayLap,
                    TenKhach = string.IsNullOrWhiteSpace(tenKhach) ? "Khách lẻ" : tenKhach.Trim(),
                    TenDangNhap = tenDangNhap,
                    TongTien = TinhTongTien(dsChiTiet)
                };
                maHD = dal.LapHoaDon(hd, dsChiTiet);
                return true;
            }
            catch (Exception ex)
            {
                thongBao = ex.Message; // Gồm cả lỗi "không đủ tồn kho" từ DAL
                return false;
            }
        }

        public DataTable LayHoaDonTheoThoiGian(DateTime tu, DateTime den, out string thongBao)
        {
            thongBao = "";
            if (tu.Date > den.Date)
            {
                thongBao = "Từ ngày không được lớn hơn đến ngày.";
                return null;
            }
            return dal.LayHoaDonTheoThoiGian(tu, den);
        }

        public DataTable LayChiTietHoaDon(int maHD) { return dal.LayChiTietHoaDon(maHD); }
    }
}