# Báo cáo bài tập lớn hoàn chỉnh

Bản biên tập theo tệp mẫu người dùng cung cấp, đối chiếu nhánh `master` tại commit `24d055113eb9b24e490ec905d3cdb5f6895009f0`, ngày 14/11/2026.

Báo cáo có **75 trang, 24 hình/sơ đồ và 54 bảng**, bao gồm ba chương theo mẫu, mục lục, danh mục hình/bảng, kết luận, tài liệu tham khảo và phụ lục đối chiếu mã nguồn.

## Tải tài liệu

- [Bản Word có thể chỉnh sửa](Bao_cao_BTL_hoan_chinh.docx)
- [Bản PDF để đọc và in](Bao_cao_BTL_hoan_chinh.pdf)
- [Tập 24 hình/sơ đồ, mỗi hình một trang PDF](Tap_so_do.pdf)
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

Phiên cloud đã kiểm tra kiến trúc và chạy 234 test .NET 8 đạt, không thất bại hoặc bỏ qua. Build net48 và 105 assertion Mono/Xvfb là kết quả lịch sử được ghi trong tài liệu repository, chưa chạy lại trong phiên biên tập.

**Đính chính Crystal Reports:** repository đã có `Reports/BangDiem.rpt` và `Reports/BangDiem.cs`. Nhận định “chưa có mẫu .rpt” trong một số tài liệu cũ và bản báo cáo đã biên tập là chưa chính xác. Tài liệu thuyết trình mới dùng trạng thái đúng: có mẫu và code tích hợp, nhưng chưa xác minh mẫu nạp dữ liệu, xem/in thành công trên Windows có SAP runtime phù hợp. Cần chỉnh các đoạn cũ này trong báo cáo trước khi nộp.

Nhánh này chỉ bổ sung tài liệu; không sửa mã nguồn ứng dụng.
