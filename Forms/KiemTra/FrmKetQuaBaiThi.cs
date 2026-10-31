using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Forms.KiemTra
{
    public partial class FrmKetQuaBaiThi : Form
    {
        private int _soCauDung;
        private int _tongSoCau;
        private double _diem;
        public bool YeuCauXemLai { get; private set; }
        public FrmKetQuaBaiThi()
        {
            InitializeComponent();
        }

        private void FrmKetQuaBaiThi_Load(object sender, EventArgs e)
        {

        }
        public FrmKetQuaBaiThi(
            int soCauDung,
            int tongSoCau,
            double diem) : this()
        {
            _soCauDung = soCauDung;
            _tongSoCau = tongSoCau;
            _diem = diem;

            TaoGiaoDien();
        }
        private Label TaoLabel(
            string noiDung,
            int x, int y, int width, int height,
            float fontSize, Color mau,
            bool inDam = false)
        {
            Label lbl = new Label();

            lbl.Text = noiDung;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(width, height);

            lbl.Font = new Font(
                "Segoe UI", fontSize,
                inDam ? FontStyle.Bold : FontStyle.Regular);

            lbl.ForeColor = mau;
            lbl.BackColor = Color.Transparent;
            lbl.TextAlign = ContentAlignment.MiddleCenter;

            return lbl;
        }
        private void BoGoc(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
                return;

            int diameter = radius * 2;
            int width = control.Width;
            int height = control.Height;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(
                    0, 0, diameter, diameter, 180, 90);

                path.AddArc(
                    width - diameter, 0,
                    diameter, diameter, 270, 90);

                path.AddArc(
                    width - diameter,
                    height - diameter,
                    diameter, diameter, 0, 90);

                path.AddArc(
                    0, height - diameter,
                    diameter, diameter, 90, 90);

                path.CloseFigure();

                Region oldRegion = control.Region;
                control.Region = new Region(path);

                if (oldRegion != null)
                    oldRegion.Dispose();
            }
        }
        private Button TaoNut(
            string text,
            int x, int y,
            Color mauNen,
            Color mauChu)
        {
            Button btn = new Button();

            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(170, 44);
            btn.BackColor = mauNen;
            btn.ForeColor = mauChu;
            btn.Font = new Font(
                "Segoe UI", 10F, FontStyle.Bold);

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.UseVisualStyleBackColor = false;
            btn.Cursor = Cursors.Hand;

            BoGoc(btn, 10);

            btn.Resize += (s, e) =>
            {
                BoGoc(btn, 10);
            };

            return btn;
        }
        private void TaoGiaoDien()
        {
            SuspendLayout();

            Color xanhDuong = Color.FromArgb(49, 85, 181);
            Color xanhLa = Color.FromArgb(22, 163, 74);
            Color mauChu = Color.FromArgb(30, 41, 59);
            Color mauPhu = Color.FromArgb(100, 116, 139);

            Text = "Kết quả bài thi";
            ClientSize = new Size(520, 540);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(248, 250, 252);

            // Header
            Panel header = new Panel();
            header.Location = new Point(0, 0);
            header.Size = new Size(520, 68);
            header.BackColor = xanhDuong;

            header.Controls.Add(TaoLabel(
                "KẾT QUẢ BÀI THI",
                0, 10, 520, 48,
                18F, Color.White, true));

            Controls.Add(header);

            // Thẻ nội dung
            Panel card = new Panel();
            card.Location = new Point(25, 86);
            card.Size = new Size(470, 425);
            card.BackColor = Color.White;

            Controls.Add(card);
            BoGoc(card, 16);

            // Biểu tượng hoàn thành
            Panel iconCircle = new Panel();
            iconCircle.Location = new Point(200, 22);
            iconCircle.Size = new Size(70, 70);
            iconCircle.BackColor =
                Color.FromArgb(220, 252, 231);

            card.Controls.Add(iconCircle);
            BoGoc(iconCircle, 35);

            iconCircle.Controls.Add(TaoLabel(
                "✓", 0, 0, 70, 70,
                30F, xanhLa, true));

            // Thông báo
            card.Controls.Add(TaoLabel(
                "NỘP BÀI THÀNH CÔNG!",
                15, 103, 440, 32,
                16F, xanhLa, true));

            card.Controls.Add(TaoLabel(
                "Bạn đã hoàn thành bài kiểm tra.",
                15, 139, 440, 25,
                10F, mauPhu));

            // Điểm số
            card.Controls.Add(TaoLabel(
                "ĐIỂM SỐ CỦA BẠN",
                15, 179, 440, 24,
                10F, mauPhu, true));

            card.Controls.Add(TaoLabel(
                $"{_diem:0.##}/10",
                15, 204, 440, 58,
                32F, xanhDuong, true));

            // Tính phần trăm câu đúng
            int phanTram = 0;

            if (_tongSoCau > 0)
            {
                phanTram = (int)Math.Round(
                    100.0 * _soCauDung / _tongSoCau);
            }

            phanTram = Math.Max(
                0, Math.Min(100, phanTram));

            // Thanh tiến độ
            Panel progressBg = new Panel();
            progressBg.Location = new Point(65, 272);
            progressBg.Size = new Size(340, 10);
            progressBg.BackColor =
                Color.FromArgb(226, 232, 240);

            card.Controls.Add(progressBg);
            BoGoc(progressBg, 5);

            Panel progressFill = new Panel();
            progressFill.Location = new Point(0, 0);
            progressFill.Size = new Size(
                (int)(340 * phanTram / 100.0), 10);
            progressFill.BackColor = xanhLa;

            progressBg.Controls.Add(progressFill);

            // Số câu đúng
            card.Controls.Add(TaoLabel(
                $"Số câu đúng: {_soCauDung}/{_tongSoCau}",
                15, 291, 440, 30,
                12F, mauChu, true));

            // Xếp loại
            string xepLoai;
            Color mauXepLoai;

            if (_diem >= 8)
            {
                xepLoai = "Giỏi";
                mauXepLoai = xanhLa;
            }
            else if (_diem >= 6.5)
            {
                xepLoai = "Khá";
                mauXepLoai = xanhDuong;
            }
            else if (_diem >= 5)
            {
                xepLoai = "Trung bình";
                mauXepLoai = Color.DarkOrange;
            }
            else
            {
                xepLoai = "Cần cố gắng";
                mauXepLoai = Color.Firebrick;
            }

            card.Controls.Add(TaoLabel(
                $"Xếp loại: {xepLoai}",
                15, 324, 440, 28,
                11F, mauXepLoai, true));

            // Nút xem lại bài
            Button btnXemLai = TaoNut(
                "XEM LẠI BÀI",
                53, 365,
                Color.FromArgb(226, 232, 240),
                mauChu);

            btnXemLai.Click += (s, e) =>
            {
                YeuCauXemLai = true;
                DialogResult = DialogResult.OK;
                Close();
            };

            // Nút hoàn thành
            Button btnHoanThanh = TaoNut(
                "HOÀN THÀNH",
                247, 365,
                xanhDuong,
                Color.White);

            btnHoanThanh.Click += (s, e) =>
            {
                YeuCauXemLai = false;
                DialogResult = DialogResult.Cancel;
                Close();
            };

            card.Controls.Add(btnXemLai);
            card.Controls.Add(btnHoanThanh);

            AcceptButton = btnHoanThanh;

            ResumeLayout(false);
        }
    }
}
