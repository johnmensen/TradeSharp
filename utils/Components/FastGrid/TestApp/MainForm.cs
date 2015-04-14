using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FastGrid;
using TestApp.BL;

namespace TestApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void BtnTestClick(object sender, EventArgs e)
        {
            var tmStart = DateTime.Now;
            for (var i = 0; i < 100; i++ )
            {
                fastGrid.ScrollTo(100);
                fastGrid.Invalidate();
                fastGrid.ScrollTo(00);
                fastGrid.Invalidate();
            }            
            var deltaMils = (int)(DateTime.Now - tmStart).TotalMilliseconds;
            MessageBox.Show("Invalidating lasted " + deltaMils + " mils.");          
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            fastCombo.AddColumn(new FastColumn("FirstName", "Name"));
            fastCombo.AddColumn(new FastColumn("LastName", "Surname"));
            fastCombo.DataBind(Person.MakePeople(10));

            var crowd = Person.MakePeople(20);
            fastGrid.GroupingFunctions = new List<FastGrid.FastGrid.GroupingFunctionDel> { GroupByGender };
            fastGrid.GroupingComparisons = new List<Comparison<object>>
                {
                    (a, b) => (int) a - (int) b
                };
            fastGrid.DataBind(crowd, typeof(Person), false, new[] { "male", "female", "-" }, 65, "dd.MM.yyyy");
        }

        private void GroupByGender(object person, out object groupData, out string groupLabel)
        {
            groupData = 0;
            groupLabel = "";
            if (person == null)
                return;
            var p = person as Person;
            if (p == null)
                return;
            if (!p.IsMale.HasValue)
                return;
            groupData = p.IsMale.Value ? 1 : 2;
            groupLabel = p.IsMale.Value ? "Males" : "Females";
        }
    }    
}
