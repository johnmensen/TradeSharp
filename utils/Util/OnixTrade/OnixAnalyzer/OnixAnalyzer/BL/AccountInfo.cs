using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace OnixAnalyzer.BL
{
    class AccountInfo
    {
        public int Id { get; set; }

        public string Broker { get; set; }

        public string Account { get; set; }

        public string Trader { get; set; }

        public string Login { get; set; }

        public decimal InitBalance { get; set; }

        private readonly List<DealInfo> deals = new List<DealInfo>();

        public List<DealInfo> Deals
        {
            get { return deals; }
        }

        private readonly List<BalanceInfo> balances = new List<BalanceInfo>();

        public List<BalanceInfo> Balances
        {
            get { return balances; }
        }

        public XmlDocument SaveInXml()
        {
            var doc = new XmlDocument();
            var root = doc.AppendChild(doc.CreateElement("account"));
            DealInfo.AddAttribute(root, "Id", Id);
            DealInfo.AddAttribute(root, "Broker", Broker);
            DealInfo.AddAttribute(root, "Account", Account);
            DealInfo.AddAttribute(root, "Trader", Trader);
            DealInfo.AddAttribute(root, "Login", Login);
            DealInfo.AddAttribute(root, "InitBalance", InitBalance);

            var rootDeal = root.AppendChild(doc.CreateElement("deals"));
            foreach (var deal in deals) deal.SaveInXml(rootDeal);

            var rootBalance = root.AppendChild(doc.CreateElement("balances"));
            foreach (var balance in balances) balance.SaveInXml(rootBalance);

            return doc;
        }

        public void LoadFromXml(XmlDocument xmlDoc)
        {
            if (xmlDoc == null) return;
            var root = xmlDoc.DocumentElement;
            if (root == null) return;
            
            Id = int.Parse(root.Attributes["Id"].Value);
            Broker = root.Attributes["Broker"].Value;
            Account = root.Attributes["Account"].Value;
            Login = root.Attributes["Login"].Value;
            Trader = root.Attributes["Trader"].Value;
            InitBalance = decimal.Parse(root.Attributes["InitBalance"].Value, CultureInfo.InvariantCulture);

            var rootDeal = root.ChildNodes[0].Name == "deals" ? root.ChildNodes[0] : root.ChildNodes[1];
            var rootBalance = root.ChildNodes[0].Name == "balances" ? root.ChildNodes[0] : root.ChildNodes[1];

            foreach (XmlNode node in rootDeal)
            {
                deals.Add(new DealInfo(node));
            }

            foreach (XmlNode node in rootBalance)
            {
                balances.Add(new BalanceInfo(node));
            }
        }

        public void LoadFromFile(string fileName)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);
            LoadFromXml(doc);
        }

        public void SaveInFile(string path)
        {
            var doc = SaveInXml();
            using (var writer = new XmlTextWriter(path, Encoding.UTF8) { Indentation = 4, IndentChar = ' ', Formatting = Formatting.Indented })
            {
                doc.Save(writer);
            }
        }
    }
}
