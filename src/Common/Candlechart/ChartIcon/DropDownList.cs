using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Candlechart.Chart;
using FastGrid;
using BrushesStorage = Entity.BrushesStorage;

namespace Candlechart.ChartIcon
{
    public class DropDownList : Control
    {
        /// <summary>
        /// если объекты списка реализуют IChartIconDropDownRow,
        /// отображение колонок настроено в соответствии со списком columns
        /// </summary>
        public List<FastColumn> columns;

        public delegate void CellClickedDel(object selObj, string selText);

        public int MaxLines = 10;

        public int MinWidth = 60;

        public int MaxWidth = 200;

        private const int ScrollBarWidth = 12;

        private const int MinPadHeight = 12;

        private const int MinPadPadding = 12;

        public delegate string FormatValueDel(object val);

        private List<object> values;

        public List<object> Values
        {
            get { return values; }
            set
            {
                values = value;
                if (CalcHeightAuto)
                {
                    var count = values == null ? 0 : values.Count;
                    if (count > MaxLines) count = MaxLines;
                    Height = cellHeight * count;
                }
                if (CalcWidthAuto)
                    MeasureWidth();
                FormatValues();
            }
        }

        private List<ChartIconDropDownRowCells> formattedValues;

        private List<ChartIconDropDownRowCells> displayedValuesAndButtons;

        private bool btnUpShown, btnDownShown;

        private FormatValueDel formatter;

        public FormatValueDel Formatter
        {
            set
            {
                formatter = value;
                FormatValues();
            }
            get { return formatter; }
        }

        public CellClickedDel cellClicked;

        public Action closeControl;

        #region Визуальные настройки
        private int cellHeight = 16;
        public int CellHeight
        {
            get { return cellHeight; }
            set { cellHeight = value; }
        }

        private int paddingLeft = 5;
        public int PaddingLeft
        {
            get { return paddingLeft; }
            set { paddingLeft = value; }
        }

        private Font cellFont;

        public Font CellFont
        {
            get { return cellFont ?? Font; }
            set { cellFont = value; }
        }
        #endregion

        public bool CalcHeightAuto { get; set; }

        public bool CalcWidthAuto { get; set; }

        private int rowsDisplayed;

        private Rectangle? scrollPadRect;

        private Point scrollStartPoint;

        private bool scrollingByMouse;
        
        private int startItemIndex;

        private int selectedCellIndex;

        public int SelectedCellIndex
        {
            get { return selectedCellIndex; }
        }

        #region Цветовая схема
        private Color clCellBack = Color.FromArgb(192, 230, 230, 230);
        public Color ClCellBack
        {
            get { return clCellBack; }
            set { clCellBack = value; }
        }

        private Color clCellBackHl = Color.GhostWhite;
        public Color ClCellBackHl
        {
            get { return clCellBackHl; }
            set { clCellBackHl = value; }
        }

        private Color clCellFont = Color.FromArgb(77, 77, 77);
        public Color ClCellFont
        {
            get { return clCellFont; }
            set { clCellFont = value; }
        }

        private Color clCellFontHl = Color.Black;
        public Color ClCellFontHl
        {
            get { return clCellFontHl; }
            set { clCellFontHl = value; }
        }
        #endregion

        public ChartControl owner;

        public DropDownList()
        {
            DoubleBuffered = true;
        }

        public DropDownList(DropDownList lst) : this()
        {
            values = lst.values == null ? null : lst.values.ToList();
            Formatter = lst.Formatter;
            cellHeight = lst.cellHeight;
            paddingLeft = lst.paddingLeft;
            cellFont = lst.cellFont;
            CalcHeightAuto = lst.CalcHeightAuto;
            CalcWidthAuto = lst.CalcWidthAuto;
            startItemIndex = lst.startItemIndex;
            selectedCellIndex = lst.selectedCellIndex;
            clCellBack = lst.clCellBack;
            clCellFont = lst.clCellFont;
            clCellFontHl = lst.clCellFontHl;
            clCellBackHl = lst.clCellBackHl;
            CalcWidthAuto = lst.CalcWidthAuto;
            CalcHeightAuto = lst.CalcHeightAuto;
            MinWidth = lst.MinWidth;
            MaxWidth = lst.MaxWidth;

            Width = lst.Width;
            Height = lst.Height;
            DoubleBuffered = true;
            columns = lst.columns == null ? null : lst.columns.ToList();
        }

        private void FormatValues()
        {
            if (values == null || values.Count == 0)
            {
                formattedValues = null;
                return;
            }
            formattedValues = Formatter != null
                                  ? values.Select(v => new ChartIconDropDownRowCells(v, formatter)).ToList()
                                  : values.Select(
                                      v => new ChartIconDropDownRowCells(v, val => val.ToString())).ToList();
            // настроить колонки
            if (columns != null && columns.Count > 0) return;
            var speciman = formattedValues[0];
            columns = speciman.cells.Select(c => new FastColumn("empty")
                {
                    ColumnWidth = Width/speciman.cells.Length
                }).ToList();
        }

        /// <summary>
        /// нарисовать выпадающий список
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (values == null || values.Count == 0) return;
            
            // посчитать количество строк
            var numLines = Height / cellHeight;
            if (values.Count <= numLines)
                startItemIndex = 0;
            
            // отображать стрелочки вниз - вверх?
            btnUpShown = startItemIndex > 0;
            if (btnUpShown) numLines--;
            btnDownShown = numLines < (values.Count - startItemIndex);

            // таки нарисовать
            DrawList(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (owner != null)
                using (var brush = new SolidBrush(owner.visualSettings.PaneBackColor))
                {
                    pevent.Graphics.FillRectangle(brush, 0, 0, Width, Height);
                    return;
                }
            base.OnPaintBackground(pevent);
        }

        /// <summary>
        /// подсветить определенную ячейку
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (scrollingByMouse && scrollPadRect.HasValue)
            {
                // скроллить
                var deltaY = e.Y - scrollStartPoint.Y;
                var padTop = deltaY + scrollPadRect.Value.Top;
                var startIndex = CalculateStartItemIndexByScrollPad(padTop);
                if (startItemIndex == startIndex) return;
                scrollStartPoint.Y = e.Y;
                startItemIndex = startIndex;
                Invalidate();
                return;
            }

            var row = GetSelectedRow(e.Y);
            if (selectedCellIndex == row) return;
            selectedCellIndex = row;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            if (displayedValuesAndButtons.Count == 0) return;

            // скроллбар?
            if (scrollPadRect.HasValue && scrollPadRect.Value.Contains(e.X, e.Y))
            {
                scrollingByMouse = true;
                scrollStartPoint = new Point(e.X, e.Y);
                return;
            }

            var row = GetSelectedRow(e.Y);
            if (row < 0 || row >= displayedValuesAndButtons.Count) return;
            var dispValue = displayedValuesAndButtons[row];
            
            // если нажата кнопка вверх - вниз...
            if (dispValue.TypeOfRow == ChartIconDropDownRowCells.RowType.ArrowUp)
            {
                startItemIndex--;
                Invalidate();
                return;
            }
            if (dispValue.TypeOfRow == ChartIconDropDownRowCells.RowType.ArrowDown)
            {
                startItemIndex++;
                Invalidate();
                return;
            }

            var datIndex = row + startItemIndex;
            if (startItemIndex > 0)
                datIndex--; // учесть отображаемую кнопку вверх
            if (cellClicked != null)
                cellClicked(values[datIndex], dispValue.ToString());
            closeControl();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            scrollingByMouse = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            var shift = -Math.Sign(e.Delta);
            if (shift == 0) return;
            if ((shift < 0 && !btnUpShown) ||
                (shift > 0 && !btnDownShown)) return;

            startItemIndex += shift;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Focus();
        }

        private int GetSelectedRow(int y)
        {
            return y / cellHeight;
        }

        private void DrawList(Graphics g)
        {
            // подготовить массив строк displayedValuesAndButtons
            displayedValuesAndButtons = new List<ChartIconDropDownRowCells>();
            var linesCount = Height / cellHeight;
            var linesSpare = linesCount;
            if (btnUpShown) linesSpare--;
            if (btnDownShown) linesSpare--;
            rowsDisplayed = Math.Min(linesSpare, formattedValues.Count - startItemIndex);

            if (btnUpShown) 
                displayedValuesAndButtons.Add(new ChartIconDropDownRowCells(ChartIconDropDownRowCells.RowType.ArrowUp));
            for (var i = 0; i < rowsDisplayed; i++)
            {                
                displayedValuesAndButtons.Add(formattedValues[i + startItemIndex]);
            }
            if (btnDownShown) 
                displayedValuesAndButtons.Add(new ChartIconDropDownRowCells(ChartIconDropDownRowCells.RowType.ArrowDown));

            // подготовить словарь шрифтов
            var fonts =
                displayedValuesAndButtons.Where(c => c.cells != null).SelectMany(c => c.cells.Select(cell => cell.FontStyle ?? default(FontStyle)))
                                         .Distinct()
                                         .ToDictionary(
                                             s => s, s => s == default(FontStyle) ? CellFont : new Font(CellFont, s));
            // нарисовать обычные ячейки с текстом
            using (var brushes = new BrushesStorage())
            {
                // фон
                var brush = brushes.GetBrush(clCellBack);
                g.FillRectangle(brush, 0, 0, Width - 1, Height - 1);

                // ячейки
                for (var i = 0; i < displayedValuesAndButtons.Count; i++)
                {
                    DrawCell(g, fonts, i == selectedCellIndex, brushes, i, displayedValuesAndButtons[i], rowsDisplayed);
                }

                // скроллинг
                scrollPadRect = null;
                if (btnDownShown || btnUpShown) 
                    DrawScrollBar(g, brushes);
            }
        }

        private void DrawScrollBar(Graphics g, BrushesStorage brushes)
        {
            // нарисовать фон
            var brushBack = brushes.GetBrush(clCellBack);
            using (var pen = new Pen(ClCellFont))
            {
                var rect = new Rectangle(Width - ScrollBarWidth - 1, 0, ScrollBarWidth, Height - 1);
                g.FillRectangle(brushBack, rect);
                g.DrawRectangle(pen, rect);
      
                // нарисовать пипочку для перетягивания
                CalcScrollPadRect();
                g.FillRectangle(brushBack, scrollPadRect.Value);
                g.DrawRectangle(pen, scrollPadRect.Value);
            }
        }

        private void CalcScrollPadRect()
        {
            var padHeight = (Height - 1) * rowsDisplayed / values.Count;
            if (padHeight < MinPadHeight) padHeight = MinPadHeight;
            else
            {
                if ((Height - padHeight) < MinPadPadding)
                    padHeight = Height - MinPadPadding;
            }
            var padY = startItemIndex * (Height - 1) / values.Count;
            scrollPadRect = new Rectangle(Width - ScrollBarWidth + 1, padY, ScrollBarWidth - 4, padHeight);
        }

        private int CalculateStartItemIndexByScrollPad(int padTop)
        {
            var index = padTop*values.Count / (Height - 1);
            if (index < 0) index = 0;
            var delta = values.Count - index;
            if (delta < rowsDisplayed)
                index = values.Count - rowsDisplayed;
            return index;
        }

        private void DrawCell(Graphics g, Dictionary<FontStyle, Font>  fonts,
            bool highlighted, BrushesStorage brushes, int y, ChartIconDropDownRowCells cells, int rowsDisplayed)
        {
            DrawBack(g, highlighted, brushes, y);
            var top = y * cellHeight;
            var cy = top + cellHeight / 2;

            var itemsLeftAbove = startItemIndex;
            var itemsLeftBelow = formattedValues.Count - rowsDisplayed - startItemIndex - 1;

            if (cells.TypeOfRow == ChartIconDropDownRowCells.RowType.ArrowUp)
            {
                DrawArrowUpDown(g, highlighted, brushes, true, cy, itemsLeftAbove);
                return;
            }
            if (cells.TypeOfRow == ChartIconDropDownRowCells.RowType.ArrowDown)
            {
                DrawArrowUpDown(g, highlighted, brushes, false, cy, itemsLeftBelow);
                return;
            }

            // вписать в ячеи текст
            var leftPos = paddingLeft;
            for (var i = 0; i < cells.cells.Length; i++)
            {
                var col = columns[i];
                DrawText(g, fonts, highlighted, brushes, y, cells.cells[i], leftPos);
                leftPos += col.ColumnWidth;
            }
        }

        private void DrawBack(Graphics g, bool highlighted, BrushesStorage brushes, int y)
        {
            if (!highlighted) return;
            
            // подсветить ячейку
            var top = y * cellHeight;
            var brush = brushes.GetBrush(clCellBackHl);
            var area = new Rectangle(0, top, Width - 1, cellHeight - 1);
            g.FillRectangle(brush, area);
            
            // обвести
            using (var pen = new Pen(ClCellFontHl))
            {
                g.DrawRectangle(pen, area);
            }
        }

        private void DrawText(Graphics g, Dictionary<FontStyle, Font> fonts,
            bool highlighted, BrushesStorage brushes, int y,
            ChartIconDropDownCell cell, int left)
        {
            var top = y * cellHeight;
            var cy = top + cellHeight / 2;
            
            // вывести текст
            var fontBrush = brushes.GetBrush(!highlighted 
                ? cell.ColorFont ?? ClCellFont
                : ClCellFontHl);
            g.DrawString(cell.CellString, fonts[cell.FontStyle ?? default(FontStyle)], fontBrush, left, cy, 
                new StringFormat { LineAlignment = StringAlignment.Center });
        }

        private void DrawArrowUpDown(Graphics g, bool highlighted, BrushesStorage brushes, bool up, int cy,
            int itemsLeft)
        {
            var cx = Width / 2;
            const int aSzM = 3, sSzL = 3;
            var points = up
                             ? new[] { new Point(cx, cy - aSzM), new Point(cx + aSzM, cy + sSzL), new Point(cx - aSzM, cy + sSzL) }
                             : new[] { new Point(cx, cy + aSzM), new Point(cx + aSzM, cy - sSzL), new Point(cx - aSzM, cy - sSzL) };
            var brush = brushes.GetBrush(!highlighted ? ClCellFont : ClCellFontHl);
            g.FillPolygon(brush, points);
            
            // справа подписать - сколько осталось
            g.DrawString(itemsLeft.ToString(), CellFont, brush, cx + aSzM + 3, cy, new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                });
        }

        private void MeasureWidth()
        {
            if (values == null || values.Count == 0) return;
            FormatValues();
            var wd = MinWidth;
            var font = CellFont;

            using (var g = CreateGraphics())
            {
                foreach (var str in formattedValues)
                {
                    var sz = g.MeasureString(str.ToString(), font);
                    var curWd = paddingLeft*2 + (int)sz.Width + 1;
                    if (curWd > wd) wd = curWd;
                }
            }
            if (wd > MaxWidth) wd = MaxWidth;
            Width = wd;
        }
    }

    public class DropDownListPopup : ToolStripDropDown
    {
        public DropDownListPopup(DropDownList list)
        {
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            AutoSize = false;
            Width = list.Width;
            Height = list.Height;
            list.closeControl += Close;

            var host = new ToolStripControlHost(list)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false                
            };
            Items.Add(host);
        }        
    }
}
