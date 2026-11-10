using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Forms
{
    public partial class Frm_XemLaiBaiThi : ReaLTaiizor.Forms.MaterialForm
    {
        #region Dữ liệu & Trạng thái

        private readonly int _ketQuaID;
        private readonly IKetQuaService _ketQuaService = new KetQuaService();

        #endregion

        #region 1. Khởi tạo & Cấu Hình Giao Diện

        public Frm_XemLaiBaiThi(int ketQuaID)
        {
            _ketQuaID = ketQuaID;
            InitializeComponent();
            DataGridViewPager.AttachAll(this);
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            LoadChiTiet();
        }

        private void Frm_XemLaiBaiThi_Load(object sender, EventArgs e)
        {
        }

        #endregion

        #region 2. Tải Chi Tiết Bài Làm

        private void LoadChiTiet()
        {
            DataTable dt = _ketQuaService.LayChiTietBaiLamTheoKetQua(_ketQuaID);
            dgvChiTiet.DataSource = dt;
        }

        #endregion

        #region 3. Hiển Thị Đáp Án Đúng / Sai

        private void DgvChiTiet_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvChiTiet.Rows.Count)
            {
                var row = dgvChiTiet.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    bool laDung = drv["LaDung"] != DBNull.Value && Convert.ToBoolean(drv["LaDung"]);
                    if (laDung)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); // Xanh nhạt: Đúng
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235); // Đỏ nhạt: Sai
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }
            }
        }

        #endregion

    }
}
