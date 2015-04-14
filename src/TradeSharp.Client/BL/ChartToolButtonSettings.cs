using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Candlechart;
using Candlechart.Series;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    public class ToolButtonGroup
    {
        public string Title { get; set; }

        public int ImageIndex { get; set; }

        private ChartToolButtonSettings.ToolButtonType buttonType = ChartToolButtonSettings.ToolButtonType.None;
        public ChartToolButtonSettings.ToolButtonType ButtonType
        {
            get { return buttonType; }
            set { buttonType = value; }
        }

        public void SaveInXml(XmlElement parent)
        {
// ReSharper disable PossibleNullReferenceException
            var node = parent.AppendChild(parent.OwnerDocument.CreateElement("group"));
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("title")).Value = Title;
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("imageIndex")).Value = ImageIndex.ToString();
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("buttonType")).Value = ButtonType.ToString();
// ReSharper restore PossibleNullReferenceException
        }

        public static ToolButtonGroup LoadFromXml(XmlElement node)
        {
            if (node.Attributes["title"] == null ||
                node.Attributes["imageIndex"] == null) return null;
            var group = new ToolButtonGroup
                            {
                                Title = node.Attributes["title"].Value,
                                ImageIndex = node.Attributes["imageIndex"].Value.ToInt()
                            };
            var buttonTypeAttr = node.Attributes["buttonType"];
            if (buttonTypeAttr != null)
                group.ButtonType =
                    (ChartToolButtonSettings.ToolButtonType)
                    Enum.Parse(typeof (ChartToolButtonSettings.ToolButtonType), buttonTypeAttr.Value);
            return group;
        }
    }

    /// <summary>
    /// описывает кнопку на инструментальной панели;
    /// также описывает кнопку - инструмент графика, его пиктограмму и параметры
    /// </summary>
    public class ChartToolButtonSettings
    {
        public enum ToolButtonType
        {
            None,
            Chart,
            System
        }

        public ToolButtonGroup Group { get; set; }

        // индекс картинки в списке
        public int Image { get; set; }
        
        // стандартное название (runtime, localized)
        public string Title { get; set; }

        // пользовательское название
        public string DisplayName { get; set; }

        // отображает на кнопке текст DisplayName
        public bool IsVisibleDisplayName { get; set; }

        protected bool defaultVisible = true;
        public bool DefaultVisible
        {
            get { return defaultVisible; }
            set { defaultVisible = value; }
        }

        private ToolButtonType buttonType = ToolButtonType.None;
        public ToolButtonType ButtonType
        {
            get { return buttonType; }
            set { buttonType = value; }
        }

        // инструмент графика
        public CandleChartControl.ChartTool Tool { get; set; }  

        // функция терминала
        public SystemToolButton SystemTool { get; set; }

        public List<SeriesEditParameter> toolParams = new List<SeriesEditParameter>();

        public static ChartToolButtonSettings LoadFromXml(XmlElement node, List<ToolButtonGroup> groups, ToolButtonType defaultButtonType)
        {
            /*if (node.Attributes["tool"] == null && node.Attributes["systemTool"] == null) // игнорируем - все равно не будет работать
                return null;*/
            var imgIndex = node.Attributes["image"].Value.ToInt();
            var isShowName = node.Attributes["showDisplayName"] != null && node.Attributes["showDisplayName"].Value.ToBool();
            var groupTitle = node.Attributes["group"] == null ? string.Empty : node.Attributes["group"].Value;
            var displayName = node.Attributes["displayName"] == null ? string.Empty : node.Attributes["displayName"].Value;
            var type = defaultButtonType;
            if (node.Attributes["type"] != null)
                Enum.TryParse(node.Attributes["type"].Value, out type);
            var tool = CandleChartControl.ChartTool.None;
            if (node.Attributes["tool"] != null)
                Enum.TryParse(node.Attributes["tool"].Value, out tool);
            var systemTool = SystemToolButton.None;
            if (node.Attributes["systemTool"] != null)
                Enum.TryParse(node.Attributes["systemTool"].Value, out systemTool);

            // поддержка старых версий файлов
            var title = node.Attributes["title"] == null ? string.Empty : node.Attributes["title"].Value;

            var btn = new ChartToolButtonSettings
            {
                Image = imgIndex,
                Title = title, // поддержка старых версий файлов
                IsVisibleDisplayName = isShowName,
                DisplayName = displayName,
                ButtonType = type,
                Tool = type == ToolButtonType.Chart ? tool : CandleChartControl.ChartTool.None,
                SystemTool = type == ToolButtonType.System ? systemTool : SystemToolButton.None
            };

            if (!string.IsNullOrEmpty(groupTitle))
            {
                btn.Group = groups.FirstOrDefault(g => g.Title == groupTitle);
                // корректируем тип группы
                if (btn.Group != null)
                    btn.Group.ButtonType = btn.ButtonType;
            }

            foreach (XmlElement ptrNode in node.ChildNodes)
            {
                if (ptrNode.Attributes["name"] == null || ptrNode.Attributes["paramType"] == null ||
                    ptrNode.Attributes["value"] == null) continue;
                var ptrName = ptrNode.Attributes["name"].Value;
                var ptrType = Type.GetType(ptrNode.Attributes["paramType"].Value);
                var ptrValue = StringFormatter.StringToObject(ptrNode.Attributes["value"].Value, ptrType);
                btn.toolParams.Add(new SeriesEditParameter(ptrName, ptrType, ptrValue));
            }

            return btn;
        }

        public ChartToolButtonSettings()
        {
        }

        public ChartToolButtonSettings(ChartToolButtonSettings spec)
        {
            Group = spec.Group;
            Image = spec.Image;
            Title = spec.Title;
            defaultVisible = spec.defaultVisible;
            DisplayName = spec.DisplayName;
            Tool = spec.Tool;
            toolParams =
                spec.toolParams.Select(t => new SeriesEditParameter
                    {
                        defaultValue = t.defaultValue,
                        paramType = t.paramType,
                        title = t.title,
                        Name = t.Name
                    })
                    .ToList();
            ButtonType = spec.ButtonType;
        }

        public void SaveInXml(XmlElement parent)
        {
            if (parent.OwnerDocument == null)
                return;
            var node = parent.AppendChild(parent.OwnerDocument.CreateElement("button"));
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("image")).Value = Image.ToString();
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("showDisplayName")).Value = IsVisibleDisplayName.ToString();
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("type")).Value = ButtonType.ToString();
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("tool")).Value = Tool.ToString();
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("systemTool")).Value = SystemTool.ToString();
            if (Group != null)
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("group")).Value = Group.Title;
            if (!string.IsNullOrEmpty(DisplayName))
                node.Attributes.Append(parent.OwnerDocument.CreateAttribute("displayName")).Value = DisplayName;
            foreach (var param in toolParams)
            {
                var nodePtr = node.AppendChild(parent.OwnerDocument.CreateElement("param"));
                nodePtr.Attributes.Append(parent.OwnerDocument.CreateAttribute("name")).Value = param.Name;
                nodePtr.Attributes.Append(parent.OwnerDocument.CreateAttribute("paramType")).Value =
                    param.paramType.AssemblyQualifiedName;// +";" + param.paramType.FullName;
                nodePtr.Attributes.Append(parent.OwnerDocument.CreateAttribute("value")).Value =
                    StringFormatter.ObjectToString(param.defaultValue);
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(DisplayName) ? Title : DisplayName;
        }
    }

    // все кнопки панели инструментов
    public class ChartToolButtonStorage
    {
        private static ChartToolButtonStorage instance;

        public static ChartToolButtonStorage Instance
        {
            get { return instance ?? (instance = new ChartToolButtonStorage()); }
        }

        public List<ToolButtonGroup> groups = new List<ToolButtonGroup>();

        public List<ChartToolButtonSettings> allButtons = new List<ChartToolButtonSettings>();

        public List<ChartToolButtonSettings> selButtons = new List<ChartToolButtonSettings>();

        public List<ChartToolButtonSettings> selSystemButtons = new List<ChartToolButtonSettings>();

        private ChartToolButtonStorage()
        {
            // прочитать из файла, либо инициализировать по-умолчанию
            InitAllButtons();
            if (!LoadSettings())
                RestoreDefaults();
        }

        private void InitAllButtons()
        {
            // предустановленные режимы
            allButtons.Add(new ChartToolButtonSettings
                {
                    Image = (int)ToolButtonImageIndex.Arrow,
                    Tool = CandleChartControl.ChartTool.Cursor,
                    DefaultVisible = true,
                    Title = Localizer.GetString("TitleCursor"),
                    ButtonType = ChartToolButtonSettings.ToolButtonType.Chart
                });
            allButtons.Add(new ChartToolButtonSettings
            {
                Image = (int)ToolButtonImageIndex.Ruller,
                Tool = CandleChartControl.ChartTool.TrendLine,
                DefaultVisible = true,
                Title = Localizer.GetString("TitleMeasurement"),
                toolParams = new List<SeriesEditParameter>
                    {
                        new SeriesEditParameter("MeasureOnly", typeof(bool), true),
                        new SeriesEditParameter("Subtitles", typeof(bool), true),
                        new SeriesEditParameter("Edit", typeof(bool), false)
                    },
                    ButtonType = ChartToolButtonSettings.ToolButtonType.Chart
            });
            allButtons.Add(new ChartToolButtonSettings
            {
                Image = (int)ToolButtonImageIndex.CrossLines,
                Tool = CandleChartControl.ChartTool.Cross,
                DefaultVisible = true,
                Title = Localizer.GetString("TitleCrosshair"),
                ButtonType = ChartToolButtonSettings.ToolButtonType.Chart
            });
            allButtons.Add(new ChartToolButtonSettings
            {
                Image = (int)ToolButtonImageIndex.Script,
                Tool = CandleChartControl.ChartTool.Script,
                DefaultVisible = true,
                Title = Localizer.GetString("TitleScript"),
                ButtonType = ChartToolButtonSettings.ToolButtonType.Chart
            });

            // кнопки графика
            var lstSeries = InteractiveObjectSeries.GetObjectSeriesTypes();
            foreach (var t in lstSeries)
            {
                var attrBtn = (SeriesToolButtonAttribute) t.GetCustomAttributes(typeof(SeriesToolButtonAttribute), false)[0];

                var button = new ChartToolButtonSettings
                    {
                        Image = (int) attrBtn.ImageIndex,
                        DefaultVisible = attrBtn.DefaultTool,
                        Title = attrBtn.Title,
                        ButtonType = ChartToolButtonSettings.ToolButtonType.Chart,
                        Tool = attrBtn.Tool
                    };

                foreach (SeriesToolButtonParamAttribute ptrAttr in t.GetCustomAttributes(typeof(SeriesToolButtonParamAttribute), false))
                {
                    button.toolParams.Add(new SeriesEditParameter
                                              {
                                                  Name = ptrAttr.ParamName,
                                                  title = ptrAttr.ParamTitle,
                                                  defaultValue = ptrAttr.DefaultValue,
                                                  paramType = ptrAttr.ParamType
                                              });
                }
                allButtons.Add(button);
            }

            // терминальные кнопки
            foreach (var systemButtonType in Enum.GetValues(typeof(SystemToolButton)).Cast<SystemToolButton>().ToList())
            {
                if (systemButtonType == SystemToolButton.None)
                    continue;
                var systemButton = CommonToolButtonName.GetButtonDisplayName(systemButtonType);
                if (HiddenModes.ManagerMode || !systemButton.b)
                    allButtons.Add(new ChartToolButtonSettings
                    {
                        Title = systemButton.a,
                        Image = CommonToolButtonName.buttonImageIndex[systemButtonType],
                        DefaultVisible = true,
                        ButtonType = ChartToolButtonSettings.ToolButtonType.System,
                        SystemTool = systemButtonType
                    });
            }
        }

        public void RestoreDefaults()
        {
            groups.Clear();
            selButtons = allButtons.Where(b => b.DefaultVisible && b.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart).ToList();
            selSystemButtons = allButtons.Where(b => b.DefaultVisible && b.ButtonType == ChartToolButtonSettings.ToolButtonType.System).ToList();
        }

        public void SaveSettings()
        {
            var nodeToolBtnSets = ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameToolButtons);
            var doc = nodeToolBtnSets.OwnerDocument;
            var groupsNode = (XmlElement) nodeToolBtnSets.AppendChild(doc.CreateElement("groups"));
            foreach (var group in groups)
            {
                group.SaveInXml(groupsNode);
            }
            // todo: simplify to one tag-container "buttons"
            var buttonsNode = (XmlElement) nodeToolBtnSets.AppendChild(doc.CreateElement("buttons"));
            foreach (var btn in selButtons)
            {
                btn.SaveInXml(buttonsNode);
            }
            var sysButtonsNode = (XmlElement) nodeToolBtnSets.AppendChild(doc.CreateElement("systembuttons"));
            foreach (var btn in selSystemButtons)
            {
                btn.SaveInXml(sysButtonsNode);
            }
            ToolSettingsStorageFile.SaveXml(doc);
        }

        private bool LoadSettings()
        {
            groups.Clear();
            selButtons.Clear();
            selSystemButtons.Clear();
            var nodeToolSets = ToolSettingsStorageFile.LoadNode(ToolSettingsStorageFile.NodeNameToolButtons);
            if (nodeToolSets == null)
                return false;

            var nodeGroups = nodeToolSets.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "groups");
            if (nodeGroups != null)
            {
                foreach (XmlElement node in nodeGroups.ChildNodes)
                {
                    var group = ToolButtonGroup.LoadFromXml(node);
                    if (group != null)
                        groups.Add(group);
                }
            }

            // todo: simplify to one tag-container "buttons"
            // кнопки графика
            var nodeIndex = 0; // 4 log
            var nodeButtons = nodeToolSets.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "buttons");
            if (nodeButtons == null)
                return false;
            foreach (XmlElement node in nodeButtons.ChildNodes)
            {
                nodeIndex++;
                try
                {
                    var btn = ChartToolButtonSettings.LoadFromXml(node, groups, ChartToolButtonSettings.ToolButtonType.Chart);
                    if (btn == null)
                        continue;

                    // поддержка старых версий файлов (установка функции кнупки по названию)
                    if (btn.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart && btn.Tool == CandleChartControl.ChartTool.None)
                    {
                        var toolButton = allButtons.FirstOrDefault(b => b.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart && b.Title == btn.Title);
                        if (toolButton != null)
                            btn.Tool = toolButton.Tool;
                    }

                    selButtons.Add(btn);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка загрузки кнопки графика {0}: {1}", nodeIndex, ex);
                }
            }
            
            // кнопки терминала
            nodeIndex = 0;
            nodeButtons = nodeToolSets.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "systembuttons");
            if (nodeButtons == null)
                return false;
            foreach (XmlElement node in nodeButtons.ChildNodes)
            {
                nodeIndex++;
                try
                {
                    var btn = ChartToolButtonSettings.LoadFromXml(node, groups, ChartToolButtonSettings.ToolButtonType.System);
                    if (btn == null)
                        continue;

                    // поддержка старых версий файлов (установка функции кнупки по названию)
                    if (btn.ButtonType == ChartToolButtonSettings.ToolButtonType.Chart && btn.Tool == CandleChartControl.ChartTool.None)
                    {
                        var toolButton = allButtons.FirstOrDefault(b => b.ButtonType == ChartToolButtonSettings.ToolButtonType.System && b.Title == btn.Title);
                        if (toolButton != null)
                            btn.SystemTool = toolButton.SystemTool;
                    }

                    selSystemButtons.Add(btn);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка загрузки терминальной кнопки {0}: {1}", nodeIndex, ex);
                }
            }

            // set localized title
            var buttons = new List<ChartToolButtonSettings>();
            buttons.AddRange(selButtons);
            buttons.AddRange(selSystemButtons);
            foreach (var btn in buttons)
            {
                var storageButton =
                    allButtons.FirstOrDefault(
                        b => b.ButtonType == btn.ButtonType && b.Tool == btn.Tool && b.SystemTool == btn.SystemTool);
                if (storageButton != null)
                    btn.Title = storageButton.Title;
            }

            return true;
        }
    }

    // терминальные (системные) кнопки панели инструментов
    public enum SystemToolButton
    {
        None = 0, 
        RobotsStart,
        Indicators,
        ZoomIn,
        ZoomOut,
        Account,
        SetOrder,
        RollGraph,
        RollAllGraphs,
        Forecast,
        MakeSignalChart,
        MakeSignalText,
        ChatWindow,
        RobotState,
        RobotPortfolio,
        TradeSignals
    }

    // менеджер названий и иконкок по-умолчания для терминальных кнопок
    public static class CommonToolButtonName
    {
        // Cortege2<string, bool> - если показывать кнопку только в режиме TerminalEnvironment.ManagerMode = true 
        public static readonly Dictionary<SystemToolButton, Cortege2<string, bool>> buttonsNames = new Dictionary<SystemToolButton, Cortege2<string, bool>>
        {
            { SystemToolButton.RobotsStart, new Cortege2<string, bool>(Localizer.GetString("TitleTurnOnRobots"), false)},
            { SystemToolButton.Indicators, new Cortege2<string, bool>(Localizer.GetString("TitleIndicators"), false)},
            { SystemToolButton.ZoomIn, new Cortege2<string, bool>(Localizer.GetString("TitleIncreaseScale"), false)},
            { SystemToolButton.ZoomOut, new Cortege2<string, bool>(Localizer.GetString("TitleDecreaseScale"), false)},
            { SystemToolButton.Account, new Cortege2<string, bool>(Localizer.GetString("TitleTradeAccount"), false)},
            { SystemToolButton.SetOrder, new Cortege2<string, bool>(Localizer.GetString("TitleNewOrder"), false)},
            { SystemToolButton.RollGraph, new Cortege2<string, bool>(Localizer.GetString("TitleAutoscrollChart"), false)},
            { SystemToolButton.RollAllGraphs, new Cortege2<string, bool>(Localizer.GetString("TitleAutoscrollAllCharts"), false)},
            { SystemToolButton.Forecast, new Cortege2<string, bool>(Localizer.GetString("TitleForecast"), true)},
            { SystemToolButton.MakeSignalChart, new Cortege2<string, bool>(Localizer.GetString("TitleSignal") + " - " + Localizer.GetString("TitleChartSmall"), false)},
            { SystemToolButton.MakeSignalText, new Cortege2<string, bool>(Localizer.GetString("TitleSignal") + " - " + Localizer.GetString("TitleTextSmall"), false)},
            { SystemToolButton.ChatWindow, new Cortege2<string, bool>(Localizer.GetString("TitleChatWindow"), false)},
            { SystemToolButton.RobotState, new Cortege2<string, bool>(Localizer.GetString("TilteRobotsState"), false)},
            { SystemToolButton.RobotPortfolio, new Cortege2<string, bool>(Localizer.GetString("TitleRobotsPortfolio"), false)},
            { SystemToolButton.TradeSignals, new Cortege2<string, bool>(Localizer.GetString("TitleStrategies"), false)}
        };

        public static readonly Dictionary<SystemToolButton, int> buttonImageIndex
            = new Dictionary<SystemToolButton, int>
            { { SystemToolButton.RobotsStart, 11 },
                { SystemToolButton.Indicators, 68 },
                { SystemToolButton.ZoomIn, 18 },
                { SystemToolButton.ZoomOut, 19 },
                { SystemToolButton.Account, 69 },
                { SystemToolButton.SetOrder, 65 },
                { SystemToolButton.RollGraph, 66 },
                { SystemToolButton.RollAllGraphs, 67 },
                { SystemToolButton.Forecast, 64 },
                { SystemToolButton.MakeSignalChart, 75 },
                { SystemToolButton.MakeSignalText, 74 },
                { SystemToolButton.ChatWindow, 40 },
                { SystemToolButton.RobotState, 113 },
                { SystemToolButton.RobotPortfolio, 76 },
                { SystemToolButton.TradeSignals, 118 },
            };

        public static Cortege2<string, bool> GetButtonDisplayName(SystemToolButton type)
        {
            return buttonsNames.ContainsKey(type) ? buttonsNames[type] : new Cortege2<string, bool>();
        }
    }
}
