using System;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Chat.StandAloneClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Logger.Info("Starting...");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChatForm(args));
        }
    }
}
