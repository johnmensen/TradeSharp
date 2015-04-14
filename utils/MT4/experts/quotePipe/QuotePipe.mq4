//+------------------------------------------------------------------+
//|                                                    QuotePipe.mq4 |
//|                      Copyright © 2009, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2009, MetaQuotes Software Corp."
#property link      "http://www.metaquotes.net"

#import "QuotePipe.dll"  
void SendToPort(string ip, int port, string pair, double bid, double ask, double volume);

extern string address = "127.0.0.1";
extern int port = 8600;

//+------------------------------------------------------------------+
//| expert initialization function                                   |
//+------------------------------------------------------------------+
int init()
  {
  
//----
   
//----
   return(0);
  }
//+------------------------------------------------------------------+
//| expert deinitialization function                                 |
//+------------------------------------------------------------------+
int deinit()
  {
//----
   
//----
   return(0);
  }
//+------------------------------------------------------------------+
//| expert start function                                            |
//+------------------------------------------------------------------+
int start()
  {
//----
   SendToPort(address, port, Symbol(), Bid, Ask, Volume[0]);
//----
   return(0);
  }
//+------------------------------------------------------------------+