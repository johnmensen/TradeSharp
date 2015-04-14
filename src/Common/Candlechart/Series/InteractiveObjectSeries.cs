using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Core;

namespace Candlechart.Series
{
    public abstract class InteractiveObjectSeries : Series
    {
        protected CandleChartControl.ChartTool linkedChartTool;

        protected InteractiveObjectSeries(string name, 
            CandleChartControl.ChartTool linkedTool) : base(name)
        {
            linkedChartTool = linkedTool;
        }

        public bool OnMouseDown(MouseEventArgs e, 
            List<SeriesEditParameter> parameters,
            CandleChartControl.ChartTool activeChartTool,
            Keys modifierKeys,
            out IChartInteractiveObject objectToEdit)
        {
            if (activeChartTool == linkedChartTool)
            {
                OnMouseDown(parameters, e, modifierKeys, out objectToEdit);
                return true;
            }
            objectToEdit = null;
            return false;
        }

        protected abstract void OnMouseDown(List<SeriesEditParameter> parameters,
            MouseEventArgs e, Keys modifierKeys, out IChartInteractiveObject objectToEdit);

        public bool OnMouseUp(List<SeriesEditParameter> parameters,
            MouseEventArgs e,
            CandleChartControl.ChartTool activeChartTool, 
            Keys modifierKeys,            
            out IChartInteractiveObject objectToEdit)
        {
            if (activeChartTool != linkedChartTool)
            {                
                objectToEdit = null;
                return false;
            }
            return OnMouseUp(parameters, e, modifierKeys, out objectToEdit);
        }

        protected virtual bool OnMouseUp(List<SeriesEditParameter> parameters,
            MouseEventArgs e,            
            Keys modifierKeys,
            out IChartInteractiveObject objectToEdit)
        {
            objectToEdit = null;
            return false;
        }

        public bool OnMouseMove(MouseEventArgs e, Keys modifierKeys,
            CandleChartControl.ChartTool activeChartTool)
        {
            if (activeChartTool != linkedChartTool) return false;
            return OnMouseMove(e, modifierKeys);
        }

        protected virtual bool OnMouseMove(MouseEventArgs e, Keys modifierKeys)
        {
            return false;
        }

        public abstract void AddObjectsInList(List<IChartInteractiveObject> interObjects);

        public abstract void RemoveObjectFromList(IChartInteractiveObject interObject);

        public abstract void RemoveObjectByNum(int num);

        public abstract IChartInteractiveObject LoadObject(XmlElement objectNode, CandleChartControl owner, bool skipObjectOutOfHistory = false);

        public abstract IChartInteractiveObject GetObjectByNum(int num);

        public abstract IChartInteractiveObject FindObject(Func<IChartInteractiveObject, bool> predicate, out int objIndex);
        
        /// <param name="screenX">экранные координаты курсора</param>
        /// <param name="screenY">экранные координаты курсора</param>
        /// <param name="tolerance">точность попадания, пикс</param>
        /// <returns>список объектов, м.б. пустой</returns>
        public abstract List<IChartInteractiveObject> GetObjectsUnderCursor(int screenX, int screenY, int tolerance);

        /// <summary>
        /// вызывается после того, как все объекты серии загружены
        /// </summary>        
        public abstract void ProcessLoadingCompleted(CandleChartControl owner);

        public virtual void OnTimeframeChanged()
        {
        }

        public static List<Type> GetObjectSeriesTypes()
        {
            var lstSeries = new List<Type>();
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var t in a.GetTypes())
            {
                if (!t.IsSubclassOf(typeof (InteractiveObjectSeries))) continue;
                lstSeries.Add(t);
            }
            return lstSeries;
        }

        public abstract void AdjustColorScheme(CandleChartControl chart);
    }
}
