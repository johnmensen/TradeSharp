using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Client.Forms;
using TradeSharp.Client.Subscription.Control;
using TradeSharp.Client.Util;
using TradeSharp.Util;

namespace TradeSharp.Client
{
    partial class MainForm
    {
        private void MenuitemNavPaneClick(object sender, EventArgs e)
        {
            ShowOrHideNavPanel(!panelNavi.Visible, true);
            UserSettings.Instance.NavPanelIsVisible = panelNavi.Visible;
            UserSettings.Instance.SaveSettings();
        }

        private void ShowOrHideNavPanel(bool isShown, bool correctSize)
        {
            if (panelNavi.Visible == isShown) return;
            panelNavi.Visible = isShown;
            menuitemNavPane.Checked = panelNavi.Visible;
            // всем открытым окошкам скорректировать размеры с учетом изменения ширины            
            if (correctSize)
                ScaleChildrenToNewSize();
        }

        /// <summary>
        /// в зависимости от того, показана или спрятана панель инструментов, масштабировать окошки
        /// </summary>
        private void ScaleChildrenToNewSize()
        {
            var rectWoPanel = new Rectangle(ClientRectangle.Left, ClientRectangle.Top,
                ClientRectangle.Width, ClientRectangle.Height - panelStatus.Height);
            var rectWithPanel = new Rectangle(rectWoPanel.Left + (panelNavi.Visible ? panelNavi.Width : 0),
                                              rectWoPanel.Top,
                                              rectWoPanel.Width - panelNavi.Width,
                                              rectWoPanel.Height);

            var rectBefore = panelNavi.Visible ? rectWoPanel : rectWithPanel;
            var rectAfter = !panelNavi.Visible ? rectWoPanel : rectWithPanel;

            var relSize = MdiChildren.ToDictionary(w => w,
                                                   w => new RectangleF(
                                                            (w.Left - rectBefore.Left) / (float)rectBefore.Width,
                                                            (w.Top - rectBefore.Top) / (float)rectBefore.Height,
                                                            w.Width / (float)rectBefore.Width,
                                                            w.Height / (float)rectBefore.Height));

            foreach (var pair in relSize)
            {
                var left = pair.Value.Left * rectAfter.Width;
                var top = pair.Value.Top * rectAfter.Height;
                var width = pair.Value.Width * rectAfter.Width;
                var height = pair.Value.Height * rectAfter.Height;

                pair.Key.Location = new Point(Convert.ToInt32(left), Convert.ToInt32(top));
                pair.Key.Size = new Size(Convert.ToInt32(width), Convert.ToInt32(height));
            }
        }

        #region Упорядочивание окошек из меню
        private void MenuItemCascadeClick(object sender, EventArgs e)
        {
            foreach (var child in MdiChildren)
            {
                if (child.Visible && child.WindowState == FormWindowState.Minimized)
                    child.WindowState = FormWindowState.Normal;
            }
            LayoutMdi(MdiLayout.Cascade);
        }

        private void MenuItemVerticalClick(object sender, EventArgs e)
        {
            foreach (var child in MdiChildren)
            {
                if (child.Visible && child.WindowState == FormWindowState.Minimized)
                    child.WindowState = FormWindowState.Normal;
            }
            LayoutMdi(MdiLayout.TileVertical);            
        }

        private void MenuItemHorizontalClick(object sender, EventArgs e)
        {
            foreach (var child in MdiChildren)
            {
                if (child.Visible && child.WindowState == FormWindowState.Minimized)
                    child.WindowState = FormWindowState.Normal;
            }
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void MenuItemMinimizeAllClick(object sender, EventArgs e)
        {
            foreach (var child in MdiChildren)
            {
                if (child.Visible) child.WindowState = FormWindowState.Minimized;
            }
            LayoutMdi(MdiLayout.ArrangeIcons);
        }
        #endregion

        private void MenuWindowQuoteClick(object sender, EventArgs e)
        {
            EnsureQuoteWindow();
        }

        private void MenuWindowAccountClick(object sender, EventArgs e)
        {
            EnsureAccountWindow();
        }

        private Form EnsureForm(Type formType)
        {
            try
            {
                var allForms = MdiChildren.ToList();
                var form = allForms.FirstOrDefault(f => f.GetType() == formType);
                if (form != null)
                {
                    form.Focus();
                    return form;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в EnsureForm() - проверка открытых форм", ex);
                throw;
            }
            
            try
            {
                Form child = null;

                if (InvokeRequired)
                    Invoke(new Action<Type>(type =>
                    {
                        child = (Form)formType.GetConstructor(Type.EmptyTypes).Invoke(null);
                        SetupNonMdiForm((IMdiNonChartWindow)child);
                        child.Show();
                    }), formType);
                else
                {
                    child = (Form)formType.GetConstructor(Type.EmptyTypes).Invoke(null);
                    SetupNonMdiForm((IMdiNonChartWindow)child);
                    child.Show();
                }
                return child;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка в EnsureForm() - открытие нового окна", ex);
                throw;
            }
        }

        private void EnsureWebBrowseWindow()
        {
            EnsureForm(typeof (BrowserForm));
        }

        private void EnsureQuoteWindow()
        {
            EnsureForm(typeof(QuoteTableForm));
        }

        private void EnsureAccountWindow()
        {
            if (!AccountStatus.Instance.isAuthorized)
                OpenLoginDialog();
            EnsureForm(typeof(AccountMonitorForm));
        }

        // this made public for opening from SubscriptionForm
        public void EnsureChatWindow()
        {
            EnsureForm(typeof(ChatForm));
        }

        public void EnsureRoboTesterForm(string robotUniqueName = "")
        {
            var form = EnsureForm(typeof(RoboTesterForm));
            ((RoboTesterForm)form).OnRobotResultsBoundToCharts += OnRobotResultsBoundToCharts;
            if (!string.IsNullOrEmpty(robotUniqueName))
                ((RoboTesterForm) form).ShowRobotSettings(robotUniqueName);
        }

        public void EnsureRiskSetupForm()
        {
            EnsureForm(typeof(RiskSetupForm));
        }

        public void EnsureStatisticsWindow(bool instantCalculation = false)
        {
            foreach (var form in MdiChildren.Where(form => form is AccountTradeResultsForm))
            {
                form.Focus();
                return;
            }

            var child = new AccountTradeResultsForm {InstantCalculation = instantCalculation};
            SetupNonMdiForm(child);
            child.Show();            
        }

        public void EnsureSubscriptionForm(SubscriptionControl.ActivePage page)
        {
            if (!AccountStatus.Instance.isAuthorized)
                OpenLoginDialog();

            var form = EnsureForm(typeof(SubscriptionForm));
            ((IMdiNonChartWindow)form).WindowInnerTabPageIndex = (int)page;
        }

        public void EnsureWalletWindow()
        {
            if (!AccountStatus.Instance.isAuthorized)
                OpenLoginDialog();
            EnsureForm(typeof(WalletForm));
        }
     
        private void LayoutChildren(MdiLayout layout)
        {
            var children = MdiChildren.Where(c => c.Visible).ToList();
            if (children.Count == 0) return;
            
            // размеры окна
            var mdiSize = GetMdiClientWindow().ClientSize;
            if (mdiSize.Height < 40)
                mdiSize = new Size(mdiSize.Width, 40);
            if (mdiSize.Width < 65)
                mdiSize = new Size(65, mdiSize.Height);

            var capH = SystemInformation.CaptionHeight;
            const int capW = 60;
            
            // одно окошко развернуть во всю ширину
            if (children.Count == 1)
            {
                children[0].WindowState = FormWindowState.Normal;
                children[0].Location = new Point(0, 0);
                children[0].Size = mdiSize;
                return;
            }

            // упорядочить каскадом
            if (layout == MdiLayout.Cascade)
            {
                const int maxInRow = 6;

                var delta = capH * Math.Min(maxInRow - 1, children.Count - 1);
                var wSize = new Size(mdiSize.Width - delta, mdiSize.Height - delta);
                var leftTop = new Point(0, 0);
                var counter = 0;
                foreach (var window in children)
                {
                    window.WindowState = FormWindowState.Normal;
                    window.Location = leftTop;
                    window.Size = wSize;
                    leftTop = new Point(leftTop.X + capH, leftTop.Y + capH);
                    counter++;
                    if (counter == maxInRow)
                        leftTop = new Point(0, 0);
                }
                return;
            }

            // дать окошкам больше места
            if (mdiSize.Height < 90)
                mdiSize = new Size(mdiSize.Width, 90);
            if (mdiSize.Width < 120)
                mdiSize = new Size(120, mdiSize.Height);

            // выстроить окошки в линию по вертикали или по горизонтали
            if (children.Count < 4)
            {
                var w = layout == MdiLayout.TileHorizontal ? mdiSize.Width / children.Count : mdiSize.Width;
                var h = layout == MdiLayout.TileVertical ? mdiSize.Height / children.Count : mdiSize.Height;
                var wSize = new Size(w, h);
                var stepX = layout == MdiLayout.TileHorizontal ? w : 0;
                var stepY = layout == MdiLayout.TileVertical ? h : 0;
                var leftTop = new Point(0, 0);
                foreach (var window in children)
                {
                    window.WindowState = FormWindowState.Normal;
                    window.Location = leftTop;
                    window.Size = wSize;
                    leftTop = new Point(leftTop.X + stepX, leftTop.Y + stepY);                    
                }
                return;
            }

            // дать окошкам еще больше места и расположить в виде таблицы
            if (mdiSize.Height < 120)
                mdiSize = new Size(mdiSize.Width, 120);
            if (mdiSize.Width < 160)
                mdiSize = new Size(160, mdiSize.Height);

            // посчитать количество строк и столбцов
            var rows = (int) Math.Sqrt(children.Count);
            var cols = children.Count / rows;
            var mod = children.Count - cols * rows;
            if (mod > 0) cols++;

            // разместить
            var ht = mdiSize.Height/rows;
            var wd = mdiSize.Width/cols;
            var size = new Size(wd, ht);
            var top = 0;
            var childIndex = 0;

            for (var row = 0; row < rows; row++)
            {
                var left = 0;
                for (var col = 0; col < cols; col++)
                {
                    var child = children[childIndex++];
                    child.WindowState = FormWindowState.Normal;
                    child.Location = new Point(left, top);
                    child.Size = size;

                    left += wd;
                }
                top += ht;
                var itemsLeft = children.Count - childIndex;
                if (itemsLeft > 0 && itemsLeft < cols)
                {
                    cols = itemsLeft;
                    wd = mdiSize.Width / cols;
                    size = new Size(wd, ht);
                }
            }
        }

        private MdiClient GetMdiClientWindow()
        {
            return Controls.OfType<MdiClient>().Select(ctl => ctl as MdiClient).FirstOrDefault();
        }
    }
}
