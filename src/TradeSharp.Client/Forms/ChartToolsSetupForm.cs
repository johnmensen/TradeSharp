using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Candlechart;
using Candlechart.Series;
using Entity;
using TradeSharp.Client.BL;
using TradeSharp.Client.Controls;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class ChartToolsSetupForm : Form
    {
        public enum ChartToolsFormTab
        {
            Buttons = 0,
            SysButtons = 1,
            Series = 2,
            Scripts = 3
        }

        private readonly Dictionary<Control, string> helpIndexes;

        public ChartToolsSetupForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            helpIndexes = new Dictionary<Control, string>
            {
                { tabPageButtons, HelpManager.ToolPanel },
                { tabPageSeriesSettings, HelpManager.ToolPanel }
            };
        }

        public ChartToolsSetupForm(ImageList lstIcons, ChartToolsFormTab tab = ChartToolsFormTab.Buttons) : this()
        {
            chartButtonSettingsPanel.SetIcons(lstIcons);
            systemButtonSettingsPanel.SetIcons(lstIcons);
            treeSeries.ImageList = lstIcons;
            SetupScriptsGrid();

            if (tab == ChartToolsFormTab.SysButtons)
                tabControlMain.SelectedTab = tabPageSystemButtons;
            else
                if (tab == ChartToolsFormTab.Series)
                    tabControlMain.SelectedTab = tabPageSeriesSettings;
            else
                if (tab == ChartToolsFormTab.Scripts)
                    tabControlMain.SelectedTab = tabPageScripts;
        }

        private void ChartToolsSetupFormLoad(object sender, EventArgs e)
        {
            // заполнить дерево с настройками серий
            LoadSeriesList();
            // заполнить список кнопок графика
            var groups =
                ChartToolButtonStorage.Instance.groups.Where(
                    g => g.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart).ToList();
            chartButtonSettingsPanel.SetButtonsAndGroups(ChartToolButtonStorage.Instance.selButtons,
                                                         groups, ChartToolButtonSettings.ToolButtonType.Chart);
            // заполнить список системных кнопок
            groups =
                ChartToolButtonStorage.Instance.groups.Where(
                    g => g.ButtonType == ChartToolButtonSettings.ToolButtonType.System).ToList();
            systemButtonSettingsPanel.SetButtonsAndGroups(ChartToolButtonStorage.Instance.selSystemButtons,
                                                          groups, ChartToolButtonSettings.ToolButtonType.System);
            // заполнить таблицу скриптов
            LoadScripts();
        }

        private void BtnUseDefaultClick(object sender, EventArgs e)
        {
            ChartToolButtonStorage.Instance.RestoreDefaults();
            // заполнить список кнопок графика
            var groups =
                ChartToolButtonStorage.Instance.groups.Where(
                    g => g.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart).ToList();
            chartButtonSettingsPanel.SetButtonsAndGroups(ChartToolButtonStorage.Instance.selButtons,
                                                         groups, ChartToolButtonSettings.ToolButtonType.Chart);
            // заполнить список системных кнопок
            groups =
                ChartToolButtonStorage.Instance.groups.Where(
                    g => g.ButtonType == ChartToolButtonSettings.ToolButtonType.System).ToList();
            systemButtonSettingsPanel.SetButtonsAndGroups(ChartToolButtonStorage.Instance.selSystemButtons,
                                                          groups, ChartToolButtonSettings.ToolButtonType.System);
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            var groups = chartButtonSettingsPanel.GetGroups();
            groups.AddRange(systemButtonSettingsPanel.GetGroups());
            ChartToolButtonStorage.Instance.groups = groups;
            ChartToolButtonStorage.Instance.selButtons = chartButtonSettingsPanel.GetButtons();
            ChartToolButtonStorage.Instance.selSystemButtons = systemButtonSettingsPanel.GetButtons();
            ChartToolButtonStorage.Instance.SaveSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        #region Series
        private void LoadSeriesList()
        {
            treeSeries.Nodes.Clear();

            foreach (var sr in InteractiveObjectSeries.GetObjectSeriesTypes())
            {
                var attrs = sr.GetCustomAttributes(typeof (SeriesToolButtonAttribute), false);
                var seriesName = attrs.Length == 1
                                     ? ((SeriesToolButtonAttribute) attrs[0]).Title
                                     : sr.Name;
                var imageIndex = attrs.Length == 1
                                     ? (int)((SeriesToolButtonAttribute) attrs[0]).ImageIndex
                                     : -1;
                var node = treeSeries.Nodes.Add(seriesName, seriesName, imageIndex, imageIndex);
                node.Tag = sr;
            }
        }
        
        private void ShowSeriesSettings(Type seriesType)
        {
            // очистить
            panelSeriesParams.Controls.Clear();
            if (seriesType == null) return;

            // добавить редакторы
            const int editorHeight = 24;
            foreach (var prop in seriesType.GetProperties(BindingFlags.Static | BindingFlags.Public))
            {
                if (prop.GetCustomAttributes(typeof(PropertyXMLTagAttribute), true).Length == 0) continue;
                // имя параметра
                var paramName = prop.Name;
                var displayAttrs = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                if (displayAttrs.Length == 1)
                    paramName = ((DisplayNameAttribute) displayAttrs[0]).DisplayName;

                // редактор
                var editor = new GenericObjectEditor(paramName, prop.PropertyType, prop.GetValue(null, null))
                                 {Tag = prop};
                editor.SetBounds(0, 0, 100, editorHeight);
                editor.Dock = DockStyle.Top;
                panelSeriesParams.Controls.Add(editor);
            }
        }

        private void TreeSeriesAfterSelect(object sender, TreeViewEventArgs e)
        {
            Type typeSr = null;
            if (e.Node != null)
                typeSr = (Type)e.Node.Tag;
            ShowSeriesSettings(typeSr);
        }
        #endregion

        private void BtnUpdateSeriesClick(object sender, EventArgs e)
        {
            if (panelSeriesParams.Controls.Count == 0) return;
            var node = treeSeries.SelectedNode;
            if (node == null) return;
            
            // изменить настройки для серии
            foreach (GenericObjectEditor ctrl in panelSeriesParams.Controls)
            {
                var propInf = (PropertyInfo) ctrl.Tag;
                try
                {
                    propInf.SetValue(null, ctrl.PropertyValue, null);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Update series value ({0}) error: {1}",
                                       ctrl.Title, ex);
                    MessageBox.Show(string.Format(Localizer.GetString("MessageValueNotRecognizedFmt"), ctrl.Title));
                }                
            }

            // сохранить настройки
            var nodeXml = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameSeries);
            CandleChartControl.SaveSeriesSettingsInXml(nodeXml);
            ToolSettingsStorageFile.SaveXml(nodeXml.OwnerDocument);
        }

        private void ChartToolsSetupFormHelpRequested(object sender, HelpEventArgs hlpevent)
        {
            if (helpIndexes.ContainsKey(tabControlMain.SelectedTab))
            {
                var index = helpIndexes[tabControlMain.SelectedTab];
                HelpManager.Instance.ShowHelp(index);
                hlpevent.Handled = true;
                return;
            }

            HelpManager.Instance.ShowHelp(null);
            hlpevent.Handled = true;
        }        
    }
}
