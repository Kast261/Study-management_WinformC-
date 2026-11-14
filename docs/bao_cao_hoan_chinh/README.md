# Báo cáo bài tập lớn hoàn chỉnh

Bản biên tập theo tệp mẫu người dùng cung cấp, đối chiếu bản mã có 10 procedure trên nhánh `docs/bao-cao-hoan-chinh`, commit `b9c3b42c28d82be7c445a0062cd21793b587a3c3`, ngày 14/11/2026.

Báo cáo có **83 trang, 27 hình/sơ đồ và 64 bảng**, bao gồm ba chương theo mẫu, mục lục, danh mục hình/bảng, kết luận, tài liệu tham khảo và phụ lục đối chiếu mã nguồn.

## Tải tài liệu

- [Bản Word có thể chỉnh sửa](Bao_cao_BTL_hoan_chinh.docx)
- [Bản PDF để đọc và in](Bao_cao_BTL_hoan_chinh.pdf)
- [Tập 27 hình/sơ đồ, mỗi hình một trang PDF](Tap_so_do.pdf)
- [Hình PNG, SVG và mã sơ đồ DOT](So_do_PNG_SVG_DOT.zip)
- [Gợi ý và lời thuyết trình dự án — Word](Goi_y_va_loi_thuyet_trinh_du_an.docx)
- [Gợi ý và lời thuyết trình dự án — PDF](Goi_y_va_loi_thuyet_trinh_du_an.pdf)

Để tải trên GitHub: mở tệp rồi chọn **Download raw file** hoặc **View raw**. Bản Word có thể mở bằng Microsoft Word hoặc nhập vào Google Docs.

## Tài liệu luyện thuyết trình

Tài liệu thuyết trình có **24 trang**, gồm:

- Dàn ý 12 slide, ba ý chính mỗi slide, lời nói gợi ý và câu chuyển phần.
- Kế hoạch khoảng 15 phút: 11 phút 10 giây trình bày, 3 phút demo, 50 giây dự phòng; hỏi đáp tính riêng.
- Chuẩn bị dữ liệu thử, demo đăng ký–duyệt–giao đề–làm bài–xem điểm và cách xử lý lỗi khi demo.
- 18 câu hỏi–trả lời về kiến trúc, ADO.NET, interface, quyền, transaction, procedure, Crystal và giới hạn.
- Gợi ý chia phần cho nhóm năm người, phiên bản rút gọn 5 phút và bảng thuật ngữ.

Đây là kịch bản luyện nói, chưa phải bộ PowerPoint mới. Tên người nói và đóng góp cần điền theo thực tế.

## Thông tin cần bổ sung trước khi nộp

Số nhóm, họ tên, mã sinh viên, lớp, khóa và phân công thực tế chưa được cung cấp nên các ô trên bìa vẫn để trống. Phân công trong báo cáo là phương án đề xuất, chưa xác nhận đóng góp cá nhân. Sau khi chỉnh sửa trong Word, nhấn Ctrl+A rồi F9 để cập nhật mục lục và số trang trước khi xuất lại PDF.

## Phạm vi hình ảnh và bằng chứng

Các hình là sơ đồ phân tích, thiết kế hoặc chức năng màn hình dựng từ mã nguồn. Sáu sơ đồ màn hình không phải ảnh chụp ứng dụng Windows đang chạy.

Phiên cloud đã kiểm tra kiến trúc và chạy 256 test .NET 8 đạt, không thất bại hoặc bỏ qua. Build net48 và 105 assertion Mono/Xvfb là kết quả lịch sử được ghi trong tài liệu repository, chưa chạy lại trong phiên biên tập.

**Crystal Reports:** repository đã có `Reports/BangDiem.rpt` và `Reports/BangDiem.cs`. Bản Word/PDF cập nhật đã sửa nhận định cũ về sự tồn tại của mẫu. Chưa xác minh mẫu nạp dữ liệu/xem/in thành công trên Windows có SAP runtime phù hợp; có tệp không thay thế nghiệm thu.

## Cập nhật 9 procedure mới

Nhánh hiện có **10 procedure**: thêm/sửa/xóa khóa học, giáo viên, học viên và đóng nhận đăng ký lớp. Tất cả được gọi từ DAL; `ProcedureExecutor` xử lý CALL/INOUT và hướng dẫn khi thiếu script. Có 22 test mới, tổng ApplicationLogic 172; toàn bộ năm suite 256 test đạt.

- [Danh mục, tham số, đường gọi và kiểm thử](../Procedures_PostgreSQL.md)
- [Script cài đủ 10 procedure](../../Database/PostgreSQL/02_Procedures.sql)
- [Kết quả kiểm thử](../Ket_qua_kiem_thu.md)
- [PowerPoint nháp 12 slide đã đồng bộ](../submission/Thuyet_trinh_BTL_nhap.pptx)

**Trước khi chạy mã mới trên máy của bạn:** cài lại toàn bộ `Database/PostgreSQL/02_Procedures.sql` lên database phát triển đang dùng, kể cả trước đó đã cài phiên bản chỉ có một procedure. Cloud chỉ chạy kiểm thử trên DB dùng một lần; không tự cập nhật DB Windows của bạn. Nhánh `master` chưa bị thay đổi.
