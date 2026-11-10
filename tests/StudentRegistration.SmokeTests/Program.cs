using System;
using System.Data;
using System.Windows.Forms;

// Only dialogs and persistence are replaced. Actual production handlers,
// activation refresh and DataGridView/DataRowView selection run in WinForms.
static class DialogStub
{
    public static Action BeforeConfirmationReturns;
    public static DialogResult Answer = DialogResult.Yes;
    public static DialogResult Show(string text, string title) => DialogResult.OK;
    public static DialogResult Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        => Confirm(buttons);
    public static DialogResult Show(IWin32Window owner, string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        => Confirm(buttons);
    private static DialogResult Confirm(MessageBoxButtons buttons)
    {
        if (buttons != MessageBoxButtons.YesNo) return DialogResult.OK;
        BeforeConfirmationReturns?.Invoke();
        return Answer;
    }
}

static class Session { public static int NguoiDungID = 22; }

sealed class ClassStore
{
    public int RegisteredClass, CancelledClass, Student;
    public int Reads;
    public DataTable LayLopChuaDangKy(int student) { Reads++; return Classes(); }
    public DataTable LayLopHocVienDangHocVaChoDuyet(int student) => Classes();
    private static DataTable Classes()
    {
        var table = new DataTable();
        table.Columns.Add("LopHocID", typeof(int)); table.Columns.Add("TenLop");
        table.Columns.Add("TrangThaiDangKy");
        table.Rows.Add(7, "Toán Lớp 2", "Chờ duyệt");
        table.Rows.Add(8, "Toán Lớp 2", "Chờ duyệt");
        return table;
    }
    public bool DangKyLop(int classroom, int student, out string message)
    {
        Console.WriteLine("Register: class=" + classroom + ", student=" + student);
        RegisteredClass = classroom; Student = student; message = "Đăng ký thành công"; return classroom > 0;
    }
    public bool HuyDangKyLop(int classroom, int student, out string message)
    {
        Console.WriteLine("Cancel: class=" + classroom + ", student=" + student);
        CancelledClass = classroom; Student = student; message = "Đã hủy"; return classroom > 0;
    }
}

partial class RegistrationHost : Form
{
    private readonly ClassStore _lopService = new ClassStore();
    private readonly DataGridView dgvLopDangKi = new DataGridView { MultiSelect = false };
    private readonly DataGridView dgvLopThamGia = new DataGridView { MultiSelect = false };
    private readonly Label lblHV_DangChon = new Label();
    private readonly TabControl tabHocVien = new TabControl();
    private readonly TabPage pageHV_TrangChu = new TabPage(), pageHV_BaiThi = new TabPage(),
        pageHV_KetQua = new TabPage(), pageHV_LopHoc = new TabPage(), pageHV_ThongTin = new TabPage();
    private int _selectedLopDangKyID, _selectedLopDangHocID;
    private bool _daTaiHocVien, _dangLamMoiTrang, _dangXuLyDangKyLop;
    public ClassStore Store => _lopService;
    public RegistrationHost()
    {
        Controls.Add(tabHocVien);
        tabHocVien.TabPages.Add(pageHV_LopHoc);
        pageHV_LopHoc.Controls.Add(dgvLopDangKi);
        pageHV_LopHoc.Controls.Add(dgvLopThamGia);
        dgvLopDangKi.Dock = DockStyle.Top; dgvLopThamGia.Dock = DockStyle.Bottom;
        Show(); Application.DoEvents();
        LoadTabLopHocCuaToi();
        _daTaiHocVien = true;
    }
    public void SelectClass(bool cancellation)
    {
        var grid = cancellation ? dgvLopThamGia : dgvLopDangKi;
        grid.ClearSelection(); grid.CurrentCell = grid.Rows[0].Cells[0]; grid.Rows[0].Selected = true;
        _selectedLopDangKyID = _selectedLopDangHocID = 7;
    }
    public void ActivateAgain() => OnActivated(EventArgs.Empty);
    public void ForceRebind() => LoadTabLopHocCuaToi();
    public void Submit(bool cancellation)
    {
        if (cancellation) btnHV_HuyDK_Click(this, EventArgs.Empty);
        else btnDangKyLop_Click(this, EventArgs.Empty);
    }
    private void LoadLopHoc() { }
    private void LoadDanhSachDeThi() { }
    private void LoadLichSuThi() { }
    private void LoadThongTinHocVien() { }
}

static class Program
{
    [STAThread]
    static int Main()
    {
        try
        {
            foreach (bool cancellation in new[] { false, true })
            foreach (bool forceRebind in new[] { false, true })
            {
                using (var host = new RegistrationHost())
                {
                    host.SelectClass(cancellation);
                    DialogStub.Answer = DialogResult.Yes;
                    DialogStub.BeforeConfirmationReturns = () => {
                        host.ActivateAgain();
                        if (forceRebind) host.ForceRebind();
                    };
                    host.Submit(cancellation);
                    Check((cancellation ? host.Store.CancelledClass : host.Store.RegisteredClass) == 7,
                        "Confirmed operation must retain class #7 after activation/rebinding");
                    Check(host.Store.Student == 22, "Operation must retain the student ID");
                    int reads = host.Store.Reads;
                    host.ActivateAgain();
                    Check(host.Store.Reads > reads, "Refresh must resume after the operation");
                }
            }
            foreach (bool cancellation in new[] { false, true })
            {
                using (var host = new RegistrationHost())
                {
                    host.SelectClass(cancellation);
                    DialogStub.Answer = DialogResult.No;
                    DialogStub.BeforeConfirmationReturns = host.ActivateAgain;
                    host.Submit(cancellation);
                    Check(host.Store.RegisteredClass == 0 && host.Store.CancelledClass == 0,
                        "Declining confirmation must not write");
                    int reads = host.Store.Reads;
                    host.ActivateAgain();
                    Check(host.Store.Reads > reads, "Refresh must resume after declining");
                }
            }
            Console.WriteLine("PASS: 16 registration/cancellation activation assertions");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
    static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
