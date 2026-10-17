using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Colors;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ReaLTaiizor.Controls.HopeTabPage;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class HocVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        public HocVien()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            this.DrawerTabControl = tabHocVien;

        }



        private void HocVien_Load(object sender, EventArgs e)
        {
            try
            {
                tabHocVien.SelectedIndexChanged += TabHocVien_SelectedIndexChanged;
                LoadThongTinHocVien();
                LoadLopHoc();
                LoadLichSuThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin học viên: " + ex.Message, "Thông báo");
            }
        }
        private void LoadThongTinHocVien()
        {
            try
            {
                if (Session.NguoiDungID > 0)
                {
                    HocSinh hs = new HocSinhDAL().LayTheoID(Session.NguoiDungID);
                    if (hs != null)
                    {
                        txtHV_Profile_HoTen.Text = hs.HoTen;
                        txtHV_Profile_Email.Text = hs.Email;
                        txtHV_Profile_SDT.Text = hs.SoDienThoai;
                        txtHV_Profile_DiaChi.Text = hs.DiaChi;
                        if (hs.NgaySinh.HasValue)
                            txtHV_Profile_NgaySinh.Text = hs.NgaySinh.Value.ToString("dd/MM/yyyy");
                        cboHV_Profile_GioiTinh.SelectedItem = hs.GioiTinh;
                    }
                }
            }
            catch { }
        }
        private void LoadLopHoc()
        {
            try
            {
                DataTable dtLop = _lopDAL.LayTheoHocSinh(Session.NguoiDungID);
                dgvHV_LopDangHoc.DataSource = dtLop;
            }
            catch { }
        }
        private void LoadLichSuThi()
        {
            try
            {
                DataTable dtKQ = _ketQuaDAL.LayLichSuTheoHocSinh(Session.NguoiDungID);
                dgvHV_LichSuThi.DataSource = dtKQ;
            }
            catch { }
        }
        private void BatDauThi(int deThiID)
        {
            if (_ketQuaDAL.KiemTraDaThi(Session.NguoiDungID, deThiID))
            {
                MessageBox.Show("Bạn đã hoàn thành bài thi này rồi, không thể thi lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Frm_KiemTra frm = new Frm_KiemTra(deThiID);
            frm.ShowDialog();
            LoadLichSuThi();
        }

        private void TabHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabHocVien.SelectedTab != null &&
                tabHocVien.SelectedTab.Text.Trim().Contains("Đăng Xuất"))
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn đăng xuất?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Session.Clear();
                    this.Hide();
                    Form loginForm = Application.OpenForms["frm_DangNhap"];
                    if (loginForm != null) loginForm.Show();
                    else new frm_DangNhap().Show();
                    this.Close();
                }
                else
                {
                    tabHocVien.SelectedIndex = 0;
                }
            }
        }



    }
}
