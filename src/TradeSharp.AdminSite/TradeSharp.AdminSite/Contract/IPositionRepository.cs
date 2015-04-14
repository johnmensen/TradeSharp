using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;
using TradeSharp.Linq;
using TradeSharp.SiteAdmin.Helper;
using TradeSharp.SiteAdmin.Models;
using TradeSharp.SiteAdmin.Models.Items;

namespace TradeSharp.SiteAdmin.Contract
{
    public interface IPositionRepository
    {
        PositionListModel GetPositionList(int accountId);
        PositionListModel GetPositionList(PositionListModel positionListModel);
        MarketOrder GetPositionItemDetails(int positionId);
        List<PositionItem> GetPositionsById(int[] idList);
        bool UpdateSavePositionItem(string strId, List<SystemProperty> propertyMetaData, PositionState state);
        bool ReopenPositions(string strId);
        List<string> ClosingPositions(string strId, DateTime timeExit, PositionExitReason exitReason, List<Tuple<string, int, float>> lstPrice);
        List<int> CancelingOpenPositions(string strId);
        List<int> CancelingClosedPositions(string strId);
        bool EditDangerDeal(string strId, PositionState currentState, string newTicker, int? newSide, int? newVolume, float? newEnterPrice, float? newExitPrice);
        bool NewDeal(PositionItem positionItem);
        void UpdateBalanceChange(TradeSharpConnection ctx, MarketOrder closedPos, bool deleteTransferOnly);
        bool ReCalculateAccountBalance(TradeSharpConnection ctx, int accountId);
    }
}