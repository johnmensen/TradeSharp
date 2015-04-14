//+------------------------------------------------------------------+
//|                                            TrailExpertSimple.mq4 |
//|                                                    Andrey Sitaev |
//|                                        http://www.forexinvest.ru |
//+------------------------------------------------------------------+
#property copyright "Andrey Sitaev"
#property link      "http://www.forexinvest.ru"

#define deals_max 50

// дл€ всех остальных сделок
extern int        commonLevel = 500;
extern int        commonTarget = 100;
// TP дл€ всех сделок, контролируетс€ самим экспертом (не ордер)
extern int        commonTP = 0;
extern int        maxCloseSlippage = 200;

void ProtectDeal(int ticket, double sl)
{
    Print("«ащита ордера ", ticket, " - перенос стопа на ", sl);
    if (!OrderModify(ticket, OrderOpenPrice(), sl, OrderTakeProfit(), 0, Blue))
    {
        Print("Ќевозможно защитить ордер: ", GetLastError());
    }
}

void CloseDeal(int ticket, string reason)
{
    double curPrice;
    int priceMode = MODE_BID, error;
    Print("«акрытие ордера ", ticket, " TrailExpertSimple (", reason, ")");
    
    if (OrderType() == OP_SELL) priceMode = MODE_ASK;
    curPrice = MarketInfo(OrderSymbol(), priceMode);
    
    if (!OrderClose(ticket, OrderLots(), curPrice, maxCloseSlippage))
    {
        error = GetLastError();
        Print("ќшибка закрыти€ ордера (", error, ")");
    }
}

void CheckDeals()
{
    int orderType, orderSide;
    double pipCost, curPrice, stop, tpPrice;
    double controlLevel, targetLevel;
    
    for (int i = 0; i < 1000; i++)
    {
        if (!OrderSelect(i, SELECT_BY_POS))
        {// такой ордер не найден - исключить его
            break;
        }
        orderType = OrderType();
        if (orderType != OP_BUY && orderType != OP_SELL) continue;
        orderSide = 1;
        if (orderType == OP_SELL) orderSide = -1;
        
        pipCost = MarketInfo(OrderSymbol(), MODE_POINT);
        controlLevel = OrderOpenPrice() + orderSide * commonLevel * pipCost;
        targetLevel = OrderOpenPrice() + orderSide * commonTarget * pipCost;            
        curPrice = MarketInfo(OrderSymbol(), MODE_BID);
                             
        // проверить условие TP
        if (commonTP > 0)
        {
            tpPrice = OrderOpenPrice() + orderSide * commonTP * pipCost;
            if (orderType == OP_BUY && curPrice >= tpPrice) CloseDeal(OrderTicket(), "TP");
            if (orderType == OP_SELL && curPrice <= tpPrice) CloseDeal(OrderTicket(), "TP");
        }
        
        // проверить условие переноса стопа - переносить стоп, если он установлен хуже        
        stop = OrderStopLoss();
        
        if (orderType == OP_BUY && curPrice >= controlLevel)
        {
            // не находитс€ ли сделка в лучшей позиции по стопу?
            if (targetLevel > stop)            
                ProtectDeal(OrderTicket(), targetLevel);
            continue;
        }
        if (orderType == OP_SELL && curPrice <= controlLevel)
        {
            // не находитс€ ли сделка в лучшей позиции по стопу?
            if (targetLevel < stop || stop == 0)            
                ProtectDeal(OrderTicket(), targetLevel);            
            continue;
        }
    }
}

//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
{    
    return(0);
}
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
{
    return(0);
}
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
{
    CheckDeals();
    return (0);
}
//+------------------------------------------------------------------+