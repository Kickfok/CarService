using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UserRegistration;

namespace UserRegistration.Tests
{
    [TestClass]
    public class PasswordStrengthCheckerTests
    {

        [TestMethod]
        public void GetPasswordStrength_AllChars_5Points()
        {
            // arrange
            string password = "P2ssw0rd#";
            int expected = 5;
            // act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void GetPasswordStrength_UpperCase_3Points()
        {
            // Arrange
            string password = "Password";
            int expected = 3; // верхний регистр 1, за длину строки 1, за нижний регистр 1
                              // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPasswordStrength_ContainsNumber_0_4Points()
        {
            // Arrange
            string password = "Passw0rd";
            // верхний регистр 1, за длину строки 1, за нижний регистр 1
            // число 1
            int expected = 4;
            // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void GetPasswordStrength_ContainsNumber_1_4Points()
        {
            // Arrange
            string password = "Passw1rd";
            // верхний регистр 1, за длину строки 1, за нижний регистр 1
            // число 1
            int expected = 4;
            // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }

        // Tests for special chars
        [TestMethod]
        public void GetPasswordStrength_ContainsSpecialChar_at_5Points()
        {
            // Arrange
            string password = "Passw0rd@";
            // верхний регистр 1, за длину строки 1, за нижний регистр 1
            // число 1, специальный символ 1
            int expected = 5;
            // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void GetPasswordStrength_ContainsSpecialChar_Hash_5Points()
        {
            // Arrange
            string password = "Passw0rd#";
            // верхний регистр 1, за длину строки 1, за нижний регистр 1
            // число 1, специальный символ 1
            int expected = 5;
            // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void GetPasswordStrength_ContainsSpecialChar_Excl_5Points()
        {
            // Arrange
            string password = "Passw0rd!";
            // верхний регистр 1, за длину строки 1, за нижний регистр 1
            // число 1, специальный символ 1
            int expected = 5;
            // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void GetPasswordStrength_ContainsSpecialChar_Doll_5Points()
        {
            // Arrange
            string password = "Passw0rd$";
            // верхний регистр 1, за длину строки 1, за нижний регистр 1
            // число 1, специальный символ 1
            int expected = 5;
            // Act
            int actual = PasswordStrengthChecker.GetPasswordStrength(password);
            // Assert
            Assert.AreEqual(expected, actual);
        }

        // Граничные случаи
        [TestMethod]
        public void GetPasswordStrength_Null_0Points()
        {
            Assert.AreEqual(0, PasswordStrengthChecker.GetPasswordStrength(null));
        }

        [TestMethod]
        public void GetPasswordStrength_Empty_0Points()
        {
            Assert.AreEqual(0, PasswordStrengthChecker.GetPasswordStrength(string.Empty));
        }

        [TestMethod]
        public void GetPasswordStrength_SevenChars_NoLengthPoint()
        {
            // только нижний регистр 1, длина 7 меньше минимальной
            Assert.AreEqual(1, PasswordStrengthChecker.GetPasswordStrength("abcdefg"));
        }

        [TestMethod]
        public void GetPasswordStrength_EightChars_LengthPoint()
        {
            // нижний регистр 1, длина 8 равна минимальной 1
            Assert.AreEqual(2, PasswordStrengthChecker.GetPasswordStrength("abcdefgh"));
        }

        [TestMethod]
        public void GetPasswordStrength_Cyrillic_CountsLetters()
        {
            // верхний регистр 1, нижний регистр 1, длина 1, число 1, специальный символ 1
            Assert.AreEqual(5, PasswordStrengthChecker.GetPasswordStrength("Пароль12!"));
        }

        [TestMethod]
        public void GetPasswordStrength_Space_IsNotSpecialChar()
        {
            // нижний регистр 1, длина 1, пробел баллов не дает
            Assert.AreEqual(2, PasswordStrengthChecker.GetPasswordStrength("pass word"));
        }
    }
}
