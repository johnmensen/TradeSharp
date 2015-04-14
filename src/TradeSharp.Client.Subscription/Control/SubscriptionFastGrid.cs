using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FastGrid;
using System.Windows.Forms;
using TradeSharp.Client.Subscription.Dialog;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Entity;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Util;

namespace TradeSharp.Client.Subscription.Control
{
    // вкладка "Подписка" (действующая для данного счета подписка)
    public partial class SubscriptionFastGrid : UserControl
    {
        public delegate void ShowTopPortfolioDel(TopPortfolio portfolio);

        public ShowTopPortfolioDel ShowTopPortfolio;

        private static Contract.Entity.Subscription emptySubs = new Contract.Entity.Subscription();

        private Contract.Entity.Subscription selectedCategory;

        public SubscriptionFastGrid()
        {
            InitializeComponent();

            SetupGrid();
            SubscriptionModel.Instance.ModelIsLoaded += UpdateSubscriptionSafe;
            SubscriptionModel.Instance.ModelStateChanged += wasModified =>
                {
                    try
                    {
                        Invoke(new Action<bool>(b =>
                            {
                                btnAccept.Enabled = wasModified;
                                btnCancel.Enabled = wasModified;
                            }), wasModified);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                };
            topSubscriptionControl.PageTargeted += page =>
                {
                    if (ShowTopPortfolio != null)
                        ShowTopPortfolio(topSubscriptionControl.Portfolio);
                };
            cbActionOnSignal.DataSource = Enum.GetValues(typeof(ActionOnSignal)).Cast<ActionOnSignal>().Select(EnumFriendlyName<ActionOnSignal>.GetString).ToList();
            UpdateSubscriptionUnsafe();
        }

        public List<Contract.Entity.Subscription> GetCurrentSubscriptions()
        {
            return grid.GetRowValues<Contract.Entity.Subscription>(false).ToList();
        }

        private void SetupGrid()
        {
            var fontBold = new Font(Font, FontStyle.Bold);
            grid.Columns.Add(new FastColumn(emptySubs.Property(s => s.Title), Localizer.GetString("TitleSignal"))
                {
                    ColumnMinWidth = 150,
                    SortOrder = FastColumnSort.Descending,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = fontBold,
                });
            grid.Columns.Add(new FastColumn(emptySubs.Property(s => s.RenewAuto), Localizer.GetString("TitleProlong"))
                {
                    ColumnWidth = 80,
                    formatter =
                        v => (bool) v ? Localizer.GetString("TitleAutomatically") : Localizer.GetString("TitleManually"),
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = fontBold,
                    SortOrder = FastColumnSort.None,
                    IsEditable = true
                });
            grid.Columns.Add(new FastColumn(emptySubs.Property(s => s.TradeAuto), Localizer.GetString("TitleTradeVerb"))
                {
                    ColumnWidth = 100,
                    formatter =
                        v => (bool) v ? Localizer.GetString("TitleAutomatically") : Localizer.GetString("TitleManually"),
                    colorColumnFormatter = (object value, out Color? color, out Color? fontColor) =>
                        {
                            color = null;
                            fontColor = ((bool) value) ? Color.Navy : Color.Black;
                        },
                    IsHyperlinkStyleColumn = true,
                    SortOrder = FastColumnSort.None,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ColorHyperlinkTextActive = Color.Blue,
                    HyperlinkFontActive = fontBold
                });
            grid.Columns.Add(new FastColumn(emptySubs.Property(s => s.TradeAuto), Localizer.GetString("TitleConfigure"))
                {
                    Tag = contextMenu,
                    ColumnWidth = 70,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand,
                    ImageList = imageListGlyph
                });
            grid.colorFormatter += (object value, out Color? color, out Color? fontColor) =>
                {
                    color = null;
                    fontColor = null;
                    var cat = (Contract.Entity.Subscription) value;
                    // !
                    //if (!cat.IsSubscriber && !cat.IsSignalMaker)
                    {
                        // новодобавленная категория (не сохранена в БД)
                        //fontColor = Color.Maroon;
                    }
                };

            grid.ColorAltCellBackground = Color.White;
            grid.UserHitCell += GridUserHitCell;
            grid.CalcSetTableMinWidth();
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            if (e.Button != MouseButtons.Left) return;

            var sets = (Contract.Entity.Subscription) grid.rows[rowIndex].ValueObject;
            selectedCategory = sets;

            // действие со счетом
            if (col.Tag == contextMenu || col.PropertyName == emptySubs.Property(s => s.Title))
            {
                var pointTop = grid.GetCellCoords(col, rowIndex);
                contextMenu.Show(grid, pointTop);
                return;
            }

            // настроить торговлю
            if (col.PropertyName == emptySubs.Property(s => s.TradeAuto))
            {
                EditTradeSettings();
            }
        }

        /// <summary>
        /// пользователь подписался / отписался от сигнала
        /// </summary>
        private void UpdateSubscriptionSafe(List<Contract.Entity.Subscription> cats)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(UpdateSubscriptionUnsafe));
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                UpdateSubscriptionUnsafe();
            }
        }

        private void BtnAcceptClick(object sender, EventArgs e)
        {
            SubscriptionModel.Instance.SaveSubscribedCategories();
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            SubscriptionModel.Instance.LoadSubscribedCategories();
        }

        private void BtnRefreshClick(object sender, EventArgs e)
        {
            SubscriptionModel.Instance.LoadSubscribedCategories();
        }

        private void UpdateSubscriptionUnsafe()
        {
            var login = AccountStatus.Instance.Login;
            if (string.IsNullOrEmpty(login))
                return;

            try
            {
                summaryFlowLayoutPanel.Hide();
                topSubscriptionControl.Hide();
                var subscribedPortfolioId = TradeSharpAccountStatistics.Instance.proxy.GetSubscribedTopPortfolioId(login);
                if (subscribedPortfolioId == -1)
                {
                    var services = TradeSharpAccount.Instance.proxy.GetPaidServices(AccountStatus.Instance.Login);
                    if (services == null)
                    {
                        MessageBox.Show(this, 
                            Localizer.GetString("MessageUnableToGetSubscriptions"), 
                            Localizer.GetString("TitleWarning"),
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        summaryLabel.Text = string.Format(
                            Localizer.GetString("MessageYourAreSubscribedOnSignalsFmt"),
                            services.Count, services.Select(s => s.FixedPrice).Sum(), PerformerStat.FeeCurrency);
                    }
                    summaryFlowLayoutPanel.Show();
                }
                else
                {
                    AccountEfficiency userAccountEfficiency;
                    topSubscriptionControl.Portfolio =
                        TradeSharpAccountStatistics.Instance.proxy.GetTopPortfolio(subscribedPortfolioId,
                                                                                   out userAccountEfficiency);
                    topSubscriptionControl.Show();
                }
                if (SubscriptionModel.Instance.getActionOnSignal != null)
                    cbActionOnSignal.SelectedItem =
                        EnumFriendlyName<ActionOnSignal>.GetString(SubscriptionModel.Instance.getActionOnSignal());
                grid.DataBind(SubscriptionModel.Instance.SubscribedCategories);
                grid.Invalidate();
            }
            catch (Exception ex)
            {
                Logger.Error("SubscriptionFastGrid.UpdateSubscriptionUnsafe", ex);
            }
        }

        private void MenuItemUnsubscribeClick(object sender, EventArgs e)
        {
            if (selectedCategory == null) return;
            var msg = string.Format(
                Localizer.GetString("MessageUnsubscribeSignalFmt"),
                                    selectedCategory.Title);
            if (MessageBox.Show(msg, 
                Localizer.GetString("TitleConfirmation"), 
                MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            SubscriptionModel.Instance.RemoveSubscription(selectedCategory);
        }

        private void MenuitemTradeSettingsClick(object sender, EventArgs e)
        {
            EditTradeSettings();
        }

        private void EditTradeSettings()
        {
            if (selectedCategory == null) return;
            var dlg = new AutoTradeSettingsForm(selectedCategory.AutoTradeSettings);
            if (dlg.ShowDialog() == DialogResult.Cancel)
                return;
            selectedCategory.AutoTradeSettings = dlg.sets;
            SubscriptionModel.Instance.ModifySubscription(selectedCategory);
        }

        private void MenuitemStatisticsClick(object sender, EventArgs e)
        {
            if (selectedCategory == null) return;
            ShowSignallerStat(selectedCategory);
        }

        private void ShowSignallerStat(Contract.Entity.Subscription cat)
        {
            // получить счет перформера по кат. сигнала
            PerformerStat stat = null;
            try
            {
                stat = TradeSharpAccountStatistics.Instance.proxy.GetPerformerStatBySignalCatId(cat.Service);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localizer.GetString("MessageCannotGetSubscribersInfo"));
                Logger.Info("ShowSignallerStat", ex);
                return;
            }

            if (stat == null) return;
            var form = new SubscriberStatisticsForm(stat);
            //form.EnterRoomRequested += OnEnterRoomRequested;
            form.Show(this);
        }
    }
}