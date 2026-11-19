namespace QuanLyNhaSachKimDong.DTO
{
    public class ChiTietHoaDonDTO
    {
        public int MaHD { get; set; }
        public string MaSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGiaBan { get; set; }

        // Thành tiền tính tự động, không lưu DB
        public decimal ThanhTien { get { return SoLuong * DonGiaBan; } }
    }
}