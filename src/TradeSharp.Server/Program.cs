using System.ServiceProcess;
using TradeSharp.Localisation;
using TradeSharp.Util;

namespace TradeSharp.Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            Localizer.ResourceResolver = new ResourceResolver();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new LiveServer() 
			};
            ServiceBase.Run(ServicesToRun);
        }

        class ResourceResolver : IResourceResolver
        {
            public string TryGetResourceValue(string resxKey)
            {
                return LocalisationManager.Instance.GetString(resxKey);
            }
        }
    }
}
