using System;
using System.Collections.Generic;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Client.BL.Script
{
    /// <summary>
    /// хранилище скриптов
    /// </summary>
    class ScriptManager
    {
        public static ScriptManager Instance { get; private set; }
        
        private List<TerminalScript> scripts;

        public Action scriptListIsUpdated;

        public static void Initialize()
        {
            Instance = new ScriptManager();
            TerminalScript.Initialize();
            // загрузить коллекцию настроенных скриптов
            using (new TimeLogger("ScriptManager: LoadScripts"))
            Instance.LoadScripts();
        }

        public void LoadScripts()
        {
            var nodeScript = ToolSettingsStorageFile.LoadNode(ToolSettingsStorageFile.NodeNameScripts);
            if (nodeScript == null) return;
            scripts = TerminalScript.LoadFromXml(nodeScript);
        }

        public void SaveScripts()
        {
            // получить узел скриптов
            var nodeScript = 
                ToolSettingsStorageFile.LoadOrCreateNode(ToolSettingsStorageFile.NodeNameScripts);
            // и почистить его
            while (nodeScript.HasChildNodes)            
                nodeScript.RemoveChild(nodeScript.FirstChild);
            // сохранить скрипты
            if (scripts != null)
                foreach (var script in scripts)
                    script.SaveInXml(nodeScript);
            // сохранить документ
            ToolSettingsStorageFile.SaveXml(nodeScript.OwnerDocument);
        }

        public List<TerminalScript> GetScripts()
        {
            return scripts == null
                       ? new List<TerminalScript>() : scripts.ToList();
        }

        public void UpdateScripts(List<TerminalScript> newScripts)
        {
            scripts = newScripts;
            SaveScripts();
            if (scriptListIsUpdated != null)
                scriptListIsUpdated();
        }
    }
}
