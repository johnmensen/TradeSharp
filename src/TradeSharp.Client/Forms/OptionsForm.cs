using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class OptionsForm : Form
    {
        private HelpProvider help;

        private readonly Dictionary<Control, string> helpIndexes;            

        public OptionsForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            SetupGrid();
            //SetupHelp();
            helpIndexes = new Dictionary<Control, string>
                {
                    { tabPageTrade, HelpManager.KeyTradeSettings },
                    { tabPageChartIcons, HelpManager.KeyFastButtons }
                };

            toolTipBrowserRegistration.SetToolTip(btnBrowserRegistration,
                                                  "Зарегистрировать в реестре операционной системы обозреватель (браузер) терминала TradeSharpe");
        }

        private void SetupHelp()
        {
            //help = new HelpProvider();
            //help.HelpNamespace = ExecutablePath.ExecPath + "\\terminal.chm";
            //help.SetHelpNavigator(tabPageChartIcons, HelpNavigator.Topic); // tabPageChartIcons
            //help.SetHelpKeyword(tabPageChartIcons, "pg_fastbuttons.htm");//HelpKeyword.KeyFastButtons);
        }

        private void SetupGrid()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            var blankAccountEventSettings = new AccountEventSettings();
            gridEvents.Columns.Add(new FastColumn(blankAccountEventSettings.Property(p => p.CodeName), Localizer.GetString("TitleEvent"))
                {
                    ColumnMinWidth = 60
                });
            gridEvents.Columns.Add(new FastColumn(blankAccountEventSettings.Property(p => p.ActionName), Localizer.GetString("TitleAction"))
                {
                    ColumnMinWidth = 60,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = fontBold,
                    HyperlinkActiveCursor = Cursors.Hand
                });

            foreach (var action in Enum.GetValues(typeof(AccountEventAction)).Cast<AccountEventAction>())
            {
                var item = contextMenuAction.Items.Add(EnumFriendlyName<AccountEventAction>.GetString(action));
                item.Tag = action;
                item.Click += ActionMenuItemClicked;
            }

            var blankBarSettings = new BarSettings();
            gridCandles.Columns.Add(new FastColumn(blankBarSettings.Property(p => p.Title), Localizer.GetString("TitleName")));
            gridCandles.Columns.Add(new FastColumn(blankBarSettings.Property(p => p.TimeDescription), Localizer.GetString("TitleIntervals")));

            var blankGridImageRecord = new GridImageRecord(string.Empty, null, null);
            gridTradeSettings.Columns.Add(new FastColumn(blankGridImageRecord.Property(p => p.Name), Localizer.GetString("TitleParameter"))
                {
                    SortOrder = FastColumnSort.Ascending,
                    ColumnMinWidth = 100
                });
            gridTradeSettings.Columns.Add(new FastColumn(blankGridImageRecord.Property(p => p.Name), "?")
            {
                ColumnWidth = 20,
                formatter = v => "?",
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                HyperlinkFontActive = fontBold,
                ColorHyperlinkTextActive = Color.Blue
            });
            gridTradeSettings.Columns.Add(new FastColumn(blankGridImageRecord.Property(p => p.ImageIndex), "*")
            {
                ColumnWidth = 23,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                ImageList = imageListSetsGrid
            });
            gridTradeSettings.UserHitCell += GridTradeSettingsOnUserHitCell;
            // заполнить таблицу настроек
            var sets = new List<GridImageRecord>
                {
                    new GridImageRecord(Localizer.GetString("MessageQuotesConfirmOrder"), 
                        val => { UserSettings.Instance.PromptTradeFromQuoteWindow = (bool) val; }, 
                        () => UserSettings.Instance.PromptTradeFromQuoteWindow)
                        {
                            BooleanValue = UserSettings.Instance.PromptTradeFromQuoteWindow
                        },
                    new GridImageRecord(Localizer.GetString("MessageQuotesConfirmOrder"), 
                        val => { UserSettings.Instance.PromptFastButtonTrade = (bool) val; }, 
                        () => UserSettings.Instance.PromptFastButtonTrade)
                        {
                            BooleanValue = UserSettings.Instance.PromptFastButtonTrade
                        },
                    new GridImageRecord(Localizer.GetString("MessageInstantOrderPriceCheck"), 
                        val => { UserSettings.Instance.CheckInstTradePrice = (bool) val; }, 
                        () => UserSettings.Instance.CheckInstTradePrice)
                        {
                            BooleanValue = UserSettings.Instance.CheckInstTradePrice
                        }
                };
            gridTradeSettings.CalcSetTableMinWidth();
            gridTradeSettings.DataBind(sets);
            gridTradeSettings.CheckSize();
        }

        private void GridTradeSettingsOnUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var record = (GridImageRecord) gridTradeSettings.rows[rowIndex].ValueObject;

            // показать раздел справки
            if (col.Title == "?")
            {
                HelpManager.Instance.ShowHelp(HelpManager.KeyTradeSettings);
                return;
            }

            // изменить соотв. настройку
            if (col.PropertyName == record.Property(p => p.ImageIndex))
            {
                if (record.BooleanValue.HasValue)
                {
                    record.BooleanValue = !record.BooleanValue.Value;
                    record.setValue(record.BooleanValue.Value);
                }
                gridTradeSettings.UpdateRow(rowIndex, record);
                gridTradeSettings.InvalidateRow(rowIndex);
            }
        }

        private void ApplyBtnClick(object sender, EventArgs e)
        {
            AcceptSettings();
        }

        private void AcceptSettings()
        {
            UserSettings.Instance.SynchGraphics = cbSynchGraphics.Checked;
            UserSettings.Instance.SynchGraphicsAnyPair = cbSyncAllSymbols.Checked;
            UserSettings.Instance.EnableExtendedThemes = cbEnableExtendedThemes.Checked;
            UserSettings.Instance.CloseChartPrompt = cbCloseConfirm.Checked;
            UserSettings.Instance.CloseTerminalPrompt = cbConfirmTerminalClosing.Checked;
            UserSettings.Instance.DeleteAccountEventsOnRead = cbDeleteEventsOnRead.Checked;
            UserSettings.Instance.HideToTray = cbHideInTray.Checked;
            UserSettings.Instance.GapRecordActualMinutes = (tbGapHistoryHours.Text.ToIntSafe() ?? 48) * 60;
            UserSettings.Instance.ConfirmGapFilling = cbConfirmGapFill.Checked;
            hotKeysTableControl.ApplySettings();
            UserSettings.Instance.SaveSettings();
            BarSettingsStorage.Instance.SaveSeriesSettings();
            chartIconSetupControl.AcceptSettings();
        }

        private void OkBtnClick(object sender, EventArgs e)
        {
            AcceptSettings();
            DialogResult = DialogResult.OK;            
        }

        private void CancelBtnClick(object sender, EventArgs e)
        {
            hotKeysTableControl.CancelChanges();
        }

        private void OptionsFormLoad(object sender, EventArgs e)
        {
            cbSynchGraphics.Checked = UserSettings.Instance.SynchGraphics;
            cbSyncAllSymbols.Checked = UserSettings.Instance.SynchGraphicsAnyPair;
            cbEnableExtendedThemes.Checked = UserSettings.Instance.EnableExtendedThemes;
            cbCloseConfirm.Checked = UserSettings.Instance.CloseChartPrompt;
            cbConfirmTerminalClosing.Checked = UserSettings.Instance.CloseTerminalPrompt;
            cbDeleteEventsOnRead.Checked = UserSettings.Instance.DeleteAccountEventsOnRead;
            cbHideInTray.Checked = UserSettings.Instance.HideToTray;
            tbGapHistoryHours.Text = ((int)Math.Round(UserSettings.Instance.GapRecordActualMinutes / 60.0)).ToString();
            cbConfirmGapFill.Checked = UserSettings.Instance.ConfirmGapFilling;
            
            // список событий/действий
            // добавить недостающие события
            var needUpdate = false;
            var actions = UserSettings.Instance.AccountEventAction;
            foreach (AccountEventCode code in Enum.GetValues(typeof(AccountEventCode)))
            {
                var evtCode = code;
                if (actions.Any(a => a.EventCode == evtCode)) continue;
                needUpdate = true;
                actions.Add(new AccountEventSettings(evtCode, AccountEventAction.DefaultAction));
            }
            if (needUpdate)            
                UserSettings.Instance.SaveSettings();            
            // привязать к таблице
            gridEvents.DataBind(actions);

            //список настроек свечек
            //foreach(var barSettings in BarSettingsStorage.Instance.GetCollection())
            gridCandles.DataBind(BarSettingsStorage.Instance.GetCollection());
            gridCandles.CheckSize();
        }

        private void BtnSoundsClick(object sender, EventArgs e)
        {
            new SoundSetupForm().ShowDialog();
        }

        private void GridEventsUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (!col.IsHyperlinkStyleColumn) return;
            contextMenuAction.Tag = rowIndex;
            var sets = (AccountEventSettings) gridEvents.rows[rowIndex].ValueObject;
            foreach (ToolStripMenuItem item in contextMenuAction.Items)
                item.Checked = sets.EventAction == (AccountEventAction) item.Tag;
            contextMenuAction.Show(gridEvents, e.X, e.Y);
        }

        private void ActionMenuItemClicked(object sender, EventArgs e)
        {
            var rowIndex = (int) contextMenuAction.Tag;
            var sets = (AccountEventSettings)gridEvents.rows[rowIndex].ValueObject;
            var newAction = (AccountEventAction) ((ToolStripMenuItem) sender).Tag;
            if (sets.EventAction == newAction) return;
            sets.EventAction = newAction;
            gridEvents.UpdateRow(rowIndex, sets);
            gridEvents.InvalidateCell(1, rowIndex);
            // сохранить настройки
            UserSettings.Instance.AccountEventAction =
                gridEvents.rows.Select(r => (AccountEventSettings) r.ValueObject).ToList();
            UserSettings.Instance.SaveSettings();
        }

        private void GridCandlesSelectionChanged(MouseEventArgs e, int rowIndex, FastColumn col)
        {
            buttonCandleChange.Enabled = false;
            buttonCandleRemove.Enabled = false;
            var selectedRows = gridCandles.rows.Where(r => r.Selected).ToList();
            if (selectedRows.Count != 0)
            {
                buttonCandleChange.Enabled = true;
                buttonCandleRemove.Enabled = ((BarSettings)(selectedRows[0].ValueObject)).IsUserDefined;
            }
        }

        private void ButtonCandleAddClick(object sender, EventArgs e)
        {
            var form = new ChronometerForm();
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            BarSettings data = form.Data;
            data.IsUserDefined = true;
            BarSettingsStorage.Instance.GetCollection().Add(data);
            gridCandles.DataBind(BarSettingsStorage.Instance.GetCollection());
            gridCandles.CheckSize();
        }

        private void ButtonCandleChangeClick(object sender, EventArgs e)
        {
            var selectedRows = gridCandles.rows.Where(r => r.Selected).ToList();
            if (selectedRows.Count == 0)
                return;
            var form = new ChronometerForm();
            BarSettings data = (BarSettings) selectedRows[0].ValueObject;
            form.Data = data;
            form.SetReadOnly(!data.IsUserDefined);
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;
            BarSettingsStorage.Instance.RemoveBarSettingsByName(data.Title);
            data = form.Data;
            data.IsUserDefined = true;
            BarSettingsStorage.Instance.GetCollection().Add(data);
            gridCandles.DataBind(BarSettingsStorage.Instance.GetCollection());
            gridCandles.CheckSize();
        }

        private void ButtonCandleRemoveClick(object sender, EventArgs e)
        {
            var selectedRows = gridCandles.rows.Where(r => r.Selected).ToList();
            if (selectedRows.Count == 0)
                return;
            if (MessageBox.Show(this, String.Format(
                Localizer.GetString("MessageShouldDeleteCandleSetsFmt"),
                ((BarSettings)(selectedRows[0].ValueObject)).Title), 
                Localizer.GetString("TitleConfirmation"), 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;
            BarSettings data = (BarSettings)selectedRows[0].ValueObject;
            BarSettingsStorage.Instance.RemoveBarSettingsByName(data.Title);
            gridCandles.DataBind(BarSettingsStorage.Instance.GetCollection());
            gridCandles.CheckSize();
        }

        private void GridCandlesUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Clicks == 2)
                ButtonCandleChangeClick(this, null);
        }

        private void OptionsFormHelpRequested(object sender, HelpEventArgs hlpevent)
        {
            if (helpIndexes.ContainsKey(tabControl.SelectedTab))
            {
                var index = helpIndexes[tabControl.SelectedTab];
                HelpManager.Instance.ShowHelp(index);
                hlpevent.Handled = true;
                return;
            }

            HelpManager.Instance.ShowHelp(null);
            hlpevent.Handled = true;
        }

        private void BtnBrowserRegistrationClick(object sender, EventArgs e)
        {
            BrowserSwitch.SetBrowserFeatureControl();
        }
    }

    /// <summary>
    /// описывает запись в таблице настроек, в которой пустому значению соответствует
    /// картинка с индексом 0, истине картинка с индексом 1, false - 2
    /// </summary>
    class GridImageRecord
    {
        public delegate void SetValueDel(object val);

        public delegate object GetValueDel();

        public SetValueDel setValue;

        public GetValueDel getValue;

        public string Name { get; set; }

        public int ImageIndex { get; set; }

        private bool? booleanValue;

        public bool? BooleanValue
        {
            get { return booleanValue; }
            set 
            { 
                booleanValue = value;
                if (value.HasValue)
                    ImageIndex = value.Value ? 1 : 2;
                else                
                    ImageIndex = 0;
            }
        }

        public GridImageRecord(string name, SetValueDel setValue, GetValueDel getValue)
        {
            Name = name;
            this.setValue = setValue;
            this.getValue = getValue;
        }
    }
}
