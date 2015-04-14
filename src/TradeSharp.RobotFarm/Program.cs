using System.ServiceProcess;
using TradeSharp.Localisation;
using TradeSharp.Util;

namespace TradeSharp.RobotFarm
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            Localizer.ResourceResolver = new ResourceResolver();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new RobotFarmService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }

    class ResourceResolver : IResourceResolver
    {
        public string TryGetResourceValue(string resxKey)
        {
            return LocalisationManager.Instance.GetString(resxKey);
        }
    }
}
