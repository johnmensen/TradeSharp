using System.ServiceModel;
using TradeSharp.Contract.Util.BL;

namespace TradeSharp.Delivery.Service.WebServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ModuleStatusController : DefaultModuleStatusController
    {
        private static ModuleStatusController instance;
        public static ModuleStatusController Instance
        {
            get { return instance ?? (instance = new ModuleStatusController()); }
        }

        private ModuleStatusController() { }

        protected override string ModuleName
        {
            get { return "ModuleStatusController"; }
        }
    }
}
