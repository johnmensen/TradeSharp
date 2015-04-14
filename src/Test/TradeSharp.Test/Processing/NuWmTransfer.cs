using System;
using Finance.Entities;
using WebMoney.BasicObjects;
using WebMoney.XmlInterfaces.Responses;

namespace TradeSharp.Test.Processing
{
    /// <summary>
    /// Вспомогательный клас для того, что бы можно было создать тестовые данные для тестов
    /// </summary>
    public class NuWmTransfer : Transfer
    {
        public NuWmTransfer(decimal amount, DateTime createTime, string description, uint id, Purse targetPurse,
                uint ts, DateTime updateTime, decimal commission, uint invoiceId, bool isLocked, uint orderId,
                WmId partner, byte period, decimal rest, Purse sourcePurse, uint transferId, TransferType transferType)
            {
                Amount = (Amount) amount;
                CreateTime = createTime;
                Description = (Description)description;
                Id = id;
                TargetPurse = targetPurse;
                Ts = ts;
                UpdateTime = updateTime;


                Commission = (Amount)commission;
                InvoiceId = invoiceId;
                IsLocked = isLocked;
                OrderId = orderId;
                Partner = partner;
                Period = period;
                Rest = (Amount)rest;
                SourcePurse = sourcePurse;
                TransferId = transferId;
                TransferType = transferType;
            }
    }
}
