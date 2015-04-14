using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Util.Forms;

namespace TradeSharp.Client.Controls.NavPanel
{
    public partial class NavPanelControl : UserControl
    {
        private readonly List<NavPanelPageControl> pages = new List<NavPanelPageControl>();

        public Action closePanelClicked;

        public NavPanelControl()
        {
            InitializeComponent();
        }

        public void SetupPages(string[] pageNames)
        {
            if (pageNames == null || pageNames.Length == 0)
                pageNames = NavPanelPageControl.pageCodes.ToArray();

            // очистить список закладок
            while (pages.Count > 0)
            {
                panelPages.Controls.Remove(pages[pages.Count - 1]);
                pages.RemoveAt(pages.Count - 1);
            }

            // создать закладки
            foreach (var name in pageNames)
            {
                var page = new NavPanelPageControl(name, false, delegate(NavPanelPageControl control)
                    {
                        pages.Remove(control);
                        panelPages.Controls.Remove(control);
                        SavePagesToShow();
                    })
                    {
                        Dock = DockStyle.Top
                    };
                panelPages.Controls.Add(page);
                pages.Add(page);
            }
        }

        private void BtnSetupClick(object sender, EventArgs e)
        {
            var pageState = new Dictionary<object, bool>();
            foreach (var code in NavPanelPageControl.pageCodes)
            {
                var title = NavPanelPageControl.pageTitles[code];
                var pageCode = code;
                var isShown = pages.Any(p => p.Code == pageCode);
                pageState.Add(title, isShown);
            }

            if (CheckedListBoxDialog.Show(pageState, GetParentForm(Parent), "Закладки") != DialogResult.OK) return;
            var pagesToShow = new List<string>();
            foreach (var page in pageState)
            {
                if (!page.Value) continue;
                var pageTitle = (string)page.Key;
                var pageCode = NavPanelPageControl.pageTitles.First(p => p.Value == pageTitle);
                pagesToShow.Add(pageCode.Key);
            }

            if (pagesToShow.Count == 0) return;
            SetupPages(pagesToShow.ToArray());
            
            // сохранить закладки
            SavePagesToShow();
        }

        private void SavePagesToShow()
        {
            var pageCodes = pages.Select(p => p.Code).ToList();
            // сохранить закладки
            if (!UserSettings.Instance.NavPanelPages.SequenceEqual(pageCodes))
            {
                UserSettings.Instance.NavPanelPages = pageCodes.ToArray();
                UserSettings.Instance.SaveSettings();
            }
        }

        private Form GetParentForm(Control parent)
        {
            var form = parent as Form;
            if (form != null)
            {
                return form;
            }
            if (parent != null)
            {
                // Walk up the control hierarchy
                return GetParentForm(parent.Parent);
            }
            return null; // Control is not on a Form
        }

        /// <summary>
        /// закрыть панель
        /// </summary>
        private void BtnCloseClick(object sender, EventArgs e)
        {
            if (closePanelClicked != null)
                closePanelClicked();
        }
    }
}
