using System.Security.Cryptography;
using System.Text;

namespace TradeSharp.Util
{
    public static class HashMd5
    {
        public static string CreateMD5Hash(this string input, Encoding encoding)
        {
            return CreateMD5Hash(input, encoding, false);
        }

        public static string CreateMD5Hash(this string input, Encoding encoding, bool capitalize)
        {
            var md5 = MD5.Create();
            var inputBytes = encoding.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            var formatStr = capitalize ? "X2" : "x2";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString(formatStr));
            }
            return sb.ToString();
        }
    }
}
