using System;
using System.Data;
using System.Windows.Forms;
using QuanLyNhaSachKimDong.BLL;

namespace QuanLyNhaSachKimDong.GUI.BanHang
{
    public partial class fThongKe : Form
    {
        private readonly HoaDonBLL hdBLL = new HoaDonBLL();

        public fThongKe()
        {
            InitializeComponent();
        }

        private void fThongKe_Load(object sender, EventArgs e)
        {
            // Mặc định: từ đầu tháng đến hôm nay
            DateTime homNay = DateTime.Today;
            dtpTuNgay.Value = new DateTime(homNay.Year, homNay.Month, 1);
            dtpDenNgay.Value = homNay;
            LocHoaDon();
        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            LocHoaDon();
        }

        /// <summary>Lọc hóa đơn theo khoảng ngày và hiển thị lên lưới.</summary>
        private void LocHoaDon()
        {
            try
            {
                string thongBao;
                DataTable dt = hdBLL.LayHoaDonTheoThoiGian(dtpTuNgay.Value, dtpDenNgay.Value, out thongBao);
                if (dt == null)
                {
                    MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvHoaDon.DataSource = dt;
                dgvHoaDon.Columns["MaHD"].HeaderText = "Mã HD";
                dgvHoaDon.Columns["NgayLap"].HeaderText = "Ngày lập";
                dgvHoaDon.Columns["NgayLap"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                dgvHoaDon.Columns["TenKhach"].HeaderText = "Khách hàng";
                dgvHoaDon.Columns["TenDangNhap"].HeaderText = "Người lập";
                dgvHoaDon.Columns["TongTien"].HeaderText = "Tổng tiền";
                dgvHoaDon.Columns["TongTien"].DefaultCellStyle.Format = "N0";
                dgvHoaDon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Tính tổng kết cho các hóa đơn đã lọc
                decimal tong = 0;
                foreach (DataRow r in dt.Rows) tong += Convert.ToDecimal(r["TongTien"]);
                lblTongSoHD.Text = "Số hóa đơn: " + dt.Rows.Count;
                lblTongTien.Text = "Tổng tiền: " + tong.ToString("N0") + " đ";

                dgvChiTiet.DataSource = null; // Xóa chi tiết cũ
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc hóa đơn: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Bấm vào một hóa đơn thì hiện chi tiết bên dưới.</summary>
        private void dgvHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Bấm vào tiêu đề cột
            try
            {
                int maHD = Convert.ToInt32(dgvHoaDon.Rows[e.RowIndex].Cells["MaHD"].Value);
                dgvChiTiet.DataSource = hdBLL.LayChiTietHoaDon(maHD);
                dgvChiTiet.Columns["MaSach"].HeaderText = "Mã sách";
                dgvChiTiet.Columns["TenSach"].HeaderText = "Tên sách";
                dgvChiTiet.Columns["SoLuong"].HeaderText = "Số lượng";
                dgvChiTiet.Columns["DonGiaBan"].HeaderText = "Đơn giá";
                dgvChiTiet.Columns["ThanhTien"].HeaderText = "Thành tiền";
                dgvChiTiet.Columns["DonGiaBan"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
                dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xem chi tiết: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}