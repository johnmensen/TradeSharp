using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Dialog
{
    public partial class TopPortfolioForm : Form
    {
        public bool IsSubscribed
        {
            get { return topPortfolioControl.IsSubsribed; }
            set { topPortfolioControl.IsSubsribed = value; }
        }

        public TopPortfolioForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public TopPortfolioForm(TopPortfolio portfolio) : this()
        {
            Text = portfolio.Name;
            topPortfolioControl.Portfolio = portfolio;
        }

        private void CloseButtonClick(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
