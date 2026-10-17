using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using ReaLTaiizor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_GiaoVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        public frm_GiaoVien()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;

        }

        private void GiaoVien_Load(object sender, EventArgs e)
        {
            try
            {
                lblGV_LoiChao.Text = $"Xin Chào, {Session.HoTen ?? Session.TenDangNhap}";
                LoadLopHocCuaToi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải trang giáo viên: " + ex.Message, "Thông báo");
            }
        }
        private void LoadLopHocCuaToi()
        {
            try
            {
                DataTable dt = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                if (dgvGV_LopCuaToi != null)
                {
                    dgvGV_LopCuaToi.DataSource = dt;
                }
            }
            catch { }
        }
        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialTabControl1.SelectedTab != null && materialTabControl1.SelectedTab.Text.Contains("Đăng Xuất"))
            {
                if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
                    materialTabControl1.SelectedIndex = 0;
                }
            }
        }
        private void materialLabel14_Click(object sender, EventArgs e)
        {

        }

        private void materialTextBoxEdit2_Click(object sender, EventArgs e)
        {

        }

        private void materialTextBoxEdit3_Click(object sender, EventArgs e)
        {

        }

        private void tabControl_GiaoVien_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MaterialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtCH_DapAnB_Click(object sender, EventArgs e)
        {

        }

        private void txtCH_DapAnD_Click(object sender, EventArgs e)
        {

        }
    }
}
