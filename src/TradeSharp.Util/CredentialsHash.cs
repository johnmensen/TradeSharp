using System.Text;

namespace TradeSharp.Util
{
    /// <summary>
    /// создает MD5-хэш из логина, пароля и некого числа    
    /// код нужно подвергнуть об
    /// </summary>
    public static class CredentialsHash
    {
        public static string MakeCredentialsHash(string login, string pwrd, long magic)
        {
            var sb = new StringBuilder((char)(magic & 0xFF));
            sb.Append(pwrd);
            sb.Append((char) ((magic & 0xFF0000) >> 16));
            sb.Append(login);
            sb.Append((char) ((magic & 0xFF00000000) >> 32));
            return sb.ToString().CreateMD5Hash(Encoding.ASCII, true);
        }

        public static string MakeOperationParamsHash(long time, int sessionTag, long terminalId)
        {
            var timeStr = time.ToString();
            var sb = new StringBuilder((char)(terminalId & 0xFF));
            sb.Append(timeStr);
            sb.Append((char)((sessionTag & 0xFF0000) >> 16));
            sb.Append(timeStr);
            sb.Append((char)((terminalId & 0xFF00000000) >> 32));
            return sb.ToString().CreateMD5Hash(Encoding.ASCII, true);
        }
    }
}
