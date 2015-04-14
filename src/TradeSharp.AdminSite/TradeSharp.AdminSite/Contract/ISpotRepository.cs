using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Models;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface ISpotRepository
    {
        List<SPOT> GetAllSpotSymbols(TradeSharpConnection context);
        string AddNewSpotItem(TradeSharpConnection context, SpotModel model);
        string EditSpotItem(TradeSharpConnection context, SpotModel model);
        string DeleteSpotItem(TradeSharpConnection context, string title);
    }
}