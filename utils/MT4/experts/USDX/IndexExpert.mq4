//+------------------------------------------------------------------+
//|                                                  IndexExpert.mq4 |
//|                                                    Andrey Sitaev |
//|                                        http://www.forexinvest.ru |
//+------------------------------------------------------------------+
#property copyright "Andrey Sitaev"
#property link      "http://www.forexinvest.ru"

extern double volume = 0.10;
extern int magic = 300;
extern double slippage = 5;
extern double stoploss = 0;
extern double takeprofit = 0;

int lastBarsCount = 0;
double lastIndex = 0;

//+------------------------------------------------------------------+
//| выбрать ордер по его magic среди открытых ордеров                |
//| вернуть false, если не выбран                                    |
//+------------------------------------------------------------------+
bool SelectByMagic()
{
   //int curMagic;
   //for (int i = 0; i < 500; i++)
   //{
     // if (OrderSelect(i, SELECT_BY_POS) == false) break;
      //curMagic = OrderMagicNumber();
      //if (curMagic == magic) return (true);
   //}
   //return (false);
   return (OrderSelect(0, SELECT_BY_POS, MODE_TRADES));
}
//+------------------------------------------------------------------+
//| cmd = OP_BUY | OP_SELL                                           |
//+------------------------------------------------------------------+
void OpenPos(int cmd, string symb, string comment)
{    
    double price = 0, stoploss = 0, takeprofit = 0;
    magic = magic + 1;

    if (cmd == OP_BUY) 
        price = MarketInfo(symb, MODE_ASK);
    else
        price = MarketInfo(symb, MODE_BID);

    int ticket = OrderSend(symb, cmd, volume, price, slippage, stoploss, takeprofit,
      comment, magic);

    if (ticket >= 0)
    {
        Print("Order succeeded: ", cmd, " ", symb, " ", volume, " at ", 
            price, " magic is ", magic);
    }
    else
    {
        int err = GetLastError();
        Print("Order failed: ", cmd, " ", symb, " ", volume, " at ", 
            price, " magic is ", magic, ". Error code is ", err);
    }
}
//+------------------------------------------------------------------+
//| закрыть позицию и открыть новую в противоположном направлении,   |
//| если направление позиции отличается от прежнего                  |
//| newSide == OP_BUY || OP_SELL                                     |
//+------------------------------------------------------------------+
void OpenClosePos(int newSide)
{
   if (SelectByMagic())   
   {
      int cmd = OrderType();
      double price = 0;
      
      Print("Side is: ", cmd, ", new side is: ", newSide);
      if (cmd == newSide) return;
      
      if (newSide == OP_BUY) 
         price = MarketInfo(OrderSymbol(), MODE_ASK);
      else
         price = MarketInfo(OrderSymbol(), MODE_BID);
      
      OrderClose(OrderTicket(), OrderLots(), price, slippage);
   }
   OpenPos(newSide, Symbol(), "[IndexExpert]");
}
//+------------------------------------------------------------------+
//| вернуть EURX - lastIndex                                         |
//+------------------------------------------------------------------+
double GetDeltaIndex()
{
    // коэффициенты EURX   
    double kEusd = 0.342377, kEgbp = 0.331633, kEjpy = 0.205209, 
        kEchf = 0.120781, kEsek = 0; 
    double eurusd, eurgbp, eurjpy, eurchf, 
        eurx, delta;
    
    eurusd = iClose("EURUSD", 0, 0);      
    eurgbp = iClose("EURGBP", 0, 0);
    eurjpy = iClose("EURJPY", 0, 0);
    eurchf = iClose("EURCHF", 0, 0);     
    eurx = 1 * MathPow(eurusd, kEusd) * MathPow(eurgbp, kEgbp) * 
        MathPow(eurjpy, kEjpy) * MathPow(eurchf, kEchf); 
    delta = eurx - lastIndex;
    lastIndex = eurx;
    return (delta);
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
    double eurx;
    int newSide = 0;
    
    if (Bars <= lastBarsCount) return (0);
    lastBarsCount = Bars;
    
    // получить направление индекса
    eurx = GetDeltaIndex();
    if (eurx > 0) newSide = OP_BUY;
        else newSide = OP_SELL;
    OpenClosePos(newSide);
    
    return (0);
}
//+------------------------------------------------------------------+