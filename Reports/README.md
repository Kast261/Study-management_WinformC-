# Hoàn tất Crystal Reports trên Windows

## Trạng thái chính xác

Đã có dữ liệu báo cáo qua BLL, schema XSD và hộp thoại gọi SAP ReportDocument/CrystalReportViewer, load .rpt, push DataSet, xem/in bằng viewer và giải phóng tài nguyên. Excel vẫn dùng được khi không có SAP.

**Chưa có mẫu `BangDiem.rpt` được tạo bằng SAP designer; chưa xác minh engine/viewer trên Windows. Vì vậy mục Crystal của thang điểm chưa hoàn tất.** Không đổi tên XML/PDF thành .rpt: đây là định dạng riêng của SAP.

## Cài công cụ chính thức

1. Trên máy Windows có Visual Studio, cài SAP Crystal Reports, developer version for Microsoft Visual Studio từ https://www.sap.com/products/technology-platform/crystal-reports/visual-studio.html (tài khoản/tải xuống theo điều kiện SAP). Chọn bản hỗ trợ đúng phiên bản Visual Studio.
2. Cài runtime tương ứng cho máy chạy ứng dụng; thống nhất x86 hoặc x64. Khi cần, chọn Platform target x86/x64 trong Visual Studio phù hợp runtime, không dùng runtime khác kiến trúc tiến trình.
3. Giữ xác minh chữ ký và chứng chỉ bộ cài. Không lấy DLL không rõ nguồn hoặc nhúng binary có điều kiện phân phối chưa xác định vào Git.

## Tạo mẫu thật, không cần kết nối DB trong mẫu

1. Add New Item → Crystal Report → Standard Report. Lưu `Reports/BangDiem.rpt`.
2. Database Expert → Create New Connection → ADO.NET (XML). Chọn `Reports/BangDiem.xsd`, bảng `BangDiem`.
3. Đặt tiêu đề “BẢNG ĐIỂM HỌC VIÊN”. Report Header dùng trường HoTen; Details gồm MonHoc, TenDeThi, Diem, ThoiGianNop. Report Footer dùng NguoiLap và thời điểm in.
4. Format Diem hai chữ số thập phân. Có thể group theo MonHoc và thêm Average(Diem); không chuyển điểm NULL thành 0. Bật Can Grow cho tên dài. Chọn font Unicode có sẵn trên máy.
5. Không bật Save Data with Report cho dữ liệu cá nhân; xóa sample data trước commit. Mẫu chỉ cần bảng XSD, không lưu mật khẩu DB.
6. Build. Project tự copy `.rpt` nếu file tồn tại. Không có file mặc định thì ứng dụng cho chọn một mẫu `.rpt` tương thích.

## Nghiệm thu

- Tạo dữ liệu thử có điểm 0, 7.5, 10, điểm NULL, ngày NULL, tên tiếng Việt và nhiều môn.
- Từ Học viên → Kết quả → Báo cáo hoặc Giáo viên → Kết quả → Xuất báo cáo, chọn Crystal.
- Xác minh đúng học viên/bộ lọc, số dòng, giá trị số và ngày, xem nhiều trang và in thử/preview.
- Đóng/mở nhiều lần; không giữ tệp/report bị khóa. Thử thiếu runtime, hủy chọn mẫu và mẫu sai schema.
- Ghi kết quả cùng phiên bản Visual Studio, SAP service pack, runtime, kiến trúc, ảnh và người review trong docs/Phan_cong_va_danh_gia_cheo.md.

Chỉ sau khi mẫu thật và kiểm tra Windows đạt mới đánh dấu tiêu chí Crystal hoàn tất. Việc build ứng dụng mặc định trên Linux không kiểm chứng API SAP được gọi qua reflection.
