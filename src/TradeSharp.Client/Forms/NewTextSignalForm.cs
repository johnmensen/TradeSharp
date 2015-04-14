using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class NewTextSignalForm : Form
    {
        private readonly CandleChartControl chart;
        private readonly List<PaidService> signals;
        /// <summary>
        /// имя - шаблон сообщения
        /// </summary>
        private readonly Dictionary<string, string> messageTemplate = new Dictionary<string, string>();

        /// <summary>
        /// текст был отредактирован с момента открытия окна / выбора шаблона
        /// </summary>
        private volatile bool textWasUpdated;

        private bool doNotProcessEnterKey;

        public NewTextSignalForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public NewTextSignalForm(CandleChartControl sender, List<PaidService> signals) : this()
        {
            chart = sender;
            this.signals = signals;
            InitInterface();
        }

        private void InitInterface()
        {
            // категория
            foreach (var signal in signals)
                cbSignalCat.Items.Add(signal);
            cbSignalCat.SelectedIndex = 0;

            // таймфреймы
            cbTimeframe.Items.Add("-");
            int selIndex = 0, counter = 0;
            foreach (var timeframe in BarSettingsStorage.Instance.GetCollection())
            {
                counter++;
                if (timeframe == chart.Timeframe) selIndex = counter;
                cbTimeframe.Items.Add(BarSettingsStorage.Instance.GetBarSettingsFriendlyName(timeframe));
            }
            cbTimeframe.SelectedIndex = selIndex;

            // тикеры
            cbTicker.Items.Add("-");
            selIndex = 0;
            counter = 0;
            foreach (var ticker in DalSpot.Instance.GetTickerNames())
            {
                counter++;
                if (ticker == chart.Symbol) selIndex = counter;
                cbTicker.Items.Add(ticker);
            }
            cbTicker.SelectedIndex = selIndex;

            // шаблоны
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            cbTemplate.Items.Clear();
            cbTemplate.Items.Add("");
            cbTemplate.SelectedIndex = 0;

            messageTemplate.Clear();
            var node = ToolSettingsStorageFile.LoadNode(
                ToolSettingsStorageFile.NodeNameMessageTemplates);
            if (node == null || node.ChildNodes.Count == 0) return;
            foreach (XmlElement child in node.ChildNodes)
            {
                var templateName = child.Attributes["name"].Value;
                var templateText = child.InnerText;
                messageTemplate.Add(templateName, templateText);
            }

            // добавить в комбо
            foreach (var name in messageTemplate.Keys)
                cbTemplate.Items.Add(name);
        }

        private void BtnSaveTemplateClick(object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(tbMessage.Text)) return;
            // предложить сохранить / перезаписать шаблон
            object selObj;
            string selText;
            // имя для шаблона
            var choices = messageTemplate.Keys.Count == 0
                              ? new List<object> { "" }
                              : messageTemplate.Keys.Cast<object>().ToList();

            if (!Dialogs.ShowComboDialog(Localizer.GetString("MessageSaveTemplateAs"),
                choices, out selObj, out selText, false)) return;
            if (string.IsNullOrEmpty(selText)) return;

            if (messageTemplate.ContainsKey(selText))
            {
                if (MessageBox.Show(
                    Localizer.GetString("MessageRewriteTemplate") + "\"" + selText + "\"?",
                    Localizer.GetString("TitleConfirmation"), 
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No) return;
                messageTemplate[selText] = tbMessage.Text;
            }
            else messageTemplate.Add(selText, tbMessage.Text);

            // сохранить шаблоны    
            SaveTemplates();

            // показать в списке
            LoadTemplates();
        }

        private void SaveTemplates()
        {
            // получить узел шаблонов
            var nodeTemplate =
                ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameMessageTemplates);
            // и почистить его
            while (nodeTemplate.HasChildNodes)
                nodeTemplate.RemoveChild(nodeTemplate.FirstChild);
            // сохранить шаблоны
            foreach (var templ in messageTemplate)
            {
                var templNode = nodeTemplate.AppendChild(nodeTemplate.OwnerDocument.CreateElement("template"));
                templNode.Attributes.Append(nodeTemplate.OwnerDocument.CreateAttribute("name")).Value = templ.Key;
                templNode.InnerText = templ.Value;
            }
            // сохранить документ
            ToolSettingsStorageFile.SaveXml(nodeTemplate.OwnerDocument);
        }

        private void CbTemplateSelectedIndexChanged(object sender, System.EventArgs e)
        {
            var templateName = (string)cbTemplate.SelectedItem;
            if (string.IsNullOrEmpty(templateName)) return;

            // вписать в окно текст шаблона
            if (textWasUpdated)
                if (MessageBox.Show(
                    Localizer.GetString("MessageApplyTemplate") + " \"" + templateName + "\"?",
                    Localizer.GetString("TitleConfirmation"), 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

            tbMessage.Text = messageTemplate[templateName];
            textWasUpdated = false;
        }

        private void TbMessageTextChanged(object sender, EventArgs e)
        {
            textWasUpdated = true;
        }

        /// <summary>
        /// отправить сообщение адресатам
        /// </summary>        
        private void BtnSendClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbMessage.Text)) return;
            var msg = tbMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            var cat = (PaidService)cbSignalCat.SelectedItem;
            BarSettings timeframe = null;
            var timeframeStr = cbTimeframe.Text;
            if (!string.IsNullOrEmpty(timeframeStr))
                timeframe = BarSettingsStorage.Instance.GetBarSettingsByName(timeframeStr);

            var rst = MainForm.Instance.PutTextMessage(cat.Id, msg, cbTicker.Text, timeframe);
            if (rst)
            {
                MainForm.Instance.AddMessageToStatusPanelSafe(
                    DateTime.Now, 
                    string.Format(Localizer.GetString("MessageTextSignalPublishedFmt"), 
                        cat.Comment, msg.Length > 40 ? msg.Substring(0, 40) : msg));
                Close();
                return;
            }
            MessageBox.Show(
                Localizer.GetString("MessageCannotSendTextSignal"), 
                Localizer.GetString("TitleError"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void TbMessageKeyDown(object sender, KeyEventArgs e)
        {
            if (((ModifierKeys & Keys.Control) != Keys.Control))
                if (e.KeyCode == Keys.Enter)
                {
                    BtnSendClick(sender, EventArgs.Empty);
                    doNotProcessEnterKey = true;
                }
        }

        private void TbMessageKeyPress(object sender, KeyPressEventArgs e)
        {
            if (doNotProcessEnterKey)
            {
                doNotProcessEnterKey = false;
                tbMessage.Text = string.Empty;
            }
        }
    }
}
