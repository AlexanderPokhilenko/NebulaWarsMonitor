using System.Security.Cryptography;
using System.Text;

namespace NebulaWarsMonitor
{
    public static class PasswordManager
    {
        private static string _passwordHash;
        private static string _secretHash;

        public static void SetPassword(string password) => _passwordHash = GetHash(password);

        public static void SetSecret(string secret) => _secretHash = GetHash(secret);

        public static bool PasswordIsCorrect(string password) => GetHash(password) == _passwordHash;

        public static bool SecretIsCorrect(string secret) => GetHash(secret) == _secretHash;

        private static string GetHash(string pwd)
        {
            var data = Encoding.ASCII.GetBytes(pwd ?? "");
            using var sha1 = new SHA1CryptoServiceProvider();
            var sha1data = sha1.ComputeHash(data);
            return Encoding.ASCII.GetString(sha1data);
        }
    }
}
