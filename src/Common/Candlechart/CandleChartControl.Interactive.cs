using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Core;
using System.Linq;

namespace Candlechart
{
    /*
     * вывести подсказку под курсором мышки
     */
    public partial class CandleChartControl
    {
        public const int MouseHitTolerancePix = 5;
        private const int MaxTempItemsInMenu = 15;
        private const int TooltipDurationMills = 8000;

        /// <summary>
        /// редактируемый объект - рисуется с маркерами, за которые его можно
        /// перетащить или поменять объекту форму
        /// </summary>
        private IChartInteractiveObject editingObject;

        private IChartInteractiveObject selectedObject;
        /// <summary>
        /// выбранный объект графика - выбран простым наведением курсора мыши
        /// рисуется жирненьким
        /// </summary>
        protected bool UpdateSelectedObject(IChartInteractiveObject value)
        {
            if (value == selectedObject) return false;
            if (selectedObject != null)
            {
                if (selectedObject != editingObject)
                selectedObject.Selected = false;
            }
            selectedObject = value;
            if (selectedObject != null)
                selectedObject.Selected = true;

            return true;
        }

        private ChartObjectMarker editingMarker;

        /// <summary>
        /// вызывается при нажатии в режиме курсора
        /// </summary>        
        private void ShowHint(int mouseX, int mouseY)
        {
            var ptScreen = chart.PointToScreen(new Point(mouseX, mouseY));
            int x = ptScreen.X, y = ptScreen.Y;


            var hint = new StringBuilder();
            // данные с индюков...
            foreach (var indi in indicators)
            {
                var hnt = indi.GetHint(x, y, 0, 0, MouseHitTolerancePix);
                if (!string.IsNullOrEmpty(hnt))hint.AppendLine(hnt);
            }
            // данные с графических объектов...            

            if (hint.Length == 0) return;
            var tooltip = new ToolTip();
            tooltip.Show(hint.ToString(), this, mouseX, mouseY, TooltipDurationMills);
        }

        /// <summary>
        /// дополнить всплывающее меню элементами - объектами графика, над которыми находится курсор
        /// </summary>        
        private void MakeupPopupMenu(int mouseX, int mouseY)
        {            
            // получить список объектов под курсором
            var ptScreen = chart.PointToScreen(new Point(mouseX, mouseY));
            int x = ptScreen.X, y = ptScreen.Y;

            var selected = GetSelectedObjects(x, y);

            // удалить старые элементы всплывающего меню
            for (var i = 0; i < contextMenu.Items.Count; i++ )
            {
                if (contextMenu.Items[i].Tag != null)                    
                {
                    contextMenu.Items.RemoveAt(i);
                    i--;
                }
            }

            // добавить меню по свечке?
            int candleIndex;
            var candleMenuTitle = MakeCandleMenu(mouseX, mouseY, out candleIndex);
            if (candleIndex >= 0 && string.IsNullOrEmpty(candleMenuTitle) == false)
            {
                // добавить меню
                var menuCandle = contextMenu.Items.Add(candleMenuTitle);
                menuCandle.Tag = candleIndex;
                menuCandle.Click += CandleMenuClicked;
            }

            if (selected.Count == 0) return;
            // добавить разделитель
            var separator = contextMenu.Items.Add("-");
            separator.Tag = "temp";            
            
            // добавить объекты
            var maxIndex = Math.Min(selected.Count, MaxTempItemsInMenu);
            for (var i = 0; i < maxIndex; i++)
            {
                var menuItem = contextMenu.Items.Add(string.IsNullOrEmpty(selected[i].Name) ?
                    selected[i].GetType().Name : selected[i].Name);
                menuItem.Tag = selected[i];
                menuItem.Click += (sender, args) =>
                    {
                        EditSelectedChartObjects(new List<IChartInteractiveObject>
                            {
                                (IChartInteractiveObject) menuItem.Tag
                            });
                        RedrawChartSafe();
                    };
            }
            // добавить опцию удаления
            var deleteOptionName = selected.Count == 1
                                       ? "Удалить " +
                                         (string.IsNullOrEmpty(selected[0].Name)
                                             ? selected[0].GetType().Name
                                             : selected[0].Name)
                                       : "Удалить " + selected.Count + " объектов";
            var delItem = contextMenu.Items.Add(deleteOptionName);
            delItem.Tag = selected;
            delItem.ImageIndex = 12; // крестик
            delItem.Click += (sender, args) =>
                {
                    var objs = (List<IChartInteractiveObject>) delItem.Tag;
                    DeleteSelectedChartObjects(objs);
                    RedrawChartSafe();
                };
        }

        private List<IChartInteractiveObject> GetSelectedObjects(int x, int y)
        {
            var selected = new List<IChartInteractiveObject>();
            foreach (var indi in indicators)
                selected.AddRange(indi.GetObjectsUnderCursor(x, y, MouseHitTolerancePix));
            foreach (var ser in listInteractiveSeries)            
                selected.AddRange(ser.GetObjectsUnderCursor(x, y, MouseHitTolerancePix));
            return selected;
        }

        public static void EditSelectedChartObjects(List<IChartInteractiveObject> objs)
        {
            if (objs.Count == 1 && objs[0] is ICustomEditDialogChartObject)
                ((ICustomEditDialogChartObject) objs[0]).ShowEditDialog();
            else
                new ObjectPropertyWindow(objs.Select(o => o as object).ToList()).ShowDialog();

            // обновить время модификации объектов
            timeUpdateObjects.Touch();
        }

        public void DeleteSelectedChartObjects(List<IChartInteractiveObject> objs)
        {
            while (objs.Count > 0)
            {
                DeleteSeriesObject(objs[0]);
                objs.RemoveAt(0);
            }
            // обновить время модификации объектов
            timeUpdateObjects.Touch();
        }
        
        private string MakeCandleMenu(int mouseX, int mouseY, out int candleIndex)
        {
            candleIndex = -1;
            Point clientPoint = chart.PointToScreen(new Point(mouseX, mouseY));
            clientPoint = chart.StockPane.PointToClient(clientPoint);

            var pointD = Conversion.ScreenToWorld(new PointD(clientPoint.X, clientPoint.Y),
               chart.StockPane.WorldRect, chart.StockPane.CanvasRect);

            var index = (int)(pointD.X + 0.5);
            if (index < 0 || index >= chart.StockSeries.Data.Count) return string.Empty;
            candleIndex = index;

            var open = chart.StockSeries.Data[index].open;
            var close = chart.StockSeries.Data[index].close;
            if (pointD.Y > (double)Math.Max(open, close) || pointD.Y < (double)Math.Min(open, close))
                return string.Empty;
            var tip = string.Format("Свеча[{0}]: {1:dd/MM/yyyy HH:mm}",
                                    index, chart.StockSeries.Data[index].timeOpen);
            return tip;
        }

        private void CandleMenuClicked(object sender, EventArgs e)
        {
            var candleIndex = (int)((ToolStripItem)sender).Tag;
            var dlg = new CandleMenuWindow(this, candleIndex);
            dlg.ShowDialog();
        }

        private void EditChartObjectMouseUp()
        {
            zoomTool.Enabled = true;
            if (editingObject != null && editingMarker != null)
            {
                // завершить перетаскивание маркера
                editingMarker = null;
            }
        }

        private bool EditChartObjectMouseMove(int x, int y)
        {
            // перетащить маркер
            if (editingMarker == null) return false;
            var ptScreen = chart.PointToScreen(new Point(x, y));
            var ptClient = editingObject.Owner.Owner.PointToClient(ptScreen);
            editingMarker.centerScreen = new PointD(ptClient.X, ptClient.Y);                
            
            // уведомить объект о перемещенном маркере
            editingObject.OnMarkerMoved(editingMarker);
            // перерисовать
            chart.Invalidate();
            return true;
        }

        /// <summary>
        /// возвращает флаг - необходимость перерисовки
        /// </summary>
        private bool EditChartObjectMouseDown(int mouseX, int mouseY, Keys modifiers)
        {
            var ptScreen = chart.PointToScreen(new Point(mouseX, mouseY));
            int x = ptScreen.X, y = ptScreen.Y;

            // если объект не выбран - показать диалог выбора объекта
            if (editingObject == null)
            {
                var selected = GetSelectedObjects(x, y);
                if (selected.Count == 0) return false;
                if (selected.Count == 1)
                {
                    SelectObject(selected[0]);
                    return true;
                }
                // множественный выбор
                chart.toolSkipMouseDown = true;
                var dlg = new ChartObjectSelectForm(selected);
                dlg.ShowDialog();
                var selectedObj = dlg.SelectedObject;
                if (selectedObj != null)
                {
                    SelectObject(selectedObj);                    
                    return true;
                }
                return false;
            }

            // иначе проверить попадание в маркер
            var marker = editingObject.IsInMarker(x, y, modifiers);
            if (marker == null)
            {
                // снять выделение с объекта
                DeselectObject();
                return true;
            }

            // маркер будет перемещаться - событие OnMouseMove
            editingMarker = marker;
            zoomTool.Enabled = false;

            return false;
        }

        private void QuitEditMode()
        {
            if (editingObject == null) return;
            DeselectObject();
            Invalidate();
        }

        private bool SelectObject(IChartInteractiveObject obj)
        {
            if (editingObject == obj) return false;
            if (editingObject != null)
                editingObject.Selected = false;
            editingObject = obj;
            obj.Selected = true;
            editingMarker = null;
            return true;
        }

        private void DeselectObject()
        {
            editingObject.Selected = false;
            editingObject = null;
            editingMarker = null;
        }

        /// <summary>
        /// по клику - если под курсором один объект - редактировать его
        /// </summary>
        private bool TryEditObject(int mouseX, int mouseY)
        {
            var ptScreen = chart.PointToScreen(new Point(mouseX, mouseY));
            int x = ptScreen.X, y = ptScreen.Y;

            var selected = GetSelectedObjects(x, y);
            return selected.Count != 0 && SelectObject(selected[0]);
        }
    }
}