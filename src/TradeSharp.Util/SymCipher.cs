using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TradeSharp.Util
{
    public class SymCipher
    {
        private readonly Encoding enc = Encoding.Unicode;
        private readonly ICryptoTransform encryptor,
                                          decryptor;

        public SymCipher(Encoding enc, string pwrd)
        {
            this.enc = enc;
            var pdb = new Rfc2898DeriveBytes(pwrd,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 
                0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            var Key = pdb.GetBytes(32);
            var IV = pdb.GetBytes(16);

            var alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            alg.Padding = PaddingMode.PKCS7;
            encryptor = alg.CreateEncryptor();
            decryptor = alg.CreateDecryptor();
        }

        public bool EncryptSafe(string clearData, out string encrypedStr)
        {
            encrypedStr = "";
            if (string.IsNullOrEmpty(clearData)) return false;            
            try
            {
                encrypedStr = Encrypt(clearData);
                return true;
            }
            catch
            {
                encrypedStr = "";
                return false;
            }
        }

        public bool DecryptSafe(string codedData, out string clearStr)
        {
            clearStr = "";
            if (string.IsNullOrEmpty(codedData)) return false;
            try
            {
                clearStr = Decrypt(codedData);
                return true;
            }
            catch
            {
                clearStr = "";
                return false;
            }
        }

        public string Encrypt(string clearData)
        {
            var byteResult = Encrypt(enc.GetBytes(clearData));
            var sb = new StringBuilder();
            foreach (var bt in byteResult)
                sb.AppendFormat("{0:X2}", bt);
            return sb.ToString();
        }

        public string Decrypt(string cipherData)
        {
            var byteLen = cipherData.Length / 2;
            var bytesCrypted = new byte[byteLen];
            for (var i = 0; i < byteLen; i++)
            {
                var strByte = cipherData.Substring(i << 1, 2);
                bytesCrypted[i] = Convert.ToByte(strByte, 16);
            }
            return enc.GetString(Decrypt(bytesCrypted));
        }

        public byte[] Encrypt(byte[] clearData)
        {
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms,
               encryptor, CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        public byte[] Decrypt(byte[] cipherData)
        {
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms,
                decryptor, CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }
    }

}
