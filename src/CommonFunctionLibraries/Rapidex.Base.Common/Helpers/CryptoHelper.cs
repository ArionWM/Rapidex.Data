using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Rapidex
{
    public static class CryptoHelper
    {
        //From Pro-Core
        protected sealed class PasswordHasherV2
        {
            /// <summary>
            /// Size of salt
            /// </summary>
            private const int SaltSize = 16;

            /// <summary>
            /// Size of hash
            /// </summary>
            private const int HashSize = 30;

            public static byte[] Hash(string password, int iterations)
            {
                //create salt
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

                //create hash
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
                var hash = pbkdf2.GetBytes(HashSize);

                //combine salt and hash
                var hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                return hashBytes;
            }

            /// <summary>
            /// verify a password against a hash
            /// </summary>
            /// <param name="password">the password</param>
            /// <param name="hashedPassword">the hash</param>
            /// <param name="iterations"></param>
            /// <returns>could be verified?</returns>
            public static bool Verify(string password, byte[] hashedPassword, int iterations)
            {
                //get hashbytes
                var hashBytes = hashedPassword;

                //get salt
                var salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                //create hash with given salt
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
                byte[] hash = pbkdf2.GetBytes(HashSize);

                //get result
                for (var i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //https://dotnettips.wordpress.com/2023/06/06/microsoft-net-code-analysis-the-rijndael-and-rijndaelmanaged-types-are-superseded/
        public static byte[] Encrypt(string plainText, SymmetricAlgorithm algorithm)
        {
            byte[] encryptedData;

            using (var encryptor = algorithm.CreateEncryptor())
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                encryptedData = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            return encryptedData;
        }

        public static string Decrypt(byte[] encryptedData, SymmetricAlgorithm algorithm)
        {
            string decryptedText;

            using (var decryptor = algorithm.CreateDecryptor())
            {
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                decryptedText = Encoding.UTF8.GetString(decryptedBytes);
            }

            return decryptedText;
        }

        private static byte[] GenerateAesKey(string keyBase)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(keyBase);
            byte[] key = new byte[32];

            for (int i = 0; i < 32; i++)
            {
                int index = i % keyBytes.Length;
                key[i] = keyBytes[index];
            }

            return key;
        }

        //Create encrypt method with AESManaged 
        public static string EncryptAes(string plainText, string key)
        {
            using (var algorithm = new AesManaged())
            {
                algorithm.Key = GenerateAesKey(key);
                algorithm.IV = new byte[16];
                byte[] encryptedData = Encrypt(plainText, algorithm);
                return Convert.ToBase64String(encryptedData);
            }
        }

        //Create decrypt method with AESManaged
        public static string DecryptAes(string encryptedText, string key)
        {
            using (var algorithm = new AesManaged())
            {
                algorithm.Key = GenerateAesKey(key);
                algorithm.IV = new byte[16];
                byte[] encryptedData = Convert.FromBase64String(encryptedText);
                return Decrypt(encryptedData, algorithm);
            }
        }

        public static string HashPassword(string password)
        {
            Byte[] hashBytes = PasswordHasherV2.Hash(password, 2100);
            byte[] hashResult = new byte[hashBytes.Length + 1];
            Array.Copy(hashBytes, 0, hashResult, 1, hashBytes.Length);
            hashResult[0] = 1; //v1
            string hashedPassword = Convert.ToBase64String(hashResult);
            if (hashedPassword.Contains(" "))
            {
                Log.Warn($"Whitespace in base64! :{hashedPassword}");
                Thread.Sleep(500);
                string hashedPassword2 = Convert.ToBase64String(hashResult);
                Log.Warn($"Whitespace in base64! :{hashedPassword}, reconvert: {hashedPassword2}");

                if (!hashedPassword2.Contains(" "))
                    hashedPassword = hashedPassword2;
            }

            return hashedPassword;
            //string packagedPassword = $"{CRIPTO_TEXT_PREFIX}{hashedPassword}";
            //return packagedPassword;
        }

        public static bool ValidatePassword(string hashedPasswordStr, string password)
        {
            if (hashedPasswordStr.IsNullOrEmpty() || password.IsNullOrEmpty())
                return false;

            if (hashedPasswordStr.Length > 1000)
                throw new InvalidOperationException("Invalid password h");

            if (password.Length > 100)
                throw new InvalidOperationException("Invalid password 2");

            try
            {

                byte[] result = Convert.FromBase64String(hashedPasswordStr);


                byte[] hashedPassword = new byte[result.Length - 1];
                Array.Copy(result, 1, hashedPassword, 0, result.Length - 1);

                switch (result[0])
                {
                    case 1:
                        return PasswordHasherV2.Verify(password, hashedPassword, 2100);

                    default:
                        throw new NotSupportedException($"Unsupported password version: {result[0]}");
                }
            }
            catch (FormatException fex)
            {
                fex.Log(); //$"hs: {hashedPasswordStr}"
                throw fex;
            }
        }
    }
}
