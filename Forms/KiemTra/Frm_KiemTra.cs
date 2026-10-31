using BLT_Quan_Ly_Hoc_Tap.DAL;
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
        private readonly int _deThiID;
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        private DeThi _deThi;
        private List<CauHoi> _cauHois = new List<CauHoi>();
        private Dictionary<int, string> _dapAnChon = new Dictionary<int, string>(); 
        private int _currentIndex = 0;
        private int _thoiGianConLai; // tính theo giây
        private Timer _timer;
        private bool _daNopBai = false;

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

            // Timer đếm ngược
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Đăng ký sự kiện các nút điều hướng theo đúng tên button của bạn
            btnKiemTra_CauTruoc.Click += (s, ev) => ChuyenCau(_currentIndex - 1);
            btnKiemTra_CauSau.Click += (s, ev) => ChuyenCau(_currentIndex + 1);
            btnKiemTra_NopBai.Click += (s, ev) => XacNhanNopBai();

            // Đăng ký sự kiện chọn đáp án
            radKiemTra_DapAnA.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged += Radio_CheckedChanged;

            HienThiCauHoi(_currentIndex);
            CapNhatTienDoProgressBar();
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

        private void HuyDangKySuKienRadio()
        {
            radKiemTra_DapAnA.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged -= Radio_CheckedChanged;
        }

        private void DangKySuKienRadio()
        {
            radKiemTra_DapAnA.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged += Radio_CheckedChanged;
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
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

        private void ChuyenCau(int newIndex)
        {
            if (newIndex >= 0 && newIndex < _cauHois.Count)
            {
                HienThiCauHoi(newIndex);
            }
        }

        private void XacNhanNopBai()
        {
            DialogResult res = MessageBox.Show("Bạn có muốn nộp bài không?", "Xác nhận nộp bài", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                _timer.Stop();
                NopBai();
            }
            // Nếu chọn No thì hộp thoại đóng lại, thí sinh tiếp tục làm bài bình thường
        }


        private void NopBai()
        {
            // Tránh nộp bài nhiều lần
            if (_daNopBai)
                return;

            int tongSoCau = _cauHois.Count;

            if (tongSoCau == 0)
            {
                MessageBox.Show(
                    "Bài thi không có câu hỏi!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int soCauDung = 0;

            // Đếm số câu trả lời đúng
            for (int i = 0; i < tongSoCau; i++)
            {
                CauHoi q = _cauHois[i];

                if (_dapAnChon.ContainsKey(i) &&
                    string.Equals(
                        _dapAnChon[i],
                        q.DapAnDung,
                        StringComparison.OrdinalIgnoreCase))
                {
                    soCauDung++;
                }
            }

            // Tính điểm trên thang 10
            decimal diem = Math.Round(
                ((decimal)soCauDung / tongSoCau) * 10,
                2);

            // Chặn nộp trùng trong phiên hiện tại
            _daNopBai = true;

            try
            {
                // Lưu kết quả bài thi
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

                // Lưu chi tiết từng câu trả lời
                for (int i = 0; i < tongSoCau; i++)
                {
                    CauHoi q = _cauHois[i];

                    string chon = _dapAnChon.ContainsKey(i)
                        ? _dapAnChon[i]
                        : "";

                    bool laDung = string.Equals(
                        chon,
                        q.DapAnDung,
                        StringComparison.OrdinalIgnoreCase);

                    _ketQuaDAL.LuuChiTietBaiLam(
                        kqID,
                        q.CauHoiID,
                        chon,
                        laDung);
                }
            }
            catch (Exception ex)
            {
                // Cho phép thử lại nếu lưu thất bại
                // Chú ý: cần transaction để tránh lưu một phần
                _daNopBai = false;

                MessageBox.Show(
                    "Lỗi khi lưu kết quả bài thi:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            // Hiển thị Form kết quả thay cho MessageBox
            using (FrmKetQuaBaiThi frm = new FrmKetQuaBaiThi(
                soCauDung,
                tongSoCau,
                (double)diem))
            {
                frm.ShowDialog(this);

                if (frm.YeuCauXemLai)
                {
                    // TODO: Tích hợp chức năng xem lại bài thi.
                }
            }

            // Đóng Form làm bài sau khi xem kết quả
            this.Close();
        }


        private void Frm_KiemTra_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_daNopBai)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang trong quá trình làm bài thi. Nếu thoát bây giờ, hệ thống sẽ tự động thu bài và chấm điểm.\nBạn có muốn nộp bài không?",
                    "Cảnh báo thoát phòng thi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _timer.Stop();
                    NopBai();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}