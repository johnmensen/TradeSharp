using System.Windows.Forms;
using TradeSharp.Client.BL;
using TradeSharp.Util;

namespace TradeSharp.Client.Forms
{
    public partial class RobotPortfolioForm : Form
    {
        private readonly RobotFarm robotFarm;

        private readonly string robotToShowSettingsUniqueName;

        public RobotPortfolioForm()
        {
            InitializeComponent();
            Localizer.LocalizeControl(this);
        }

        public RobotPortfolioForm(RobotFarm robotFarm) : this()
        {
            this.robotFarm = robotFarm;
        }

        public RobotPortfolioForm(RobotFarm robotFarm, string robotUniqieName)
            : this(robotFarm)
        {
            robotToShowSettingsUniqueName = robotUniqieName; // robotFarm.GetRobotCopies().FirstOrDefault(r => r.GetUniqueName() == robotUniqieName);
        }

        private void BtnAcceptClick(object sender, System.EventArgs e)
        {
            // заменить роботов на ферме
            if (robotFarm.State != RobotFarm.RobotFarmState.Stopped)
            {
                MessageBox.Show(Localizer.GetString("MessageUnableApplySettingsWhileRobotsAreStarted"));
                return;
            }
            // поправить номера Magic
            robotPortfolioControl.AssignFreeMagicsToRobots();

            // сохранить настройки роботов по дефолтовому пути
            var robots = robotPortfolioControl.GetUsedRobots();
            MainForm.Instance.RobotFarm.SetRobotSettings(robots);
            RobotFarm.SaveRobots(robots);
            Close();
        }

        private void RobotPortfolioFormLoad(object sender, System.EventArgs e)
        {
            // показать настройки киборгов
            ShowDefaultRobotsSettings();

            if (robotFarm.State != RobotFarm.RobotFarmState.Stopped)
            {
                btnAccept.Enabled = false;
                robotPortfolioControl.Enabled = false;
            }

            // показать настроки переданного по имени робота
            if (!string.IsNullOrEmpty(robotToShowSettingsUniqueName))
                robotPortfolioControl.ShowDialogforRobotGivenByItsUniqueName(robotToShowSettingsUniqueName);
        }

        private void ShowDefaultRobotsSettings()
        {
            robotPortfolioControl.BindRobots(RobotFarm.LoadRobots());
        }
    }
}
