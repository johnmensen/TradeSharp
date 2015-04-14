//+------------------------------------------------------------------+
//|                                         UdpCommandTranslator.mq4 |
//|                      Copyright © 2010, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2010, MetaQuotes Software Corp."
#property link      "http://www.metaquotes.net"

#import "UdpQueueLib.dll"
int StartListenAddr(string addr, int port);
int StopListen();
string PickMessage();
int GetPickFlag();
int SendMessageUDP(string str, string addr, int port);

// входные параметры
// если режим - slave - MT4 исполняет ордера от T#
// если slave = false, MT4 транслирует ордера в T#
extern bool      slave = true;
extern string    address = "127.0.0.1";
extern int       portOwn = 8011;
extern int       portTs = 8010;
extern int       slipMilliseconds = 300;
extern double    slippage = 6;

//+------------------------------------------------------------------+
//| распарсить строку в массив                                       |
//+------------------------------------------------------------------+
void split(string& arr[], string str, string sym) 
{
   ArrayResize(arr, 0);

   string item;
   int pos, size;

   int len = StringLen(str);
   for (int i = 0; i < len;) 
   {
      pos = StringFind(str, sym, i);
      if (pos == -1) pos = len;

      item = StringSubstr(str, i, pos - i);
      item = StringTrimLeft(item);
      item = StringTrimRight(item);

      size = ArraySize(arr);
      ArrayResize(arr, size + 1);
      arr[size] = item;

      i = pos + 1;
   }
}

//+------------------------------------------------------------------+
//| выбрать ордер по его magic среди открытых ордеров                |
//| вернуть false, если не выбран                                    |
//+------------------------------------------------------------------+
bool SelectByMagic(int magic)
{
   int curMagic;
   for (int i = 0; i < 500; i++)
   {
      if (OrderSelect(i, SELECT_BY_POS) == false) break;
      curMagic = OrderMagicNumber();
      if (curMagic == magic) return (true);
   }
   return (false);
}

//+------------------------------------------------------------------+
//| войти в рынок (попытка)                                          |
//| cmd = OP_BUY | OP_SELL                                           |
//+------------------------------------------------------------------+
void OpenPos(int cmd, string symb, int volume, int magic)
{
   double price = 0, stoploss = 0, takeprofit = 0;   
   double lotSize = MarketInfo(symb, MODE_LOTSIZE);
   double lotStep = MarketInfo(symb, MODE_LOTSTEP);
   double lotMin = MarketInfo(symb, MODE_MINLOT);
   if (volume < lotMin)
   {
       Print("Order volume is less than min lot: ", symb, ", vol=", volume, ", min lot=", lotMin);
       return; 
   }
   double numLots = MathRound(((volume - lotMin * lotSize) / lotSize) / lotStep);
   double lots = lotMin + lotStep * numLots;
      
   if (cmd == OP_BUY) 
      price = MarketInfo(symb, MODE_ASK);
   else
      price = MarketInfo(symb, MODE_BID);
   
   int ticket = OrderSend(symb, cmd, lots, price, slippage, stoploss, takeprofit,
      "UDP C.T.", magic);
   
   if (ticket >= 0)
   {
      Print("Order succeeded: ", cmd, " ", symb, " ", lots, " at ", 
         price, " magic is ", magic);
   }
   else
   {
      int err = GetLastError();
      Print("Order failed: ", cmd, " ", symb, " ", lots, " at ", 
         price, " magic is ", magic, ". Error code is ", err);
   }
}

void CloseCurrentPos()
{
   int cmd = OrderType();
   double price = 0;
   if (cmd == OP_SELL) 
      price = MarketInfo(OrderSymbol(), MODE_ASK);
   else
      price = MarketInfo(OrderSymbol(), MODE_BID);
   Print("Exit price is ", price); 
   if (!OrderClose(OrderTicket(), OrderLots(), price, slippage))
   {
      int errCode = GetLastError();
      Print("Can not close order. The reason code is ", errCode);
   }
}

//+------------------------------------------------------------------+
//| закрыть позицию                                                  |
//+------------------------------------------------------------------+
void ClosePos(int magic)
{
   Print("Close pos with magic ", magic);
   if (SelectByMagic(magic))   
      CloseCurrentPos();   
}

//+------------------------------------------------------------------+
//| закрыть позиции, не попавшие в список                            |
//+------------------------------------------------------------------+
void CloseNotActual(string &arPtrs[])
{
   // сформировать массив magic-номеров позиций, которые должны остаться
   int count = ArraySize(arPtrs) - 1;
   int magics[1] = { 0 };
   int i = 0;
   ArrayResize(magics, count);
      
   for (i = 0; i < count; i++)
   {
      int magic = StrToInteger(arPtrs[i + 1]);
      magics[i] = magic;
   }
   
   // пройтись по позициям...
   for (i = 0; i < 500; i++)
   {
      if (OrderSelect(i, SELECT_BY_POS) == false) break;
      int curMagic = OrderMagicNumber();
      bool hasMagic = false;
      for (int j = 0; j < count; j++)
      {
         if (magics[j] == curMagic)
         {
            hasMagic = true;
            break;
         }
      }
      
      if (hasMagic) continue;
      
      // закрыть позицию
      CloseCurrentPos();
   }
   
}

//+------------------------------------------------------------------+
//| исполнить команду                                                |
//| CLOS_1020_1021                                                   |
//| BUY_1032_USDCHF_1030                                             |
//| ACTLIST_1011_1012_1045_1091_1092_1093                            |
//+------------------------------------------------------------------+
void ExecuteCommand(string cmdStr)
{
   string arr[];
   split(arr, cmdStr, "_");
      
   if (arr[0] == "CLOS")
   {
      int count = ArraySize(arr) - 1;
      for (int i = 0; i < count; i++)
      {
         int magic = StrToInteger(arr[i + 1]);
         ClosePos(magic);
      }   
      return;
   }
   
   if (arr[0] == "ACTLIST")
   {
      CloseNotActual(arr);
      return;
   }

   Print(cmdStr);
   
   int cmd = OP_BUY;
   if (arr[0] == "SELL") cmd = OP_SELL;
   // cmd - smb - vol - magic
   OpenPos(cmd, arr[2], StrToInteger(arr[1]), StrToInteger(arr[3]));
}

//+------------------------------------------------------------------+
//| отправить в T# актуальную информацию об открытых по счету        |
//| позициях и текущему балансу                                      |
//+------------------------------------------------------------------+
void SendOrdersToTradeSharp()
{
   string strBal = StringConcatenate("EQT=", DoubleToStr(AccountEquity(), 0));
      
   // собрать строку 
   // пройтись по позициям...
   for (int i = 0; i < 500; i++)
   {
      if (OrderSelect(i, SELECT_BY_POS) == false) break;
      
      int sideType = OrderType();
      if (sideType != OP_BUY && sideType != OP_SELL) continue;
      int side = 1;
      if (sideType == OP_SELL) side = -1;
      
      string orderSymbol = OrderSymbol();
      double lotSize = MarketInfo(orderSymbol, MODE_LOTSIZE);
      double volume = lotSize * OrderLots();
                 
      strBal = StringConcatenate(strBal, ";",
          OrderTicket(), ",",                      /* 510142 - ID  */
          orderSymbol, ",",                        /* EURUSD - Symbol */
          volume, ",",                             /* 10000 - Volume */
          side, ",",                               /* -1 - Side */
          DoubleToStr(OrderStopLoss(), 5), ",",    /* 1.3562 - Stoploss */
          DoubleToStr(OrderTakeProfit(), 5), ",",  /* 1.3262 - Takeprofit */
          DoubleToStr(OrderOpenPrice(), 5));       /* 1.3462 - Open price */
   }
   
   // отправить строку на сервер
   SendMessageUDP(strBal, address, portTs);
}

//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
{
   if (StartListenAddr(address, portOwn) == 0)
      Print("Error in StartListenAddr");
   if (GetPickFlag() == 0)
      Print("Started listening ", address, ":", portOwn);
   else
      Print("Error in StartListenAddr: ", GetPickFlag());
   EventSetMillisecondTimer(50);
   return(0);
}

//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
{
   EventKillTimer();
   Print("Stopping listening");
   StopListen();
   Print("Stopped listening");
   return(0);
}

void OnTimer()
{
   string str = PickMessage();         
   if (StringLen(str) > 0) 
   {
      Comment(str);
      if (slave) ExecuteCommand(str);
   }
   // сообщить свой баланс
   if (slave)
   {
       string strBal = StringConcatenate("EQT=", DoubleToStr(AccountEquity(), 0));
       SendMessageUDP(strBal, address, portTs);
   }
   else
   {
      SendOrdersToTradeSharp();
   }
}
