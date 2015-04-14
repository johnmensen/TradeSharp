using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TradeSharp.Client.Controls
{
    public partial class ConnectionStatusImageControl : UserControl
    {
        private Image[] serverConnectionStatusImages;

        private bool hasServer;
        public bool HasServer
        {
            get { return hasServer; }
            set
            {
                if (hasServer == value)
                    return;
                hasServer = value;
                ChangeImageIndexThreadSafe();
            }
        }

        private bool hasQuotes;
        public bool HasQuotes
        {
            get { return hasQuotes; }
            set
            {
                if (hasQuotes == value)
                    return;
                hasQuotes = value;
                ChangeImageIndexThreadSafe();
            }
        }

        public ConnectionStatusImageControl()
        {
            InitializeComponent();
            Load += (sender, args) =>
            {
                try
                {
                    LoadImages();
                }
                catch
                {
                }
            };
        }

        private void LoadImages()
        {            
            var imageNames = new[]
                {
                    "TradeSharp.Client.Images.connection_state01.gif",
                    "TradeSharp.Client.Images.connection_state02.gif",
                    "TradeSharp.Client.Images.connection_state03.gif",
                    "TradeSharp.Client.Images.connection_state04.gif",
                };
            serverConnectionStatusImages = new Image[imageNames.Length];
            for (var i = 0; i < imageNames.Length; i++)
            {
                /*using (*/var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(imageNames[i]);//)
                {
                    if (stream == null) continue;
                    serverConnectionStatusImages[i] = new Bitmap(stream);
                }
            }
            if (serverConnectionStatusImages[0] != null)
                pictureBox.Image = serverConnectionStatusImages[0];
        }

        private void ChangeImageIndexThreadSafe()
        {
            Invoke(new Action(ChangeImageIndex));
        }

        private void ChangeImageIndex()
        {
            var imageIndex = 0;
            if (hasServer) imageIndex |= 1;
            if (hasQuotes) imageIndex |= 2;
            pictureBox.Image = serverConnectionStatusImages[imageIndex];
        }
    }
}
