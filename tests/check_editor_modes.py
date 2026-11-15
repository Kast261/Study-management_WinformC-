"""Exercise production row-selection/reset handlers with control doubles (.NET 8).

Checks editor state transitions and populated inputs, not WinForms rendering.
"""
import argparse
from pathlib import Path
import re
import subprocess
import tempfile
from xml.sax.saxutils import quoteattr

parser = argparse.ArgumentParser()
parser.add_argument('--dotnet', default='dotnet')
parser.add_argument('--source-root', type=Path, help='Optional older source tree for regression verification')
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
source_root = args.source_root or root

scenarios = [
    ('Admin', 'dgvGiaoVien', 'GiaoVienID', '_selectedGiaoVienID', 'btnGV_Them', 'btnGV_Sua', 'txtGV_HoTen', 'LamMoiFormGV'),
    ('Admin', 'dgvHocVien', 'HocSinhID', '_selectedHocVienID', 'btnHV_Them', 'btnHV_Sua', 'txtHV_HoTen', 'LamMoiFormHV'),
    ('Admin', 'dgvKhoaHoc', 'KhoaHocID', '_selectedKhoaHocID', 'btnKH_Them', 'btnKH_Sua', 'txtKH_TenKhoaHoc', 'LamMoiFormKH'),
    ('Admin', 'dgvLopHoc', 'LopHocID', '_selectedLopHocID', 'btnLop_Them', 'btnLop_Sua', 'txtLop_TenLop', 'LamMoiFormLop'),
    ('Teacher', 'dgvLopHoc', 'LopHocID', '_selectedLopHocID', 'btnLH_Them', 'btnLH_CapNhap', 'txtLH_TenLop', 'LamMoiFormLop'),
    ('Teacher', 'dgvCauHoi', 'CauHoiID', '_selectedCauHoiID', 'btnCH_ThemCH', 'btnCH_CapNhap', 'txtCH_NhapCauHoi', 'LamMoiFormCauHoi'),
]

def method(source, name):
    match = re.search(r'(?m)^        private void ' + re.escape(name) + r'\(', source)
    if not match:
        raise RuntimeError('Missing production method: ' + name)
    opening = source.index('{', match.start())
    end, depth = opening + 1, 1
    while depth:
        depth += (source[end] == '{') - (source[end] == '}')
        end += 1
    return source[match.start():end]

hosts = []
for host, path in [('Admin', 'Forms/Admin/Frm_Admin.cs'), ('Teacher', 'Forms/GiaoVien/frm_GiaoVien.cs')]:
    source = (source_root / path).read_text(encoding='utf-8-sig')
    cases = [case for case in scenarios if case[0] == host]
    names = [name for _, grid, *_, reset in cases for name in (grid + '_CellClick', reset)]
    methods = '\n'.join(method(source, name) for name in names)
    controls = set(re.findall(r'\b(?:txt|btn|headerLabel|materialLabel|materialComboBox|cbo|cmb|date|nUD|_lopCho)[A-Za-z0-9_]+\b', methods))
    fields = '\n'.join('FakeControl ' + name + ' = new FakeControl();' for name in sorted(controls))
    fields += '\n' + '\n'.join('int ' + name + ';' for name in sorted({case[3] for case in cases}))
    fields += '\n' + '\n'.join('FakeGrid ' + case[1] + ' = new FakeGrid();' for case in cases)
    fields += '\nbool _dangNhapExcel;'
    checks = []
    for _, grid, key, selected, add, edit, text, reset in cases:
        item = ('new CauHoi { CauHoiID=17, ThuTu=1, NoiDung="Existing", DapAnA="A", '
                'DapAnB="B", DapAnC="C", DapAnD="D", DapAnDung="A" }'
                if key == 'CauHoiID' else f'Program.Row("{key}",17)')
        checks.append(f'''
        {grid}.Rows.Clear();
        {grid}.Rows.Add(new DataGridViewRow {{ DataBoundItem={item} }});
        {reset}();
        Program.Check({add}.Enabled && !{edit}.Enabled, "{host}/{grid}: fresh editor");
        {grid}_CellClick(this,new DataGridViewCellEventArgs(0));
        Program.Check({selected}==17 && {text}.Text=="Existing", "{host}/{grid}: selected record populated");
        Program.Check(!{add}.Enabled && {edit}.Enabled, "{host}/{grid}: selection must edit, not insert");
        {grid}_CellClick(this,new DataGridViewCellEventArgs(-1));
        Program.Check(!{add}.Enabled && {selected}==17, "{host}/{grid}: header click must not allow insert");
        {reset}();
        Program.Check({selected}==0 && {text}.Text=="" && {add}.Enabled && !{edit}.Enabled,
            "{host}/{grid}: reset clears copied data before allowing insert");
''')
    # Filtering/paging must not clear the search box when clearing copied inputs.
    if host == 'Admin':
        checks.append('''txtGV_TimKiem.Text="filter"; LamMoiFormGV(false);
        Program.Check(txtGV_TimKiem.Text=="filter", "teacher filter preserved");
        txtHV_TimKiem.Text="filter"; LamMoiFormHV(false);
        Program.Check(txtHV_TimKiem.Text=="filter", "student filter preserved");''')
    hosts.append(f'class {host}Host {{\n{fields}\n{methods}\npublic void Verify() {{ {"".join(checks)} }}\n}}')

account_source = (source_root / 'Forms/Admin/Frm_Admin.cs').read_text(encoding='utf-8-sig')
account_methods = '\n'.join(method(account_source, name) for name in
    ['dgvTaiKhoan_SelectionChanged', 'LamMoiFormTK', 'LoadDropdownGanCho'])
account_controls = set(re.findall(r'\b(?:txt|btn|cmb)[A-Za-z0-9_]+\b', account_methods))
account_fields = '\n'.join('FakeControl ' + name + ' = new FakeControl();' for name in sorted(account_controls))
hosts.append('class AccountHost {\n' + account_fields + r'''
    FakeGrid dgvTaiKhoan = new FakeGrid();
    FakeAccountService _tkService = new FakeAccountService();
    int _selectedTaiKhoanID;
    bool _selectedTrangThaiKhoa, _selectedTaiKhoanLaAdmin, _dangSuaTaiKhoan;
    void UpdateNutKhoaMoKhoa() { }
    public void Verify()
    {
        cmbTK_VaiTro.SelectedItem="HocSinh";
        cmbTK_VaiTro.SelectedIndex=0;
        var selected = new DataGridViewRow { DataBoundItem=Program.Row("TaiKhoanID",17) };
        dgvTaiKhoan.SelectedRows.Add(selected);
        dgvTaiKhoan_SelectionChanged(this,EventArgs.Empty);
        Program.Check(_selectedTaiKhoanID==17 && !btnTK_TaoTaiKhoan.Enabled, "account selection disables creation");
        LoadDropdownGanCho();
        Program.Check(!btnTK_TaoTaiKhoan.Enabled, "role change must not reenable creation");
        // Clearing a search can synchronously bind/select a row again.
        txt_TimTaiKhoan.OnClear = () => {
            dgvTaiKhoan.SelectedRows.Add(selected);
            dgvTaiKhoan_SelectionChanged(this,EventArgs.Empty);
        };
        LamMoiFormTK();
        Program.Check(!_dangSuaTaiKhoan && _selectedTaiKhoanID==0 && btnTK_TaoTaiKhoan.Enabled,
            "account reset must survive search rebinding and enable creation");
        Program.Check(dgvTaiKhoan.SelectedRows.Count==0, "account reset clears selection");
    }
''' + account_methods + '\n}')

harness = r'''
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.Models;
class FakeControl
{
    public string Text="";
    public bool Enabled=true, Checked, Visible;
    public object DataSource;
    public string DisplayMember, ValueMember;
    public Action OnClear;
    public ArrayList Items = new ArrayList { "Nam", "A" };
    public int SelectedIndex;
    public object SelectedItem, SelectedValue;
    public decimal Value, Minimum=0, Maximum=100;
    public void Clear() { Text=""; OnClear?.Invoke(); }
    public void Focus() { }
}
class DataGridViewRow { public object DataBoundItem; }
class FakeGrid
{
    public List<DataGridViewRow> Rows = new List<DataGridViewRow>();
    public List<DataGridViewRow> SelectedRows = new List<DataGridViewRow>();
    public void ClearSelection() => SelectedRows.Clear();
}
class DataGridViewCellEventArgs : EventArgs
{
    public int RowIndex;
    public DataGridViewCellEventArgs(int row) { RowIndex=row; }
}
enum MessageBoxButtons { OK }
enum MessageBoxIcon { Error }
static class MessageBox { public static void Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon) { throw new Exception(text); } }
class FakeAccountService
{
    public DataTable LayHoSoChuaCoTaiKhoan(string role)
    {
        var table=new DataTable();table.Columns.Add("NguoiDungID",typeof(int));table.Columns.Add("HoTen",typeof(string));
        table.Rows.Add(99,"Available");return table;
    }
}
static class DatePickerInput { public static void SetValue(FakeControl input, object value) { } }
static class Program
{
    public static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    public static DataRowView Row(string key, int id)
    {
        var table=new DataTable();
        table.Columns.Add(key,typeof(int));
        foreach(var name in new[] { "HoTen","ChuyenMon","Email","SoDienThoai","DiaChi","GioiTinh",
            "TenKhoaHoc","MoTa","TenLop","LichHoc","TenGiaoVien","SiSo","VaiTro" })
            table.Columns.Add(name,typeof(string));
        foreach(var name in new[] { "NgaySinh","NgayBatDau","NgayKetThuc","KhoaHocID","GiaoVienID","SiSoToiDa" })
            if(!table.Columns.Contains(name)) table.Columns.Add(name,typeof(int));
        table.Columns.Add("TrangThaiKhoa",typeof(bool));
        var row=table.NewRow();
        row["TrangThaiKhoa"]=false;
        row[key]=id;
        foreach(DataColumn column in table.Columns) if(column.DataType==typeof(string)) row[column]="Existing";
        row["SiSoToiDa"]=40;
        table.Rows.Add(row);
        return table.DefaultView[0];
    }
    static void Main()
    {
        new AdminHost().Verify();
        new TeacherHost().Verify();
        new AccountHost().Verify();
        Console.WriteLine("PASS: 6 production editors and account creation select/reset safely; search filters and rebind verified.");
    }
}
'''
with tempfile.TemporaryDirectory(prefix='editor-modes-') as tmp:
    folder = Path(tmp)
    (folder / 'Program.cs').write_text(harness + '\n' + '\n'.join(hosts), encoding='utf-8')
    (folder / 'EditorModes.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup>
      <ItemGroup><Compile Include=''' + quoteattr(str(root / 'Models/Entities/CauHoi.cs')) + ''' Link="CauHoi.cs" /></ItemGroup>
      </Project>''')
    subprocess.run([args.dotnet, 'run', '--project', str(folder / 'EditorModes.csproj'), '--no-launch-profile'], check=True)
