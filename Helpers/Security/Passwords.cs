using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class Passwords
    {
        public static string Hash(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return string.Empty;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plain));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static bool Verify(string plain, string hash)
        {
            if (string.IsNullOrEmpty(plain) || string.IsNullOrEmpty(hash)) return false;
            string hashOfInput = Hash(plain);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
        }

    }
}
