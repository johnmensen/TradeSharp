using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TradeSharp.Contract.Entity
{
    [Serializable]
    public class UserInfoEx
    {
        public static string ComputeHash(byte[] data)
        {
            if (data == null)
                return null;
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(data);
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }

        public int Id { get; set; }

        // Bitmap will not serialize, use byte[]
        private byte[] avatarBigData;
        public byte[] AvatarBigData
        {
            get { return avatarBigData; }
            set
            {
                avatarBigData = value;
                avatarBig = GetBitmap(avatarBigData);
                AvatarBigHashCode = avatarBig == null ? null : ComputeHash(avatarBigData);
            }
        }

        private Bitmap avatarBig;
        [XmlIgnore]
        public Bitmap AvatarBig
        {
            get { return avatarBig; }
            set
            {
                avatarBig = value;
                avatarBigData = GetBitmapData(avatarBig);
                AvatarBigHashCode = avatarBig == null ? null : ComputeHash(avatarBigData);
            }
        }

        public string AvatarBigFileName { get; set; }

        public string AvatarBigHashCode { get; set; }

        // Bitmap will not serialize, use byte[]
        private byte[] avatarSmallData;
        public byte[] AvatarSmallData
        {
            get { return avatarSmallData; }
            set
            {
                avatarSmallData = value;
                avatarSmall = GetBitmap(avatarSmallData);
                AvatarSmallHashCode = avatarSmall == null ? null : ComputeHash(avatarSmallData);
            }
        }

        private Bitmap avatarSmall;
        [XmlIgnore]
        public Bitmap AvatarSmall
        {
            get
            {
                return avatarSmall;
            }
            set
            {
                avatarSmall = value;
                avatarSmallData = GetBitmapData(avatarSmall);
                AvatarSmallHashCode = avatarSmall == null ? null : ComputeHash(avatarSmallData);
            }
        }

        public string AvatarSmallFileName { get; set; }

        public string AvatarSmallHashCode { get; set; }

        private string about;
        public string About
        {
            get { return about; }
            set
            {
                about = value;
                AboutHashCode = about == null ? null : ComputeHash(GetTextData(about));
            }
        }

        // redundant for serialization, About is enough
        [XmlIgnore]
        public byte[] AboutData
        {
            get
            {
                return GetTextData(about);
            }
            set
            {
                about = GetText(value);
                AboutHashCode = about == null ? null : ComputeHash(value);
            }
        }

        public string AboutFileName { get; set; }

        public string AboutHashCode { get; set; }

        public string Contacts { get; set; }

        public static byte[] GetBitmapData(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            try
            {
                var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
            catch
            {
                return null;
            }
        }

        public static Bitmap GetBitmap(byte[] data)
        {
            if (data == null)
                return null;
            try
            {
                return new Bitmap(new MemoryStream(data));
            }
            catch
            {
                return null;
            }
        }

        public static byte[] GetTextData(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var codec = new UTF8Encoding();
            return codec.GetBytes(text);
        }

        public static string GetText(byte[] data)
        {
            if (data == null)
                return null;
            var codec = new UTF8Encoding();
            try
            {
                return codec.GetString(data);
            }
            catch
            {
                return null;
            }
        }

// ReSharper disable EmptyConstructor
        public UserInfoEx()
// ReSharper restore EmptyConstructor
        {
        }
    }
}
