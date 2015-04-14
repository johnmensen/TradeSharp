using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart;
using Candlechart.Indicator;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class SaveChartTemplate : Form
    {       
        private readonly CandleChartControl chart;

        public SaveChartTemplate(CandleChartControl chart)
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            this.chart = chart;
        }

        private void SaveChartTemplateLoad(object sender, EventArgs e)
        {
            txtCurrentTickerValue.Text = chart.Symbol;
            cbTemplateName.Items.Clear();
            cbTemplateName.Items.AddRange(ChartTemplate.GetChartTemplateNames(chart.Symbol));
            if (!string.IsNullOrEmpty(chart.CurrentTemplateName))
            {
                chbxBindCurrencyTicket.Checked = ChartTemplate.IsUniversalTemplate(chart.CurrentTemplateName);
                cbTemplateName.SelectedItem =
                    cbTemplateName.Items.Cast<string>().FirstOrDefault(x => x == chart.CurrentTemplateName);
            }
        }

        /// <summary>
        /// Обработчик кликов по checkbox-ам выбора сохраняемых в шаблоне объектов
        /// </summary>
        /// <param name="sender">какой либо выбранный checkbox</param>
        private void ChebxSaveCheckedChanged(object sender, EventArgs e)
        {
            var checkbox = sender as CheckBox;
            if (checkbox == null) return;

            btnSave.Enabled = chebxSaveIndicators.Checked || chbxSaveChartSettings.Checked;
        }

        /// <summary>
        /// Сохранение навого шаблона
        /// </summary>
        private void BtnSaveClick(object sender, EventArgs e)
        {
            var newTemplateName = cbTemplateName.Text.Trim();

            if (string.IsNullOrEmpty(newTemplateName))
            {
                MessageBox.Show(
                    Localizer.GetString("MessageTemplateNameMustBeNotEmpty"),
                    Localizer.GetString("TitleError"));
            }
            else
            {
                var rootNodeTemplates = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameChartTemplates, false);
                var doc = rootNodeTemplates.OwnerDocument;

                if (doc != null)
                {
                    XmlElement templateNode;
                    if (!ChartTemplate.GetChartTemplateNames().Contains(newTemplateName))
                        //Если узла "chartTemplate" в документе с таким именем ещё не существует, то создаём новый узел с таким именем
                        templateNode =(XmlElement)rootNodeTemplates.AppendChild(doc.CreateElement(ToolSettingsStorageFile.NodeNameChartTemplate));
                    else
                    {
                        //Если узл "chartTemplate" в документе с таким именем уже существует, то спрашиваем у пользователя, нужноли перезаписать в этот узел новый шаблон
                        if (
                            MessageBox.Show(
                            Localizer.GetString("MessageSuchNamedTemplateExistsOverwrite"), 
                            Localizer.GetString("TitleConfirmation"), 
                            MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            // если перезаписать, то находим по имени шаблона, описывающий его XmlElement
                            templateNode = (XmlElement) rootNodeTemplates.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.Attributes != null &&
                                                                                     node.Attributes[ChartTemplate.AttributeName].Value != null &&
                                                                                     node.Attributes[ChartTemplate.AttributeName].Value == newTemplateName);
                            // и удаляем  из него  все дочерние узлы (они содержат описания индикаторов) и атрибуты
                            if (templateNode != null) templateNode.RemoveAll();
                        }
                        else return;
                    }
                    SaveTemplate(templateNode, doc);
                }

            }                    
        }

        /// <summary>
        /// вспомогательный метод, нопосредствено сохраняющий/пересохраняющий шаблон
        /// </summary>
        /// <param name="templateNode">XmlElement представляющий описание шаблона</param>
        /// <param name="doc">XmlDocument, в который нужно записать орисание шаблона</param>
        private void SaveTemplate(XmlElement templateNode, XmlDocument doc)
        {
            if (templateNode != null && templateNode.OwnerDocument != null)
            {
                var xmlAttrTicker = templateNode.OwnerDocument.CreateAttribute(ChartTemplate.AttributeSymbol);
                xmlAttrTicker.Value = chbxBindCurrencyTicket.Checked
                                          ? txtCurrentTickerValue.Text
                                          : ChartTemplate.UniversalSymbol;
                templateNode.Attributes.Append(xmlAttrTicker);

                var xmlAttrChartType = templateNode.OwnerDocument.CreateAttribute(ChartTemplate.AttributeChartType);
                xmlAttrChartType.Value = chart.chart.ChartType.ToString();
                templateNode.Attributes.Append(xmlAttrChartType);

                var xmlAttrName = templateNode.OwnerDocument.CreateAttribute(ChartTemplate.AttributeName);
                xmlAttrName.Value = cbTemplateName.Text;
                templateNode.Attributes.Append(xmlAttrName);

                if (chbxSaveChartSettings.Checked)
                {
                    var xmlAttrSize =templateNode.OwnerDocument.CreateAttribute(ChartTemplate.AttributeChartSize);
                    xmlAttrSize.Value = string.Format("{0};{1}", chart.Parent.Size.Width,chart.Parent.Size.Height);
                    templateNode.Attributes.Append(xmlAttrSize);
                }

                if (chebxSaveIndicators.Checked)
                    foreach (var indcator in chart.indicators)
                    {
                        var indicatorNode =(XmlElement)templateNode.AppendChild(doc.CreateElement(ToolSettingsStorageFile.NodeNameChartTemplateIndicator));
                        BaseChartIndicator.MakeIndicatorXMLNode(indcator, indicatorNode);
                    }

                ToolSettingsStorageFile.SaveXml(doc);
            }
        }
    }
}