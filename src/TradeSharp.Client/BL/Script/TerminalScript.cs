using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Xml;
using Candlechart;
using Candlechart.ChartMath;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    /// <summary>
    /// базовый класс для скрипта терминала
    /// </summary>
    public abstract class TerminalScript
    {
        private static Dictionary<string, Type> scriptClasses;

        public enum TerminalScriptTarget
        {
            График = 0, Тикер = 1, Терминал = 2
        }

        /// <summary>
        /// тип скрипта: применяется к графику, тикеру или ко всему терминалу
        /// </summary>
        [Browsable(false)]
        public TerminalScriptTarget ScriptTarget { get; protected set; }

        /// <summary>
        /// "класс" скрипта - неизменяемое пользователем имя
        /// </summary>
        [Browsable(false)]
        public string ScriptName { get; protected set; }

        protected string title;
        /// <summary>
        /// название, которое пользователь дал скрипту
        /// </summary>
        [LocalizedDisplayName("TitleName")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageArbitraryName")]
        public string Title
        {
            get { return string.IsNullOrEmpty(title) ? ScriptName : title; }
            set { title = value; }
        }

        protected TerminalScriptTrigger trigger;
        [LocalizedDisplayName("TitleTrigger")]
        [LocalizedCategory("TitleMain")]
        [LocalizedDescription("MessageScriptTrigger")]
        [Editor(typeof(ScriptTriggerUIEditor), typeof(UITypeEditor))]
        public TerminalScriptTrigger Trigger
        {
            get { return CanBeTriggered ? trigger : null; }
            set { if (CanBeTriggered) trigger = value; }
        }

        /// <summary>
        /// скрипт может быть оснащен триггером для автоматического вызова
        /// </summary>
        [Browsable(false)]
        public virtual bool CanBeTriggered
        {
            get { return true; }
        }

        public abstract string ActivateScript(CandleChartControl chart, PointD worldCoords);

        public abstract string ActivateScript(string ticker);

        public abstract string ActivateScript(bool byTrigger);

        public virtual XmlElement SaveInXml(XmlElement parent)
        {
            var node = (XmlElement)parent.AppendChild(parent.OwnerDocument.CreateElement("script"));
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("name")).Value = ScriptName;
            node.Attributes.Append(parent.OwnerDocument.CreateAttribute("title")).Value = Title;
            if (Trigger != null) Trigger.SaveInXml(node);

            PropertyXMLTagAttribute.SaveObjectProperties(this, node);
            return node;
        }

        protected virtual TerminalScript LoadScriptFromXml(XmlElement node)
        {
            try
            {
                PropertyXMLTagAttribute.InitObjectProperties(this, node);
                var triggers = node.GetElementsByTagName("trigger");
                if (triggers.Count > 0)
                    Trigger = TerminalScriptTrigger.LoadFromXml((XmlElement)triggers[0]);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error while LoadScriptFromXml() for \"{0}\": {1}",
                    ScriptName, ex);
                return null;
            }
            return this;
        }

        public static List<TerminalScript> LoadFromXml(XmlElement parent)
        {
            var scripts = new List<TerminalScript>();
            if (parent == null) return scripts;
            foreach (XmlElement node in parent)
            {
                if (node.Attributes["name"] == null) continue;
                var scriptName = node.Attributes["name"].Value;
                Type tpScript;
                scriptClasses.TryGetValue(scriptName, out tpScript);
                if (tpScript == null) continue;
                var scriptObj = MakeScriptByType(tpScript);
                if (node.Attributes["title"] != null)
                    scriptObj.Title = node.Attributes["title"].Value;
                scriptObj.ScriptName = scriptName;
                try
                {
                    var filledScript = scriptObj.LoadScriptFromXml(node);
                    if (filledScript != null)
                        scripts.Add(filledScript);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error loading script \"{0}\" from XML: {1}",
                        scriptName, ex);                    
                }                
            }
            return scripts;
        }

        /// <summary>
        /// загрузить общий список скриптов (из собственных сборок и из dll)
        /// </summary>
        public static void Initialize()
        {
            scriptClasses = new Dictionary<string, Type>();
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            var types = new List<Type>();
            types.AddRange(a.GetTypes());
            try
            {
                foreach (var p in PluginManager.Instance.PluginAssemblies)
                {
                    try
                    {
                        types.AddRange(p.GetTypes());
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat(Localizer.GetString("MessageLoadingScriptsErrorFmt"), 
                            ex, p.FullName);
                    }
                }
                foreach (var t in types)
                {
                    if (t.BaseType != typeof(TerminalScript)) continue;
                    foreach (var atr in t.GetCustomAttributes(typeof(DisplayNameAttribute), false))
                    {
                        var dispName = ((DisplayNameAttribute)atr).DisplayName;
                        scriptClasses.Add(dispName, t);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TerminalScript - ошибка загрузки скриптов", ex);
            }
        }

        public static Dictionary<string, Type> GetAllTerminalScripts()
        {
            return scriptClasses.ToDictionary(p => p.Key, p => p.Value);
        }

        public static TerminalScript MakeScriptByType(Type scriptType)
        {
            return (TerminalScript) scriptType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        }

        public static TerminalScript MakeScriptByScriptName(string scriptName)
        {
            return MakeScriptByType(scriptClasses[scriptName]);
        }
    }

    public class DummyScript : TerminalScript
    {
        public DummyScript()
        {
            ScriptName = "<" + Localizer.GetString("TitleNewScriptSmall") + ">";
            Title = "<" + Localizer.GetString("TitleNewScriptSmall") + ">";
        }

        public override string ActivateScript(CandleChartControl chart, PointD worldCoords)
        {
            throw new NotImplementedException();
        }

        public override string ActivateScript(string ticker)
        {
            throw new NotImplementedException();
        }

        public override string ActivateScript(bool byTrigger)
        {
            throw new NotImplementedException();
        }
    }
}
