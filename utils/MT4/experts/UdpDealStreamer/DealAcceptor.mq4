//+------------------------------------------------------------------+
//|                                         UdpCommandTranslator.mq4 |
//|                      Copyright © 2010, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2010, MetaQuotes Software Corp."
#property link      "http://www.metaquotes.net"

#import "UdpQueueLib.dll"
int StartListen(int port);
int StopListen();
string PickMessage();
int GetPickFlag();

//---- input parameters
extern int       port = 8001;
extern int       slipMilliseconds = 300;
extern double    volume = 0.2;
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
void OpenPos(int cmd, string symb, string comment, int magic)
{
   double price = 0, stoploss = 0, takeprofit = 0;
   
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
//| закрыть позицию                                                  |
//+------------------------------------------------------------------+
void ClosePos(int magic)
{
   if (SelectByMagic(magic))   
   {
      int cmd = OrderType();
      double price = 0;
      if (cmd == OP_BUY) 
         price = MarketInfo(OrderSymbol(), MODE_ASK);
      else
         price = MarketInfo(OrderSymbol(), MODE_BID);
      
      OrderClose(OrderTicket(), OrderLots(), price, slippage);
   }
}

//+------------------------------------------------------------------+
//| исполнить команду                                                |
//| BUY_EURUSD_Comment string_666                                    |
//| CLOSE_666                                    |
//+------------------------------------------------------------------+
void ExecuteCommand(string cmdStr)
{
   string arr[0];
   split(arr, cmdStr, "_");
   int magic;
   
   if (arr[0] == "CLOSE")
   {
      magic = StrToInteger(arr[1]);
      ClosePos(magic);
      return;
   }
   
   int cmd = OP_BUY;
   if (arr[0] == "SELL") cmd = OP_SELL;
   magic = StrToInteger(arr[3]);
   OpenPos(cmd, arr[1], arr[2], magic);
}

//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
{
   StartListen(port);
   Print("Started listening");
   return(0);
}
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
{
   Print("Stopping listening");
   StopListen();
   Print("Stopped listening");
   return(0);
}
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
{      
   string str;
   while (true)
   {      
      str = PickMessage();         
      if (StringLen(str) > 0) 
      {
         Comment(str);      
         Print(str);
         ExecuteCommand(str);
      }
      Sleep(slipMilliseconds);
   }     
   return(0);
}
//+------------------------------------------------------------------+