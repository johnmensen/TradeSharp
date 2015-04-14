using System.Windows.Forms;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Control
{
    public partial class CompleteSubscribeOnPortfolioControl : UserControl
    {
        private TopPortfolio portfolio;

        public CompleteSubscribeOnPortfolioControl()
        {
            InitializeComponent();
        }

        public void SetupPortfolio(TopPortfolio portfolio)
        {
            this.portfolio = portfolio;

            lblPortfolioName.Text =
                (portfolio.IsCompanyPortfolio
                    ? (Localizer.GetString("TitlePortfolio") + " \"" + portfolio.Name + "\"")
                    : (Localizer.GetString("TitleUserPortfolio")) +
                " [" + portfolio.ParticipantCount + "]");

            // сформировать формулу с подсветкой
            formulaBrowser.DocumentText = PerformerStatField.GetFormulaHighlightedHtml(portfolio.Criteria);

            tbCount.Value = portfolio.ParticipantCount;
            if (portfolio.IsCompanyPortfolio)
                tbCount.Enabled = false;
        }

        private void btnOk_Click(object sender, System.EventArgs e)
        {
            ((Form)Parent).DialogResult = DialogResult.OK;
        }
    }
}
