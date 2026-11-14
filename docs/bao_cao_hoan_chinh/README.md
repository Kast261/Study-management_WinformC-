# Báo cáo bài tập lớn hoàn chỉnh

Bản biên tập theo tệp mẫu người dùng cung cấp, đối chiếu nhánh `master` tại commit `24d055113eb9b24e490ec905d3cdb5f6895009f0`, ngày 14/11/2026.

Báo cáo có **75 trang, 24 hình/sơ đồ và 54 bảng**, bao gồm ba chương theo mẫu, mục lục, danh mục hình/bảng, kết luận, tài liệu tham khảo và phụ lục đối chiếu mã nguồn.

## Tải tài liệu

- [Bản Word có thể chỉnh sửa](Bao_cao_BTL_hoan_chinh.docx)
- [Bản PDF để đọc và in](Bao_cao_BTL_hoan_chinh.pdf)
- [Tập 24 hình/sơ đồ, mỗi hình một trang PDF](Tap_so_do.pdf)
- [Hình PNG, SVG và mã sơ đồ DOT](So_do_PNG_SVG_DOT.zip)

Để tải trên GitHub: mở tệp rồi chọn **Download raw file** hoặc **View raw**. Bản Word có thể mở bằng Microsoft Word hoặc nhập vào Google Docs.

## Thông tin cần bổ sung trước khi nộp

Số nhóm, họ tên, mã sinh viên, lớp, khóa và phân công thực tế chưa được cung cấp nên các ô trên bìa vẫn để trống. Phân công trong báo cáo là phương án đề xuất, chưa xác nhận đóng góp cá nhân. Sau khi chỉnh sửa trong Word, nhấn Ctrl+A rồi F9 để cập nhật mục lục và số trang trước khi xuất lại PDF.

## Phạm vi hình ảnh và bằng chứng

Các hình là sơ đồ phân tích, thiết kế hoặc chức năng màn hình dựng từ mã nguồn. Sáu sơ đồ màn hình không phải ảnh chụp ứng dụng Windows đang chạy.

Phiên cloud đã kiểm tra kiến trúc và chạy 234 test .NET 8 đạt, không thất bại hoặc bỏ qua. Build net48 và 105 assertion Mono/Xvfb là kết quả lịch sử được ghi trong tài liệu repository, chưa chạy lại trong phiên biên tập. Crystal Reports còn thiếu mẫu `BangDiem.rpt` và nghiệm thu SAP/Windows theo `Reports/README.md`.

Nhánh này chỉ bổ sung tài liệu; không sửa mã nguồn ứng dụng.
