namespace QuanLyNhaSachKimDong.DTO
{
    public class BaoCaoDTO
    {
        public int SoHoaDon { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal GiaVon { get; set; }
        public decimal LoiNhuan { get { return DoanhThu - GiaVon; } }
    }
}