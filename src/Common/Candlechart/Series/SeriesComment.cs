using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    public class CommentaryPatternsList : StringConverter
    {
        private string[] patterns =
            {
                "{BUY}",
                "{SELL}",
                Localizer.GetString("TitlePurchase") + "{color:Blue}",
                Localizer.GetString("TitleSelling") + "{color:Red}",
                "TP{color:Green}",
                "SL{color:Red}",
                Localizer.GetString("TitleBuyingArea"),
                Localizer.GetString("TitleSellingArea"),
                Localizer.GetString("TitlePurchaseWithHighConfirmationLevel"),
                Localizer.GetString("TitleSellingWithHighConfirmationLevel"),
                "TP{color:Blue}\n_________\n",
                "SL{color:Red}\n_________\n",
            };

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(patterns);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    // ReSharper disable LocalizableElement
    [LocalizedSeriesToolButton(CandleChartControl.ChartTool.Comment, "TitleComments", ToolButtonImageIndex.Comment)]
    [LocalizedSeriesToolButtonParam("Text", typeof(string), "...")]
    [LocalizedSeriesToolButtonParam("Stroke", typeof(Color), DefaultValueString = "-16777216")]
    [LocalizedSeriesToolButtonParam("Filling", typeof(Color), DefaultValueString = "-1")]
    [LocalizedSeriesToolButtonParam("TextColor", typeof(Color), DefaultValueString = "-16777216")]
    [LocalizedSeriesToolButtonParam("Transparency", typeof(int), DefaultValue = 192)]
    [LocalizedSeriesToolButtonParam("HideArrow", typeof(bool), DefaultValue = false)]
    [LocalizedSeriesToolButtonParam("ShowWindow", typeof(bool), DefaultValue = true)]
    [LocalizedSeriesToolButtonParam("DrawFrame", typeof(bool), DefaultValue = true)]
    public class SeriesComment : InteractiveObjectSeries
    {
        public List<ChartComment> data = new List<ChartComment>();

        public override int DataCount { get { return data.Count; } }

        public const int DefaultMouseTolerance = 5;

        private bool drawText = true;
        public bool DrawText
        {
            get { return drawText; }
            set { drawText = value; }
        }

        private static float fontSize = 8;
        [PropertyXMLTag("FontSize", "8")]
        [DisplayName("Размер шрифта")]
        public static float FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        private static bool fontBold = true;
        [PropertyXMLTag("FontBold", "true")]
        [DisplayName("Полужирный")]
        public static bool FontBold
        {
            get { return fontBold; }
            set { fontBold = value; }
        }
        
        /// <summary>
        /// если флаг включен и один комментарий накрывает другой, то нижний комментарий
        /// показан не будет
        /// </summary>
        public bool hideSpannedComments;

        public SeriesComment(string name)
            : base(name, CandleChartControl.ChartTool.Comment)
        {            
        }

        public override bool GetXExtent(ref double left, ref double right)
        {
            return false;
        }
        
        public override bool GetYExtent(double left, double right, ref double top, ref double bottom)
        {            
            return false;
        }
        
        public override void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            base.Draw(g, worldRect, canvasRect);
            DrawComments(g, worldRect, canvasRect);
        }

        private void DrawComments(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            List<Rectangle> areasDrawn = null;
            // контроль перекрывающихся комментов
            if (hideSpannedComments)
                areasDrawn = new List<Rectangle>();

            using (var fonts = new FontStorage(new Font(Chart.Font.FontFamily, fontSize,
                FontBold ? FontStyle.Bold : FontStyle.Regular)))
            using (var penStorage = new PenStorage())
            using (var brushStorage = new BrushesStorage())
            {
                
                for (var i = data.Count - 1; i >= 0; i--)
                {
                    data[i].DrawComment(g, fonts, worldRect, canvasRect, penStorage, brushStorage, areasDrawn);
                    if (!hideSpannedComments) continue;
                }
            }
        }

        public ChartComment GetCommentByCoords(int x, int y)
        {
            foreach (var comment in data)
            {
                if (comment.IsDotInTextArea(x, y)) return comment;
            }
            return null;
        }

        protected override void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            if (e.Button != MouseButtons.Left) return;
            var editedComment = data.Find(c => c.IsBeingCreated);
            if (editedComment != null) return;
            
            // получить время и цену
            var clientPoint = Chart.PointToScreen(new Point(e.X, e.Y));
            clientPoint = Chart.StockPane.PointToClient(clientPoint);
            var x = clientPoint.X;
            var y = clientPoint.Y;

            var pointD = Conversion.ScreenToWorld(new PointD(x, y),
               Chart.StockPane.WorldRect, Chart.StockPane.CanvasRect);

            // начинается редактирование - выбрана первая точка                                
            var text = SeriesEditParameter.TryGetParamValue(parameters, "Text", "...");
            var lineColor = SeriesEditParameter.TryGetParamValue(parameters, "Stroke", Color.Black);
            var fillColor = SeriesEditParameter.TryGetParamValue(parameters, "Filling", Color.White);
            var colorText = SeriesEditParameter.TryGetParamValue(parameters, "TextColor", Color.Black);
            var alphaColor = SeriesEditParameter.TryGetParamValue(parameters, "Transparency", 192);
            var hideArrow = SeriesEditParameter.TryGetParamValue(parameters, "HideArrow", false);
            var drawFrame = SeriesEditParameter.TryGetParamValue(parameters, "DrawFrame", true);
                
            var comment = new ChartComment
            {
                IsBeingCreated = true,
                PivotIndex = pointD.X,
                PivotPrice = pointD.Y,
                ArrowLength = 0,
                ArrowAngle = -45,
                Owner = this,
                Text = text,
                Color = lineColor,
                ColorFill = fillColor,
                FillTransparency = alphaColor,
                HideArrow = hideArrow,
                DrawFrame = drawFrame,
                ColorText = colorText
            };
            if (Owner.Owner.Owner.AdjustObjectColorsOnCreation)
                comment.AjustColorScheme(Owner.Owner.Owner);
            data.Add(comment);            
        }

        protected override bool OnMouseMove(MouseEventArgs e, Keys modifierKeys)
        {
            var editedComment = data.Find(c => c.IsBeingCreated);
            if (editedComment == null) return false;

            // обновить вторую точку комментария (вычислить пеленг-дистанцию)
            var clientPoint = Chart.PointToScreen(new Point(e.X, e.Y));
            clientPoint = Chart.StockPane.PointToClient(clientPoint);
            
            var pivot = Conversion.WorldToScreen(new PointD(editedComment.PivotIndex,
                                                            editedComment.PivotPrice),
                                                            Chart.StockPane.WorldRect, 
                                                            Chart.StockPane.CanvasRect);
            double deltaX = clientPoint.X - pivot.X,
                   deltaY = clientPoint.Y - pivot.Y;
            editedComment.ArrowLength = (int)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            editedComment.ArrowAngle = Math.Atan2(deltaY, deltaX) * 180 / Math.PI;

            return true;
        }

        protected override bool OnMouseUp(List<SeriesEditParameter> parameters, MouseEventArgs e, 
            Keys modifierKeys, out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            var editedComment = data.Find(c => c.IsBeingCreated);
            if (editedComment == null) return false;

            // завершить создание комментария - открыть окно редактирования текста
            editedComment.IsBeingCreated = false;
            var showWindow = SeriesEditParameter.TryGetParamValue(parameters, "ShowWindow", true);
            if (showWindow)
            {
                Chart.toolSkipMouseDown = true;
                if (editedComment.ShowEditDialog() == DialogResult.Cancel)
                {
                    // удалить комментарий
                    data.Remove(editedComment);
                    return true;
                }
            }
            return false;
        }

        public override void AddObjectsInList(List<IChartInteractiveObject> interObjects)
        {
            foreach (var item in data) interObjects.Add(item);
        }

        public override IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool trimObjectsOutOfHistory = false)
        {
            var obj = new ChartComment();
            obj.LoadFromXML(objectNode, owner);
            obj.Owner = this;
            data.Add(obj);
            return obj;
        }

        public override void RemoveObjectFromList(IChartInteractiveObject interObject)
        {
            if (interObject == null) return;
            if (interObject is ChartComment == false) return;
            data.Remove((ChartComment)interObject);
        }

        public override void RemoveObjectByNum(int num)
        {
            data.RemoveAt(num);
        }

        public override IChartInteractiveObject GetObjectByNum(int num)
        {
            return data[num];
        }

        public override List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance)
        {
            var ptClient = Owner.PointToClient(new Point(screenX, screenY));
            var list = new List<IChartInteractiveObject>();
            foreach (var cmt in data)
            {
                if (cmt.IsDotInTextArea(ptClient.X, ptClient.Y)) list.Add(cmt);
            }
            return list;
        }

        public override void ProcessLoadingCompleted(CandleChartControl owner){}

        public override void AdjustColorScheme(CandleChartControl chart)
        {
            foreach (var obj in data)
                obj.AjustColorScheme(chart);
        }

        public override IChartInteractiveObject FindObject(Func<IChartInteractiveObject, bool> predicate, out int objIndex)
        {
            objIndex = -1;
            for (var i = 0; i < data.Count; i++)
            {
                if (predicate(data[i]))
                {
                    objIndex = i;
                    return data[i];
                }
            }
            return null;
        }
    }
    // ReSharper restore LocalizableElement
}
