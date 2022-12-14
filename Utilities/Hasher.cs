using System.Security.Cryptography;
using System.Text;

namespace SignalMQ.Core.Utilities
{
    public enum HashMethod
    {
        MD5,
        SHA256
    }

    public static class Hasher
    {
        public static class Md5
        {
            public static string Hash(string secret)
            {
                using var md5 = MD5.Create();
                byte[] inputBytes = Encoding.UTF8.GetBytes(secret);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }
        public static class Sha256
        {
            public static string Hash(string secret)
            {
                using var md5 = SHA256.Create();
                byte[] inputBytes = Encoding.UTF8.GetBytes(secret);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        public static string Hash(string secret, HashMethod method = HashMethod.MD5)
        {
            return method switch
            {
                HashMethod.MD5 => Md5.Hash(secret),
                HashMethod.SHA256 => Sha256.Hash(secret),
                _ => throw new Exception("Unknown hash method")
            };
        }
    }
}
