using System;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Test.WinFormContainer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Logger.Error("TradeSharp.Test.WinFormContainer.Main() - не корректно переданный аргумент с именем формы и её сборкой");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form form = null;
            try
            {
                var typeInfo = Type.GetType(args[0]);

                if (typeInfo != null)
                {
                    var constructorInfo = typeInfo.GetConstructor(new Type[0]);
                    if (constructorInfo != null)
                    {
                        form = (Form) constructorInfo.Invoke(new object[0]);
                    }
                    else
                    {
                        Logger.Error("TradeSharp.Test.WinFormContainer.Main() - не удалось создать форму типа " + args[0]);
                        return;
                    } 
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharp.Test.WinFormContainer.Main()", ex);
                return;
            }

            try
            {
                Application.Run(form);
            }
            catch (Exception ex)
            {
                Logger.Error("WinFormContainer: Application.Run)", ex);
            }
        }
    }
}
