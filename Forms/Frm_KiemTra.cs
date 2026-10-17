using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
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

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class Frm_KiemTra : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly int _deThiID;
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        private DeThi _deThi;
        private List<CauHoi> _cauHois = new List<CauHoi>();
        private Dictionary<int, string> _dapAnChon = new Dictionary<int, string>(); // index -> 'A'/'B'/'C'/'D'
        private int _currentIndex = 0;
        private int _thoiGianConLai; // giây
        private Timer _timer;
        public Frm_KiemTra(int deThiID)
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            _deThiID = deThiID;

        }

        private void KiemTra_Load(object sender, EventArgs e)
        {
            _deThi = _deThiDAL.LayTheoID(_deThiID);
            _cauHois = _deThiDAL.LayDanhSachCauHoi(_deThiID);
            if (_deThi == null || _cauHois.Count == 0)
            {
                MessageBox.Show("Đề thi chưa có câu hỏi nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }
            this.Text = $"BÀI THI: {_deThi.TenDeThi} - Môn: {_deThi.MonHoc}";
            _thoiGianConLai = _deThi.ThoiGian * 60;
            // Khởi tạo Timer đếm ngược
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            // Khởi tạo các sự kiện nút điều hướng
            btnKiemTra_CauTruoc.Click += (s, ev) => ChuyenCau(_currentIndex - 1);
            btnKiemTra_CauSau.Click += (s, ev) => ChuyenCau(_currentIndex + 1);
            btnKiemTra_NopBai.Click += (s, ev) => XacNhanNopBai();
            // Sự kiện chọn radio button
            radKiemTra_DapAnA.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged += Radio_CheckedChanged;
            HienThiCauHoi(_currentIndex);

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_thoiGianConLai > 0)
            {
                _thoiGianConLai--;
                int phut = _thoiGianConLai / 60;
                int giay = _thoiGianConLai % 60;
                lblKiemTra_SoCauHoi.Text = $"Thời gian: {phut:D2}:{giay:D2}";
            }
            else
            {
                _timer.Stop();
                MessageBox.Show("Đã hết thời gian làm bài! Hệ thống tự động nộp bài.", "Hết giờ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NopBai();
            }
        }
        private void HienThiCauHoi(int index)
        {
            if (index < 0 || index >= _cauHois.Count) return;
            _currentIndex = index;
            CauHoi q = _cauHois[index];
            lblKiemTra_CauHoi.Text = $"Câu {index + 1}/{_cauHois.Count}: {q.NoiDung}";
            radKiemTra_DapAnA.Text = $"A. {q.DapAnA}";
            radKiemTra_DapAnB.Text = $"B. {q.DapAnB}";
            radKiemTra_DapAnC.Text = $"C. {q.DapAnC}";
            radKiemTra_DapAnD.Text = $"D. {q.DapAnD}";
            // Bỏ chọn trước khi set lại trạng thái đã lưu
            radKiemTra_DapAnA.Checked = false;
            radKiemTra_DapAnB.Checked = false;
            radKiemTra_DapAnC.Checked = false;
            radKiemTra_DapAnD.Checked = false;
            if (_dapAnChon.ContainsKey(index))
            {
                string da = _dapAnChon[index];
                if (da == "A") radKiemTra_DapAnA.Checked = true;
                else if (da == "B") radKiemTra_DapAnB.Checked = true;
                else if (da == "C") radKiemTra_DapAnC.Checked = true;
                else if (da == "D") radKiemTra_DapAnD.Checked = true;
            }
            proKT_TienDo.Value = (int)((index + 1) / (float)_cauHois.Count * 100);
        }
        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            if (radKiemTra_DapAnA.Checked) _dapAnChon[_currentIndex] = "A";
            else if (radKiemTra_DapAnB.Checked) _dapAnChon[_currentIndex] = "B";
            else if (radKiemTra_DapAnC.Checked) _dapAnChon[_currentIndex] = "C";
            else if (radKiemTra_DapAnD.Checked) _dapAnChon[_currentIndex] = "D";
        }
        private void ChuyenCau(int newIndex)
        {
            if (newIndex >= 0 && newIndex < _cauHois.Count)
            {
                HienThiCauHoi(newIndex);
            }
        }
        private void XacNhanNopBai()
        {
            int soCauDaLam = _dapAnChon.Count;
            string msg = $"Bạn đã làm {soCauDaLam}/{_cauHois.Count} câu.\nBạn có chắc chắn muốn nộp bài?";
            if (MessageBox.Show(msg, "Xác nhận nộp bài", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _timer.Stop();
                NopBai();
            }
        }
        private void NopBai()
        {
            int soCauDung = 0;
            for (int i = 0; i < _cauHois.Count; i++)
            {
                CauHoi q = _cauHois[i];
                if (_dapAnChon.ContainsKey(i) && string.Equals(_dapAnChon[i], q.DapAnDung, StringComparison.OrdinalIgnoreCase))
                {
                    soCauDung++;
                }
            }
            decimal diem = Math.Round(((decimal)soCauDung / _cauHois.Count) * 10, 2);
            KetQuaThi kq = new KetQuaThi
            {
                HocSinhID = Session.NguoiDungID,
                DeThiID = _deThiID,
                SoCauDung = soCauDung,
                Diem = diem,
                ThoiGianNop = DateTime.Now,
                TrangThai = "Hoàn thành"
            };
            int kqID = _ketQuaDAL.LuuKetQua(kq);
            // Lưu chi tiết bài làm
            for (int i = 0; i < _cauHois.Count; i++)
            {
                CauHoi q = _cauHois[i];
                string chon = _dapAnChon.ContainsKey(i) ? _dapAnChon[i] : "";
                bool laDung = string.Equals(chon, q.DapAnDung, StringComparison.OrdinalIgnoreCase);
                _ketQuaDAL.LuuChiTietBaiLam(kqID, q.CauHoiID, chon, laDung);
            }
            MessageBox.Show($"Kết quả: {soCauDung}/{_cauHois.Count} câu đúng.\nĐiểm số: {diem}/10",
                            "Kết quả bài thi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void foreverButton2_Click(object sender, EventArgs e)
        {

        }
        private bool _daNopBai = false;
        private void btn_CauTruoc_Click(object sender, EventArgs e)
        {
            if (_currentIndex > 0)
            {
                ChuyenCau(_currentIndex - 1);
            }
        }

        private void btn_NopBai_Click(object sender, EventArgs e)
        {
            XacNhanNopBai();
        }
        private void Frm_KiemTra_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_daNopBai)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang trong quá trình làm bài thi. Nếu thoát bây giờ, hệ thống sẽ tự động thu bài và chấm điểm hiện tại.\n\nBạn có chắc chắn muốn thoát?",
                    "Cảnh báo thoát phòng thi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    //LuuKetQuaBaiThi(); // Tự động lưu kết quả trước khi đóng
                    _daNopBai = true;
                }
                else
                {
                    e.Cancel = true; // Hủy lệnh đóng form, cho thí sinh làm tiếp
                }
            }
        }
    }
}
