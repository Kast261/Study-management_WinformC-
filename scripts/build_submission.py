"""Generate draft Word/slides and diagrams. Requires pandoc, graphviz, python-pptx.
Run from any directory: python3 scripts/build_submission.py
No application source or database is modified.
"""
from pathlib import Path
import subprocess
from pptx import Presentation
from pptx.util import Inches, Pt

root = Path(__file__).resolve().parents[1]
out = root / 'docs/submission'
assets = out / 'assets'
assets.mkdir(parents=True, exist_ok=True)
graphs = {
    'kien_truc': '''rankdir=TB;
        gui[label="GUI — Windows Forms"]; bus[label="BLL — interface / Service"];
        dal[label="DAL — Npgsql / transaction"]; db[label="PostgreSQL",shape=cylinder];
        gui -> bus[label="Yêu cầu và dữ liệu"]; bus -> dal[label="Thao tác dữ liệu"];
        dal -> db[label="SQL / CALL có tham số"];
        db -> dal[label="Dữ liệu / kết quả ghi"]; dal -> bus[label="Kết quả"];
        bus -> gui[label="Dữ liệu / thông báo"];
    ''',
    'du_lieu': '''rankdir=LR;
        label="Quan hệ nghiệp vụ — không đồng nghĩa mọi FK đã được khai báo";
        TaiKhoan -> GiaoVien[label="Theo vai trò"];
        TaiKhoan -> HocSinh[label="Theo vai trò"];
        KhoaHoc -> LopHoc; GiaoVien -> LopHoc;
        HocSinh -> DangKyLop; LopHoc -> DangKyLop;
        GiaoVien -> DeThi; LopHoc -> DeThi;
        DeThi -> CauHoi; HocSinh -> KetQuaThi; DeThi -> KetQuaThi;
        KetQuaThi -> ChiTietBaiLam; CauHoi -> ChiTietBaiLam;
    ''',
    'dfd_ngu_canh': '''rankdir=LR;
        s[label="0. Hệ thống quản lý học tập",shape=ellipse];
        a[label="Quản trị viên"]; g[label="Giáo viên"]; h[label="Học viên"];
        a -> s[label="Tài khoản, hồ sơ, khóa/lớp"]; s -> a[label="Danh sách, thống kê"];
        g -> s[label="Quyết định duyệt, đề/câu hỏi"]; s -> g[label="Yêu cầu, lượt giao, kết quả"];
        h -> s[label="Đăng ký, hồ sơ, đáp án"]; s -> h[label="Trạng thái, bài thi, điểm"];
    ''',
    'dfd_muc_0': '''rankdir=LR;
        label="DFD mức 0 rút gọn — tác nhân/kho dữ liệu lặp lại theo tiến trình";
        subgraph cluster_1 { label="Tài khoản và hồ sơ"; color="#cccccc";
            a1[label="Admin / Giáo viên / Học viên"];
            p1[label="1.0 Quản lý tài khoản / hồ sơ",shape=ellipse];
            d1[label="D1 Tài khoản / hồ sơ",shape=cylinder];
            a1 -> p1[label="Thông tin quản lý / cá nhân"];
            p1 -> a1[label="Xác thực / hồ sơ / kết quả"];
            p1 -> d1[label="Thông tin cập nhật"]; d1 -> p1[label="Tài khoản / hồ sơ hiện có"];
        }
        subgraph cluster_2 { label="Lớp và đăng ký"; color="#cccccc";
            a2[label="Admin / Giáo viên / Học viên"];
            p2[label="2.0 Xử lý lớp / đăng ký",shape=ellipse];
            d2[label="D1 Hồ sơ; D2 Khóa / lớp / đăng ký",shape=cylinder];
            a2 -> p2[label="Lớp / yêu cầu / quyết định duyệt"];
            p2 -> a2[label="Danh sách / trạng thái / kết quả"];
            p2 -> d2[label="Lớp / đăng ký / xếp lớp"];
            d2 -> p2[label="Lớp chính / sĩ số / trạng thái"];
        }
        subgraph cluster_3 { label="Đề và câu hỏi"; color="#cccccc";
            a3[label="Giáo viên"];
            p3[label="3.0 Soạn / giao đề",shape=ellipse];
            d3[label="D1 Tài khoản; D2 Lớp; D3 Đề / câu hỏi",shape=cylinder];
            a3 -> p3[label="Đề / Excel / lớp nhận"]; p3 -> a3[label="Kết quả soạn / lượt giao"];
            p3 -> d3[label="Đề / câu hỏi / lượt mới"];
            d3 -> p3[label="Tài khoản / lớp phụ trách / đề nguồn"];
        }
        subgraph cluster_4 { label="Bài làm"; color="#cccccc";
            a4[label="Học viên"];
            p4[label="4.0 Tiếp nhận và chấm bài",shape=ellipse];
            d4[label="D1 Tài khoản; D2 Lớp; D3 Đề; D4 Bài làm",shape=cylinder];
            a4 -> p4[label="Mã đề / định danh / đáp án"];
            p4 -> a4[label="Bài được phép mở / điểm"];
            p4 -> d4[label="Kết quả / chi tiết trả lời"];
            d4 -> p4[label="Quyền lớp / đề / đáp án chuẩn / đã nộp"];
        }
        subgraph cluster_5 { label="Tra cứu"; color="#cccccc";
            a5[label="Admin / Giáo viên / Học viên"];
            p5[label="5.0 Tra cứu / báo cáo",shape=ellipse];
            d5[label="D1 Hồ sơ; D2 Lớp; D3 Đề; D4 Bài làm",shape=cylinder];
            a5 -> p5[label="Bộ lọc / yêu cầu thống kê"];
            p5 -> a5[label="Thống kê / lịch sử / báo cáo"];
            d5 -> p5[label="Hồ sơ / lớp / metadata / kết quả"];
        }
    '''
}
for name, body in graphs.items():
    dot = 'digraph G { graph [bgcolor="white",fontname="DejaVu Sans",pad="0.25"]; node [shape=box,fontname="DejaVu Sans",fontsize=12,style="rounded,filled",fillcolor="#edf3fc"]; edge [fontname="DejaVu Sans",fontsize=10];' + body + '\n}'
    source = assets / (name + '.dot')
    dot = '\n'.join(line.rstrip() for line in dot.splitlines()) + '\n'
    source.write_text(dot, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', '-Gdpi=150', str(source), '-o', str(assets / (name + '.png'))], check=True)
subprocess.run(['pandoc', 'Bao_cao_3_chuong.md', '--toc', '--toc-depth=2', '-o', 'Bao_cao_BTL_nhap.docx'], cwd=out, check=True)
slides = [
    ('QUẢN LÝ HỌC TẬP', ['C# WinForms • PostgreSQL • .NET Framework 4.8', 'BTL Lập trình trực quan — bản nháp', 'Nhóm: [điền 5 họ tên, MSSV, lớp]', 'Cần bổ sung ảnh/demo thực tế trước khi nộp']),
    ('Bài toán và phạm vi', ['Quản trị viên, giáo viên, học viên', 'Tài khoản / hồ sơ / khóa học / lớp', 'Đăng ký → duyệt → giao đề → làm bài → kết quả', 'Ứng dụng desktop, không phải REST/EF Core']),
    ('Kiến trúc ba tầng', ['GUI gọi interface Service, không truy cập DAL trực tiếp', 'BLL: hợp đồng và điều phối nghiệp vụ', 'DAL: SQL/CALL có tham số, quyền và transaction', '10 interface ứng dụng có lớp triển khai']),
    ('Hướng đối tượng', ['Form kế thừa MaterialForm hoặc Form', 'Service triển khai I*Service và chứa DAL', 'Giữ mã giáo viên khi duyệt/sửa câu hỏi', 'Kiểm tra kiến trúc và source csproj tự động']),
    ('Cơ sở dữ liệu', ['10 bảng liên quan hồ sơ, lớp, đề và bài làm', 'Lớp chính + đăng ký Đã duyệt cho nhiều lớp', 'Sơ đồ quan hệ logic không đồng nghĩa đủ FK vật lý', 'Script PostgreSQL mới tách khỏi SQL Server lịch sử']),
    ('CRUD và stored procedure', ['Npgsql là provider ADO.NET cho PostgreSQL', 'Admin sửa khóa → Service → DAL → CALL', 'study_sua_khoa_hoc: validation và INOUT boolean', 'Cài 02_Procedures.sql trước khi dùng bản mới']),
    ('Đăng ký và quyền lớp', ['Giáo viên phụ trách duyệt, kiểm tra sĩ số lúc ghi', 'Duyệt thêm lớp không ghi đè lớp chính', 'Đóng đăng ký không đóng đề đã giao', 'Giữ mã lựa chọn khi xác nhận / activation']),
    ('Câu hỏi và giao đề', ['Excel .xlsx/.xls, tối đa 20 MB / 5.000 câu', 'Kiểm tra đủ 6 cột và dữ liệu trước khi ghi', 'Mỗi lần giao tạo lượt và bộ câu hỏi riêng', 'Ẩn đề giữ metadata và lịch sử bài làm']),
    ('Làm bài và nộp bài', ['Kiểm tra quyền trước bắt đầu, đồng hồ giới hạn bởi hạn', 'Chấm theo đáp án DB, không nhận điểm từ Form', 'Kết quả + chi tiết trong cùng transaction', 'Chưa có lưu nháp bền vững / lần thi phía server']),
    ('Crystal Reports — còn nghiệm thu', ['Đã có XSD, dataset và đường gọi engine/viewer', 'Excel vẫn hoạt động khi chưa cài SAP', 'Cần SAP SDK/runtime Windows đúng kiến trúc', 'Cần tạo BangDiem.rpt và chạy xem/in thực tế']),
    ('Bằng chứng kiểm thử', ['Build net48 trên Linux: thành công', '234 kiểm thử .NET 8 đạt', '89 + 16 assertion Mono/Xvfb đạt', 'UI ReaLTaiizor và Crystal Windows chưa kiểm chứng']),
    ('Phân công và công việc còn lại', ['TV1 tài khoản; TV2 hồ sơ; TV3 DB/lớp', 'TV4 đề/Excel; TV5 phòng thi/kết quả/Crystal', 'Điền đóng góp, review và ảnh demo thực tế', 'Áp dụng template Word/slide chính thức của khoa'])
]
prs = Presentation()
prs.slide_width = Inches(13.333)
prs.slide_height = Inches(7.5)
for title, lines in slides:
    slide = prs.slides.add_slide(prs.slide_layouts[1])
    slide.shapes.title.text = title
    slide.shapes.title.text_frame.paragraphs[0].font.size = Pt(30)
    frame = slide.placeholders[1].text_frame
    frame.clear()
    for idx, line in enumerate(lines):
        p = frame.paragraphs[0] if idx == 0 else frame.add_paragraph()
        p.text = line
        p.font.name = 'Arial'
        p.font.size = Pt(24)
        p.space_after = Pt(18)
prs.save(out / 'Thuyet_trinh_BTL_nhap.pptx')
print('Generated 4 diagrams, draft Word and 12-slide PowerPoint.')
