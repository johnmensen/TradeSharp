using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using FastGrid;
using TestApp.BL;

namespace TestApp
{
    public partial class ManualSetUpGridForm : Form
    {
        public ManualSetUpGridForm()
        {
            InitializeComponent();
        }

        private void SetupGrid()
        {
            var pers = Person.MakePeople(500);
            pers[0].FirstName = "Эйяфьятлайокудль";
            pers[1].FirstName = "МужикСНереальноДлиннымИменемТакимЧтоНеВлезаетВКолонку";
            grid.Columns.Add(new FastColumn("FirstName", "Name")
                                 {
                                     ColumnWidth = 70,
                                     SortOrder = FastColumnSort.Ascending,
                                     IsEditable = true,
                                     ShowClippedContent = true
                                 });
            grid.Columns.Add(new FastColumn("LastName", "Surname")
                                 {
                                     ColumnMinWidth = 70,
                                     SortOrder = FastColumnSort.Ascending
                                 });

            grid.Columns.Add(new FastColumn("IsMale", "Gender")
                                 {
                                     ColumnWidth = 60,
                                     // format cell value: "-" if gender is not specified, "male" for true, "female" for false
                                     formatter = c => c == null ? "-" : (bool)c ? "Male" : "Female"
                                 });

            grid.Columns.Add(new FastColumn("Occupation", "Occupation")
                                 {
                                     ColumnWidth = 70,
                                     IsHyperlinkStyleColumn = true,
                                     HyperlinkActiveCursor = Cursors.Hand,
                                     HyperlinkFontActive = new Font(Font, FontStyle.Bold),
                                     ColorHyperlinkTextActive = Color.Blue,
                                     // highlight unemployed
                                     colorColumnFormatter = (object c, out Color? back, out Color? fnt) =>
                                        {
                                            back = null;
                                            fnt = ((Person.PersonOccupation)c) == Person.PersonOccupation.None ? Color.DarkBlue : Color.Black;
                                        },
                                     IsEditable = true
                                 });
            grid.Columns.Add(new FastColumn("Password", "Password")
            {
                ColumnMinWidth = 70
            });

            grid.Columns.Add(new FastColumn("Rating", "Rating") {IsEditable = true});
            grid.Columns.Add(new FastColumn("AccessColor"));

            grid.UserHitCell += GridUserHitCell;

            grid.CalcSetTableMinWidth();
            grid.DataBind(pers);
        }

        private void GridUserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            /*if (col.PropertyName != "Occupation") return;
            var pers = (Person) grid.rows[rowIndex].ValueObject;
            contextMenu.Tag = pers;
            contextMenu.Show(grid, e.X, e.Y);*/
        }

        private void SetupMenu()
        {
            foreach (Person.PersonOccupation val in Enum.GetValues(typeof(Person.PersonOccupation)))
            {
                var item = contextMenu.Items.Add(val.ToString());
                item.Tag = val;
                item.Click += ItemClick;
            }
        }

        private void ItemClick(object sender, EventArgs e)
        {
            var selOccupation = (Person.PersonOccupation)((ToolStripItem)sender).Tag;
            var pers = (Person) contextMenu.Tag;
            pers.Occupation = selOccupation;

            var rowIndex = grid.rows.FindIndex(r => r.ValueObject == pers);
            var col = grid.Columns.FindIndex(c => c.PropertyName == "Occupation");
            // force grid to update row's text
            grid.UpdateRow(rowIndex, pers);
            // force redrawing cell
            grid.InvalidateCell(col, rowIndex);
        }

        private void ManualSetUpGridFormLoad(object sender, EventArgs e)
        {
            SetupGrid();
            SetupMenu();
        }

        private void UnusedMethod()
        {
            // get selected persons as a list
            var selectedPeople = grid.rows.Where(r => r.Selected).Select(r => (Person) r.ValueObject).ToList();
        }
    }
}
