using System;
using System.Windows.Forms;
using TradeSharp.Client.BL;

namespace TradeSharp.Client.Forms
{
    public partial class QuoteTableForm : Form, IMdiNonChartWindow
    {
        public NonChartWindowSettings.WindowCode WindowCode
        {
            get { return NonChartWindowSettings.WindowCode.Quotes; }
        }
        
        public int WindowInnerTabPageIndex { get; set; }

        public QuoteTableForm()
        {
            InitializeComponent();
            quoteTable.OnClose += quoteTable_OnClose;
            quoteTable.OnPinDown += quoteTable_OnPinDown;
            //quoteTable.OnPinUp += quoteTable_OnPinDown;
        }

        private void quoteTable_OnPinDown(object sender, EventArgs e)
        {
            Dock = Dock != DockStyle.Left ? DockStyle.Left : DockStyle.None;
        }

        private void quoteTable_OnClose(object sender, EventArgs e)
        {
            Close();
        }

        private Action<Form> formMoved;
        public event Action<Form> FormMoved
        {
            add { formMoved += value; }
            remove { formMoved -= value; }
        }

        /// <summary>
        /// перемещение формы завершено - показать варианты Drop-a
        /// </summary>
        private Action<Form> resizeEnded;
        public event Action<Form> ResizeEnded
        {
            add { resizeEnded += value; }
            remove { resizeEnded -= value; }
        }

        private void QuoteTableFormLoad(object sender, EventArgs e)
        {            
            quoteTable.StartPolling();
            // запомнить окошко
            MainForm.Instance.AddNonChartWindowSets(new NonChartWindowSettings
            {
                Window = WindowCode,
                WindowPos = Location,
                WindowSize = Size,
                WindowState = WindowState.ToString()
            });
        }

        private void QuoteTableFormFormClosing(object sender, FormClosingEventArgs e)
        {
            quoteTable.StopPolling();
            // убрать окошко из конфигурации
            if (e.CloseReason == CloseReason.UserClosing ||
                e.CloseReason == CloseReason.None)
                MainForm.Instance.RemoveNonChartWindowSets(WindowCode);
        }

        private void QuoteTableFormMove(object sender, EventArgs e)
        {
            if (formMoved != null)
                formMoved(this);
        }

        private void QuoteTableFormResizeEnd(object sender, EventArgs e)
        {
            if (resizeEnded != null)
                resizeEnded(this);
        }        
    }
}