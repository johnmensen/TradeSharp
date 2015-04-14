using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Controls;
using Entity;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Client.Util;
using TradeSharp.Contract.Util.Proxy;
using TradeSharp.Robot.BL;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Client.Controls
{
    public partial class RobotPortfolioSetupControl : UserControl
    {
        public static string lastSavedPath;

        private FastGridTyped<RobotGridItem> grid;

        [DisplayName("Live-портфель роботов")]
        [Category("Основные")]
        public bool LivePortfolioMode { get; set; }

        public RobotPortfolioSetupControl()
        {
            InitializeComponent();
            SetupGrid();
            
            toolTip.SetToolTip(BtnUnselectRobot, Localizer.GetString("TitleRemove"));
            toolTip.SetToolTip(BtnSelectRobot, Localizer.GetString("TitleAdd"));
        }

        public void BindRobots(IEnumerable<BaseRobot> robots)
        {
            var items = robots.Select(r => new RobotGridItem(r, true)).ToList();
            gridRobot.DataBind(items);
        }

        public void ReadRobotsSettings(string xmlPath)
        {
            if (!File.Exists(xmlPath)) return;

            gridRobot.rows.Clear();
            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                var node = doc.SelectSingleNode("RobotsPortfolio");
                var nodes = node.SelectNodes("robot");

                var robots = new List<RobotGridItem>();
                foreach (XmlElement item in nodes)
                {
                    var inodes = item.SelectNodes("Robot.TypeName");
                    var inode = (XmlElement)inodes[0];
                    var title = inode.Attributes["value"].Value;
                    var robot = RobotCollection.MakeRobot(title);
                    PropertyXMLTagAttribute.InitObjectProperties(robot, item, false);
                    robots.Add(new RobotGridItem(robot, true));
                }
                gridRobot.DataBind(robots);
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка чтения файла настроек портфеля роботов: ", ex);
            }
        }

        public List<BaseRobot> GetUsedRobots()
        {
            var allRobots = grid.GetBoundValues().Select(r => r.Robot).ToList();
            return allRobots;
        }

        public void SaveRobots(List<BaseRobot> robots)
        {
            var dirName = EnsureRobotsDir();
            if (string.IsNullOrEmpty(dirName)) return;

            if (string.IsNullOrEmpty(lastSavedPath))
                lastSavedPath = dirName + "\\temp.xml";
            RobotFarm.SaveRobots(robots, lastSavedPath);
        }

        public void ShowDialogforRobotGivenByItsUniqueName(string robotUniqueName)
        {
            var robot = grid.GetBoundValues().Select(r => r.Robot).FirstOrDefault(r => r.GetUniqueName() == robotUniqueName);
            if (robot != null)
                ShowRobotParamsDialog(robot);
        }

        /// <summary>
        /// запросить на сервере номера Magic, которые никогда не прописывались роботами по текущему! счету
        /// назначить эти свободные номера роботам
        /// </summary>
        public void AssignFreeMagicsToRobots()
        {
            var robots = grid.GetBoundValues().Where(r => r.Magic == 0).ToList();
            if (robots.Count == 0)
                return;

            var magics = TradeSharpAccount.Instance.proxy.GetFreeMagicsPool(AccountStatus.Instance.accountID, robots.Count);

            for (var i = 0; i < magics.Count; i++)
            {
                robots[i].Robot.Magic = magics[i];
                robots[i].Initialized = true;
            }
        }

        private void SetupGrid()
        {
            grid = new FastGridTyped<RobotGridItem>(gridRobot);
            grid.UserHitCell += GridRobotUserHitCell;

            var fontBold = new Font(Font, FontStyle.Bold);
            gridRobot.Columns.Add(new FastColumn(RobotGridItem.speciman.Property(s => s.TypeName), Localizer.GetString("TitleRobot"))
                                      {
                                          SortOrder = FastColumnSort.Ascending,
                                          ColumnMinWidth = 66
                                      });
            gridRobot.Columns.Add(new FastColumn(RobotGridItem.speciman.Property(s => s.Magic), "Magic")
            {
                ColumnMinWidth = 48,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                HyperlinkFontActive = fontBold,
                formatter = value =>
                    {
                        var magic = (int) value;
                        return magic == 0 ? Localizer.GetString("TitleNotAssigned") : magic.ToString();
                    }
            });
            gridRobot.Columns.Add(new FastColumn(RobotGridItem.speciman.Property(s => s.HumanRTickers), Localizer.GetString("TitleCharts"))
            {
                SortOrder = FastColumnSort.Ascending,
                ColumnMinWidth = 70,
                IsHyperlinkStyleColumn = true,
                HyperlinkActiveCursor = Cursors.Hand,
                HyperlinkFontActive = fontBold
            });
        }

        private void GridRobotUserHitCell(object sender, MouseEventArgs e, int rowIndex, FastColumn col, RobotGridItem gridItem)
        {
            var robot = gridItem.Robot;
            
            // показать меню выбора Magic-a,
            // подсветить в первую очередь, незанятые номера
            if (col.PropertyName == RobotGridItem.speciman.Property(p => p.Magic) && e.Button == MouseButtons.Left)
            {
                ShowMagicMenu(e, gridItem, rowIndex);
                return;
            }

            // показать окно настройки тикеров для робота
            if (col.PropertyName == RobotGridItem.speciman.Property(p => p.HumanRTickers) && e.Button == MouseButtons.Left)
            {
                var dlg = new RobotTimeframesForm(robot.Graphics);
                if (dlg.ShowDialog() != DialogResult.OK) return;
                robot.Graphics = dlg.UpdatedGraphics;
                gridRobot.UpdateRow(rowIndex, gridItem);
                gridRobot.InvalidateCell(col, rowIndex);
                return;
            }

            // настроить робота
            if ((/*col.PropertyName == "TypeName" || */e.Clicks > 1) && e.Button == MouseButtons.Left)
            {
                ShowRobotParamsDialog(robot);
            }
        }

        private void ShowMagicMenu(MouseEventArgs e, RobotGridItem robot, int rowIndex)
        {
            menuMagic.Items.Clear();
            
            var robotMagics = grid.GetBoundValues().Select(r => r.Robot.Magic).OrderBy(m => m).ToList();
            // след. незанятый Magic
            var nextMagic = robotMagics.Count == 0 ? 1 : robotMagics[robotMagics.Count - 1] + 1;

            // найти "дырку" в Magic-ах роботов
            var freeMagic = 0;
            if (robotMagics.Count == 1)
            {
                freeMagic = robotMagics[0] > 1 ? 1 : 0;
            }
            else if (robotMagics.Count > 1)
            {
                for (var j = 1; j < robotMagics.Count; j++)
                {
                    if (robotMagics[j] > (robotMagics[j - 1] + 1))
                    {
                        freeMagic = robotMagics[j - 1] + 1;
                        break;
                    }
                }
            }
            
            // сбросить Magic (снять флаг инициализации)
            if (LivePortfolioMode)
            if (robot.Initialized)
            {
                var menuReset = menuMagic.Items.Add(Localizer.GetString("TitleReset") + " Magic");
                menuReset.Click += MagicReset;
            }

            // сформировать меню Magic-ов
            var magics = new List<int>();
            if (freeMagic > 0) magics.Add(freeMagic);
            magics.Add(nextMagic);
            foreach (var magic in magics)
            {
                var item = menuMagic.Items.Add(magic.ToString());
                item.Click += MagicSelected;
            }
            menuMagic.Tag = rowIndex;
            // показать меню
            menuMagic.Show(gridRobot, e.X, e.Y);
        }

        private void MagicSelected(object sender, EventArgs e)
        {
            MagicSelectedOrReset(sender, e, false);
        }

        private void MagicReset(object sender, EventArgs e)
        {
            MagicSelectedOrReset(sender, e, true);
        }

        private void MagicSelectedOrReset(object sender, EventArgs e, bool reset)
        {
            // назначить роботу Magic
            var rowIndex = (int)menuMagic.Tag;
            var gridItem = grid[rowIndex];
            var robot = gridItem.Robot;

            var senderItem = (ToolStripItem)sender;
            if (!reset)
            {
                var magic = senderItem.Text.ToInt();
                robot.Magic = magic;
            }
            gridItem.Initialized = !reset;
            // обновить таблицу
            grid[rowIndex] = gridItem;
            gridRobot.InvalidateCell(gridRobot.Columns.First(c => c.PropertyName == RobotGridItem.speciman.Property(p => p.Magic)), rowIndex);
        }

        /// <summary>
        /// настроить киборгов
        /// </summary>
        private void ShowRobotParamsDialog(BaseRobot robot)
        {
            var chgRobots = robot == null
                                ? grid.GetBoundValuesFromSelectedRows().Select(r => r.Robot).ToArray()
                                : new [] {robot};
            if (chgRobots.Length == 0) return;
            var dlg = GetRobotPropertiesForm(chgRobots.ToList());
            if (chgRobots.Length == 1)
                dlg.Text = Localizer.GetString("TitleSettings") + " " + chgRobots[0].TypeName;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            gridRobot.DataBind(grid.GetBoundValues().ToList());
            gridRobot.Invalidate();
        }

        private Form GetRobotPropertiesForm(List<BaseRobot> robots)
        {
            if (robots.Count != 1)
                return new ObjectPropertyWindow(robots.Select(r => r as object).ToList(), Localizer.GetString("TitleSelectedRobotsSettings"));
                //return new PropertiesDlg(robots, "Настройки выбранных роботов");

            // открытие специализированных редакторов для роботов
            var robot = robots.First();
            var editorAttribute = robot.GetType().GetCustomAttributes(true).FirstOrDefault(a => a is EditorAttribute) as EditorAttribute;
            var editorType = editorAttribute != null ? Type.GetType(editorAttribute.EditorTypeName) : null;
            var constructor = editorType != null ? editorType.GetConstructor(new Type[0]) : null;
            if (constructor != null)
            {
                var form = (Form) constructor.Invoke(new object[0]);
                var mt4Form = form as Mt4RobotEditorForm;
                if (mt4Form != null)
                {
                    mt4Form.GetUserOwnedAccounts = () => SubscriptionModel.Instance.GetUserOwnedAccounts();
                    mt4Form.Robot = robot as Mt4Robot;
                }
                return form;
            }

            // открытие обычного редактора
            return new ObjectPropertyWindow(new List<object> { robot }, Localizer.GetString("TitleSelectedRobotsSettings"));
            //return new PropertiesDlg(robot, "Настройки робота");
        }

        private void LbRobotsCollectionDoubleClick(object sender, EventArgs e)
        {
            // добавить робота
            if (lbRobotsCollection.SelectedIndex == -1) return;
            var item = lbRobotsCollection.SelectedItem;
            AddRobotInPortfolio((string) item);            
        }

        private void AddRobotInPortfolio(string robotName)
        {
            var robot = RobotCollection.MakeRobot(robotName);
            robot.Magic = gridRobot.rows.Count + 1;
            var allRobots = grid.GetBoundValues().ToList();

            var robotInitialized = !LivePortfolioMode;
            allRobots.Add(new RobotGridItem(robot, robotInitialized));
            gridRobot.DataBind(allRobots);
            gridRobot.Invalidate();
        }

        private void BtnSelectRobotClick(object sender, EventArgs e)
        {
            LbRobotsCollectionDoubleClick(sender, e);
        }

        /// <summary>
        /// удалить выбранных роботов
        /// </summary>
        private void BtnUnselectRobotClick(object sender, EventArgs e)
        {
            var robotsCleared = grid.GetBoundValues(false).ToList();
            gridRobot.DataBind(robotsCleared);
            gridRobot.Invalidate();
            SetMagicByRobotIndex();
        }

        /// <summary>
        /// назначить роботам magic по порядку
        /// </summary>
        private void SetMagicByRobotIndex()
        {
            var allRobots = grid.GetBoundValues().ToList();
            if (allRobots.Count == 0) return;
            for (var i = 0; i < allRobots.Count; i++)
            {
                allRobots[i].Robot.Magic = i + 1;
            }
            gridRobot.DataBind(allRobots);
            gridRobot.Invalidate();
        }

        private void BtnPropertiesRobotsClick(object sender, EventArgs e)
        {
            ShowRobotParamsDialog(null);
        }

        private void RobotPortfolioSetupControlLoad(object sender, EventArgs e)
        {
            foreach (var name in RobotCollection.RobotNames)
            {
                lbRobotsCollection.Items.Add(name);
            }
        }

        private void BtnSavePropertiesClick(object sender, EventArgs e)
        {
            var dirName = EnsureRobotsDir();
            if (string.IsNullOrEmpty(dirName)) return;

            var allRobots = GetUsedRobots();
            saveRobotsPortfolioDlg.InitialDirectory = dirName;
            if (saveRobotsPortfolioDlg.ShowDialog() == DialogResult.OK)
            {
                RobotFarm.SaveRobots(allRobots, saveRobotsPortfolioDlg.FileName);
                lastSavedPath = saveRobotsPortfolioDlg.FileName;
            }            
        }

        private string EnsureRobotsDir()
        {
            var dirName = ExecutablePath.ExecPath + TerminalEnvironment.RobotCacheFolder;
            if (!Directory.Exists(dirName))
            {
                try
                {
                    Directory.CreateDirectory(dirName);
                }
                catch
                {
                    MessageBox.Show(
                        string.Format(Localizer.GetString("MessageUnableToCreateRobotsDirFmt"),
                                      dirName), 
                                      Localizer.GetString("TitleError"), 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            return dirName;
        }

        private void BtnReadPropertiesClick(object sender, EventArgs e)
        {
            var dirName = ExecutablePath.ExecPath + TerminalEnvironment.RobotCacheFolder;
            if (!Directory.Exists(dirName))
            {
                return;
            }

            openRobotsPortfolioDlg.InitialDirectory = dirName;
            if (openRobotsPortfolioDlg.ShowDialog() == DialogResult.OK)
            {
                ReadRobotsSettings(openRobotsPortfolioDlg.FileName);
                lastSavedPath = openRobotsPortfolioDlg.FileName;
            }
        }
    
        private void LbRobotsCollectionMouseDown(object sender, MouseEventArgs e)
        {
            var selIndex = lbRobotsCollection.IndexFromPoint(e.X, e.Y);
            if (selIndex < 0) return;
            var robotName = (string) lbRobotsCollection.Items[selIndex];

            if (e.Button == MouseButtons.Left)
                lbRobotsCollection.DoDragDrop(robotName, DragDropEffects.Copy);
        }

        private void GridRobotDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
        }

        private void GridRobotDragDrop(object sender, DragEventArgs e)
        {
            var robotName = e.Data.GetData(DataFormats.Text).ToString();
            AddRobotInPortfolio(robotName);
        }
    }

    /// <summary>
    /// класс байндится в таблице
    /// нужен, например, для того, чтобы добавить "новосозданного" робота
    /// </summary>
    class RobotGridItem
    {
        public static RobotGridItem speciman = new RobotGridItem(new RobotMA());

        public BaseRobot Robot { get; set; }

        public bool Initialized { get; set; }

        public int Magic
        {
            get 
            { 
                if (!Initialized) return 0;
                return Robot.Magic;
            }
        }

        public string TypeName
        {
            get { return Robot.TypeName; }
        }

        public string HumanRTickers
        {
            get { return Robot.HumanRTickers; }
            set { Robot.HumanRTickers = value; }
        }

        public RobotGridItem(BaseRobot robot)
        {
            Robot = robot;
        }

        public RobotGridItem(BaseRobot robot, bool initialized)
        {
            Robot = robot;
            Initialized = initialized;
        }
    }
}
