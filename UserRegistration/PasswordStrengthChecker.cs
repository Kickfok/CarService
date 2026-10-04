using System.Text.RegularExpressions;

namespace UserRegistration
{
    /// <summary>
    /// Оценка сложности пароля по шкале от 0 до 5 баллов.
    /// </summary>
    public static class PasswordStrengthChecker
    {
        public const int MinLength = 8;
        public const int MaxScore = 5;

        // Возвращает количество баллов: по одному за длину от MinLength символов, строчную букву,
        // заглавную букву, цифру и специальный символ. Буквы учитываются любого алфавита, включая кириллицу.
        public static int GetPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return 0;
            }

            var result = 0;

            // +1 балл за длину.
            if (password.Length >= MinLength)
            {
                result++;
            }
            // +1 балл за наличие строчной буквы.
            if (Regex.IsMatch(password, @"\p{Ll}"))
            {
                result++;
            }
            // +1 балл за наличие заглавной буквы.
            if (Regex.IsMatch(password, @"\p{Lu}"))
            {
                result++;
            }
            // +1 балл за наличие цифры.
            if (Regex.IsMatch(password, @"\d"))
            {
                result++;
            }
            // +1 балл за наличие специального символа: все, что не буква, не цифра и не пробел.
            if (Regex.IsMatch(password, @"[^\p{L}\p{N}\s]"))
            {
                result++;
            }

            return result;
        }
    }
}
