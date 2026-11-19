using System;

namespace QuanLyNhaSachKimDong.DTO
{
    public class HoaDonDTO
    {
        public int MaHD { get; set; }
        public DateTime NgayLap { get; set; }
        public string TenKhach { get; set; }
        public string TenDangNhap { get; set; }   // Người lập hóa đơn
        public decimal TongTien { get; set; }
    }
}