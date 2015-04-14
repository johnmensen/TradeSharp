using System.Collections.Generic;

namespace TradeSharp.Server.Contract
{
    /// <summary>
    /// открытие позиции, закрытие позиции, логин, смена счета,
    /// редактирование ордера и т.п.
    /// 
    /// используется сервером при проверке прав
    /// </summary>
    enum UserOperation
    {
        MakeNewOrder = 0,
        CloseOrder,
        ModifyOrder,
        ChangeAccountSettings,
        GetAccountDetail,
        SendTradeSignal,
        BindToSignal
    }

    static class UserOperationRightsStorage
    {
        private static readonly Dictionary<UserOperation, bool> protectedOperations =
            new Dictionary<UserOperation, bool>
                {
                    {UserOperation.MakeNewOrder, true},
                    {UserOperation.CloseOrder, true},
                    {UserOperation.ModifyOrder, true},
                    {UserOperation.ChangeAccountSettings, true},
                    {UserOperation.GetAccountDetail, true},
                    {UserOperation.SendTradeSignal, true},
                    {UserOperation.BindToSignal, true},
                };
        private static readonly Dictionary<UserOperation, bool> tradeOperations =
            new Dictionary<UserOperation, bool>
                {
                    {UserOperation.MakeNewOrder, true},
                    {UserOperation.CloseOrder, true},
                    {UserOperation.ModifyOrder, true},
                    {UserOperation.ChangeAccountSettings, true},
                    {UserOperation.GetAccountDetail, false},
                    {UserOperation.SendTradeSignal, true},
                    {UserOperation.BindToSignal, false},
                };
        
        public static bool IsProtectedOperation(UserOperation op)
        {
            return protectedOperations[op];
        }

        public static bool IsTradeOperation(UserOperation op)
        {
            return tradeOperations[op];
        }
    }
}
