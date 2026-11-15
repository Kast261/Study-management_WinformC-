# Toàn màn hình và giám sát rời cửa sổ thi

Phòng thi mở không viền, ẩn thanh tiêu đề và nút thu nhỏ/phóng to/đóng, chiếm
toàn bộ màn hình đang chứa form. Không khóa Alt+Tab của Windows: thao tác
chuyển ứng dụng, thu nhỏ hoặc mất tiêu điểm của cửa sổ thi được ghi nhận.

- Trước khi phòng thi tải xong không ghi nhận vi phạm.
- Mỗi chu kỳ rời cửa sổ → quay lại chỉ ghi một lần; sự kiện Deactivate lặp
  trong cùng lần rời không tăng số lần.
- Đồng hồ hiển thị số lần rời cửa sổ và quy tắc dừng ở lần thứ ba.
- Hộp thoại xác nhận, báo lỗi và kết quả do ứng dụng mở không bị tính nhầm.
- Lần thứ ba khóa đáp án/điều hướng ngay, rồi tự nộp các đáp án đã chọn.
  Điểm vẫn tính theo bài làm; trạng thái lưu là `Dừng do vi phạm`.
- Nếu báo cáo vi phạm hoặc nộp bài lỗi mạng, không mở lại đáp án. Người học
  bấm Nộp bài để thử lưu lại. Báo cáo lại cùng số lần không tăng trùng và
  không thể giảm số đã lưu. Mở lại form không xóa số lần đã ghi trong DB.

## Giáo viên xem ở đâu

Tab Kết quả thi có thêm hai cột **Rời màn hình** và **Trạng thái bài thi**.
Giáo viên thấy cả bài có vi phạm chưa nộp (điểm để trống), kể cả khi tự nộp
chưa lưu được. Khi bài đã nộp, dòng đó được thay bằng kết quả thật, số lần
0–3 và trạng thái dừng nếu đạt ngưỡng. Nhãn số bài nộp chỉ đếm bài đã lưu. Dữ liệu này cũng đi theo bảng khi xuất Excel. Các bài cũ
chưa dùng giám sát hiển thị số lần 0. Đây là ghi nhận mất tiêu điểm cửa sổ,
không xác định nội dung ứng dụng bên ngoài mà học viên mở.

## Cơ sở dữ liệu

Bảng mới `study_giam_sat_thi` dùng khóa chính `(HocSinhID, DeThiID)`, lưu
số lần và thời điểm vi phạm gần nhất. Không sửa hoặc xóa dữ liệu cũ.
Lần đầu bắt đầu thi, ứng dụng tạo bảng nếu có quyền CREATE trong schema.
Nếu tài khoản DB không có quyền CREATE, quản trị viên chạy một lần:

```sh
psql "$DATABASE_URL" -v ON_ERROR_STOP=1 -f Database/PostgreSQL/03_ExamMonitoring.sql
```

Dùng đúng schema/search_path và database của ứng dụng, đồng thời cấp quyền
SELECT/INSERT/UPDATE trên bảng mới cho tài khoản ứng dụng. Không đưa giá trị
connection string hoặc mật khẩu vào repository. Khi chưa khởi tạo được bảng,
phòng thi báo lỗi và không bắt đầu thi không có giám sát.

Cập nhật cùng phiên bản ứng dụng cho học viên và giáo viên. Các thao tác ghi
vi phạm và nộp bài kiểm tra tài khoản/hồ sơ/lớp, khóa cùng dòng đề thi và chặn
thay đổi vi phạm sau khi đã nộp.

## Kiểm chứng

- ApplicationLogic.Tests: 236 đạt, trong đó 11 trường hợp mới kiểm tra trạng
  thái mất tiêu điểm, ngưỡng dừng, lưu số lần, phân quyền, báo cáo đồng thời,
  kết quả giáo viên và tương thích bài cũ.
- StudentProfile 29, Dashboard 7, ExcelImport 29, Pagination 19 đạt.
  Tổng năm suite .NET: **320 đạt, không thất bại/bỏ qua**.
- `python3 tests/check_exam_monitoring_ui.py`: chạy handler mất tiêu điểm/nộp
  bài thật với control/service giả; kiểm tra ngoại lệ hộp thoại, tự nộp,
  đáp án đã chọn, lỗi báo cáo và lỗi nộp lại không mở khóa bài.
- Các harness editor/tạo đề/xóa hồ sơ và kiểm tra kiến trúc đều đạt.
- Form thi và Designer được biên dịch với net48 reference assemblies 1.0.3,
  ReaLTaiizor 3.8.2.1 và BLL giả: không lỗi/cảnh báo. Chưa nghiệm thu thao tác
  Alt+Tab, Win+D và hiển thị toàn màn hình trên Windows thực.

Khi thử Windows: chuyển ứng dụng hai lần phải còn làm bài; lần ba phải tự
nộp, ghi trạng thái dừng và giáo viên thấy 3. Hộp thoại Nộp bài → Không không
được cộng vi phạm. Thử mất kết nối trước lần ba: đáp án phải tiếp tục bị khóa
khi lưu thất bại, và nộp lại thành công sau khi khôi phục kết nối.

Giám sát dựa trên sự kiện của ứng dụng desktop; không quản lý Task Manager,
ứng dụng bị kill hoặc thiết bị khác. Đáp án chưa nộp vẫn nằm trong bộ nhớ như
trước; kết thúc tiến trình cưỡng bức không đảm bảo khôi phục đáp án đó.
