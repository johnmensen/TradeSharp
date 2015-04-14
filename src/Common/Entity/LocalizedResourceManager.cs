using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using TradeSharp.Util;

namespace Entity
{
    public class LocalizedResourceManager : ResourceManager
    {
        private static LocalizedResourceManager instance;
        private static bool loaded;

        private LocalizedResourceManager(string baseName, Assembly assembly) : base(baseName, assembly)
        {
        }

        public static LocalizedResourceManager Instance
        {
            get
            {
                try
                {
                    if (!loaded)
                    {
                        var path = ExecutablePath.ExecPath + "\\TradeSharp.WhiteLabel.dll";
                        if (!File.Exists(path))
                            return instance;

                        var asm = Assembly.LoadFile(path);
                        if (asm == null) 
                            return instance;

                        var resFiles = asm.GetManifestResourceNames();
                        var rsxName = resFiles.FirstOrDefault(r => r.EndsWith(".resources"));
                        if (string.IsNullOrEmpty(rsxName)) 
                            return instance;
                        rsxName = rsxName.Substring(0, rsxName.Length - ".resources".Length);

                        instance = new LocalizedResourceManager(rsxName, asm);
                        Logger.Info("Дополнительные ресурсы загружены");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info("Дополнительных ресурсов не найдено или возникла ошибка при их загрузке: " + ex.Message);
                }
                finally
                {
                    loaded = true;
                }
                return instance;
            }
        }
    }
}
