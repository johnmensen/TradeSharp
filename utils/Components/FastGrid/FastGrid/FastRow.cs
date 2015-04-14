using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FastGrid
{
    public class FastRow
    {
        /// <summary>
        /// row anchor mode (stick to top, stick to bottom)
        /// </summary>
        public enum AnchorPosition { NoAnchor = 0, AnchorTop = 1, AnchorBottom = 2 }

        public object ValueObject { get; set; }

        public bool Selected { get; set; }

        public List<FastCell> cells = new List<FastCell>();

        public Color? BackgroundColor { get; set; }

        public Color? FontColor { get; set; }

        public AnchorPosition anchor;

        private readonly FastGrid owner;

        // признак строки группирования
        public bool IsGroupingRow;

        // родительская строка группирования
        public FastRow OwnerGroupingRow;

        // для строк группирования указывает, скрыты ли строки, входящие в эту группу
        public bool Collapsed;

        private readonly StringFormat cellTextFormatHCenter =
            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        private readonly StringFormat cellTextFormatHNear =
            new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

        private readonly StringFormat cellTextFormatHFar =
            new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

        private DrawCellTextDel userDrawCellText;
        public event DrawCellTextDel UserDrawCellText
        {
            add { userDrawCellText += value; }
            remove { userDrawCellText -= value; }
        }

        public FastRow(FastGrid owner, object obj, Dictionary<FastColumn, PropertyInfo> columnProperty)
        {
            this.owner = owner;
            ValueObject = obj;
            if (owner.colorFormatter != null)
            {
                Color? clBack, clFont;
                owner.colorFormatter(ValueObject, out clBack, out clFont);
                BackgroundColor = clBack;
                FontColor = clFont;
            }

            string[] strings = null;
            if (owner.rowExtraFormatter != null)
                strings = owner.rowExtraFormatter(obj, columnProperty.Select(propertyInfo => propertyInfo.Key).ToList());

            var counter = 0;
            foreach (var colPair in columnProperty)
            {
                var cell = new FastCell();
                if (colPair.Value == null)
                {
                    cells.Add(cell);
                    continue;
                }
                // do format value
                var proVal = colPair.Value.GetValue(obj, null);
                cell.CellValue = proVal;
                cell.CellString = strings == null ? 
                    (colPair.Key.formatter != null ? colPair.Key.formatter(proVal)
                    : colPair.Key.rowFormatter != null ? colPair.Key.rowFormatter(obj)
                    : proVal == null 
                    ? colPair.Key.NullString 
                    : ObjectToString(proVal, colPair.Key.FormatString, colPair.Key.FractionDigits)) : strings[counter++];
                if (colPair.Key.cellFormatting != null)
                {
                    var args = new CellFormattingEventArgs
                        {
                            cellValue = cell.CellValue,
                            resultedString = cell.CellString,
                            column = colPair.Key,
                            rowValue = obj,
                        };
                    colPair.Key.cellFormatting(args);
                    cell.CellString = args.resultedString;
                }
                cells.Add(cell);
            }
        }

        public void DrawRow(
            Graphics g,
            int rowIndex,
            Point leftTop,
            int height,
            List<FastColumn> columns,
            Pen penUp,
            Pen penDn,
            Brush brushFill,
            Brush brushFont,
            Font font,
            int cellPadding,
            BrushesStorage brushes)
        {
            if (IsGroupingRow)
            {
                var width = columns.Sum(c => c.Visible ? c.ResultedWidth : 0);
                var rect = new Rectangle(leftTop.X + 1, leftTop.Y + 2, width - 1, height - 2);
                g.FillRectangle(brushes.GetBrush(Color.DarkGray), rect);

                g.DrawLine(penUp, leftTop.X, leftTop.Y + 1, leftTop.X, leftTop.Y + height - 1);
                g.DrawLine(penDn, leftTop.X + width - 1, leftTop.Y + 2, leftTop.X + width - 1, leftTop.Y + height - 1);

                var image = owner.ImageList.Images[Collapsed ? "expand" : "collapse"];
                if (image != null)
                {
                    g.DrawImage(image,
                                new Rectangle(new Point(), image.Size).MoveCenter(
                                    new Rectangle(leftTop.X, leftTop.Y + 1, height, height).GetCenter()));
                }
                var label = cells.Count != 0 ? cells[0].CellString : "";
                rect.X += height;
                g.DrawString(label, new Font(font, FontStyle.Bold), brushFont, rect, new StringFormat {LineAlignment = StringAlignment.Center});
                return;
            }

            var lastHoveredCell = owner.LastHoveredCell;

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (!column.Visible)
                    continue;
                if (i >= cells.Count)
                    continue;
                var cell = cells[i];
                var width = column.ResultedWidth;
                
                // если ячейку надо показывать во всю ширину
                if (column.ShowClippedContent && lastHoveredCell.HasValue &&
                    lastHoveredCell.Value.X == i && lastHoveredCell.Value.Y == rowIndex && 
                    column.ImageList == null)
                {
                    var textSz = g.MeasureString(cell.CellString, font);
                    var resWd = column.Padding * 2 + (int) Math.Round(textSz.Width);
                    if (resWd > width)
                        width = resWd;
                }

                var backColor = BackgroundColor;
                var fontColor = FontColor;
                if (column.colorColumnFormatter != null)
                {
                    Color? clBack, clFont;
                    column.colorColumnFormatter(cell.CellValue, out clBack, out clFont);
                    if (clBack.HasValue) backColor = clBack;
                    if (clFont.HasValue) fontColor = clFont;
                }

                // fill brush
                var brush = brushFill;
                if (backColor.HasValue && !Selected) brush = brushes.GetBrush(backColor.Value);
                g.FillRectangle(brush, leftTop.X + 1, leftTop.Y + 2, width - 2, height - 2);

                // outline (right - left bounds)
                g.DrawLine(penUp, leftTop.X, leftTop.Y + 1, leftTop.X, leftTop.Y + height - 1);
                g.DrawLine(penDn, leftTop.X + width - 1, leftTop.Y + 2, leftTop.X + width - 1,
                           leftTop.Y + height - 1);

                // text
                var oldBounds = g.ClipBounds;
                var clipBounds = new RectangleF(leftTop.X, leftTop.Y, width, height);
                g.SetClip(clipBounds);

                // draw picture?
                if (column.ImageList != null)
                {
                    if (cell.CellValue != null)
                    {
                        Image img = null;
                        if (cell.CellValue is int)
                        {
                            var indexImg = (int) cell.CellValue;
                            if (indexImg >= 0 && indexImg < column.ImageList.Images.Count)
                                img = column.ImageList.Images[indexImg];
                        }
                        else
                        {
                            var keyImg = cell.CellValue is string
                                             ? (string) cell.CellValue
                                             : cell.CellValue.ToString();
                            img = column.ImageList.Images[keyImg];
                        }
                        if (img != null)
                        {
                            // do draw
                            var left = column.CellHAlignment == StringAlignment.Near
                                           ? column.Padding
                                           : column.CellHAlignment == StringAlignment.Center
                                                 ? column.ResultedWidth / 2 - img.Width / 2
                                                 : column.ResultedWidth - column.Padding - img.Width;
                            var top = height / 2 - img.Height / 2;
                            g.DrawImage(img, leftTop.X + left, leftTop.Y + top);
                        }
                    }
                }
                else
                {
                    // just draw text
                    DrawCellText(i, column, cell, brushes, g, leftTop,
                                 width, height, font, brushFont, fontColor, rowIndex, cellPadding);
                }

                leftTop = new Point(leftTop.X + width, leftTop.Y);
                g.SetClip(oldBounds);
            }
        }

        private void DrawCellText(int columnIndex, FastColumn column, FastCell cell, BrushesStorage brushes, 
            Graphics g, Point leftTop, int cellWidth, int cellHeight, 
            Font font, Brush brushFont, Color? fontColor, int rowIndex, int cellPadding)
        {
            if (userDrawCellText != null)
            {
                userDrawCellText(columnIndex, column, cell, brushes, g, leftTop, cellWidth, cellHeight, font, brushFont,
                                 fontColor, rowIndex, cellPadding);
                return;
            }
            
            var cellFont = column.ColumnFont ?? font;
            var brush = brushFont;
            if (fontColor.HasValue) brush = brushes.GetBrush(fontColor.Value);
                
            // apply hyperlink colors?
            if (column.IsHyperlinkStyleColumn)
            {
                var linkColor = column.ColorHyperlinkTextInactive;
                if (column.HyperlinkFontInactive != null)
                    cellFont = column.HyperlinkFontInactive;

                if (owner.LastHoveredCell.HasValue)
                {
                    var hoveredCell = owner.LastHoveredCell.Value;
                    // is there cursor above?
                    if (columnIndex == hoveredCell.X && rowIndex == hoveredCell.Y)
                    {
                        linkColor = column.ColorHyperlinkTextActive;
                        if (column.HyperlinkFontActive != null)
                            cellFont = column.HyperlinkFontActive;                            
                    }
                }
                if (linkColor.HasValue)
                    brush = brushes.GetBrush(linkColor.Value);
            }

            var cellString = cell.CellString;
            // Color render
            if (cell.CellValue is Color)
            {
                var c = (Color) cell.CellValue;
                g.FillRectangle(new SolidBrush(c), leftTop.X + cellPadding, leftTop.Y + 2, 25, cellHeight - 3);
                cellString = string.Format("{0}; {1}; {2}", c.B, c.G, c.R);
                leftTop.X += 25;
            }

            if (column.CellHAlignment == StringAlignment.Center)
                g.DrawString(cellString, cellFont, brush,
                             leftTop.X + cellWidth / 2, leftTop.Y + cellHeight / 2, cellTextFormatHCenter);
            if (column.CellHAlignment == StringAlignment.Near)
                g.DrawString(cellString, cellFont, brush,
                             leftTop.X + cellPadding, leftTop.Y + cellHeight / 2, cellTextFormatHNear);
            if (column.CellHAlignment == StringAlignment.Far)
                g.DrawString(cellString, cellFont, brush,
                             leftTop.X + cellWidth - cellPadding, leftTop.Y + cellHeight / 2, cellTextFormatHFar);
        }

        #region Formatting
        private static readonly CultureInfo culture = CultureInfo.InvariantCulture;
        private static string ObjectToString(object obj, string formatString, int digits)
        {
            if (obj is Color)
            {
                var c = (Color) obj;
                return string.Format("[__Color__] {0}; {1}; {2}", c.B, c.G, c.R);
            }
            if (obj is string)
            {
                var s = (string) obj;
                if (s.IndexOf("\n") != -1)
                    return s.Substring(0, s.IndexOf("\n"));
                return s;
            }

            if (!string.IsNullOrEmpty(formatString))
            {
                if (obj is double) return ((double) obj).ToString(formatString, culture);
                if (obj is float) return ((float) obj).ToString(formatString, culture);
                if (obj is decimal) return ((decimal) obj).ToString(formatString, culture);
                if (obj is int) return ((int) obj).ToString(formatString, culture);
                if (obj is DateTime) return ((DateTime) obj).ToString(formatString, culture);
                return obj.ToString();
            }

            var fmtStr = "f" + digits;
            if (obj is double) return ((double) obj).ToString(fmtStr, culture);
            if (obj is float) return ((float) obj).ToString(fmtStr, culture);
            if (obj is decimal) return ((decimal) obj).ToString(fmtStr, culture);
            return obj.ToString();
        }
        #endregion
    }
}
