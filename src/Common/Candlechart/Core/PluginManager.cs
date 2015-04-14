using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Candlechart.Series;
using TradeSharp.Util;

namespace Candlechart.Core
{
    /// <summary>
    /// читает из папки plugin библиотеки, содержащие
    /// индикаторы, роботов и графические объекты
    /// </summary>
    public class PluginManager
    {
        #region Singletone

        private static readonly Lazy<PluginManager> instance = new Lazy<PluginManager>(() => new PluginManager());

        public static PluginManager Instance
        {
            get { return instance.Value; }
        }

        private PluginManager()
        {
        }

        #endregion

        public readonly List<Type> typeIndicators = new List<Type>();

        public readonly List<Type> typeSeriesObjects = new List<Type>();

        public readonly List<Type> typeSeries = new List<Type>();

        private List<Assembly> pluginAssemblies = new List<Assembly>();

        public List<Assembly> PluginAssemblies
        {
            get { return pluginAssemblies; }
        }
        
        public void Initialize()
        {
            // загрузить типы из сборок самого терминала
            var ownAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var chartAsm = ownAssemblies.First(a => a.FullName.StartsWith("Candlechart,"));
            LoadTypesFromAssembly(chartAsm);

            // и из библиотек каталога plugin
            var path = ExecutablePath.ExecPath + "\\plugin";
            if (!Directory.Exists(path)) return;
            foreach (var filePath in Directory.GetFiles(path, "*.dll"))
            {
                Assembly asm = null;
                try
                {
                    asm = Assembly.LoadFrom(filePath);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Ошибка загрузки сборки плагина \"{0}\": {1}",
                        filePath, ex.Message);
                }
                if (asm != null)
                {
                    pluginAssemblies.Add(asm);
                    LoadTypesFromAssembly(asm);
                }
            }
        }

        private void LoadTypesFromAssembly(Assembly chartAsm)
        {
            try
            {
                foreach (var t in chartAsm.GetTypes())
                {
                    try
                    {
                        if (t.IsSubclassOf(typeof(InteractiveObjectSeries)))
                        {
                            typeSeries.Add(t);
                            continue;
                        }

                        foreach (var i in t.GetInterfaces())
                        {
                            if (i.FullName == "Candlechart.Indicator.IChartIndicator")
                            {
                                typeIndicators.Add(t);
                                break;
                            }
                            if (i.FullName == "Candlechart.Core.IChartInteractiveObject")
                            {
                                typeSeriesObjects.Add(t);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("Ошибка загрузки библиотеки {0}, тип {1}: {2}",
                                chartAsm.FullName, t.Name, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Ошибка загрузки библиотеки {0}: {1}",
                    chartAsm.FullName, ex);
            }
        }
    }
}
