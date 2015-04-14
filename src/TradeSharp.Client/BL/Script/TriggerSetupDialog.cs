using System;
using System.Windows.Forms;
using Entity;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    public partial class TriggerSetupDialog : Form
    {
        private TerminalScriptTrigger trigger;

        public TerminalScriptTrigger Trigger
        {
            get { return trigger; }
        }

        public TriggerSetupDialog()
        {
            InitializeComponent();
        }

        public TriggerSetupDialog(TerminalScriptTrigger trigger) : this()
        {
            this.trigger = trigger;
            SetupUI();
        }

        private void SetupUI()
        {
            // заполнить значения подстановки
            cbQuote.DataSource = DalSpot.Instance.GetTickerNames(false);
            foreach (var ordEvent in Enum.GetValues(typeof (ScriptTriggerDealEventType)))
                cbOrder.Items.Add(ordEvent);
            rtbVariables.Text = string.Join(Environment.NewLine,
                ExpressionResolverLiveParams.paramNames);

            // заполнить данные триггера
            if (trigger == null) return;
            
            // вкладка Котировки
            if (trigger is ScriptTriggerNewQuote)
            {
                var trig = (ScriptTriggerNewQuote) trigger;

                tabControl.SelectedTab = tabPageQuote;

                if (trig.quotesToCheck != null)
                for (var i = 0; i < cbQuote.Items.Count; i++)
                {
                    var ticker = (string)cbQuote.Items[i];

                    if (trig.quotesToCheck.Any(q => q.Equals(ticker)))
                        cbQuote[i] = true;
                }
                return;
            }

            // ордера
            if (trigger is ScriptTriggerPriceFormula)
            {
                var trig = (ScriptTriggerPriceFormula)trigger;
                tabControl.SelectedTab = tabPageFormula;
                tbFormula.Text = trig.Formula;
                //return;
            }
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPageEmpty)
            {
                trigger = null;
                DialogResult = DialogResult.OK;
                return;
            }
            
            // котировки
            if (tabControl.SelectedTab == tabPageQuote)
            {
                trigger = new ScriptTriggerNewQuote
                    {
                        quotesToCheck = cbQuote.GetCheckedItems().Cast<string>().ToList()
                    };
                DialogResult = DialogResult.OK;
                return;
            }

            // ордера
            if (tabControl.SelectedTab == tabPageOrder)
            {
                trigger = new ScriptTriggerDealEvent();
                for (var i = 0; i < cbOrder.Items.Count; i++)
                {
                    if (!cbOrder[i]) continue;
                    var evType = (ScriptTriggerDealEventType) cbOrder.Items[i];
                    ((ScriptTriggerDealEvent) trigger).eventType |= evType;
                }
                
                DialogResult = DialogResult.OK;
                return;
            }

            // формула
            if (tabControl.SelectedTab == tabPageFormula)
            {
                var trigForm = new ScriptTriggerPriceFormula {Formula = tbFormula.Text};
                if (!string.IsNullOrEmpty(trigForm.FormulaError))
                {
                    MessageBox.Show("Ошибка в формуле: \n" + trigForm.FormulaError,
                                    "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                trigger = trigForm;
                DialogResult = DialogResult.OK;
            }
        }
    }
}
