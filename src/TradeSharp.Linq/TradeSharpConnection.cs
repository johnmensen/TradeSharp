using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Objects;
using System.Linq;
using TradeSharp.Util;

namespace TradeSharp.Linq
{
    public partial class TradeSharpConnection
    {
        public Exception TestException { get; set; }

        public TradeSharpConnection(string connStr)
            : base(connStr)
        {
        }

        public TradeSharpConnection(DbConnection dbc)
            : base(dbc, true)
        {
        }

        public virtual IEnumerable<GetVolumesByTiker_Result> GetVolumesByTikerWrapped()
        {
            return GetVolumesByTiker();
        }

        public virtual IEnumerable<GetPositionsToSwap_Result> GetPositionsToSwapWrapped()
        {
            return GetPositionsToSwap();
        }
        
        public virtual List<GetAllAccounts_Result> GetAllAccountsWrapped()
        {
            var objResult = GetAllAccounts();
            return objResult == null ? null : objResult.ToList();
        }

        public virtual IEnumerable<GetTransfersSummary_Result> GetTransfersSummaryWrapped(int userId)
        {
            return GetTransfersSummary(userId);
        }

        public virtual List<GetPositionList_Result> GetPositionListWrapped(int? countItemShow, int? accountId, int? isDemoAccount,
            string symbol, int? status, int? side,
            DateTime? timeOpenFrom, DateTime? timeOpenTo, DateTime? timeExitFrom,
            DateTime? timeExitTo, out int totalItemCount)
        {

            var totalCountPositeion = new ObjectParameter("TotalItemCount", typeof(int));
            try
            {
                var objQuiry = GetPositionList(countItemShow, accountId, isDemoAccount, symbol, status, side, timeOpenFrom,
                    timeOpenTo, timeExitFrom, timeExitTo, totalCountPositeion);

                var objResult = objQuiry == null ? null : objQuiry.ToList();
                totalItemCount = totalCountPositeion.Value != null ? (int)totalCountPositeion.Value : -1;
                return objResult;
            }
            catch (Exception ex)
            {
                Logger.Error("TradeSharpConnection.GetPositionListWrapped()", ex);
                totalItemCount = -1;
                return null;
            }
        }

        public virtual IEnumerable<GetGroupsWithAccounts_Result> GetGroupsWithAccountsWrapped(string accountGroupCode)
        {
            return GetGroupsWithAccounts(accountGroupCode);
        }
    }
}
