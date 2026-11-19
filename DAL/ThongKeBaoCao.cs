using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhaSachKimDong.DTO;

namespace QuanLyNhaSachKimDong.DAL
{
    public class ThongKeBaoCaoDAL
    {
        /// <summary>Tổng hợp số hóa đơn, doanh thu, giá vốn trong khoảng thời gian.</summary>
        public BaoCaoDTO LayBaoCaoTongHop(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"SELECT COUNT(DISTINCT hd.MaHD) AS SoHoaDon,
                                  ISNULL(SUM(ct.SoLuong * ct.DonGiaBan), 0) AS DoanhThu,
                                  ISNULL(SUM(ct.SoLuong * ISNULL(ct.DonGiaMua, 0)), 0) AS GiaVon
                           FROM HOADON hd
                           INNER JOIN CHITIETHOADON ct ON hd.MaHD = ct.MaHD
                           WHERE hd.NgayLap >= @Tu AND hd.NgayLap < @Den";
            DataTable dt = SqlHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date.AddDays(1)));

            DataRow r = dt.Rows[0];
            return new BaoCaoDTO
            {
                SoHoaDon = Convert.ToInt32(r["SoHoaDon"]),
                DoanhThu = Convert.ToDecimal(r["DoanhThu"]),
                GiaVon = Convert.ToDecimal(r["GiaVon"])
            };
        }

        /// <summary>Doanh thu và lợi nhuận chia theo từng ngày.</summary>
        public DataTable LayBaoCaoTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"SELECT CAST(hd.NgayLap AS DATE) AS Ngay,
                                  COUNT(DISTINCT hd.MaHD) AS SoHoaDon,
                                  SUM(ct.SoLuong * ct.DonGiaBan) AS DoanhThu,
                                  SUM(ct.SoLuong * (ct.DonGiaBan - ISNULL(ct.DonGiaMua, 0))) AS LoiNhuan
                           FROM HOADON hd
                           INNER JOIN CHITIETHOADON ct ON hd.MaHD = ct.MaHD
                           WHERE hd.NgayLap >= @Tu AND hd.NgayLap < @Den
                           GROUP BY CAST(hd.NgayLap AS DATE)
                           ORDER BY Ngay";
            return SqlHelper.ExecuteQuery(sql,
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date.AddDays(1)));
        }
    }
}