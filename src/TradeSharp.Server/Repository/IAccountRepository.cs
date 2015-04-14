using System;
using System.Collections.Generic;
using TradeSharp.Contract.Entity;

namespace TradeSharp.Server.Repository
{
    public interface IAccountRepository
    {
        AccountGroup GetAccountGroup(int accountId);
        
        Account GetAccount(int accountId);

        Account GetAccount(int accountId, out RequestStatus errorCode);

        List<Account> GetAccounts(out RequestStatus errorCode);

        List<string> GetTickersTradedCheckCredentials(string hash, string userLogin, int accountId,
                                                      long localTime, bool checkCredentials);

        List<MarketOrder> GetClosedOrOpenedOrdersCheckCredentials(string hash, string userLogin, long localTime,
                                                                  int accountId, string optionalSymbolFilter,
                                                                  int startId, int maxCount, bool checkCredentials,
                                                                  bool needClosedOrders);

        RequestStatus ChangeBalance(int accountId, decimal summ, string comment, DateTime date,
                                    BalanceChangeType changeType);
    }
}
