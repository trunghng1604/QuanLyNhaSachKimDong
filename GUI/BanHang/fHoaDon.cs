using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using QuanLyNhaSachKimDong.BLL;
using QuanLyNhaSachKimDong.DTO;
using QuanLyNhaSachKimDong.Utils;

namespace QuanLyNhaSachKimDong.GUI.BanHang
{
    public partial class fHoaDon : Form
    {
        private readonly HoaDonBLL hdBLL = new HoaDonBLL();

        // Giỏ hàng (chi tiết hóa đơn đang lập). BindingList giúp DataGridView tự cập nhật khi thêm/xóa.
        private readonly BindingList<ChiTietHoaDonDTO> gioHang = new BindingList<ChiTietHoaDonDTO>();

        public fHoaDon()
        {
            InitializeComponent();
        }

        // ===== Sự kiện load form =====
        private void fHoaDon_Load(object sender, EventArgs e)
        {
            try
            {
                dtpNgayLap.Value = DateTime.Now;
                dgvChiTiet.DataSource = gioHang;
                CauHinhLuoiGioHang();
                TaiDanhSachSach();
                CapNhatTongTien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Đặt tiêu đề cột, định dạng số cho lưới giỏ hàng.</summary>
        private void CauHinhLuoiGioHang()
        {
            dgvChiTiet.Columns["MaHD"].Visible = false; // Chưa có mã HD khi đang lập
            dgvChiTiet.Columns["MaSach"].HeaderText = "Mã sách";
            dgvChiTiet.Columns["TenSach"].HeaderText = "Tên sách";
            dgvChiTiet.Columns["SoLuong"].HeaderText = "Số lượng";
            dgvChiTiet.Columns["DonGiaBan"].HeaderText = "Đơn giá";
            dgvChiTiet.Columns["ThanhTien"].HeaderText = "Thành tiền";
            dgvChiTiet.Columns["DonGiaBan"].DefaultCellStyle.Format = "N0";
            dgvChiTiet.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        /// <summary>Nạp lại danh sách sách còn tồn vào ComboBox (gọi lại sau mỗi lần lưu để cập nhật tồn).</summary>
        private void TaiDanhSachSach()
        {
            cboSach.DataSource = hdBLL.LaySachDeBan();
            cboSach.DisplayMember = "TenSach";
            cboSach.ValueMember = "MaSach";
            HienThongTinSach();
        }

        /// <summary>Hiện đơn giá, tồn kho của sách đang chọn.</summary>
        private void HienThongTinSach()
        {
            DataRowView row = cboSach.SelectedItem as DataRowView;
            if (row == null)
            {
                txtDonGia.Clear();
                txtTonKho.Clear();
                return;
            }
            txtDonGia.Text = Convert.ToDecimal(row["GiaBia"]).ToString("N0");
            txtTonKho.Text = row["SoLuongTon"].ToString();
        }

        private void CapNhatTongTien()
        {
            lblTongTien.Text = "Tổng tiền: " + hdBLL.TinhTongTien(gioHang).ToString("N0") + " đ";
        }

        private void cboSach_SelectedIndexChanged(object sender, EventArgs e)
        {
            HienThongTinSach();
        }

        // ===== Thêm sách vào hóa đơn =====
        private void btnThem_Click(object sender, EventArgs e)
        {
            DataRowView row = cboSach.SelectedItem as DataRowView;
            if (row == null)
            {
                MessageBox.Show("Vui lòng chọn sách.", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string thongBao;
            bool ok = hdBLL.ThemVaoGio(gioHang,
                row["MaSach"].ToString(),
                row["TenSach"].ToString(),
                Convert.ToDecimal(row["GiaBia"]),
                Convert.ToInt32(row["SoLuongTon"]),
                (int)numSoLuong.Value,
                out thongBao);

            if (!ok)
            {
                MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            numSoLuong.Value = 1;
            CapNhatTongTien();
        }

        // ===== Xóa dòng đang chọn khỏi giỏ =====
        private void btnXoaDong_Click(object sender, EventArgs e)
        {
            if (dgvChiTiet.CurrentRow == null) return;
            gioHang.RemoveAt(dgvChiTiet.CurrentRow.Index);
            CapNhatTongTien();
        }

        // ===== Lưu hóa đơn (Transaction nằm ở DAL) =====
        private void btnLuuHD_Click(object sender, EventArgs e)
        {
            if (gioHang.Count == 0)
            {
                MessageBox.Show("Hóa đơn chưa có sách nào.", "Thông báo",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult xacNhan = MessageBox.Show(
                "Lưu hóa đơn với tổng tiền " + hdBLL.TinhTongTien(gioHang).ToString("N0") + " đ?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (xacNhan != DialogResult.Yes) return;

            try
            {
                int maHD;
                string thongBao;
                bool ok = hdBLL.LapHoaDon(txtTenKhach.Text, dtpNgayLap.Value, Session.TenDangNhap,
                                          gioHang.ToList(), out maHD, out thongBao);
                if (!ok)
                {
                    MessageBox.Show("Lưu hóa đơn thất bại:\n" + thongBao, "Lỗi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Lập hóa đơn thành công. Mã HD: " + maHD, "Thành công",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                LamMoiForm();
                TaiDanhSachSach(); // Nạp lại để thấy tồn kho mới sau khi trừ
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
        }

        private void LamMoiForm()
        {
            gioHang.Clear();
            txtTenKhach.Clear();
            numSoLuong.Value = 1;
            dtpNgayLap.Value = DateTime.Now;
            CapNhatTongTien();
        }
    }
}