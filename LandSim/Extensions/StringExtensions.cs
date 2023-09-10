using System.Security.Cryptography;
using System.Text;

namespace LandSim.Extensions
{
    public static class StringExtensions
    {
        public static int GetDeterministicHashCode(this string str)
        {
            using var hasher = SHA1.Create();
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToInt32(hash, 0);
        }
    }
}
