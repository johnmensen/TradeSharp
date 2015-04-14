using System;
using TradeSharp.Util;

namespace Candlechart.Series
{
    /// <summary>
    /// описывает серию объектов и конкретные настройки для кнопки инструментальной панели
    /// пример: "Трендовая линия", "значок линии", "Режим: ТонкаяСтрелка", "Обводка: красная"
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SeriesToolButtonAttribute : Attribute
    {
        public CandleChartControl.ChartTool Tool { get; set; }

        public string Title { get; set; }

        public ToolButtonImageIndex ImageIndex { get; set; }

        private bool defaultTool = true;

        public bool DefaultTool
        {
            get { return defaultTool; }
            set { defaultTool = value; }
        }

        public SeriesToolButtonAttribute(CandleChartControl.ChartTool tool, string title, ToolButtonImageIndex imageIndex)
        {
            Title = title;
            Tool = tool;
            ImageIndex = imageIndex;
        }

        public SeriesToolButtonAttribute(CandleChartControl.ChartTool tool, string title, ToolButtonImageIndex imageIndex, bool defaultTool)
        {
            Title = title;
            Tool = tool;
            ImageIndex = imageIndex;
            this.defaultTool = defaultTool;
        }
    }

    public class LocalizedSeriesToolButtonAttribute : SeriesToolButtonAttribute
    {
        public LocalizedSeriesToolButtonAttribute(CandleChartControl.ChartTool tool, string title,
                                                  ToolButtonImageIndex imageIndex)
            : base(tool, Localizer.GetString(title), imageIndex)
        {
        }

        public LocalizedSeriesToolButtonAttribute(CandleChartControl.ChartTool tool, string title,
                                                  ToolButtonImageIndex imageIndex, bool defaultTool)
            : base(tool, Localizer.GetString(title), imageIndex, defaultTool)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SeriesToolButtonParamAttribute : Attribute
    {
        public string ParamName { get; set; }

        public string ParamTitle { get; set; }

        public Type ParamType { get; set; }

        public object DefaultValue { get; set; }

        public string DefaultValueString
        {
            get { return StringFormatter.ObjectToStringWithType(DefaultValue); }
            set { DefaultValue = StringFormatter.StringToObject(value, ParamType); }
        }

        public SeriesToolButtonParamAttribute(string paramName, Type paramType)
        {
            ParamName = paramName;
            ParamType = paramType;
        }

        public SeriesToolButtonParamAttribute(string paramName, Type paramType, object defaultValue)
        {
            ParamName = paramName;
            ParamType = paramType;
            DefaultValue = defaultValue;
        }
    }

    public class LocalizedSeriesToolButtonParamAttribute : SeriesToolButtonParamAttribute
    {
        public LocalizedSeriesToolButtonParamAttribute(string paramName, Type paramType) : base(paramName, paramType)
        {
            ParamTitle = Localizer.GetString("Title" + paramName);
        }

        public LocalizedSeriesToolButtonParamAttribute(string paramName, Type paramType, object defaultValue)
            : base(paramName, paramType, defaultValue)
        {
            ParamTitle = Localizer.GetString("Title" + paramName);
        }
    }

    public enum ToolButtonImageIndex
    {
        Arrow = 0,
        TrendLine = 1,
        DealMark = 2,
        Comment = 3,
        FiboChannel = 4,
        Ellipse = 5,
        HorzSpan = 6,
        TurnBar = 7,
        FiboVert = 8,
        TrendLine2 = 9,
        Asterisk = 10,
        MarkerRed = 15,
        MoveObject = 17,
        CrossLines = 20,
        Ruller = 82,
        Script = 72
    }
}
