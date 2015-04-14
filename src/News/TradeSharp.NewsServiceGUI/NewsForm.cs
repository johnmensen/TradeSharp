using System;
using System.Windows.Forms;
using TradeSharp.Contract.Proxy;
using TradeSharp.Util;

namespace TradeSharp.NewsServiceGUI
{
    public partial class NewsForm : Form
    {
        private readonly NewsReceiverProxy newsReceiver;
        public NewsForm()
        {
            InitializeComponent();
            try
            {
                newsReceiver = new NewsReceiverProxy("INewsReceiverBinding");
            }
            catch (Exception)
            {
                Logger.Error("Связь с сервером (IQuoteStorageBinding) не установлена");
            }    
        }


        private void PutNewsBtn_Click(object sender, EventArgs e)
        {
            if (newsReceiver != null)
            {
                newsReceiver.PutNews(new []{ new Contract.Entity.News(2, DateTime.Now, "option", "EUR")});
            }

        }

        
    }
}
