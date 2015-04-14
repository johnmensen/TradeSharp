//+------------------------------------------------------------------+
//|                                                 USD_EUR_diff.mq4 |
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
extern bool      useSEK    = false;
extern int       periodRSI = 14;

double    mUSD = 50.14348112;
double    mEUR = 34.38805726;


double bufferEURX[];
double bufferUSDX[];
double bufferEUR[];
double bufferUSD[];
double bufferDelta[];

void GetUSDXDelta(int i, double &deltaUsdx, double &deltaEurx)
{
   int err;
   // коэффициенты USDX
   double kUeur = -0.601, kUjpy = 0.142, kUgbp = -0.124, kUcad = 0.095, kUchf = 0.038, kUsek = 0;      
   // коэффициенты EURX   
   double kEusd = 0.342377, kEgbp = 0.331633, kEjpy = 0.205209, kEchf = 0.120781, kEsek = 0;   
   
   if (useSEK)
   {
      kUeur = -0.576; kUjpy = 0.136; kUgbp = -0.119; kUcad = 0.091; kUchf = 0.036; kUsek = 0.0042;
      kEusd = 0.3155; kEgbp = 0.3056; kEjpy = 0.1891; kEchf = 0.1113; kEsek = 0.0785;
   }
      
   double eurusd, eurgbp, eurjpy, eurchf;
   double gbpusd, usdjpy, usdchf, usdcad;
   double eurxClose, eurxOpen, usdxClose, usdxOpen;
   
   // EURX
   deltaEurx = 0;
   if (i < iBars("EURUSD", 0) && i < iBars("EURGBP", 0)
    && i < iBars("EURJPY", 0) && i < iBars("EURCHF", 0))
   {
        eurusd = iClose("EURUSD", 0, i);      
        eurgbp = iClose("EURGBP", 0, i);      
        eurjpy = iClose("EURJPY", 0, i);      
        eurchf = iClose("EURCHF", 0, i);      
   
        eurxClose = mEUR * MathPow(eurusd, kEusd) *
           MathPow(eurgbp, kEgbp) * MathPow(eurjpy, kEjpy) *
           MathPow(eurchf, kEchf); 
        if (useSEK) eurxClose = eurxClose * MathPow(iClose("EURSEK", 0, i), kEsek);      
   
        eurusd = iOpen("EURUSD", 0, i);      
        eurgbp = iOpen("EURGBP", 0, i);      
        eurjpy = iOpen("EURJPY", 0, i);      
        eurchf = iOpen("EURCHF", 0, i);      
        eurxOpen = mEUR * MathPow(eurusd, kEusd) *
           MathPow(eurgbp, kEgbp) * MathPow(eurjpy, kEjpy) *
           MathPow(eurchf, kEchf); 
        if (useSEK) eurxOpen = eurxOpen * MathPow(iOpen("EURSEK", 0, iBars("EURSEK", 0) - i - 1), kEsek);      
        deltaEurx = eurxClose - eurxOpen;
   }
      
   // USDX 
   if (i < iBars("EURUSD", 0) && i < iBars("GBPUSD", 0)
    && i < iBars("USDJPY", 0) && i < iBars("USDCHF", 0) && i < iBars("USDCAD", 0))
    {
        gbpusd = iOpen("GBPUSD", 0, i);
        usdjpy = iOpen("USDJPY", 0, i);
        usdchf = iOpen("USDCHF", 0, i);
        usdcad = iOpen("USDCAD", 0, i);
   
        usdxOpen = MathPow(eurusd, kUeur) *
           MathPow(gbpusd, kUgbp) * MathPow(usdjpy, kUjpy) *
           MathPow(usdchf, kUchf) * MathPow(usdcad, kUcad);
        if (useSEK) usdxOpen = usdxOpen * MathPow(iOpen("USDSEK", 0, i), kUsek);
        usdxOpen = usdxOpen * mUSD;
   
        
        eurusd = iClose("EURUSD", 0, i);
        gbpusd = iClose("GBPUSD", 0, i);
        usdjpy = iClose("USDJPY", 0, i);
        usdchf = iClose("USDCHF", 0, i);
        usdcad = iClose("USDCAD", 0, i);
        usdxClose = MathPow(eurusd, kUeur) *
           MathPow(gbpusd, kUgbp) * MathPow(usdjpy, kUjpy) *
           MathPow(usdchf, kUchf) * MathPow(usdcad, kUcad);
        if (useSEK) usdxClose = usdxClose * MathPow(iClose("USDSEK", 0, i), kUsek);
        usdxClose = usdxClose * mUSD;
   
        deltaUsdx = usdxClose - usdxOpen;
   }
}

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
{   
   string short_name;      
   SetIndexStyle(0, DRAW_LINE);
   SetIndexBuffer(0, bufferUSD);
   SetIndexStyle(1, DRAW_LINE);   
   SetIndexBuffer(1, bufferEUR);
   SetIndexStyle(2, DRAW_HISTOGRAM);
   SetIndexBuffer(2, bufferDelta);
   // вспомогательные буферы - сами индексы
   SetIndexStyle(3, DRAW_NONE);
   SetIndexBuffer(3, bufferUSDX);
   SetIndexStyle(4, DRAW_NONE);
   SetIndexBuffer(4, bufferEURX);
   
   short_name = "RSI {USD(red) EUR(green)}";
   
   IndicatorShortName(short_name);
   SetIndexLabel(0, "RSI USDX (red)");   
   SetIndexLabel(1, "RSI EURX (green)");
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
   double eurU, eurD, usdU, usdD;
   double deltaEur, deltaUsd;
   double x, rsiUsd, rsiEur;
   
   limit = Bars - counted_bars;
      
   // получить все дельты EURX и USDX         
   for (i = 0; i < limit; i++)
   {   
      GetUSDXDelta(i, deltaUsd, deltaEur);
      bufferUSDX[i] = deltaUsd;
      bufferEURX[i] = deltaEur;
   }   
   
   // посчитать RSI   
   limit = Bars - counted_bars;
   if (periodRSI >= Bars) return (0);
   
   for (i = 0; i < limit; i++)
   {
      eurU = 0; eurD = 0; 
      usdU = 0; usdD = 0;
      for (j = 0; j < periodRSI; j++)
      {
         x = bufferEURX[i + j];
         if (x < 0) eurD -= x;
         else eurU += x;
         
         x = bufferUSDX[i + j];
         if (x < 0) usdD -= x;
         else usdU += x;
      }
         
      if (usdD == 0 && usdU == 0) rsiUsd = 50;
      else      
         rsiUsd = 100 * usdU / (usdD + usdU);
      if (eurD == 0 && eurU == 0) rsiEur = 50;
      else
         rsiEur = 100 * eurU / (eurD + eurU);
      
      // дельта
      bufferEUR[i] = rsiEur;
      bufferUSD[i] = rsiUsd;
      bufferDelta[i] = rsiEur - rsiUsd;      
   }
   
   return (0);
}
//+------------------------------------------------------------------+