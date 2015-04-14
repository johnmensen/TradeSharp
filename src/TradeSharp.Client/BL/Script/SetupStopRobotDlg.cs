using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TradeSharp.Robot.Robot;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    public partial class SetupStopRobotDlg : Form
    {
        private readonly List<StopRobot> robots;
        
        private readonly double price;

        public decimal SelectedPrice { get; private set; }

        public StopRobot SelectedRobot { get; private set; }

        public StopRobot.PriceSide SelectedSide { get; private set; }

        public SetupStopRobotDlg()
        {
            InitializeComponent();
        }

        public SetupStopRobotDlg(List<StopRobot> robots, double price)
        {
            InitializeComponent();
            this.robots = robots;
            this.price = price;
        }

        private void SetupStopRobotDlgLoad(object sender, EventArgs e)
        {
            // заполнить комбики
            foreach (StopRobot.PriceSide side in Enum.GetValues(typeof(StopRobot.PriceSide)))
                cbPriceType.Items.Add(side);
            cbPriceType.SelectedIndex = 0;

            var number = 1;
            var robotNames = robots.Select(r => (object)string.Format("Робот {0} ({1})", number++,
                r.StopLevel == 0 ? "0" : r.StopLevel.ToStringUniformPriceFormat())).ToArray();
            cbRobots.Items.AddRange(robotNames);
            cbRobots.SelectedIndex = 0;

            tbPrice.Value = (decimal) price;
            tbPrice.DecimalPlaces = price > 25 ? 2 : price > 7 ? 3 : 4;
            tbPrice.Increment = (decimal) Math.Pow(10, -tbPrice.DecimalPlaces);
        }

        private void BtnOKClick(object sender, EventArgs e)
        {
            SelectedPrice = tbPrice.Value;
            SelectedSide = (StopRobot.PriceSide) cbPriceType.SelectedItem;
            SelectedRobot = robots[cbRobots.SelectedIndex];

            DialogResult = DialogResult.OK;
        }
    }
}
