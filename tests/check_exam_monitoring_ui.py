"""Run real focus/submission handlers with service/control doubles (.NET 8)."""
from pathlib import Path
import re
import subprocess
import tempfile
from xml.sax.saxutils import quoteattr
root = Path(__file__).resolve().parents[1]
source = (root / 'Forms/KiemTra/Frm_KiemTra.cs').read_text(encoding='utf-8-sig')
names = ['OnDeactivate', 'LuuViPham', 'CapNhatTrangThaiThi', 'KhoaDapAn', 'DungVaNopBai', 'ThongBao', 'NopBai']
methods = []
for name in names:
    match = re.search(r'(?m)^        (?:private|protected override) [^\n]+\b' + name + r'\(', source)
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
enum MessageBoxButtons { OK, YesNo }
enum MessageBoxIcon { Information, Warning, Error }
enum DialogResult { OK, Yes, No }
static class Session { public static int TaiKhoanID=1,NguoiDungID=11; }
static class MessageBox {
    public static Action DuringDialog;
    public static DialogResult Show(object owner,string text,string caption,MessageBoxButtons buttons,MessageBoxIcon icon)
    { DuringDialog?.Invoke();return DialogResult.OK; }
}
class Input { public bool Enabled=true;public string Text; }
class FakeTimer { public bool Running;public void Start() { Running=true; } public void Stop() { Running=false; } }
class Store {
    public bool FailReport,FailSubmit;
    public int Count,Submitted,Calls;
    public Dictionary<int,string> Answers;
    public int GhiNhanRoiManHinh(int account,int student,int exam,int count) {
        if(FailReport) throw new Exception("Network unavailable");Count=Math.Max(count,Count);return Count;
    }
    public KetQuaThi NopBai(int account,int student,int exam,Dictionary<int,string> answers) {
        Calls++;if(FailSubmit) throw new Exception("Network unavailable");
        Submitted++;Answers=answers;return new KetQuaThi { KetQuaID=42 };
    }
}
class FrmKetQuaBaiThi : IDisposable {
    public bool YeuCauXemLai => false;
    public FrmKetQuaBaiThi(int a,int b,double c) { }
    public void ShowDialog(object owner) { MessageBox.DuringDialog?.Invoke(); }
    public void Dispose() { }
}
namespace Forms { class Frm_XemLaiBaiThi : IDisposable {
    public Frm_XemLaiBaiThi(int id) { } public void ShowDialog(object owner) { } public void Dispose() { }
} }
class Base { protected virtual void OnDeactivate(EventArgs e) { } }
partial class Host : Base {
    bool _daNopBai,_dangNopBai,_hopThoaiNoiBo,_loiLuuViPham;
    int _soLanDaLuu,_deThiID=1,_thoiGianConLai=100;
    GiamSatPhongThi _giamSat=new GiamSatPhongThi();
    DateTime _ketThucLuc=DateTime.Now.AddHours(1);
    List<CauHoi> _cauHois=new List<CauHoi> { new CauHoi { CauHoiID=7 } };
    Dictionary<int,string> _dapAnChon=new Dictionary<int,string> { [0]="A" };
    Store _ketQuaService=new Store();
    FakeTimer _timer=new FakeTimer();
    Input lblKiemTra_SoCauHoi=new Input(),radKiemTra_DapAnA=new Input(),radKiemTra_DapAnB=new Input(),
        radKiemTra_DapAnC=new Input(),radKiemTra_DapAnD=new Input(),
        btnKiemTra_CauTruoc=new Input(),btnKiemTra_CauSau=new Input();
    List<Action> pending=new List<Action>();
    public bool IsDisposed;
    void BeginInvoke(Action action) { pending.Add(action); }
    void Close() { IsDisposed=true; }
    void Leave() { OnDeactivate(EventArgs.Empty); }
    void Flush() { var work=pending.ToArray();pending.Clear();foreach(var action in work) action(); }
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
    void ThreeDepartures() {
        for(int count=1;count<=3;count++) {
            _giamSat.KichHoat();Leave();Leave();
            Check(_giamSat.SoLan==count,"Only one violation per absence");
        }
    }
    public static void Verify() {
        var host=new Host();host.Leave();Check(host._giamSat.SoLan==0,"No violation before start");
        host._giamSat.BatDau(0);
        MessageBox.DuringDialog=host.Leave;
        host.ThongBao("Confirm","Internal dialog");
        Check(host._giamSat.SoLan==0,"Internal dialogs are exempt");
        host.ThreeDepartures();Check(!host.radKiemTra_DapAnA.Enabled && !host.btnKiemTra_CauSau.Enabled,"Lock answers on third departure");
        host.Flush();Check(host._daNopBai && host._ketQuaService.Submitted==1,"Third departure submits once");
        Check(host._ketQuaService.Count==3 && host._ketQuaService.Answers[7]=="A","Save violations and chosen answers");
        host.Leave();Check(host._giamSat.SoLan==3,"Result dialogs do not add violations");

        host=new Host();MessageBox.DuringDialog=host.Leave;host._giamSat.BatDau(0);host._ketQuaService.FailReport=true;
        host.ThreeDepartures();host.Flush();
        Check(!host._daNopBai && !host.radKiemTra_DapAnA.Enabled && !host._timer.Running,"Network failure keeps terminated attempt locked");
        host._ketQuaService.FailReport=false;host.NopBai();
        Check(host._daNopBai && host._ketQuaService.Count==3 && host._ketQuaService.Submitted==1,"Retry saves pending violations and submits");

        host=new Host();MessageBox.DuringDialog=host.Leave;host._giamSat.BatDau(0);host._ketQuaService.FailSubmit=true;
        host.ThreeDepartures();host.Flush();
        Check(!host.radKiemTra_DapAnA.Enabled && !host._daNopBai,"Submission failure cannot unlock answers");
        host._ketQuaService.FailSubmit=false;host.NopBai();Check(host._daNopBai,"Submission retry works");
        Console.WriteLine("PASS: production focus/submission handlers: dialogs, third departure, answer lock, failed reporting and submission retries.");
    }
}
class Program { static void Main() => Host.Verify(); }
'''
with tempfile.TemporaryDirectory(prefix='exam-monitoring-ui-') as tmp:
    folder = Path(tmp)
    (folder / 'Program.cs').write_text(harness + '\npartial class Host {\n' + '\n'.join(methods) + '\n}')
    links = ''.join('<Compile Include=' + quoteattr(str(root / ('Models/Entities/' + name + '.cs'))) + ' />'
                    for name in ['CauHoi', 'KetQuaThi', 'GiamSatPhongThi'])
    (folder / 'Test.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
        '<TargetFramework>net8.0</TargetFramework></PropertyGroup><ItemGroup>' + links + '</ItemGroup></Project>')
    subprocess.run(['dotnet', 'run', '--project', str(folder / 'Test.csproj'), '--no-launch-profile'], check=True)
