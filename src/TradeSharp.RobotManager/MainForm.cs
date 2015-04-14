using System;
using System.ServiceProcess;
using System.Windows.Forms;
using TradeSharp.RobotManager.Properties;
using TradeSharp.Util;

namespace TradeSharp.RobotManager
{
    public partial class MainForm : Form
    {
        private readonly string serviceName = Resources.ServiceName;
        private readonly FarmServiceManager serviceManager; 

        public MainForm()
        {
            InitializeComponent();
            serviceManager = new FarmServiceManager(serviceName);
            serviceManager.ServiceStateChange += ServiceManagerServiceStateChange;
        }

        /// <summary>
        /// Обработчик события изменения статуса службы
        /// </summary>
        /// <param name="status"></param>
        void ServiceManagerServiceStateChange(int status)
        {
            SetControlPropertyValuesAsynch(status);
        }
       

        private void BtnServiceStartClick(object sender, EventArgs e)
        {
            try
            {
                serviceManager.ServiceStart();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска службы", ex);
            }
            
        }

        private void BtnServiseStopClick(object sender, EventArgs e)
        {
            try
            {
                serviceManager.ServiceStop();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка остановки службы", ex);
            }
            
        }

        /// <summary>
        /// Обновление GUI из стороннего потока
        /// </summary>
        private void SetControlPropertyValuesAsynch(int status)
        {
            if (txtServiceStatys.InvokeRequired)
            {
                txtServiceStatys.Invoke(new Action<int>(SetControlPropertyValuesAsynch), new object[] { status });
            }
            else
            {
                // Всё условие else выполняется в основном потоке
                if (status != -1)
                {
                    txtServiceStatys.Text = serviceManager.StatusToString((ServiceControllerStatus)status);

                    if (status != (int)ServiceControllerStatus.Running && status != (int)ServiceControllerStatus.Stopped)
                    {
                        btnServiceStart.Enabled = false;
                        btnServiseStop.Enabled = false;
                    }
                    else
                    {
                        if (status == (int)ServiceControllerStatus.Stopped)
                        {
                            btnServiceStart.Enabled = true;
                            btnServiseStop.Enabled = false;
                        }
                        else
                        {
                            btnServiceStart.Enabled = false;
                            btnServiseStop.Enabled = true;
                        }
                    }
                }
                else
                {
                    txtServiceStatys.Text = string.Format("Не удалось обнаружить службу с именем {0}", serviceName);
                    btnServiceStart.Enabled = false;
                    btnServiseStop.Enabled = false;                  
                }
            }
        }

        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            serviceManager.ServiceStateChange -= ServiceManagerServiceStateChange;
        }
    }
}
