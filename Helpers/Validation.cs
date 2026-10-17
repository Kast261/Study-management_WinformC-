using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class Validation
    {
        public static bool IsEmpty(string input) => string.IsNullOrWhiteSpace(input);

        /// <summary>
        /// Validate Email: không được rỗng và đúng định dạng email
        /// </summary>
        public static bool IsValidEmail(string email, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email không được để trống!";
                return false;
            }
            email = email.Trim();
            bool match = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!match)
            {
                errorMessage = "Email không đúng định dạng (Ví dụ: user@example.com)!";
                return false;
            }
            return true;
        }

        public static bool IsValidEmail(string email)
        {
            return IsValidEmail(email, out _);
        }

        /// <summary>
        /// Validate Số điện thoại: Bắt đầu bằng số 0 và có đúng 10 chữ số
        /// </summary>
        public static bool IsValidPhone(string phone, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Số điện thoại không được để trống!";
                return false;
            }
            phone = phone.Trim();
            bool match = Regex.IsMatch(phone, @"^0\d{9}$");
            if (!match)
            {
                errorMessage = "Số điện thoại phải bắt đầu bằng số 0 và có đúng 10 chữ số (Ví dụ: 0987654321)!";
                return false;
            }
            return true;
        }

        public static bool IsValidPhone(string phone)
        {
            return IsValidPhone(phone, out _);
        }

        /// <summary>
        /// Validate Ngày sinh: Định dạng hợp lệ, không ở tương lai, tuổi từ 5 đến 120
        /// </summary>
        public static bool IsValidBirthDate(string dateStr, out DateTime? validDate, out string errorMessage)
        {
            validDate = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(dateStr))
            {
                errorMessage = "Ngày sinh không được để trống!";
                return false;
            }

            dateStr = dateStr.Trim();
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" };

            DateTime d;
            if (!DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
            {
                if (!DateTime.TryParse(dateStr, out d))
                {
                    errorMessage = "Ngày sinh không đúng định dạng! Vui lòng nhập theo định dạng dd/MM/yyyy (Ví dụ: 20/11/2004).";
                    return false;
                }
            }

            if (d.Date > DateTime.Today)
            {
                errorMessage = "Ngày sinh không được lớn hơn ngày hiện tại!";
                return false;
            }

            if (d.Date < DateTime.Today.AddYears(-120))
            {
                errorMessage = "Năm sinh không hợp lý (tuổi không được vượt quá 120 tuổi)!";
                return false;
            }

            if (d.Date > DateTime.Today.AddYears(-5))
            {
                errorMessage = "Độ tuổi chưa đủ điều kiện (yêu cầu từ 5 tuổi trở lên)!";
                return false;
            }

            validDate = d.Date;
            return true;
        }

        public static bool IsValidBirthDate(string dateStr)
        {
            return IsValidBirthDate(dateStr, out _, out _);
        }

        /// <summary>
        /// Validate Sĩ số lớp: Phải là số nguyên dương lớn hơn 0
        /// </summary>
        public static bool IsValidSiSo(string siSoStr, out int siSo, out string errorMessage)
        {
            siSo = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(siSoStr))
            {
                errorMessage = "Sĩ số lớp không được để trống!";
                return false;
            }

            siSoStr = siSoStr.Trim();
            if (!int.TryParse(siSoStr, out siSo) || siSo <= 0)
            {
                errorMessage = "Sĩ số lớp phải là chữ số nguyên dương lớn hơn 0 (Ví dụ: 30, 40)!";
                return false;
            }

            if (siSo > 500)
            {
                errorMessage = "Sĩ số lớp tối đa không vượt quá 500 học viên!";
                return false;
            }

            return true;
        }

        public static bool IsValidSiSo(string siSoStr)
        {
            return IsValidSiSo(siSoStr, out _, out _);
        }

        /// <summary>
        /// Hàm chuyển đổi ngày linh hoạt từ chuỗi sang DateTime?
        /// </summary>
        public static DateTime? ParseDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(input.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                return d;
            if (DateTime.TryParse(input.Trim(), out DateTime dt))
                return dt;
            return null;
        }
    }
}
