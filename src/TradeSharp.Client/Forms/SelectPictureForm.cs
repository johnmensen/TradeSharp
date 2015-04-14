using System;
using System.Windows.Forms;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class SelectPictureForm : Form
    {
        private readonly ImageList imgList;

        public int ImageIndex { get; private set; }

        public SelectPictureForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public SelectPictureForm(ImageList imgList) : this()
        {
            this.imgList = imgList;
        }

        private void SelectPictureFormLoad(object sender, EventArgs e)
        {
            listView.SmallImageList = imgList;
            listView.LargeImageList = imgList;
            for (var index = 0; index < imgList.Images.Count; index++)
            {
                listView.Items.Add(index.ToString(), index.ToString(), index);
            }
        }

        private void ListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            var hasSel = listView.SelectedIndices.Count > 0;
            btnAccept.Enabled = hasSel;
            if (hasSel) ImageIndex = listView.SelectedIndices[0];
        }
    }
}
