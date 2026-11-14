"""Compile the real Admin delete handlers with dialog/service doubles.

Checks handler behavior, not MaterialForm rendering. Requires .NET SDK 8.
"""
import argparse
from pathlib import Path
import re
import subprocess
import tempfile
from xml.sax.saxutils import quoteattr

parser = argparse.ArgumentParser()
parser.add_argument('--dotnet', default='dotnet')
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
source = (root / 'Forms/Admin/Frm_Admin.cs').read_text(encoding='utf-8-sig')
methods = []
for name in ['btnGV_Xoa_Click', 'btnHV_Xoa_Click']:
    match = re.search(r'(?m)^        private void ' + name + r'\(', source)
    if match is None:
        raise RuntimeError('Missing production handler: ' + name)
    opening = source.index('{', match.start())
    end, depth = opening + 1, 1
    while depth:
        depth += (source[end] == '{') - (source[end] == '}')
        end += 1
    methods.append(source[match.start():end])

harness = r'''
using System;
using System.Collections.Generic;
using BLT_Quan_Ly_Hoc_Tap.Models;
enum DialogResult { OK, Yes, No }
enum MessageBoxButtons { OK, YesNo }
enum MessageBoxIcon { Information, Warning, Error, Question }
static class MessageBox
{
    public static DialogResult Answer;
    public static Action BeforeAnswer;
    public static string LastText;
    public static MessageBoxIcon LastIcon;
    public static DialogResult Show(string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        LastText = text;LastIcon = icon;
        if (buttons != MessageBoxButtons.YesNo) return DialogResult.OK;
        if (!text.Contains("lịch sử học tập") || !text.Contains("khóa tài khoản") || !text.Contains("\n\n"))
            throw new Exception("Confirmation must describe archive/account-lock behavior");
        BeforeAnswer?.Invoke();return Answer;
    }
}
sealed class ProfileService
{
    public KetQuaXoaHoSo Result;
    public bool Fail;
    public int Calls, LastId;
    public KetQuaXoaHoSo XoaChiTiet(int id)
    {
        Calls++;LastId=id;
        if (Fail) throw new InvalidOperationException("Simulated database failure");
        return Result;
    }
}
partial class DeletionHost
{
    private int _selectedGiaoVienID=17, _selectedHocVienID=17;
    private readonly ProfileService _gvService = new ProfileService(), _hsService = new ProfileService();
    public readonly List<string> Refreshes = new List<string>();
    public ProfileService Store(bool teacher) => teacher ? _gvService : _hsService;
    public void Rebind() { _selectedGiaoVienID=99;_selectedHocVienID=99; }
    public void ClearSelection() { _selectedGiaoVienID=0;_selectedHocVienID=0; }
    public void Click(bool teacher)
    {
        if (teacher) btnGV_Xoa_Click(this,EventArgs.Empty);
        else btnHV_Xoa_Click(this,EventArgs.Empty);
    }
    private void LoadDanhSachGiaoVien() => Refreshes.Add("teachers");
    private void LoadDanhSachHocVien() => Refreshes.Add("students");
    private void LoadDropdownData() => Refreshes.Add("dropdowns");
    private void LoadDanhSachTaiKhoan() => Refreshes.Add("accounts");
    private void LoadDanhSachLopHoc() => Refreshes.Add("classes");
    private void LoadDashboard() => Refreshes.Add("dashboard");
    private void LamMoiFormGV() => Refreshes.Add("clear-teacher");
    private void LamMoiFormHV() => Refreshes.Add("clear-student");
}
static class Program
{
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Main()
    {
        int count=0;
        foreach (bool teacher in new[] {true,false})
        {
            foreach (var result in new[] {KetQuaXoaHoSo.DaXoa, KetQuaXoaHoSo.DaLuuTru, KetQuaXoaHoSo.KhongTimThay})
            {
                var host=new DeletionHost();host.Store(teacher).Result=result;
                MessageBox.Answer=DialogResult.Yes;MessageBox.BeforeAnswer=host.Rebind;
                host.Click(teacher);
                Check(host.Store(teacher).LastId==17,"The selected ID must survive confirmation/rebinding");
                Check(host.Store(teacher).Calls==1,"Exactly one service call");
                Check(MessageBox.LastIcon==MessageBoxIcon.Information,"Normal archive/delete/missing result is not an exception");
                string expected=result==KetQuaXoaHoSo.DaXoa ? "Đã xóa hồ sơ" : result==KetQuaXoaHoSo.DaLuuTru ? "Đã ngừng hoạt động" : "không còn";
                Check(MessageBox.LastText.Contains(expected),"Show the actual delete outcome");
                Check(host.Refreshes.Contains("accounts") && host.Refreshes.Contains("dashboard"),"Refresh accounts and Dashboard");
                Check(host.Refreshes.Contains(teacher ? "teachers" : "students"),"Refresh the selected profile list");
                Check(host.Refreshes.Contains(teacher ? "dropdowns" : "classes"),"Refresh teacher choices or class capacity");
                count++;
            }
            {
                var host=new DeletionHost();MessageBox.Answer=DialogResult.No;MessageBox.BeforeAnswer=host.Rebind;
                host.Click(teacher);
                Check(host.Store(teacher).Calls==0 && host.Refreshes.Count==0,"Declining must not write or clear the form");count++;
            }
            {
                var host=new DeletionHost();host.ClearSelection();MessageBox.BeforeAnswer=null;
                host.Click(teacher);
                Check(host.Store(teacher).Calls==0 && MessageBox.LastIcon==MessageBoxIcon.Warning,"No selection must not call delete");count++;
            }
            {
                var host=new DeletionHost();host.Store(teacher).Fail=true;
                MessageBox.Answer=DialogResult.Yes;MessageBox.BeforeAnswer=null;
                host.Click(teacher);
                Check(MessageBox.LastIcon==MessageBoxIcon.Error && MessageBox.LastText.Contains("Simulated database failure"),"Real failures remain visible");
                Check(host.Refreshes.Count==0,"Do not report/refresh success after a failed write");count++;
            }
        }
        Console.WriteLine("PASS: "+count+" Admin profile deletion handler scenarios from production source.");
    }
}
'''

with tempfile.TemporaryDirectory(prefix='profile-deletion-ui-') as tmp:
    folder = Path(tmp)
    (folder / 'Program.cs').write_text(harness, encoding='utf-8')
    (folder / 'ProductionHandlers.cs').write_text(
        'using System;\nusing BLT_Quan_Ly_Hoc_Tap.Models;\npartial class DeletionHost {\n'
        + '\n'.join(methods) + '\n}\n', encoding='utf-8')
    (folder / 'ProfileDeletionUi.csproj').write_text('''<Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework></PropertyGroup>
      <ItemGroup><Compile Include=''' + quoteattr(str(root / 'Models/Entities/KetQuaXoaHoSo.cs'))
        + ''' Link="KetQuaXoaHoSo.cs" /></ItemGroup></Project>''')
    subprocess.run([args.dotnet, 'run', '--project', str(folder / 'ProfileDeletionUi.csproj'),
                    '--no-launch-profile'], check=True)
