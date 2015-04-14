using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Xml;

namespace FIXViewer
{
    public partial class UpdateBaseForm : Form
    {
        public UpdateBaseForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var numStart = int.Parse(tbNumStart.Text);
            var numEnd = int.Parse(tbNumEnd.Text);
                        
            var doc = new XmlDocument();
            var nodeDic = doc.AppendChild(doc.CreateElement("Dictionary"));
            var nodeTag = nodeDic.AppendChild(doc.CreateElement(FixTag.PARENT_TAG));
            
            for (var i = numStart; i <= numEnd; i++)
            {
                ReadTagInfo(i, nodeTag);
            }

            var messages = tbMessages.Text.Split(',');
            var nodeMsg = nodeDic.AppendChild(doc.CreateElement(FixTag.PARENT_MSG));
            foreach (var msg in messages)
            {
                ReadMsgType(msg, nodeMsg);
            }

            doc.Save(Dictionary.DicPath);
        }

        private void ReadTagInfo(int num, XmlNode parentNode)
        {
            var urlStr = tbURL.Text.Replace("<num>", num.ToString());
            var req = WebRequest.Create(urlStr);
            //req.Proxy = WebProxy.GetDefaultProxy();

            var stream = req.GetResponse().GetResponseStream();
            var sr = new StreamReader(stream);
            var data = sr.ReadToEnd();

            var tag = new FixTag(data, num.ToString(), urlStr);
            tag.SaveInXML(parentNode);
        }

        private void ReadMsgType(string name, XmlNode parentNode)
        {
            var nameCode = "";
            foreach (var c in name)
            {
                if (c <= '9') nameCode = nameCode + c;
                else
                {
                    int num = c;
                    nameCode = nameCode + num;
                }
            }

            var urlStr = tbURLMsgType.Text.Replace("<tag>", name);
            urlStr = urlStr.Replace("<tagcode>", nameCode);

            var req = WebRequest.Create(urlStr);
            var stream = req.GetResponse().GetResponseStream();
            var sr = new StreamReader(stream);
            var data = sr.ReadToEnd();

            var tag = new FixTag(data, name, urlStr);
            tag.SaveInXML(parentNode);            
        }
    }

    public static class Dictionary
    {
        private static string dicPath;

        public static string DicPath
        {
            get
            {
                if (String.IsNullOrEmpty(dicPath))
                {
                    dicPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    dicPath = string.Format("{0}\\dictionary.xml", dicPath);
                }
                return dicPath;
            }
        }
    }

    public class FixTag
    {
        public const string PARENT_TAG = "Tags";
        public const string PARENT_MSG = "Messages";
        
        
        private const string PREFFIX_NAME = "<h2>FIX 4.4 : ";
        private const string SUFFIX_NAME = "</h2>";

        private const string PREFFIX_DESCRIPTION = "<h3>";
        private const string SUFFIX_DESCRIPTION = "<h3>";

        public string Num { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }

        public FixTag()
        {
            
        }

        public FixTag(string data, string num, string url)
        {
            Title = NormalizeHTMLString(GetSubstringBetweenParts(data, PREFFIX_NAME, SUFFIX_NAME));
            Description = NormalizeHTMLString(GetSubstringBetweenParts(
                data, PREFFIX_DESCRIPTION, SUFFIX_DESCRIPTION));
            URL = url;
            Num = num;
        }

        public void SaveInXML(XmlNode parent)
        {
            var node = parent.OwnerDocument.CreateElement("Tag");
            parent.AppendChild(node);
            
            var atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("Title"));
            atr.Value = Title;
            atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("Description"));
            atr.Value = Description;
            atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("URL"));
            atr.Value = URL;
            atr = node.Attributes.Append(parent.OwnerDocument.CreateAttribute("Num"));
            atr.Value = Num;  
        }

        public static Dictionary<string, FixTag> ReadTags(XmlNode parentNode)
        {
            var dic = new Dictionary<string, FixTag>();
            foreach (XmlElement node in parentNode.ChildNodes)
            {
                var tag = new FixTag();
                if (node.Attributes["Title"] != null)
                    tag.Title = node.Attributes["Title"].Value;
                if (node.Attributes["Description"] != null)
                    tag.Description = node.Attributes["Description"].Value;
                if (node.Attributes["URL"] != null)
                    tag.URL = node.Attributes["URL"].Value;
                if (node.Attributes["Num"] != null)
                    tag.Num = node.Attributes["Num"].Value;
                dic.Add(tag.Num, tag);
            }
            return dic;
        }

        public static string GetSubstringBetweenParts(string data, string preffix, string suffix)
        {
            int start = data.IndexOf(preffix);
            if (start < 0) return "";
            var end = data.IndexOf(suffix, start + 1);
            if (end < 0) end = data.Length;

            return data.Substring(start, end - start);
        }

        public static string NormalizeHTMLString(string str)
        {
            str = str.Replace("&lt;", "<");
            str = str.Replace("&gt;", ">");

            while (true)
            {
                int start = str.IndexOf("<");
                if (start < 0) break;
                int end = str.IndexOf(">", start + 1);
                if (end < 0) end = str.Length - 1;
                str = str.Remove(start, end - start + 1);
            }

            return str;
        }
    }    
}
