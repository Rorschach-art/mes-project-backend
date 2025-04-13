using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Model.Other
{
    public static class SecurityUtility
    {
        #region 正则验证模块
        // 更新版正则表达式模式（2024标准）
        private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        private const string PhonePattern = @"^1[3-9]\d{9}$";
        private const string IdCardPattern = @"^(\d{6})(\d{4})(\d{2})(\d{2})(\d{3})([0-9Xx])$";
        private const string ChineseNamePattern = @"^[\u4e00-\u9fa5]{2,8}(·[\u4e00-\u9fa5]{2,8})*$";
        private const string PasswordStrengthPattern =
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$";

        /// <summary>
        /// 通用正则验证方法
        /// </summary>
        public static bool Validate(string input, string pattern)
        {
            // 如果输入为空，直接返回 false
            if (string.IsNullOrEmpty(input))
                return false;

            // 使用正则表达式进行匹配，并设置超时时间
            return Regex.IsMatch(input, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500));
        }

        // 快捷验证方法
        public static bool IsWebUrl(string input) =>
            // 使用 Uri.IsWellFormedUriString 进行验证，确保 URL 合法且是绝对路径
            Uri.IsWellFormedUriString(input, UriKind.Absolute);

        public static bool IsEmail(string input) => Validate(input, EmailPattern);

        public static bool IsChinesePhone(string input) => Validate(input, PhonePattern);

        public static bool IsIdCard(string input) => Validate(input, IdCardPattern);

        public static bool IsChineseName(string input) => Validate(input, ChineseNamePattern);

        public static bool IsStrongPassword(string input) =>
            Validate(input, PasswordStrengthPattern);

        // 身份证号校验位验证
        public static bool IsIdCardWithCheck(string input)
        {
            if (!IsIdCard(input))
                return false;

            var digits = input.Replace("x", "X").ToUpper().ToCharArray();
            int[] weights = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1];
            char[] checkDigits = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (digits[i] - '0') * weights[i];
            }

            char expectedCheckDigit = checkDigits[sum % 11];
            return digits[17] == expectedCheckDigit;
        }
        #endregion

        #region 密码加密模块
        private const int DefaultSaltSize = 32; // 256-bit salt
        private const int DefaultIterations = 210_000; // OWASP 2023推荐迭代次数
        private const int DefaultHashSize = 32; // 256-bit hash
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

        /// <summary>
        /// 生成密码哈希（包含版本、盐值、迭代参数、算法名称）
        /// </summary>
        public static string GeneratePasswordHash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(DefaultSaltSize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                DefaultIterations,
                Algorithm,
                DefaultHashSize
            );
            return $"v2|SHA512|{DefaultIterations}|{Convert.ToBase64String(salt)}|{Convert.ToBase64String(hash)}";
        }


        private static readonly Lock LockObj = new ();
        /// <summary>
        /// 验证用户密码是否正确。
        /// </summary>
        /// <param name="password">用户输入的密码。</param>
        /// <param name="storedHash">存储在数据库中的密码哈希。</param>
        /// <returns>如果密码正确则返回true，否则返回false。</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            lock (LockObj)
            {
                // 分割存储的哈希值，以获取版本信息、算法、迭代次数、盐值和原始哈希值
                var segments = storedHash.Split('|');

                // 检查分割后的段数是否为5，版本是否为v2，算法是否为SHA512，如果不满足则返回false
                if (segments.Length != 5 || segments[0] != "v2" || segments[1] != "SHA512")
                {
                    return false; // 返回false，避免暴露具体错误信息
                }

                // 解析迭代次数和盐值，并从存储的哈希中获取原始哈希值
                var iterations = int.Parse(segments[2]);
                var salt = Convert.FromBase64String(segments[3]);
                var originalHash = Convert.FromBase64String(segments[4]);

                // 使用用户输入的密码、盐值、迭代次数和算法生成新的哈希值
                var newHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations,
                    Algorithm,
                    originalHash.Length
                );

                // 使用固定时间比较新生成的哈希值和原始哈希值，以防止时间攻击
                var result = CryptographicOperations.FixedTimeEquals(newHash, originalHash);
                return result;
            }
        }

        [Obsolete("MD5不推荐用于密码存储，仅适用于遗留系统兼容")]
        public static string ComputeMd5(string input)
        {
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }
        #endregion

        #region 扩展功能
        /// <summary>
        /// 生成符合强度要求的随机密码
        /// </summary>
        public static string GenerateRandomPassword(int length = 16)
        {
            if (length < 12)
                throw new ArgumentException("Password length should be at least 12 characters.");

            const string lower = "abcdefghjkmnpqrstuvwxyz";
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string digits = "23456789";
            const string symbols = "!@#$%^&*";
            const string allChars = lower + upper + digits + symbols;

            var password = new char[length];

            // Ensure at least one of each required character type
            password[0] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            password[1] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            password[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];

            // Fill the rest of the password with random characters
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
            }

            // Shuffle the password to ensure randomness
            for (int i = 0; i < length; i++)
            {
                int j = RandomNumberGenerator.GetInt32(length);
                (password[j], password[i]) = (password[i], password[j]);
            }

            return new string(password);
        }
        #endregion
    }
}
