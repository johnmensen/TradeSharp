using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TestMTTradeServer.BL;

namespace TestMTTradeServer
{
    public partial class MainForm : Form
    {
        private int portListen = 18001;
        private int portSend = 18001;

        private bool stopFlag = false;
        private Thread listenThread;

        public MainForm()
        {
            InitializeComponent();
        }

        private void адресToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SettingsForm
                          {
                              PortListen = portListen, PortSend = portSend
                          };
            if (dlg.ShowDialog() == DialogResult.OK)
            {                
                StopListen();
                portListen = dlg.PortListen;
                portSend = dlg.PortSend;
                StartListen();
            }
        }

        private void StartListen()
        {
            stopFlag = false;
            listenThread = new Thread(ThreadFun);
            listenThread.Start();
        }

        private void StopListen()
        {
            stopFlag = true;
            using (var client = new UdpClient("localhost", portListen))
            {
                client.Send(Encoding.ASCII.GetBytes("<end>"), 5);
            }
            listenThread.Join();
            listenThread = null;
        }

        private void ThreadFun()
        {
            AddLine(string.Format("Слушаю порт {0}", portListen));
            UdpClient client;
            try
            {
                client = new UdpClient(portListen);
            }
            catch (SocketException)
            {
                MessageBox.Show(string.Format("Порт {0} возможно занят", portListen));
                return;    
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Ошибка создания сокета: {0}", ex));
                return;
            }
            
            while (!stopFlag)
            {
                var ep = new IPEndPoint(0, 0);
                var data = client.Receive(ref ep);
                if (data.Length == 0) continue;
                
                var dataStr = Encoding.ASCII.GetString(data);                    
                if (dataStr == "<end>") break;
                AddLine(string.Format("[{0}:{1}][{2}]{3}<", 
                    ep.Address, ep.Port, DateTime.Now.ToString("HH:mm:ss"), dataStr));
                
                string response = DealerLogic.ResponseOnMessage(dataStr, int.Parse(tbShiftPP.Text));
                if (string.IsNullOrEmpty(response)) continue;
                AddLine(string.Format("Respond 2 MT4: {0}<", 
                    response));
                SendToUDP(response, ep);
            }            
        }

        private void SendToUDP(string str, IPEndPoint ep)
        {
            var bytes = Encoding.ASCII.GetBytes(str);
            ep.Port = portSend;
            using (var client = new UdpClient())
            {
                client.Send(bytes, bytes.Length, ep);
            }
        }

        #region Log
        private void AddLine(string text)
        {
            var d = new SetTextDlg(AddMemoLine);
            memo.Invoke(d, text + "\r\n");
        }

        private void AddMemoLine(string text)
        {
            memo.Text += text;
        }

        delegate void SetTextDlg(string text);
        
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            StartListen();            
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopListen();
        }

        private void теToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new UdpTestForm();
            dlg.ShowDialog();
        }
    }
}
