using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    public partial class MainForm
    {
        private void MenuSaveTemplateClick(object sender, EventArgs e)
        {
            var currentChartWindows = ActiveMdiChild as ChartForm;
            if (currentChartWindows == null) return;
            var saveChartTemplate = new SaveChartTemplate(currentChartWindows.chart);
            saveChartTemplate.ShowDialog();
        }

        /// <summary>
        /// Обработчик события открывает форму выбора шаблона для применения и применяет выбранный шаблон, если пользователь нажал "Ok"
        /// </summary>
        private void MenuLoadTemplateClick(object sender, EventArgs e)
        {
            var currentChartWindows = ActiveMdiChild as ChartForm;
            if (currentChartWindows == null) return;

            var applyChartTemplate = new ApplyChartTemplate(currentChartWindows.chart);
            if (applyChartTemplate.ShowDialog() != DialogResult.OK) return;

            // Удаляем все индикаторы, которые сейчас есть на графике
            while (currentChartWindows.chart.indicators.Count > 0) currentChartWindows.chart.RemoveIndicator(currentChartWindows.chart.indicators[0]);

            var applyTemplateXml = ChartTemplate.GetChartTemplate(currentChartWindows.chart.CurrentTemplateName);
            if (applyTemplateXml != null && applyTemplateXml.Attributes != null)
            {
                var sz = applyTemplateXml.GetAttributeSize(ChartTemplate.AttributeChartSize);
                if (sz.HasValue) currentChartWindows.Size = sz.Value;

                currentChartWindows.chart.CurrentTemplateName =
                    applyTemplateXml.GetAttributeString(ChartTemplate.AttributeName,
                                                        currentChartWindows.chart.CurrentTemplateName);
                currentChartWindows.chart.LoadIndicatorSettings(applyTemplateXml);
            }
            currentChartWindows.chart.UpdateIndicatorPanesAndSeries();
            currentChartWindows.chart.BuildIndicators(true);
        }

        /// <summary>
        /// Получение шаблона из toolset.xml и применение его к графику
        /// </summary>
        /// <param name="templateName">имя щаблона, который нужно применить</param>
        /// <param name="child">форма, на которой распологается график</param>
        private static void ApplyTemplate(string templateName, ChartForm child)
        {
            var childTemplate = ChartTemplate.GetChartTemplate(templateName);
            if (childTemplate == null) return;
            if (childTemplate.Attributes[ChartTemplate.AttributeChartSize] != null) 
            child.Size = ChartTemplate.StringToSize(childTemplate.Attributes[ChartTemplate.AttributeChartSize].Value).Value;
            
            child.chart.LoadIndicatorSettings(childTemplate);
            child.chart.UpdateIndicatorPanesAndSeries();
        }
    }
}
