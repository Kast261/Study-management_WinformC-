# 🏷️ BẢNG QUY CHUẨN ĐẶT TÊN TOOLBOX CONTROLS CHO TỪNG FORM
> **Dự án:** BLT_Quan_Ly_Hoc_Tap  
> **Áp dụng:** WinForms C# (.NET Framework 4.8) & Thư viện ReaLTaiizor  
> **Mục tiêu:** Xóa bỏ toàn bộ tên mặc định sinh tự động (`materialButton8`, `materialTextBoxEdit10`, `foreverButton1`...), chuyển sang quy chuẩn định danh rõ nghĩa, chuẩn phong cách lập trình WinForms, giúp code logic dễ dàng, không bao giờ nhầm lẫn sự kiện.

---

## 📌 QUY TẮC TIỀN TỐ (PREFIX CONVENTION)
Mỗi thành phần giao diện (Control) bắt buộc phải bắt đầu bằng 3 ký tự viết thường thể hiện loại control:

| Loại Control | Tiền tố | Ví dụ |
| :--- | :---: | :--- |
| **Button / MaterialButton / ForeverButton** | `btn` | `btnDangNhap`, `btnThem`, `btnNopBai`, `btnCauTruoc` |
| **TextBox / MaterialTextBoxEdit** | `txt` | `txtTaiKhoan`, `txtMatKhau`, `txtHoTen`, `txtEmail` |
| **ComboBox / MaterialComboBox** | `cbo` | `cboKhoaHoc`, `cboGiaoVien`, `cboDapAnDung` |
| **DataGridView** | `dgv` | `dgvGiaoVien`, `dgvHocVien`, `dgvLopHoc`, `dgvDeThi` |
| **RadioButton / MaterialRadioButton** | `rad` | `radDapAnA`, `radDapAnB`, `radDapAnC`, `radDapAnD` |
| **CheckBox / MaterialCheckBox** | `chk` | `chkGhiNho`, `chkHienMatKhau`, `chkDanhDau` |
| **Label / MaterialLabel / BigLabel** | `lbl` | `lblTieuDe`, `lblThoiGian`, `lblTongSoHocVien` |
| **TabControl / MaterialTabControl** | `tab` | `tabAdmin`, `tabGiaoVien`, `tabHocVien` |
| **TabPage** | `tab` hoặc `page` | `pageDashboard`, `pageGiaoVien`, `pageHocVien` |
| **Card / MaterialCard** | `card` | `cardDangNhap`, `cardThongKeHV`, `cardCauHoi` |
| **Panel / FlowLayoutPanel / TableLayoutPanel** | `pnl`, `flp`, `tlp` | `pnlDieuHuong`, `flpNutCauHoi`, `tlpGiaoVien` |
| **ProgressBar / HopeProgressBar** | `prg` | `prgTienDo` |
| **Chart** | `chr` | `chrDiemTrungBinh` |
| **ListView / MaterialListView** | `lvw` | `lvwHoatDong`, `lvwChiTietDiem` |

---

## 1. FORM ĐĂNG NHẬP (`frm_DangNhap`)

| Tên mặc định hiện tại | Tên quy chuẩn khuyến nghị | Loại Control | Ý nghĩa & Mục đích sử dụng |
| :--- | :--- | :--- | :--- |
| `materialCard1` | `cardDangNhap` | MaterialCard | Thẻ chứa toàn bộ form đăng nhập |
| `bigLabel1` | `lblTieuDeChinh` | BigLabel | Nhãn tiêu đề lớn: "Learning Centre" |
| `labelEdit1` | `lblTieuDePhu` | LabelEdit | Phụ đề: "Quản Lý Trung Tâm Học Tập..." |
| `labelEdit2` | `lblGhiChuAdmin` | LabelEdit | Ghi chú chân trang: "Tài Khoản do Admin Cấp" |
| `txt_TaiKhoan` | `txtTaiKhoan` | MaterialTextBoxEdit | Ô nhập Tên đăng nhập |
| `txt_MatKhau` | `txtMatKhau` | MaterialTextBoxEdit | Ô nhập Mật khẩu |
| `checkBox1` | `chkHienMatKhau` | CheckBox | Checkbox Hiện / Ẩn mật khẩu |
| `check_GhiNho` | `chkGhiNho` | MaterialCheckBox | Checkbox Ghi nhớ tài khoản |
| `btn_DangNhap` | `btnDangNhap` | MaterialButton | Nút bấm Đăng nhập |

---

## 2. FORM KIỂM TRA / LÀM BÀI THI (`Frm_KiemTra`)

> ⚠️ **LƯU Ý:** Form này trước đó bị lỗi gán ngược hàm nộp bài và câu trước. Cần đổi tên chính xác như bảng dưới.

| Tên mặc định hiện tại | Tên quy chuẩn khuyến nghị | Loại Control | Ý nghĩa & Mục đích sử dụng |
| :--- | :--- | :--- | :--- |
| `materialCard1` | `cardDanhSachCau` | MaterialCard | Khung bên trái chứa danh sách các nút câu hỏi |
| `materialCard2` | `cardNoiDungThi` | MaterialCard | Khung bên phải chứa đề bài và các lựa chọn A, B, C, D |
| `flowLayoutPanel1` | `flpNutCauHoi` | FlowLayoutPanel | Panel chứa các nút tròn 1, 2, 3... 20 để chọn nhanh câu |
| `hopeRoundButton1` .. `20` | `btnCau1` .. `btnCau20` | HopeRoundButton | Nút nhảy nhanh tới Câu 1 đến Câu 20 |
| `lbl_DeBai` | `lblNoiDungCauHoi` | MaterialLabel | Nhãn hiển thị nội dung câu hỏi hiện tại |
| `rad_A` | `radDapAnA` | MaterialRadioButton | Lựa chọn đáp án A |
| `rad_B` | `radDapAnB` | MaterialRadioButton | Lựa chọn đáp án B |
| `rad_C` | `radDapAnC` | MaterialRadioButton | Lựa chọn đáp án C |
| `rad_D` | `radDapAnD` | MaterialRadioButton | Lựa chọn đáp án D |
| `check_DanhDau` | `chkDanhDauXemLai` | MaterialCheckBox | Checkbox đánh dấu câu hỏi để xem lại |
| `materialLabel1` | `lblThoiGianDemNguoc`| MaterialLabel | Nhãn đồng hồ đếm ngược (Thời gian còn lại) |
| `hopeProgressBar1` | `prgTienDoLamBai` | HopeProgressBar | Thanh hiển thị % tiến độ hoàn thành bài thi |
| `panel1` | `pnlKhuVucCauHoi` | Panel | Panel chứa nội dung câu và 4 RadioButton |
| `panel2` | `pnlDieuHuong` | Panel | Panel đáy chứa 3 nút điều hướng & nộp bài |
| `btn_CauTruoc` | `btnCauTruoc` | MaterialButton | Nút quay lại câu trước |
| `btn_CauTiep` | `btnCauTiep` | ForeverButton | Nút chuyển sang câu tiếp theo |
| `btn_NopBai` | `btnNopBai` | ForeverButton | Nút xác nhận nộp bài thi |

---

## 3. FORM QUẢN TRỊ VIÊN (`Frm_Admin`)

### 3.1. Hệ thống Tab chính
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `materialTabControl1` | `tabAdmin` | MaterialTabControl | Bộ điều hướng Tab chính |
| `tabPage1` | `pageDashboard` | TabPage | Tab Thống kê / Tổng quan |
| `tabPage2` | `pageGiaoVien` | TabPage | Tab Quản lý Giáo Viên |
| `tabPage9` | `pageHocVien` | TabPage | Tab Quản lý Học Viên |
| `tabPage3` | `pageKhoaHocVaLop` | TabPage | Tab Quản lý Khóa Học & Lớp |
| `tabPage7` | `pageTaiKhoan` | TabPage | Tab Quản lý Tài Khoản Hệ Thống |
| `tabPage8` | `pageDangXuat` | TabPage | Tab Đăng xuất |

### 3.2. Controls trên `pageDashboard` (Thống kê)
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `materialCard1` | `cardThongKeHV` | MaterialCard | Khối thẻ học viên |
| `materialLabel1` | `lblTongSoHocVien` | MaterialLabel | Text: "Học Viên: 120" |
| `materialCard3` | `cardThongKeGV` | MaterialCard | Khối thẻ giáo viên |
| `materialLabel2` | `lblTongSoGiaoVien` | MaterialLabel | Text: "Giáo Viên: 15" |
| `materialCard2` | `cardThongKeKH` | MaterialCard | Khối thẻ khóa học |
| `materialLabel3` | `lblTongSoKhoaHoc` | MaterialLabel | Text: "Khóa Học: 8" |
| `materialCard4` | `cardThongKeLop` | MaterialCard | Khối thẻ lớp học |
| `materialLabel4` | `lblTongSoLopHoc` | MaterialLabel | Text: "Lớp Học: 12" |
| `chart1` | `chrDiemTrungBinh` | Chart | Biểu đồ cột điểm trung bình theo đề |
| `materialListView2` | `lvwHoatDongMoiNhat`| MaterialListView | Danh sách các lượt nộp bài mới nhất |

### 3.3. Controls trên `pageGiaoVien` (Quản lý Giáo viên)
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `dataGridView1` | `dgvGiaoVien` | DataGridView | Bảng danh sách giáo viên |
| `materialTextBoxEdit10` | `txtGV_HoTen` | MaterialTextBoxEdit | Nhập Họ tên giáo viên |
| `materialTextBoxEdit11` | `txtGV_ChuyenMon` | MaterialTextBoxEdit | Nhập Chuyên môn (Tin, Toán, Anh...) |
| `materialTextBoxEdit9` | `txtGV_Email` | MaterialTextBoxEdit | Nhập Email giáo viên |
| `materialTextBoxEdit8` | `txtGV_SoDienThoai`| MaterialTextBoxEdit | Nhập Số điện thoại |
| `materialTextBoxEdit7` | `txtGV_DiaChi` | MaterialTextBoxEdit | Nhập Địa chỉ |
| `materialTextBoxEdit12` | `txtGV_TimKiem` | MaterialTextBoxEdit | Ô tìm kiếm giáo viên theo từ khóa |
| `materialButton8` | `btnGV_Them` | MaterialButton | Nút Thêm mới giáo viên |
| `materialButton7` | `btnGV_Sua` | MaterialButton | Nút Cập nhật thông tin giáo viên |
| `materialButton6` | `btnGV_Xoa` | MaterialButton | Nút Xóa giáo viên |
| `materialButton5` | `btnGV_LamMoi` | MaterialButton | Nút Làm mới form nhập liệu |

### 3.4. Controls trên `pageHocVien` (Quản lý Học viên)
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `dataGridView2` | `dgvHocVien` | DataGridView | Bảng danh sách học viên |
| `materialTextBoxEdit1` | `txtHV_HoTen` | MaterialTextBoxEdit | Nhập Họ tên học viên |
| `materialTextBoxEdit2` | `txtHV_NgaySinh` | MaterialTextBoxEdit | Nhập Ngày sinh (Khuyên dùng `dtpHV_NgaySinh`) |
| `materialComboBox1` | `cboHV_GioiTinh` | MaterialComboBox | Chọn Giới tính (Nam / Nữ / Khác) |
| `materialTextBoxEdit3` | `txtHV_Email` | MaterialTextBoxEdit | Nhập Email học viên |
| `materialTextBoxEdit4` | `txtHV_SoDienThoai`| MaterialTextBoxEdit | Nhập Số điện thoại |
| `materialTextBoxEdit5` | `txtHV_DiaChi` | MaterialTextBoxEdit | Nhập Địa chỉ |
| `materialTextBoxEdit6` | `txtHV_TimKiem` | MaterialTextBoxEdit | Ô tìm kiếm học viên |
| `materialButton1` | `btnHV_Them` | MaterialButton | Nút Thêm học viên |
| `materialButton2` | `btnHV_Sua` | MaterialButton | Nút Sửa học viên |
| `materialButton3` | `btnHV_Xoa` | MaterialButton | Nút Xóa học viên |
| `materialButton4` | `btnHV_LamMoi` | MaterialButton | Nút Làm mới form nhập học viên |

### 3.5. Controls trên `pageKhoaHocVaLop` (Khóa học & Lớp học)
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| **Bảng Khóa Học:** | | | |
| `dataGridView3` | `dgvKhoaHoc` | DataGridView | Danh sách khóa học |
| `materialTextBoxEdit13` | `txtKH_TenKhoaHoc`| MaterialTextBoxEdit | Nhập Tên khóa học |
| `materialTextBoxEdit14` | `txtKH_MoTa` | MaterialTextBoxEdit | Nhập Mô tả khóa học |
| `materialButton9` | `btnKH_Them` | MaterialButton | Nút Thêm khóa học |
| `materialButton10` | `btnKH_Sua` | MaterialButton | Nút Sửa khóa học |
| `materialButton11` | `btnKH_Xoa` | MaterialButton | Nút Xóa khóa học |
| **Bảng Lớp Học:** | | | |
| `dataGridView4` | `dgvLopHoc` | DataGridView | Danh sách lớp học |
| `materialTextBoxEdit15` | `txtLop_TenLop` | MaterialTextBoxEdit | Nhập Tên lớp |
| `materialTextBoxEdit16` | `txtLop_SiSoToiDa`| MaterialTextBoxEdit | Nhập Sĩ số tối đa (hoặc Lịch học) |
| `materialComboBox3` | `cboLop_KhoaHoc` | MaterialComboBox | Chọn Khóa học mà lớp trực thuộc |
| `materialComboBox4` | `cboLop_GiaoVien` | MaterialComboBox | Chọn Giáo viên phụ trách lớp |
| `materialButton12` | `btnLop_Them` | MaterialButton | Nút Thêm lớp học |
| `materialButton13` | `btnLop_Sua` | MaterialButton | Nút Sửa thông tin lớp |
| `materialButton14` | `btnLop_Xoa` | MaterialButton | Nút Xóa lớp học |

### 3.6. Controls trên `pageTaiKhoan` (Quản lý Tài khoản)
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `dataGridView5` | `dgvTaiKhoan` | DataGridView | Bảng danh sách tài khoản toàn hệ thống |
| `materialButton15` | `btnTK_KhoaMoKhoa`| MaterialButton | Nút Khóa / Mở khóa tài khoản đã chọn |
| `materialButton16` | `btnTK_DatLaiMatKhau`| MaterialButton | Nút Reset mật khẩu về mặc định (123456) |
| `materialButton17` | `btnTK_Xoa` | MaterialButton | Nút Xóa tài khoản |

---

## 4. FORM GIÁO VIÊN (`frm_GiaoVien`)

### 4.1. Hệ thống Tab chính
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `materialTabControl1` | `tabGiaoVien` | MaterialTabControl | Bộ điều hướng Tab của giáo viên |
| `tabPage9` | `pageGV_TrangChu` | TabPage | Trang chủ tổng quan của giáo viên |
| `tabPage10` | `pageGV_LopHoc` | TabPage | Quản lý danh sách lớp được phân công |
| `tabPage12` | `pageGV_DuyetHocVien`| TabPage | Phê duyệt học viên đăng ký vào lớp |
| `tabPage13` | `pageGV_NganHangCauHoi`| TabPage | Quản lý ngân hàng câu hỏi trắc nghiệm |
| `tabPage14` | `pageGV_QuanLyDeThi`| TabPage | Tạo và quản lý đề thi trắc nghiệm |
| `tabPage15` | `pageGV_KetQuaThi` | TabPage | Xem kết quả và bảng điểm học viên |
| `tabPage16` | `pageGV_ThongTin` | TabPage | Thông tin hồ sơ & Đổi mật khẩu |
| `tabPage17` | `pageGV_DangXuat` | TabPage | Tab Đăng xuất |

### 4.2. Chi tiết Controls theo từng Tab của Giáo Viên
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| **Trang chủ & Lớp:** | | | |
| `materialLabel31` | `lblGV_LoiChao` | MaterialLabel | Text: "Xin Chào, Thầy/Cô..." |
| `dataGridView1` | `dgvGV_LopCuaToi` | DataGridView | Bảng danh sách lớp giáo viên giảng dạy |
| `dataGridView7` | `dgvGV_HocVienTrongLop`| DataGridView | Danh sách học viên trong lớp đã chọn |
| `dataGridView8` | `dgvGV_LichDay` | DataGridView | Lịch giảng dạy |
| **Tab Duyệt Đăng Ký:**| | | |
| `materialComboBox10` | `cboDuyet_ChonLop` | MaterialComboBox | Dropdown chọn lớp cần xem yêu cầu duyệt |
| `dataGridView11` | `dgvDangKyChoDuyet`| DataGridView | Bảng học viên đang chờ duyệt vào lớp |
| `foreverButton18` | `btnDuyet_ChapNhan`| ForeverButton | Nút "Duyệt" học viên vào lớp |
| `foreverButton19` | `btnDuyet_TuChoi` | ForeverButton | Nút "Từ chối" yêu cầu đăng ký |
| **Tab Ngân Hàng Câu Hỏi:**| | | |
| `materialComboBox11` | `cboCH_ChonDeThi` | MaterialComboBox | Chọn Đề thi để thêm câu hỏi vào |
| `materialTextBoxEdit23`| `txtCH_NoiDung` | MaterialTextBoxEdit | Nhập nội dung câu hỏi |
| `materialTextBoxEdit20`| `txtCH_DapAnA` | MaterialTextBoxEdit | Nhập nội dung Đáp án A |
| `materialTextBoxEdit21`| `txtCH_DapAnB` | MaterialTextBoxEdit | Nhập nội dung Đáp án B |
| `materialTextBoxEdit22`| `txtCH_DapAnC` | MaterialTextBoxEdit | Nhập nội dung Đáp án C |
| `materialTextBoxEdit30`| `txtCH_DapAnD` | MaterialTextBoxEdit | Nhập nội dung Đáp án D |
| `materialComboBox12` | `cboCH_DapAnDung` | MaterialComboBox | Chọn đáp án đúng: A, B, C, hoặc D |
| `foreverButton20` | `btnCH_Them` | ForeverButton | Nút Thêm câu hỏi vào đề |
| `foreverButton21` | `btnCH_Sua` | ForeverButton | Nút Sửa câu hỏi đã chọn |
| `materialButton3` | `btnCH_Xoa` | MaterialButton | Nút Xóa câu hỏi |
| `dataGridView12` | `dgvDanhSachCauHoi`| DataGridView | Bảng danh sách câu hỏi của đề |
| **Tab Quản Lý Đề Thi:**| | | |
| `materialTextBoxEdit24`| `txtDe_TenDeThi` | MaterialTextBoxEdit | Nhập Tên đề thi |
| `materialTextBoxEdit25`| `txtDe_ThoiGian` | MaterialTextBoxEdit | Nhập Thời gian làm bài (phút) |
| `materialTextBoxEdit26`| `txtDe_SoCau` | MaterialTextBoxEdit | Nhập Số lượng câu hỏi |
| `materialComboBox13` | `cboDe_ChonLopHoc` | MaterialComboBox | Chọn Lớp được phép làm đề này |
| `materialComboBox14` | `cboDe_MonHoc` | MaterialComboBox | Chọn/Nhập Tên môn học |
| `foreverButton22` | `btnDe_TaoDeThi` | ForeverButton | Nút Tạo đề thi mới |
| `dataGridView13` | `dgvDanhSachDeThi` | DataGridView | Bảng danh sách các đề thi đã tạo |
| **Tab Kết Quả Thi:** | | | |
| `materialComboBox15` | `cboKQ_ChonDeThi` | MaterialComboBox | Dropdown chọn đề thi cần xem điểm |
| `dataGridView14` | `dgvKetQuaHocSinh` | DataGridView | Bảng danh sách điểm thi của học sinh |
| `foreverButton23` | `btnKQ_XuatExcel` | ForeverButton | Nút Xuất bảng điểm ra file Excel |
| **Tab Thông Tin Cá Nhân & Đổi Mật Khẩu:** | | | |
| `materialTextBoxEdit27`| `txtGV_Profile_HoTen` | MaterialTextBoxEdit | Ô hiển thị/sửa Họ tên giáo viên |
| `materialTextBoxEdit28`| `txtGV_Profile_Email` | MaterialTextBoxEdit | Ô hiển thị/sửa Email |
| `materialTextBoxEdit29`| `txtGV_Profile_SDT` | MaterialTextBoxEdit | Ô hiển thị/sửa Số điện thoại |
| `foreverButton24` | `btnGV_CapNhatThongTin`| ForeverButton | Nút Lưu thông tin cá nhân |
| `materialTextBoxEdit31`| `txtGV_MatKhauCu` | MaterialTextBoxEdit | Nhập Mật khẩu cũ |
| `materialTextBoxEdit32`| `txtGV_MatKhauMoi` | MaterialTextBoxEdit | Nhập Mật khẩu mới |
| `materialTextBoxEdit33`| `txtGV_XacNhanMatKhau`| MaterialTextBoxEdit | Xác nhận mật khẩu mới |
| `foreverButton25` | `btnGV_DoiMatKhau` | ForeverButton | Nút Xác nhận đổi mật khẩu |

---

## 5. FORM HỌC VIÊN (`Frm_HocVien`)

### 5.1. Hệ thống Tab chính
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| `materialTabControl1` | `tabHocVien` | MaterialTabControl | Bộ điều hướng Tab của học viên |
| `tabPage6` | `pageHV_TrangChu` | TabPage | Trang chủ tổng quan học viên |
| `tabPage2` | `pageHV_LopHoc` | TabPage | Danh sách lớp học đang tham gia |
| `tabPage3` | `pageHV_BaiThi` | TabPage | Danh sách các đề thi cần làm |
| `tabPage4` | `pageHV_KetQua` | TabPage | Lịch sử kết quả thi & Bảng điểm |
| `tabPage5` | `pageHV_ThongTin` | TabPage | Hồ sơ cá nhân & Đổi mật khẩu |
| `tabPage1` | `pageHV_DangXuat` | TabPage | Tab Đăng xuất |

### 5.2. Chi tiết Controls theo từng Tab của Học Viên
| Tên hiện tại | Tên quy chuẩn | Loại | Ý nghĩa |
| :--- | :--- | :--- | :--- |
| **Trang chủ & Lớp học:** | | | |
| `materialLabel1` | `lblHV_LoiChao` | MaterialLabel | Text: "Xin Chào, Học Viên..." |
| `dataGridView1` | `dgvHV_LopDangHoc` | DataGridView | Danh sách lớp học đang tham gia |
| `dataGridView2` | `dgvHV_DeThiSapToi` | DataGridView | Danh sách bài thi sắp diễn ra |
| `foreverButton6` | `btnHV_VaoLopHoc` | ForeverButton | Nút Xem chi tiết lớp học |
| **Tab Bài Thi:** | | | |
| `materialComboBox1` | `cboHV_LocMonHoc` | MaterialComboBox | Dropdown lọc đề theo môn học |
| `dataGridView3` | `dgvHV_DanhSachDeThi`| DataGridView | Bảng danh sách đề thi (Tên đề, Môn, Số câu, Phút) |
| `foreverButton3` | `btnHV_BatDauLamBai`| ForeverButton | **Nút quan trọng**: Bắt đầu làm bài thi đã chọn |
| `materialButton4` | `btnHV_LamMoiDeThi` | MaterialButton | Nút Tải lại danh sách đề thi |
| **Tab Kết Quả:** | | | |
| `dataGridView4` | `dgvHV_LichSuThi` | DataGridView | Bảng lịch sử các bài thi đã hoàn thành |
| `materialListView5` | `lvwHV_ChiTietDiem` | MaterialListView | Danh sách chi tiết số câu đúng, điểm số, ngày nộp |
| `materialButton5` | `btnHV_XemLaiBaiThi`| MaterialButton | Nút Xem lại câu trả lời chi tiết |
| **Tab Thông Tin Cá Nhân & Đổi Mật Khẩu:** | | | |
| `materialTextBoxEdit1`| `txtHV_Profile_HoTen` | MaterialTextBoxEdit | Họ tên học viên |
| `materialTextBoxEdit2`| `txtHV_Profile_NgaySinh`| MaterialTextBoxEdit | Ngày sinh |
| `materialComboBox2` | `cboHV_Profile_GioiTinh`| MaterialComboBox | Giới tính |
| `materialTextBoxEdit3`| `txtHV_Profile_Email` | MaterialTextBoxEdit | Email |
| `materialTextBoxEdit4`| `txtHV_Profile_SDT` | MaterialTextBoxEdit | Số điện thoại |
| `materialTextBoxEdit5`| `txtHV_Profile_DiaChi`| MaterialTextBoxEdit | Địa chỉ |
| `foreverButton1` | `btnHV_LuuProfile` | ForeverButton | Nút Lưu thông tin cá nhân |
| `materialTextBoxEdit6`| `txtHV_MatKhauCu` | MaterialTextBoxEdit | Mật khẩu hiện tại |
| `materialTextBoxEdit7`| `txtHV_MatKhauMoi` | MaterialTextBoxEdit | Mật khẩu mới |
| `materialTextBoxEdit8`| `txtHV_XacNhanMatKhau`| MaterialTextBoxEdit | Nhập lại mật khẩu mới |
| `foreverButton4` | `btnHV_DoiMatKhau` | ForeverButton | Nút Xác nhận đổi mật khẩu |

---

## 💡 MẸO SỬA TRONG VISUAL STUDIO NHANH NHẤT
1. **Không sửa trực tiếp thủ công bằng tay trong file `.Designer.cs` nếu chưa quen**:
   - Hãy mở Form ở chế độ thiết kế đồ họa (**View Designer** - `Shift + F7`).
   - Click chọn Control $\rightarrow$ Nhìn sang cửa sổ **Properties** (F4).
   - Tìm mục **`(Name)`** ở đầu danh sách $\rightarrow$ Gõ tên chuẩn theo bảng trên và nhấn `Enter`.
   - Visual Studio sẽ tự động cập nhật tên biến ở tất cả các nơi liên quan mà không làm hỏng file Designer!
2. **Đối với Text hiển thị**:
   - Tìm mục **`Text`** trong Properties $\rightarrow$ Gõ Tiếng Việt có dấu chuẩn (`Đăng Xuất`, `Câu Trước`, `Nộp Bài`) để sửa triệt để lỗi dấu hỏi `?`.
