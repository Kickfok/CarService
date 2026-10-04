using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UserRegistration.Tests
{
    [TestClass]
    public class PasswordHasherTests
    {
        // Небольшое число итераций, чтобы тесты выполнялись быстро.
        private const int TestIterations = 1000;

        [TestMethod]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var hash = PasswordHasher.Hash("admin1", TestIterations);

            Assert.IsTrue(PasswordHasher.Verify("admin1", hash));
        }

        [TestMethod]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var hash = PasswordHasher.Hash("admin1", TestIterations);

            Assert.IsFalse(PasswordHasher.Verify("admin2", hash));
        }

        [TestMethod]
        public void Hash_SamePasswordTwice_DifferentHashes()
        {
            // Соль случайная, поэтому одинаковые пароли дают разные строки.
            var first = PasswordHasher.Hash("user1", TestIterations);
            var second = PasswordHasher.Hash("user1", TestIterations);

            Assert.AreNotEqual(first, second);
        }

        [TestMethod]
        public void Hash_DefaultIterations_FitsDatabaseColumn()
        {
            // Колонка User.Password имеет тип nvarchar(128).
            var hash = PasswordHasher.Hash("Очень длинный пароль с кириллицей 1234567890!");

            Assert.IsTrue(hash.Length <= 128, $"Длина хеша {hash.Length} больше 128");
            StringAssert.StartsWith(hash, "PBKDF2$SHA256$100000$");
        }

        [TestMethod]
        public void Verify_PlainTextValue_ReturnsFalse()
        {
            // Пароль, сохраненный открытым текстом, не должен приниматься как хеш.
            Assert.IsFalse(PasswordHasher.Verify("admin1", "admin1"));
        }

        [TestMethod]
        public void Verify_CorruptedHash_ReturnsFalse()
        {
            Assert.IsFalse(PasswordHasher.Verify("admin1", "PBKDF2$SHA256$1000$not-base64$###"));
            Assert.IsFalse(PasswordHasher.Verify("admin1", "PBKDF2$SHA256$abc$AAAA$AAAA"));
        }

        [TestMethod]
        public void Verify_NullOrEmpty_ReturnsFalse()
        {
            var hash = PasswordHasher.Hash("admin1", TestIterations);

            Assert.IsFalse(PasswordHasher.Verify(null, hash));
            Assert.IsFalse(PasswordHasher.Verify("admin1", null));
            Assert.IsFalse(PasswordHasher.Verify("admin1", string.Empty));
        }

        [TestMethod]
        public void Verify_HashWithTrailingSpaces_ReturnsTrue()
        {
            // Старые колонки nchar дополняют значение пробелами справа.
            var hash = PasswordHasher.Hash("admin1", TestIterations);

            Assert.IsTrue(PasswordHasher.Verify("admin1", hash + "   "));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Hash_Null_Throws()
        {
            PasswordHasher.Hash(null);
        }
    }
}
