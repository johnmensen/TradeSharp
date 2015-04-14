using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastGrid;

namespace TestApp
{
    public partial class TestForm1 : Form
    {
        public TestForm1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var singleValueData = new List<Util.Cortege2<string, string>>();
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Сделок всего", b = "1" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Сделок открытых", b = "2" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Ср. прибыль / ср. убыток", b = "3" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Макс. проседание, %", b = "4" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Макс. плечо", b = "5" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "С.геом. доходность (месяц), %", b = "6" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "С.геом. доходность (год), %", b = "7" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "К. Шарпа", b = "8" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Доходность, %", b = "9" });
            singleValueData.Add(new Util.Cortege2<string, string> { a = "Кол-во подписчиков", b = "10" });
            singleParametersFastGrid.DataBind(singleValueData);
        }

        private void TestForm1_Load(object sender, EventArgs e)
        {
            singleParametersFastGrid.Columns.Add(new FastColumn("a", "Название") { ColumnWidth = 200 });
            singleParametersFastGrid.Columns.Add(new FastColumn("b", "Значение"));
        }
    }
}
