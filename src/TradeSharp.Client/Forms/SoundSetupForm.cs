using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.BL.Sound;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class SoundSetupForm : Form
    {
        public SoundSetupForm()
        {
            InitializeComponent();

            Localizer.LocalizeControl(this);
            SetupGrid();
            LoadSounds();
        }

        private void SetupGrid()
        {
            var blank = new SoundEventTag(VocalizedEvent.CommonError, string.Empty);
            grid.Columns.Add(new FastColumn(blank.Property(p => p.EventName), Localizer.GetString("TitleEvent"))
                {
                    ColumnMinWidth = 80,
                    SortOrder = FastColumnSort.Ascending
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.FileName), Localizer.GetString("TitleFile"))
                {
                    ColumnMinWidth = 60,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkFontActive = new Font(Font, FontStyle.Bold),
                    HyperlinkActiveCursor = Cursors.Hand
                });
            grid.Columns.Add(new FastColumn(blank.Property(p => p.ImageIndex), Localizer.GetString("TitlePlay"))
                {
                    ColumnWidth = 40,
                    ImageList = imageList,
                    IsHyperlinkStyleColumn = true,
                    HyperlinkActiveCursor = Cursors.Hand
                });
            grid.CalcSetTableMinWidth();
        }

        private void LoadSounds()
        {
            cbMute.Checked = UserSettings.Instance.Mute;

            var tags = new List<SoundEventTag>();
            foreach (var sets in UserSettings.Instance.VocalEvents)
            {
                var tag = new SoundEventTag(sets.EventName, sets.FileName);
                var path = EventSoundPlayer.MakeSoundFilePath(sets.FileName);
                if (!File.Exists(path)) tag.FileName = "";
                tags.Add(tag);
            }
            grid.DataBind(tags);

            // заполнить выпадающее меню со звуками
            var emptyItem = menuSound.Items.Add(Localizer.GetString("TitleNoSoundSmall"));
            emptyItem.Tag = false;
            emptyItem.Click += MenuItemClick;
            var soundDir = EventSoundPlayer.MakeSoundFilePath("");
            if (!Directory.Exists(soundDir)) return;

            foreach (var file in Directory.GetFiles(soundDir))
            {
                var fileName = Path.GetFileName(file);
                var menuItem = menuSound.Items.Add(fileName);
                menuItem.Click += MenuItemClick;
            }
        }

        private void MenuItemClick(object sender, System.EventArgs e)
        {
            var rowIndex = (int) menuSound.Tag;
            var tag = (SoundEventTag) grid.rows[rowIndex].ValueObject;
            var senderItem = ((ToolStripMenuItem) sender);
            tag.FileName = senderItem.Tag == null ? senderItem.Text : "";

            grid.UpdateRow(rowIndex, tag);
            grid.InvalidateCell(1, rowIndex);
        }

        private void GridUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col)
        {
            var tag = (SoundEventTag) grid.rows[rowIndex].ValueObject;
            // если выбрана иконка проигрывателя - играть файл
            if (col.PropertyName == tag.Property(p => p.ImageIndex))
            {
                PlaySound(tag.FileName);
                return;
            }
            if (col.PropertyName == tag.Property(p => p.FileName))
            {
                // предложить выбрать файл
                foreach (ToolStripMenuItem item in menuSound.Items)
                {
                    item.Checked = item.Text == tag.FileName;
                }
                menuSound.Tag = rowIndex;
                menuSound.Show(grid, e.X, e.Y);
                return;
            }
        }

        private static void PlaySound(string fileName)
        {
            var path = EventSoundPlayer.MakeSoundFilePath(fileName);
            if (!File.Exists(path)) return;
            try
            {
                ThreadPool.QueueUserWorkItem(filePathObj =>
                {
                    using (var player = new SoundPlayer((string)filePathObj))
                    {
                        player.PlaySync();
                    }                    
                }, path);
            }
            catch
            {
            }
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            var newSets = grid.rows.Select(r =>
                                           new VocalizedEventFileName
                                               {
                                                   EventName = ((SoundEventTag) r.ValueObject).VocEvent,
                                                   FileName = ((SoundEventTag) r.ValueObject).FileName
                                               }).ToList();
            UserSettings.Instance.VocalEvents = newSets;
            UserSettings.Instance.Mute = cbMute.Checked;

            EventSoundPlayer.Instance.ApplySoundScheme();
            UserSettings.Instance.SaveSettings();
            Close();
        }
    }

    class SoundEventTag
    {
        public VocalizedEvent VocEvent { get; set; }

        public string EventName { get; set; }

        public int ImageIndex { get { return 0; } }

        public string FileName { get; set; }

        public SoundEventTag(VocalizedEvent evt, string fileName)
        {
            VocEvent = evt;
            EventName = EnumFriendlyName<VocalizedEvent>.GetString(evt);
            FileName = fileName;
        }
    }
}
