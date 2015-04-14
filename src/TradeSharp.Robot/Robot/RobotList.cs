using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using TradeSharp.Util;

namespace TradeSharp.Robot.Robot
{
    public static class RobotCollection
    {
        private static readonly Dictionary<string, Type> robots = new Dictionary<string, Type>();
        
        public static List<string> RobotNames
        {
            get { return robots.Keys.OrderBy(o => o).ToList(); }
        }

        public static BaseRobot MakeRobot(string str)
        {
            Type robType;
            robots.TryGetValue(str, out robType);
            if (robType == null)
                throw new Exception(string.Format("Отсутствует описание робота класса \"{0}\"", str));
            
            var defaultCons = robType.GetConstructor(new Type[0]);
            if (defaultCons == null)
                throw new Exception(string.Format(
                    "Для робота типа {0} не определен конструктор без аргументов", robType));
            
            var bot = (BaseRobot)defaultCons.Invoke(new object[0]);
            bot.TypeName = str;
            return bot;
        }

        public static void Initialize()
        {
            // получить роботов из своей сборки
            LoadTypesFromAssembly(Assembly.GetAssembly(typeof (BaseRobot)));

            // получить роботов из плагинов
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
                if (asm != null) LoadTypesFromAssembly(asm);
            }
        }

        private static void LoadTypesFromAssembly(Assembly chartAsm)
        {
            try
            {
                foreach (var t in chartAsm.GetTypes())
                {
                    try
                    {
                        if (t.IsSubclassOf(typeof(BaseRobot)))
                        {
                            var robotName = t.Name;
                            var attrs = t.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                            if (attrs.Length > 0)
                                robotName = ((DisplayNameAttribute)attrs[0]).DisplayName;

                            // проверить уникальность ключа
                            while (robots.ContainsKey(robotName))
                            {
                                robotName = robotName + "+";
                            }

                            robots.Add(robotName, t);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat("RobotCollection - ошибка загрузки робота {0}: {1}", t.Name, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("RobotCollection - ошибка загрузки роботов из {0}: {1}",
                    chartAsm.FullName, ex);
            }            
        }
    }
}
