using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UserRegistration
{
    public class PasswordStrengthCheker
    {
        // Возвращает значение определяющее сложность пароля пользователя.
        public static int GetPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return 0;
            }
            int result = 0;
            // +1 балл за длину.
            if (Math.Max(password.Length, 7) > 7)
            {
                result++;
            }
            //+1 балл за наличие символа в нижнем регистре
            if (Regex.Match(password, "[a-z]").Success)
            {
                result++;
            }
            //+1 балл за наличие символа в верхнем регистре
            if (Regex.Match(password, "[A-Z]").Success)
            {
                result++;
            }
            // +1 балл за наличие числа.
            if (Regex.Match(password, "[0-9]").Success)
            {
                result++;
            }
            // +1 балл за наличие специального символа.
            if (Regex.Match(password,
            "[\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)\\{\\}\\[\\]\\:\\'\\;\\\"\\/\\?\\.\\>\\,\\<\\~\\`\\-\\\\_\\=\\+\\|]").Success)
            {
                result++;
            }
            return result;
        }
    }
}
