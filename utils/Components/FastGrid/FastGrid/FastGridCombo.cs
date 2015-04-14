using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastGrid
{
    public class FastGridCombo : UserControl
    {
        public delegate void GridObjectSelectedDel(object obj);

        #region Constants
        public const int ButtonWidth = 30;
        public const int BoxMinWidth = 20;
        public const int MinWidth = 50;
        #endregion

        public readonly List<FastColumn> columns = new List<FastColumn>();

        public TextBox textBox;

        public Button button;

        #region Visual props
        public string ButtonText
        {
            get { return button.Text; }
            set { button.Text = value; }
        }

        public string SelectedText
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        private int minDropHeight = 30;
        public int MinDropHeight
        {
            get { return minDropHeight; }
            set { minDropHeight = value; }
        }

        private int maxDropHeight = 200;
        public int MaxDropHeight
        {
            get { return maxDropHeight; }
            set { maxDropHeight = value; }
        }

        private int gridItemHeight = 20;
        public int GridItemHeight
        {
            get { return gridItemHeight; }
            set { gridItemHeight = value; }
        }

        public int MinTableWidth { get; set; }

        public int FixedTableWidth { get; set; }

        private Color cellBackColor = Color.White;
        public Color CellBackColor
        {
            get { return cellBackColor; }
            set { cellBackColor = value; }
        }

        private Color altBackColor = Color.WhiteSmoke;
        public Color AltBackColor
        {
            get { return altBackColor; }
            set { altBackColor = value; }
        }

        private Color fontColor = Color.Black;
        public Color FontColor
        {
            get { return fontColor; }
            set { fontColor = value; }
        }
        #endregion

        #region Events

        private GridObjectSelectedDel gridObjectSelected;
        public event GridObjectSelectedDel GridObjectSelected
        {
            add { gridObjectSelected += value; }
            remove { gridObjectSelected -= value; }
        }
        #endregion

        /// <summary>
        /// bound objects
        /// </summary>
        public IList objects;

        /// <summary>
        /// selected object (row) - set up by control itself
        /// </summary>
        private object selectedObject;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedObject
        {
            get { return selectedObject; }
            set
            {
                selectedObject = value;
                textBox.Text = value == null ? string.Empty : value.ToString();
            }
        }

        public FastGridCombo()
        {
            textBox = new TextBox { Parent = this };
            button = new Button {Parent = this, Text = "..."};
            button.Click += ButtonClick;
            Controls.Add(textBox);
            Controls.Add(button);
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var height = objects == null
                              ? MinDropHeight
                              : (objects.Count + 1) * gridItemHeight;
            if (height < MinDropHeight) height = MinDropHeight;
            if (height > MaxDropHeight) height = MaxDropHeight;

            var location = PointToScreen(new Point(0, textBox.Bottom));
            var dlg = new GridDropDialog(location.X, location.Y, Width, height, 
                MinTableWidth, FixedTableWidth, 
                CellBackColor, AltBackColor, FontColor,
                columns, objects, SelectedObject);
            dlg.gridObjectSelected += ObjectSelected;
            dlg.Show();            
        }

        private void ObjectSelected(object obj)
        {
            SelectedObject = obj;
            if (gridObjectSelected != null)
                gridObjectSelected(obj);
        }
        
        public void AddColumn(FastColumn col)
        {
            columns.Add(col);
        }

        public void DataBind(IList objList)
        {
            objects = objList;
            SelectedObject = null;
        }

        public void SelectObject(Func<object, bool> predicate)
        {
            if (objects == null || objects.Count == 0) return;
            foreach (var o in objects)
            {
                if (predicate(o))
                {
                    SelectedObject = o;
                    break;
                }
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            // arrange controls
            if (Width < MinWidth)
            {
                textBox.SetBounds(0, 1, BoxMinWidth, Height);
                button.SetBounds(0, 0, 0, 0);
                return;
            }
            var wdBox = Width - ButtonWidth;
            textBox.SetBounds(0, 1, wdBox, Height);
            button.SetBounds(wdBox, 0, ButtonWidth, Height);
            
            base.OnSizeChanged(e);
        }
    }
}
