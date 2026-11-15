"""Exercise the real create-draft click handler; no WinForms rendering or DB."""
from pathlib import Path
import re
import subprocess
import tempfile
from xml.sax.saxutils import quoteattr
root = Path(__file__).resolve().parents[1]
source = (root / 'Forms/GiaoVien/frm_GiaoVien.cs').read_text(encoding='utf-8-sig')
match = re.search(r'(?m)^        private void btnCH_TaoDe_Click\(', source)
opening = source.index('{', match.start())
end, depth = opening + 1, 1
while depth:
    depth += (source[end] == '{') - (source[end] == '}')
    end += 1
handler = source[match.start():end]
harness = r'''
using System;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
enum MessageBoxButtons { OK }
enum MessageBoxIcon { Warning, Error, Information }
static class MessageBox {
    public static string Last;
    public static void Show(string text,string title,MessageBoxButtons buttons,MessageBoxIcon icon) { Last=text; }
}
static class Session { public static int NguoiDungID=101; }
class Input {
    public string Text="";
    public object SelectedItem,SelectedValue,DataSource;
    public void Focus() { }
    public void Clear() { Text=""; }
}
class Store {
    public DataTable Exams = new DataTable();
    public bool Fail;
    public int Calls, Result=42;
    public DeThi Created;
    public Store() {
        Exams.Columns.Add("DeThiID",typeof(int));Exams.Columns.Add("TenDeThi");Exams.Columns.Add("MonHoc");
        Exams.Columns.Add("LopHocID",typeof(int));Exams.Columns.Add("TrangThai");Exams.Columns.Add("SoCau",typeof(int));
    }
    public DataTable LayDeThiTheoGiaoVien(int id) { if(Fail) throw new Exception("Database unavailable"); return Exams; }
    public int TaoDeThiMoiLayID(DeThi exam) { Calls++;Created=exam;return Result; }
    public object LayDanhSachCauHoi(int id) => new object();
}
partial class Host {
    int _selectedCauHoiID,_selectedDeThiID;
    Input txtCH_TenDeThi = new Input { Text="Đề mới" },cmbCH_MonHoc = new Input { SelectedItem="Toán" },
        cmbDT_MonHoc=new Input(),cmbDT_DeThi=new Input(),dgvCauHoi=new Input();
    Store _deThiService=new Store();
    bool FailRefresh;
    int Loaded;
    void LoadDeThiCombobox(int id) { if(FailRefresh) throw new Exception("Refresh failed");Loaded=id; }
    void LamMoiFormCauHoi() { _selectedCauHoiID=0; }
    public void Click() { MessageBox.Last=null;btnCH_TaoDe_Click(this,EventArgs.Empty); }
    static void Check(bool value,string message) { if(!value) throw new Exception(message); }
    public static void Verify() {
        var host=new Host();host.Click();
        Check(host._deThiService.Calls==1 && host.Loaded==42,"Creates and selects saved draft");
        Check(host._deThiService.Created.TrangThai=="Chưa giao" && MessageBox.Last.Contains("Tạo đề thi"),"Creation message reflects actual state");
        host=new Host();host._deThiService.Exams.Rows.Add(7,"Đề mới","Toán",DBNull.Value,"Chưa giao",2);host.Click();
        Check(host.Loaded==7 && host._deThiService.Calls==0 && MessageBox.Last.Contains("đã tồn tại"),"Existing draft provides feedback without duplication");
        host=new Host();host.txtCH_TenDeThi.Text=" ";host.Click();
        Check(host._deThiService.Calls==0 && MessageBox.Last.Contains("nhập tên"),"Empty title warns");
        host=new Host();host._deThiService.Fail=true;host.Click();
        Check(MessageBox.Last.Contains("Không thể tạo"),"Database failure handled");
        host=new Host();host.FailRefresh=true;host.Click();
        Check(host._deThiService.Calls==1 && MessageBox.Last.Contains("Không cần tạo lại"),"Saved draft survives refresh failure");
        host=new Host();host._deThiService.Result=-1;host.Click();
        Check(MessageBox.Last.Contains("thất bại"),"Failed creation does not claim success");
        host=new Host();host._selectedCauHoiID=17;host.Click();
        Check(host._deThiService.Calls==0 && MessageBox.Last.Contains("Làm mới"),"Edit mode explains how to create another draft");
        Console.WriteLine("PASS: 7 production create-draft handler scenarios.");
    }
}
class Program { static void Main() => Host.Verify(); }
'''
with tempfile.TemporaryDirectory(prefix='exam-creation-ui-') as tmp:
    folder = Path(tmp)
    (folder / 'Program.cs').write_text(harness + '\npartial class Host {\n' + handler + '\n}')
    links = ''.join('<Compile Include=' + quoteattr(str(root / name)) + ' />' for name in
                    ['Models/Entities/DeThi.cs', 'Helpers/ExamSelection.cs'])
    (folder / 'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
        '<TargetFramework>net8.0</TargetFramework></PropertyGroup><ItemGroup>' + links + '</ItemGroup></Project>')
    subprocess.run(['dotnet', 'run', '--project', str(folder / 'Test.csproj'), '--no-launch-profile'], check=True)
