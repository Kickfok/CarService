using System;
using System.Security.Cryptography;

namespace UserRegistration
{
    /// <summary>
    /// Хеширование и проверка паролей по алгоритму PBKDF2 (HMAC-SHA256) со случайной солью.
    /// </summary>
    /// <remarks>
    /// Формат хранимой строки: <c>PBKDF2$SHA256$&lt;итерации&gt;$&lt;соль Base64&gt;$&lt;хеш Base64&gt;</c>.
    /// Число итераций хранится вместе с хешем, поэтому его можно увеличить без пересчета старых паролей.
    /// </remarks>
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const string Algorithm = "SHA256";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const char Separator = '$';

        public const int DefaultIterations = 100000;

        // Возвращает строку для хранения в БД.
        public static string Hash(string password)
        {
            return Hash(password, DefaultIterations);
        }

        public static string Hash(string password, int iterations)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = Derive(password, salt, iterations, HashSize);

            return string.Join(Separator.ToString(),
                Prefix, Algorithm, iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        // Проверяет пароль по сохраненной строке. Для строки неизвестного формата возвращает false.
        public static bool Verify(string password, string storedHash)
        {
            if (password == null || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Trim().Split(Separator);
            if (parts.Length != 5 || parts[0] != Prefix || parts[1] != Algorithm)
            {
                return false;
            }

            int iterations;
            if (!int.TryParse(parts[2], out iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[3]);
                expected = Convert.FromBase64String(parts[4]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (expected.Length == 0)
            {
                return false;
            }

            var actual = Derive(password, salt, iterations, expected.Length);
            return FixedTimeEquals(actual, expected);
        }

        private static byte[] Derive(string password, byte[] salt, int iterations, int length)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(length);
            }
        }

        // Сравнение за постоянное время, чтобы по времени ответа нельзя было подобрать хеш.
        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }
            return difference == 0;
        }
    }
}
