//+------------------------------------------------------------------+
//|                                                IndexRSIDelta.mq4 |
//|                                                    Andrey Sitaev |
//|                                        http://www.forexinvest.ru |
//+------------------------------------------------------------------+
#property copyright "Andrey Sitaev"
#property link      "http://www.forexinvest.ru"

#property indicator_separate_window
#property indicator_buffers 5

#property indicator_color1 Red
#property indicator_color2 Green
#property indicator_color3 Blue

//---- input parameters
extern int       periodRSI = 14;
extern string    lineGreen = "EUR";
extern string    lineRed = "USD";

int              lineGreenType, lineRedType;
#define          symbolUSD  1
#define          symbolEUR  2 
#define          symbolGBP  3
#define          symbolJPY  4
#define          symbolCHF  5


double bufferIndexGreen[];
double bufferIndexRed[];
double bufferRSIGreen[];
double bufferRSIRed[];
double bufferDelta[];


double GetEURXDelta(int i)
{
    double deltaEurx = 0;    
    double eurusd, eurgbp, eurjpy, eurchf;    
    double eurxClose, eurxOpen;
   
    // EURX
    deltaEurx = 0;
    if (i < iBars("EURUSD", 0) && i < iBars("EURGBP", 0)
        && i < iBars("EURJPY", 0) && i < iBars("EURCHF", 0))
    {
        eurusd = iClose("EURUSD", 0, i);      
        eurgbp = iClose("EURGBP", 0, i);      
        eurjpy = iClose("EURJPY", 0, i);      
        eurchf = iClose("EURCHF", 0, i);      
        eurxClose = eurusd * eurgbp * eurjpy * eurchf;
        if (i < iBars("EURCAD", 0))  eurxClose = eurxClose * iClose("EURCAD", 0, i);
        if (i < iBars("EURAUD", 0))  eurxClose = eurxClose * iClose("EURAUD", 0, i);
        

        eurusd = iOpen("EURUSD", 0, i);      
        eurgbp = iOpen("EURGBP", 0, i);      
        eurjpy = iOpen("EURJPY", 0, i);      
        eurchf = iOpen("EURCHF", 0, i);      
        eurxOpen = eurusd * eurgbp * eurjpy * eurchf;
        if (i < iBars("EURCAD", 0))  eurxOpen = eurxOpen * iOpen("EURCAD", 0, i);
        if (i < iBars("EURAUD", 0))  eurxOpen = eurxOpen * iOpen("EURAUD", 0, i);

        deltaEurx = eurxClose - eurxOpen;
    }
    return (deltaEurx);
}

double GetUSDXDelta(int i)
{
    double deltaUsdx = 0;    
    double gbpusd, usdjpy, usdchf, usdcad, eurusd, 
        usdxOpen, usdxClose;   
      
    // USDX 
    if (i < iBars("EURUSD", 0) && i < iBars("GBPUSD", 0)
        && i < iBars("USDJPY", 0) && i < iBars("USDCHF", 0) && i < iBars("USDCAD", 0))
    {
        gbpusd = iOpen("GBPUSD", 0, i);
        eurusd = iOpen("EURUSD", 0, i);
        usdjpy = iOpen("USDJPY", 0, i);
        usdchf = iOpen("USDCHF", 0, i);
        usdcad = iOpen("USDCAD", 0, i);   
        usdxOpen = usdjpy * usdchf * usdcad / (eurusd * gbpusd);
        if (i < iBars("AUDUSD", 0))   usdxOpen = usdxOpen / iOpen("AUDUSD", 0, i);
   
        
        eurusd = iClose("EURUSD", 0, i);
        gbpusd = iClose("GBPUSD", 0, i);
        usdjpy = iClose("USDJPY", 0, i);
        usdchf = iClose("USDCHF", 0, i);
        usdcad = iClose("USDCAD", 0, i);
        usdxClose = usdjpy * usdchf * usdcad / (eurusd * gbpusd);
        if (i < iBars("AUDUSD", 0))   usdxClose = usdxClose / iClose("AUDUSD", 0, i);
   
        deltaUsdx = usdxClose - usdxOpen;
    }
    return (deltaUsdx);
}

double GetJPYXDelta(int i)
{
    double deltaJpyx = 0;    
    double usdjpy, eurjpy, gbpjpy, chfgpy;
    double jpyxOpen, jpyxClose;
      
    // USDX 
    if (i < iBars("USDJPY", 0) && i < iBars("EURJPY", 0)
        && i < iBars("GBPJPY", 0) && i < iBars("CHFJPY", 0))
    {
        usdjpy = iOpen("USDJPY", 0, i);
        eurjpy = iOpen("EURJPY", 0, i);
        gbpjpy = iOpen("GBPJPY", 0, i);
        chfgpy = iOpen("CHFJPY", 0, i);   
        jpyxOpen = 1 / (usdjpy * eurjpy * gbpjpy * chfgpy);          
        
        usdjpy = iClose("USDJPY", 0, i);
        eurjpy = iClose("EURJPY", 0, i);
        gbpjpy = iClose("GBPJPY", 0, i);
        chfgpy = iClose("CHFJPY", 0, i);
        jpyxClose = 1 / (usdjpy * eurjpy * gbpjpy * chfgpy);
   
        deltaJpyx = jpyxClose - jpyxOpen;
    }
    return (deltaJpyx);
}

double GetGBPXDelta(int i)
{
    double deltaGbpx = 0;    
    double gbpusd, eurgbp, gbpjpy, gbpchf;
    double gbpxOpen, gbpxClose;
      
    // USDX 
    if (i < iBars("GBPUSD", 0) && i < iBars("EURGBP", 0)
         && i < iBars("GBPCHF", 0) && i < iBars("GBPJPY", 0))
    {
        gbpusd = iOpen("GBPUSD", 0, i);
        eurgbp = iOpen("EURGBP", 0, i);
        gbpchf = iOpen("GBPCHF", 0, i);   
        gbpjpy = iOpen("GBPJPY", 0, i);        
        gbpxOpen = gbpusd * gbpchf * gbpjpy / eurgbp;
        
        gbpusd = iClose("GBPUSD", 0, i);
        eurgbp = iClose("EURGBP", 0, i);
        gbpchf = iClose("GBPCHF", 0, i);   
        gbpjpy = iClose("GBPJPY", 0, i);        
        gbpxClose = gbpusd * gbpchf * gbpjpy / eurgbp;
   
        deltaGbpx = gbpxClose - gbpxOpen;
    }
    return (deltaGbpx);
}

double GetCHFXDelta(int i)
{
    double deltaChfx = 0;    
    double usdchf, eurchf, gbpchf, chfjpy;
    double chfxOpen, chfxClose;
      
    // USDX 
    if (i < iBars("USDCHF", 0) && i < iBars("EURCHF", 0)
         && i < iBars("GBPCHF", 0) && i < iBars("CHFJPY", 0))
    {
        usdchf = iOpen("USDCHF", 0, i);
        eurchf = iOpen("EURCHF", 0, i);
        gbpchf = iOpen("GBPCHF", 0, i);   
        chfjpy = iOpen("CHFJPY", 0, i);        
        chfxOpen = chfjpy / (usdchf * eurchf * gbpchf);
        
        usdchf = iClose("USDCHF", 0, i);
        eurchf = iClose("EURCHF", 0, i);
        gbpchf = iClose("GBPCHF", 0, i);   
        chfjpy = iClose("CHFJPY", 0, i);        
        chfxClose = chfjpy / (usdchf * eurchf * gbpchf);
   
        deltaChfx = chfxClose - chfxOpen;
    }
    return (deltaChfx);
}


int GetSymbolCode(string symb)
{
    if (symb == "USD") return (symbolUSD);
    if (symb == "EUR") return (symbolEUR);
    if (symb == "GBP") return (symbolGBP);
    if (symb == "JPY") return (symbolJPY);
    if (symb == "CHF") return (symbolCHF);
    return (0);
}
//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
{   
    lineGreenType = GetSymbolCode(lineGreen);
    lineRedType = GetSymbolCode(lineRed);    

    string short_name;      
    SetIndexStyle(0, DRAW_LINE);
    SetIndexBuffer(0, bufferRSIRed);
    SetIndexStyle(1, DRAW_LINE);   
    SetIndexBuffer(1, bufferRSIGreen);
    SetIndexStyle(2, DRAW_HISTOGRAM);
    SetIndexBuffer(2, bufferDelta);
    // вспомогательные буферы - сами индексы
    SetIndexStyle(3, DRAW_NONE);
    SetIndexBuffer(3, bufferIndexRed);
    SetIndexStyle(4, DRAW_NONE);
    SetIndexBuffer(4, bufferIndexGreen);

    short_name = StringConcatenate("Index delta RSI: ", lineGreen, "(green), ", lineRed, "(red)");    

    IndicatorShortName(short_name);
    SetIndexLabel(0, StringConcatenate("RSI ", lineRed));   
    SetIndexLabel(1, StringConcatenate("RSI ", lineGreen));
    SetIndexLabel(2, "delta (blue)");   

    // учет "запаздывания" RSI
    SetIndexDrawBegin(0, periodRSI);
    SetIndexDrawBegin(1, periodRSI);
    SetIndexDrawBegin(2, periodRSI);
  
    return(0);
}
//+------------------------------------------------------------------+
//| Custom indicator deinitialization function                       |
//+------------------------------------------------------------------+
int deinit()
{
   return(0);
}
//+------------------------------------------------------------------+
//| Custom indicator iteration function                              |
//+------------------------------------------------------------------+
int start()
{
   int    i, j, limit, counted_bars = IndicatorCounted();      
   double greenU, greenD, redU, redD;
   double deltaGreen, deltaRed;
   double x, rsiRed, rsiGreen;
   
   limit = Bars - counted_bars;
      
   // получить все дельты EURX и USDX         
   for (i = 0; i < limit; i++)
   {
      if (lineGreenType == symbolUSD)  deltaGreen = GetUSDXDelta(i);
      if (lineGreenType == symbolEUR)  deltaGreen = GetEURXDelta(i);
      if (lineGreenType == symbolGBP)  deltaGreen = GetGBPXDelta(i);
      if (lineGreenType == symbolJPY)  deltaGreen = GetJPYXDelta(i);
      if (lineGreenType == symbolCHF)  deltaGreen = GetCHFXDelta(i);
      
      if (lineRedType == symbolUSD)  deltaRed = GetUSDXDelta(i);
      if (lineRedType == symbolEUR)  deltaRed = GetEURXDelta(i);
      if (lineRedType == symbolGBP)  deltaRed = GetGBPXDelta(i);
      if (lineRedType == symbolJPY)  deltaRed = GetJPYXDelta(i);
      if (lineRedType == symbolCHF)  deltaRed = GetCHFXDelta(i);
                  
      bufferIndexRed[i] = deltaRed;
      bufferIndexGreen[i] = deltaGreen;
   }   
   
   // посчитать RSI   
   limit = Bars - counted_bars;
   if (periodRSI >= Bars) return (0);
   
   for (i = 0; i < limit; i++)
   {
      greenU = 0; greenD = 0; 
      redU = 0; redD = 0;
      for (j = 0; j < periodRSI; j++)
      {
         x = bufferIndexGreen[i + j];
         if (x < 0) greenD -= x;
         else greenU += x;
         
         x = bufferIndexRed[i + j];
         if (x < 0) redD -= x;
         else redU += x;
      }
         
      if (redD == 0 && redU == 0) rsiRed = 50;
      else      
         rsiRed = 100 * redU / (redD + redU);
      if (greenD == 0 && greenU == 0) rsiGreen = 50;
      else
         rsiGreen = 100 * greenU / (greenD + greenU);
      
      // дельта
      bufferRSIGreen[i] = rsiGreen;
      bufferRSIRed[i] = rsiRed;
      bufferDelta[i] = rsiGreen - rsiRed;
   }
   
   return (0);
}
//+------------------------------------------------------------------+