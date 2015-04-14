using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using TradeSharp.Localisation;
using TradeSharp.Util;
using System.Windows.Forms;

namespace TradeSharp.Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // update files
            var programPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (!string.IsNullOrEmpty(programPath))
            {
                var files = Directory.GetFiles(programPath).ToList();
                foreach (var file in files)
                {
                    const string suffix = ".new";
                    if (!file.EndsWith(suffix))
                        continue;
                    for (var attempt = 0; attempt < 20; attempt++)
                    {
                        try
                        {
                            File.Copy(file, file.Substring(0, file.Length - suffix.Length), true);
                            //File.Replace(file, file.Substring(0, file.Length - suffix.Length), null);
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }

            /*if (Debugger.IsAttached)
            {
                var explicitCulture = CultureInfo.CreateSpecificCulture("ru-RU");
                //CultureInfo.DefaultThreadCurrentCulture = explicitCulture;
                //CultureInfo.DefaultThreadCurrentUICulture = explicitCulture;
                Thread.CurrentThread.CurrentCulture = explicitCulture;
                Thread.CurrentThread.CurrentUICulture = explicitCulture;
            }*/

            Localizer.ResourceResolver = new ResourceResolver();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new MainForm();
            Application.AddMessageFilter(new AppMessageFilter(form));
            Application.Run(form);
        }

        class ResourceResolver : IResourceResolver
        {
            public string TryGetResourceValue(string resxKey)
            {
                return LocalisationManager.Instance.GetString(resxKey);
            }
        }
    }
}
