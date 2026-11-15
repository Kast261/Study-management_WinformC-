using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.Forms.KiemTra;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class Frm_KiemTra : ReaLTaiizor.Forms.MaterialForm
    {
        #region Dữ liệu & Trạng thái

        private readonly int _deThiID;
        private readonly IDeThiService _deThiService = new DeThiService();
        private readonly IKetQuaService _ketQuaService = new KetQuaService();
        private DeThi _deThi;
        private List<CauHoi> _cauHois = new List<CauHoi>();
        private Dictionary<int, string> _dapAnChon = new Dictionary<int, string>(); 
        private int _currentIndex = 0;
        private int _thoiGianConLai; // tính theo giây
        private Timer _timer;
        private bool _daNopBai = false;
        private bool _dangNopBai;
        private DateTime _ketThucLuc;
        private readonly GiamSatPhongThi _giamSat = new GiamSatPhongThi();
        private int _soLanDaLuu, _soLanBanDau;
        private bool _hopThoaiNoiBo, _sanSangGiamSat;
        private bool _loiLuuViPham;

        #endregion

        #region 1. Khởi tạo, Nạp Dữ Liệu & Đăng Ký Sự Kiện

        public Frm_KiemTra(int deThiID)
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            _deThiID = deThiID;
            FormStyle = ReaLTaiizor.Enum.Material.FormStyles.StatusAndActionBar_None;
            Sizable = false;
            FormBorderStyle = FormBorderStyle.None;
            ControlBox = MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Normal;
            Bounds = Screen.FromControl(this).Bounds;
        }

        private void KiemTra_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Session.IsHocSinh || !_deThiService.CoTheBatDauThi(Session.NguoiDungID, _deThiID))
                {
                    ThongBao("Đề thi đã hết hạn, đã làm hoặc không thuộc lớp của bạn.", "Không thể bắt đầu thi");
                    _daNopBai = true;
                    Close();
                    return;
                }
                _deThi = _deThiService.LayTheoID(_deThiID);
                _cauHois = _deThiService.LayDanhSachCauHoi(_deThiID);

                if (_deThi == null || _cauHois.Count == 0)
                {
                    ThongBao("Đề thi chưa có câu hỏi nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _daNopBai = true;
                    this.Close();
                    return;
                }

                _soLanBanDau = _ketQuaService.BatDauGiamSat(Session.TaiKhoanID, Session.NguoiDungID, _deThiID);
                _soLanDaLuu = _soLanBanDau;
                _sanSangGiamSat = true;
                this.Text = $"BÀI THI: {_deThi.TenDeThi} - Môn: {_deThi.MonHoc}";
                _ketThucLuc = DateTime.Now.AddMinutes(_deThi.ThoiGian);
                if (_deThi.HanNop.HasValue && _deThi.HanNop.Value < _ketThucLuc)
                    _ketThucLuc = _deThi.HanNop.Value;
                _thoiGianConLai = Math.Max(0, (int)Math.Ceiling((_ketThucLuc - DateTime.Now).TotalSeconds));
                CapNhatTrangThaiThi();

                // Timer đếm ngược
                _timer = new Timer { Interval = 1000 };
                _timer.Tick += Timer_Tick;
                _timer.Start();

                HienThiCauHoi(_currentIndex);
                CapNhatTienDoProgressBar();
            }
            catch (Exception ex)
            {
                _daNopBai = true; // Không hỏi nộp bài khi chưa tải được phòng thi.
                ThongBao("Không thể mở bài thi: " + ex.Message, "Lỗi");
                Close();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Bounds = Screen.FromControl(this).Bounds;
            if (!_sanSangGiamSat || _daNopBai) return;
            _giamSat.BatDau(_soLanBanDau);
            CapNhatTrangThaiThi();
            if (_giamSat.BiDung) DungVaNopBai();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _giamSat.KichHoat();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (_daNopBai || _dangNopBai || !_giamSat.RoiCuaSo(_hopThoaiNoiBo)) return;
            // Khóa ngay trên máy học viên ở lần 3, kể cả khi mạng đang lỗi.
            if (_giamSat.BiDung) KhoaDapAn();
            try { LuuViPham(); }
            catch { _loiLuuViPham = true; } // Nộp bài sẽ thử lưu lại trước khi chấm điểm.
            CapNhatTrangThaiThi();
            if (_giamSat.BiDung) DungVaNopBai();
        }

        private void LuuViPham()
        {
            if (_giamSat.SoLan <= _soLanDaLuu) return;
            _soLanDaLuu = _ketQuaService.GhiNhanRoiManHinh(Session.TaiKhoanID,
                Session.NguoiDungID, _deThiID, _giamSat.SoLan);
            _loiLuuViPham = false;
        }

        private void CapNhatTrangThaiThi()
        {
            lblKiemTra_SoCauHoi.Text = _giamSat.BiDung
                ? "ĐÃ DỪNG: rời cửa sổ thi 3/3 lần. Nộp bài để lưu kết quả."
                : $"Thời gian: {_thoiGianConLai / 60:D2}:{_thoiGianConLai % 60:D2} | Rời cửa sổ: {_giamSat.SoLan}/3\nLần thứ 3 sẽ dừng và tự nộp bài.";
            if (_loiLuuViPham) lblKiemTra_SoCauHoi.Text += " · Chưa đồng bộ vi phạm";
        }

        private void KhoaDapAn()
        {
            radKiemTra_DapAnA.Enabled = radKiemTra_DapAnB.Enabled =
                radKiemTra_DapAnC.Enabled = radKiemTra_DapAnD.Enabled = false;
            btnKiemTra_CauTruoc.Enabled = btnKiemTra_CauSau.Enabled = false;
        }

        private void DungVaNopBai()
        {
            KhoaDapAn();_timer?.Stop();
            BeginInvoke(new Action(() => { if (!IsDisposed && !_daNopBai) NopBai(); }));
        }

        private DialogResult ThongBao(string text, string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            bool previous = _hopThoaiNoiBo;
            _hopThoaiNoiBo = true;
            try { return MessageBox.Show(this, text, caption, buttons, icon); }
            finally { _hopThoaiNoiBo = previous; }
        }

        private void DangKySuKienRadio()
        {
            radKiemTra_DapAnA.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged += Radio_CheckedChanged;
        }

        private void HuyDangKySuKienRadio()
        {
            radKiemTra_DapAnA.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged -= Radio_CheckedChanged;
        }

        #endregion

        #region 2. Hiển Thị Câu Hỏi & Điều Hướng

        private void HienThiCauHoi(int index)
        {
            if (index < 0 || index >= _cauHois.Count) return;

            _currentIndex = index;
            CauHoi q = _cauHois[index];

            // Hiển thị vị trí câu đang làm
            lblKiemTra_CauHoi.Text = $"Câu {index + 1}/{_cauHois.Count}: {q.NoiDung}";
            radKiemTra_DapAnA.Text = $"A. {q.DapAnA}";
            radKiemTra_DapAnB.Text = $"B. {q.DapAnB}";
            radKiemTra_DapAnC.Text = $"C. {q.DapAnC}";
            radKiemTra_DapAnD.Text = $"D. {q.DapAnD}";

            HuyDangKySuKienRadio();

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

            DangKySuKienRadio();
        }

        private void btnKiemTra_CauTruoc_Click(object sender, EventArgs e)
        {
            ChuyenCau(_currentIndex - 1);
        }

        private void btnKiemTra_CauSau_Click(object sender, EventArgs e)
        {
            ChuyenCau(_currentIndex + 1);
        }

        private void ChuyenCau(int newIndex)
        {
            if (_giamSat.BiDung) return;
            if (newIndex >= 0 && newIndex < _cauHois.Count)
            {
                HienThiCauHoi(newIndex);
            }
        }

        #endregion

        #region 3. Chọn Đáp Án & Tiến Độ Làm Bài

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            if (_giamSat.BiDung || _dangNopBai || DateTime.Now >= _ketThucLuc) return;
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                if (rb == radKiemTra_DapAnA) _dapAnChon[_currentIndex] = "A";
                else if (rb == radKiemTra_DapAnB) _dapAnChon[_currentIndex] = "B";
                else if (rb == radKiemTra_DapAnC) _dapAnChon[_currentIndex] = "C";
                else if (rb == radKiemTra_DapAnD) _dapAnChon[_currentIndex] = "D";

                CapNhatTienDoProgressBar();
            }
        }

        private void CapNhatTienDoProgressBar()
        {
            if (_cauHois.Count > 0 && proKT_TienDo != null)
            {
                int phanTram = (int)((float)_dapAnChon.Count / _cauHois.Count * 100);
                proKT_TienDo.Value = Math.Min(100, phanTram);
            }
        }

        #endregion

        #region 4. Đồng Hồ Đếm Ngược

        private void Timer_Tick(object sender, EventArgs e)
        {
            _thoiGianConLai = Math.Max(0, (int)Math.Ceiling((_ketThucLuc - DateTime.Now).TotalSeconds));
            CapNhatTrangThaiThi();
            if ((_thoiGianConLai == 0 || _giamSat.BiDung) && !_dangNopBai)
            {
                _timer.Stop();
                NopBai();
            }
        }

        #endregion

        #region 5. Xác Nhận & Nộp Bài

        private void btnKiemTra_NopBai_Click(object sender, EventArgs e)
        {
            XacNhanNopBai();
        }

        private void XacNhanNopBai()
        {
            if (_giamSat.BiDung) { NopBai(); return; }
            DialogResult res = ThongBao("Bạn có muốn nộp bài không?", "Xác nhận nộp bài", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                _timer.Stop();
                NopBai();
            }
            // Nếu chọn No thì hộp thoại đóng lại, thí sinh tiếp tục làm bài bình thường
        }

        private void NopBai(bool dongForm = true)
        {
            if (_daNopBai || _dangNopBai || _cauHois.Count == 0) return;
            _dangNopBai = true;
            _timer?.Stop();
            KetQuaThi result;
            try
            {
                var answers = new Dictionary<int, string>();
                for (int i = 0; i < _cauHois.Count; i++)
                    if (_dapAnChon.ContainsKey(i)) answers[_cauHois[i].CauHoiID] = _dapAnChon[i];
                LuuViPham();
                result = _ketQuaService.NopBai(Session.TaiKhoanID, Session.NguoiDungID, _deThiID, answers);
                _daNopBai = true;
                _giamSat.KetThuc();
            }
            catch (Exception ex)
            {
                ThongBao("Chưa lưu được bài thi. Bạn có thể bấm Nộp bài để thử lại.\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Không cho sửa đáp án sau khi hết giờ, nhưng vẫn cho phép nộp lại.
                bool conGio = !_giamSat.BiDung && DateTime.Now < _ketThucLuc;
                radKiemTra_DapAnA.Enabled = radKiemTra_DapAnB.Enabled =
                    radKiemTra_DapAnC.Enabled = radKiemTra_DapAnD.Enabled = conGio;
                if (conGio) _timer?.Start();
                return;
            }
            finally { _dangNopBai = false; }

            if (_giamSat.BiDung)
                ThongBao("Bài thi đã dừng và tự nộp vì rời cửa sổ thi 3 lần. Giáo viên sẽ thấy vi phạm trong kết quả.",
                    "Dừng do vi phạm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            using (var frm = new FrmKetQuaBaiThi(result.SoCauDung, _cauHois.Count, (double)result.Diem))
            {
                frm.ShowDialog(this);
                if (frm.YeuCauXemLai)
                    using (var review = new Forms.Frm_XemLaiBaiThi(result.KetQuaID)) review.ShowDialog(this);
            }
            if (dongForm) Close();
        }

        #endregion

        #region 6. Đóng Form & Giải Phóng Tài Nguyên

        private void Frm_KiemTra_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_daNopBai || _timer == null) return;
            if (_dangNopBai) { e.Cancel = true; return; }
            if (_giamSat.BiDung)
            {
                NopBai(false);e.Cancel = !_daNopBai;return;
            }
            if (ThongBao("Thoát phòng thi sẽ nộp các đáp án hiện tại. Bạn có muốn nộp bài không?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                NopBai(false);
            // Nếu lưu thất bại, giữ phòng thi mở để người dùng có thể thử lại.
            e.Cancel = !_daNopBai;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _giamSat.KetThuc();
            _timer?.Dispose();
            base.OnFormClosed(e);
        }

        #endregion

    }
}
