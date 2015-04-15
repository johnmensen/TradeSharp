using System;
using System.Collections.Generic;
using System.ServiceModel;
using Entity;
using TradeSharp.Util;

namespace TradeSharp.Reports.Lib.Contract
{
    /// <summary>
    /// запросить данные по индексам
    /// </summary>
    [ServiceContract]    
    public interface IIndexGrabber
    {
        /// <summary>
        /// имя индекса - данные по индексу
        /// </summary>        
        [OperationContract(IsOneWay = false)]
        Dictionary<string, List<Cortege2<DateTime, float>>> GetIndexData();
    }
}
