using System;
using System.Data;
using QuanLyNhaSachKimDong.DAL;
using QuanLyNhaSachKimDong.DTO;

namespace QuanLyNhaSachKimDong.BLL
{
    public class ThongKeBaoCaoBLL
    {
        private readonly ThongKeBaoCaoDAL dal = new ThongKeBaoCaoDAL();

        public bool KiemTraKhoangNgay(DateTime tu, DateTime den, out string thongBao)
        {
            thongBao = "";
            if (tu.Date > den.Date) { thongBao = "Từ ngày không được lớn hơn đến ngày."; return false; }
            return true;
        }

        public BaoCaoDTO LayBaoCaoTongHop(DateTime tu, DateTime den) { return dal.LayBaoCaoTongHop(tu, den); }
        public DataTable LayBaoCaoTheoNgay(DateTime tu, DateTime den) { return dal.LayBaoCaoTheoNgay(tu, den); }
    }
}