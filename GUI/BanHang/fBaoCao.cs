using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhaSachKimDong.BLL;
using QuanLyNhaSachKimDong.DTO;

namespace QuanLyNhaSachKimDong.GUI.BanHang
{
    public partial class fBaoCao : Form
    {
        private readonly ThongKeBaoCaoBLL bcBLL = new ThongKeBaoCaoBLL();

        public fBaoCao()
        {
            InitializeComponent();
            this.Load += fBaoCao_Load;                  // Gắn sự kiện Load bằng code
            btnXemBaoCao.Click += btnXemBaoCao_Click;   // Gắn sự kiện Click bằng code
        }

        private void fBaoCao_Load(object sender, EventArgs e)
        {
            // Mặc định: từ đầu tháng đến hôm nay
            DateTime homNay = DateTime.Today;
            dtpTuNgay.Value = new DateTime(homNay.Year, homNay.Month, 1);
            dtpDenNgay.Value = homNay;
            XemBaoCao();
        }

        private void btnXemBaoCao_Click(object sender, EventArgs e)
        {
            XemBaoCao();
        }

        /// <summary>Tính và hiển thị doanh thu, giá vốn, lợi nhuận trong khoảng ngày đã chọn.</summary>
        private void XemBaoCao()
        {
            string thongBao;
            if (!bcBLL.KiemTraKhoangNgay(dtpTuNgay.Value, dtpDenNgay.Value, out thongBao))
            {
                MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                BaoCaoDTO bc = bcBLL.LayBaoCaoTongHop(dtpTuNgay.Value, dtpDenNgay.Value);
                lblSoHoaDon.Text = "Số hóa đơn: " + bc.SoHoaDon;
                lblDoanhThu.Text = "Doanh thu: " + bc.DoanhThu.ToString("N0") + " đ";
                lblGiaVon.Text = "Giá vốn: " + bc.GiaVon.ToString("N0") + " đ";
                lblLoiNhuan.Text = "Lợi nhuận: " + bc.LoiNhuan.ToString("N0") + " đ";
                lblLoiNhuan.ForeColor = bc.LoiNhuan >= 0 ? Color.Green : Color.Red; // Lỗ thì hiện đỏ

                dgvTheoNgay.DataSource = bcBLL.LayBaoCaoTheoNgay(dtpTuNgay.Value, dtpDenNgay.Value);
                dgvTheoNgay.Columns["Ngay"].HeaderText = "Ngày";
                dgvTheoNgay.Columns["Ngay"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvTheoNgay.Columns["SoHoaDon"].HeaderText = "Số HĐ";
                dgvTheoNgay.Columns["DoanhThu"].HeaderText = "Doanh thu";
                dgvTheoNgay.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
                dgvTheoNgay.Columns["LoiNhuan"].HeaderText = "Lợi nhuận";
                dgvTheoNgay.Columns["LoiNhuan"].DefaultCellStyle.Format = "N0";
                dgvTheoNgay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo báo cáo: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}