using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ini;
using OnixAnalyzer.BL;

namespace OnixAnalyzer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            SaveSettings();
            var accounts = GetAccounts();
            foreach (var acNum in accounts)
            {
                LoadAccountDeals(acNum);
            }
            MessageBox.Show("Готово");
        }

        private void LoadAccountDeals(int account)
        {
            var url = string.Format("http://www.onix-trade.net/?act=monitoring_history&xid={0}", account);

            var acInf = new AccountInfo { Id = account };

            // запросить данные с сервера            
            string pageData = LoadPageData(url);
            if (!IsHistoryPage(pageData)) return;

            // количество страниц
            var pagesTotalReg = new Regex("Total: \\d+");
            var matches = pagesTotalReg.Matches(pageData);
            if (matches.Count == 0) return;
            var pagesCount = int.Parse(matches[0].Value.Substring(7));
            
            // загрузка сделок            
            LoadPageDeals(pageData, acInf);
            LoadAccontDetail(pageData, acInf);
            
            // и по страницам
            for (var i = 1; i < pagesCount; i++)
            {
                try
                {
                    LoadPageDeals(LoadPageData(string.Format("{0}&p={1}", url, i)), acInf);
                } catch { }
            }

            acInf.SaveInFile(string.Format("{0}\\trader_{1}.xml", tbFolder.Text, account));
        }

        private static string LoadPageData(string url)
        {
            string pageData;
            var req = WebRequest.Create(url);
            using (var stream = req.GetResponse().GetResponseStream())
            {
                var sr = new StreamReader(stream);
                pageData = sr.ReadToEnd();
            }
            return pageData;
        }

        private static void LoadAccontDetail(string pageData, AccountInfo ac)
        {
           // <td valign="top" width="20%">
           //         <DIV noWrap><a href="/?act=portfolio&xid=616"><B>BoostErr</B></a></DIV>
           //         <DIV noWrap><FONT color=#006600><a href="/?act=broker&id=114"><FONT color=#006600>Alpari-Micro</FONT></a></FONT></DIV>
           // <DIV noWrap><FONT color=#6a5acd><B><span style="color:red">Real</span></B> | USD</FONT></DIV>
           //         <DIV noWrap><B>204677</B></DIV>
           //         <DIV noWrap><B>*****</B></DIV>
           //         <DIV noWrap><B>08/10/2007</B>. &nbsp;days: <B>863</B></DIV>
           //         <DIV noWrap><B>08/10/2007</B>. &nbsp;days: <B>863</B></DIV>
           //         <DIV noWrap><B>16/02/2010</B>. &nbsp;days: <B>1</B></DIV>
           //         <DIV noWrap><FONT color=#339966><B>300.00</B></FONT></DIV>
           //         <DIV noWrap><B>1055</B></DIV>
           //</td>

            //<a href="/\?act=portfolio&xid=\d+"><B>
            //var trader = GetStringBetween("<a href=\"/\\?act=portfolio&xid=\\d+\"><B>", "</B>", pageData);
            //var broker = GetStringBetween("<a href=\"/\\?act=broker&id=\\d+\"><FONT color=#\\d+>", "</FONT>", pageData);
            //var login = GetStringBetween("<DIV noWrap><B>", "</B>", pageData);            
            var depo = GetStringBetween("<DIV noWrap><FONT color=#339966><B>", "</B>", pageData);
            ac.InitBalance = DealInfo.ParseDecimalSafe(depo);                            
        }

        private static int GetPatternStart(string regex, string data)
        {
            var brokerRegex = new Regex(regex);
            var matches = brokerRegex.Matches(data);
            if (matches.Count > 0)
            {
                return matches[0].Index + regex.Length;
            }
            return -1;
        }

        private static string GetStringBetween(string regexStart, string endTag, string data)
        {
            var start = GetPatternStart(regexStart, data);
            if (start < 0) return "";
            var end = data.IndexOf(endTag, start);
            return data.Substring(start, end - start);
        }

        private static bool IsHistoryPage(string pageData)
        {
            return pageData.Contains("'>History</a>");
        }
        
        private static void LoadPageDeals(string pageData, AccountInfo ac)
        {            
            // найти вхождения строк таблицы
            // содержит таблицу вида
            //
            //Ticket OpenTime Type Lots Symbol Open SL TP CloseTime Close Swap Profit Comment 
            //32344939 2009-04-03 22:00:00 sell 0.10 EURUSD 1.3482 0.0000 1.3467 2009-04-03 22:07:00 1.3467 0.00 15.00 (51) 
            //
            // HTML:
            //<tr bgcolor=#F0F0F0 style='padding: 3 5 3 5'>
            //    <td align='right' nowrap><font style='font-size: 5pt'>32344939</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>2009-04-03 22:00:00</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>sell</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>0.10</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>EURUSD</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>1.3482</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>0.0000</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>1.3467</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>2009-04-03 22:07:00</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>1.3467</font></td>
            //    <!--<td align='right' nowrap><font style='font-size: 5pt'>0.00</font></td>-->
            //    <td align='right' nowrap><font style='font-size: 5pt'>0.00</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'>15.00</font></td>
            //    <td align='right' nowrap><font style='font-size: 5pt'> (51)</font></td>
            //</tr>

            // либо

            //<tr bgcolor=#FFFFFF style='padding: 3 5 3 5'>
            //<td align='right' nowrap><font style='font-size: 5pt'>2597784</font></td>
            //<td align='right' nowrap><font style='font-size: 5pt'>2010-01-12 02:14:00</font></td>
            //<td align='right' nowrap><font style='font-size: 5pt'>balance</font></td>
            //<td colspan='8'></td>
            //<td align='right' nowrap><font style='font-size: 5pt'>-4 050.00</font></td>
            //<td align='right' nowrap><font style='font-size: 5pt'> (98)</font></td>
            //</tr>

            var reg = new Regex("<tr bgcolor=#[A-Z,0-9]+ style='padding: 3 5 3 5'>");
            foreach (Match m in reg.Matches(pageData))
            {
                var startIndex = m.Index + m.Value.Length;
                var endIndex = pageData.IndexOf("</tr>", startIndex);
                var obj = ParseDeal(pageData.Substring(startIndex, endIndex - startIndex));
                if (obj != null)
                {
                    if (obj is DealInfo) ac.Deals.Add((DealInfo)obj);
                    if (obj is BalanceInfo) ac.Balances.Add((BalanceInfo)obj);
                }
            }
        }

        private static object ParseDeal(string tableRowStr)
        {
            var cells = new List<string>();
            var nextStartIndex = 0;
            while (true)
            {
                var startIndex = tableRowStr.IndexOf("pt'>", nextStartIndex);
                if (startIndex < 0) break;
                var endIndex = tableRowStr.IndexOf("</font>", startIndex);
                cells.Add(tableRowStr.Substring(startIndex + 4, endIndex - startIndex - 4));
                nextStartIndex = endIndex;
            }
            if (cells.Count == 14)
                return new DealInfo(cells);
            if (cells.Count == 5)
                return new BalanceInfo(cells);
            return null;
        }

        private List<int> GetAccounts()
        {
            var numbers = new List<int>();
            var reg = new Regex("\\d+");
            foreach (Match match in reg.Matches(tbNumbers.Text))
            {
                numbers.Add(int.Parse(match.Value));
            }
            return numbers;
        }

        private void btnLoadAccounts_Click(object sender, EventArgs e)
        {
            tbNumbers.Text = "";
            var numbersStr = "";

            string html;
            using (var sr = new StreamReader(tbTradersHTML.Text))
            {
                html = sr.ReadToEnd();
            }                
            // <a href="/?act=monitoring_stat&xid=538"><font color="#003399" size=2><b>SIG_Lite:</b></font></a>
            const string pattern = "<a href=\"/?act=monitoring_stat&xid=";
            
            var startIndex = 0;
            while (true)
            {
                var index = html.IndexOf(pattern, startIndex);
                if (index < 0) break;
                var endIndex = html.IndexOf("\"", index + pattern.Length);
                startIndex = endIndex;

                var idStr = html.Substring(index + pattern.Length, endIndex - index - pattern.Length);
                numbersStr += string.Format("{0} ", idStr);
            }
            tbNumbers.Text = numbersStr;
        }

        private void btnStat_Click(object sender, EventArgs e)
        {
            SaveSettings();
            var form = new ReportForm {PathToSave = tbFolder.Text};
            form.ShowDialog();
        }

        private void SaveSettings()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var saveFileName = string.Format("settings.txt", appPath);
            var iniFile = new IniFile(saveFileName);
            iniFile.IniWriteValue("Paths", "Saves", tbFolder.Text);
        }

        private void LoadSettings()
        {
            var appPath = Assembly.GetExecutingAssembly().Location;
            var saveFileName = string.Format("settings.txt", appPath);
            var iniFile = new IniFile(saveFileName);
            tbFolder.Text = iniFile.IniReadValue("Paths", "Saves");
        }
    }        
}
