using System;
using System.Reflection;
using System.Resources;

namespace TradeSharp.Localisation
{
    public class LocalisationManager
    {
        private readonly ResourceManager resourceManager;

        #region Signletone

        private static readonly Lazy<LocalisationManager> instance = new Lazy<LocalisationManager>(() => new LocalisationManager());

        public static LocalisationManager Instance
        {
            get { return instance.Value; }
        }

        private LocalisationManager()
        {
            try
            {
                resourceManager = 
                    new ResourceManager("TradeSharp.Localisation.Properties.Resources", Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                resourceManager = null;
            }
        }

        #endregion

        public string GetString(string key)
        {
            return resourceManager != null ? resourceManager.GetString(key) : null;
        }
    }
}
