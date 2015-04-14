//+------------------------------------------------------------------+
//|                                                MTHedgeTester.mq4 |
//|                      Copyright © 2009, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2009, MetaQuotes Software Corp."
#property link      "http://www.metaquotes.net"

//---- input parameters
extern double    lotSize = 0.2;
extern int       candlesBetweenOpen = 4;
extern int       candlesBetweenClose = 3;
extern int       side = 0; // BUY = 0, SELL = 1
extern int       slippagePP = 30;

int lastCandleSinceOpen = -1;
int magicNum = 1;
int lastTicket = -1;

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
void OpenPosition()  
{
   double price = Bid;
   if (side == OP_BUY) price = Ask;
   lastTicket = OrderSend(Symbol(), OP_SELL, lotSize, price, 
      slippagePP, 0, 0, "MT4 stress test", magicNum, 0, Red);
   if (lastTicket > 0)   
      lastCandleSinceOpen = Bars;
}
//+------------------------------------------------------------------+  
void ClosePosition()  
{
   if (lastTicket < 0) return;   
   double price = Ask;
   if (side == OP_BUY) price = Bid;
   OrderClose(lastTicket, lotSize, price, slippagePP, White);

}
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
{
   if (lastCandleSinceOpen < 0) lastCandleSinceOpen = Bars;
   else
   {
      if (Bars - lastCandleSinceOpen > candlesBetweenOpen)
         OpenPosition();
      if (Bars - lastCandleSinceOpen > candlesBetweenClose)
         ClosePosition();
   }
   
   return(0);
}
//+------------------------------------------------------------------+