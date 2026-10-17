using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class Validation
    {
        public static bool IsEmpty(string input) => string.IsNullOrWhiteSpace(input);
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        }
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone, @"^(0[3|5|7|8|9])+([0-9]{8})$");
        }
    }
}
