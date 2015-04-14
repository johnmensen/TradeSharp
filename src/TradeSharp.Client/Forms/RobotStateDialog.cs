using System;
using System.ComponentModel;
using System.Windows.Forms;
using FastGrid;
using TradeSharp.Client.BL;
using TradeSharp.Robot.BL;
using TradeSharp.Robot.Robot;
using System.Linq;

namespace TradeSharp.Client.Forms
{
    public partial class RobotStateDialog : Form
    {
        private readonly RobotFarm happyFarm;

        public RobotStateDialog()
        {
            InitializeComponent();
            InitGrid();
        }

        public RobotStateDialog(RobotFarm happyFarm) : this()
        {
            this.happyFarm = happyFarm;
            var robots = happyFarm.GetRobotsAsIs(); //GetRobotCopies();
            gridRobot.DataBind(robots);
        }

        public RobotStateDialog(RobotFarm happyFarm, string selectedrobotUniqueName)
            : this(happyFarm)
        {
            if (!string.IsNullOrEmpty(selectedrobotUniqueName))
            {
                var robot = gridRobot.rows.Select(r => (BaseRobot) r.ValueObject).FirstOrDefault(r =>
                                                                                                 r.GetUniqueName() ==
                                                                                                 selectedrobotUniqueName);
                if (robot != null)
                    propertyGrid.SelectedObject = robot;
            }
        }

        private void InitGrid()
        {
            gridRobot.Columns.Add(new FastColumn("TypeName", "Робот")
                                      {
                                          SortOrder = FastColumnSort.Ascending,
                                          ColumnMinWidth = 60
                                      });
            gridRobot.Columns.Add(new FastColumn("HumanRTickers", "Графики"));            
            gridRobot.CalcSetTableMinWidth();
            gridRobot.UserHitCell += (obj, args, index, col) =>
                                         {
                                             var robot = (BaseRobot) gridRobot.rows[index].ValueObject;
                                             propertyGrid.SelectedObject = robot;
                                         };

            // property grid
            propertyGrid.BrowsableAttributes = new AttributeCollection(
                new Attribute[] { new RuntimeAccessAttribute(true)});
        }
    }
}
