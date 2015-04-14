//+------------------------------------------------------------------+
//|                                                    USD_EUR_X.mq4 |
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
#property indicator_color4 Yellow
#property indicator_color5 Magenta

//---- input parameters
extern bool      showUSD = true;
extern bool      showEUR = true;
extern bool      showJPY  = false;
extern bool      showGBP = false;
extern bool      showCHF = false;

extern bool      useSEK  = false;

extern double    mUSD = 50.14348112;
extern double    mEUR = 34.38805726;
extern double    mJPY = 100.0;
extern double    mGBP = 0.65;
extern double    mCHF = 1.05;

extern double    deltaUSD = 0;
extern double    deltaEUR = 0;
extern double    deltaJPY = 0;
extern double    deltaGBP = 0;
extern double    deltaCHF = 0;



double bufferEUR[];
double bufferUSD[];
double bufferJPY[];
double bufferGBP[];
double bufferCHF[];


double    originalKUSD = 50.14348112;
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
   SetIndexStyle(2, DRAW_LINE);
   SetIndexBuffer(2, bufferJPY);
   SetIndexStyle(3, DRAW_LINE);
   SetIndexBuffer(3, bufferGBP);
   SetIndexStyle(4, DRAW_LINE);
   SetIndexBuffer(4, bufferCHF);
   
   short_name = "";   
   if (showUSD == true) short_name = short_name + " USDX (red)";   
   if (showEUR == true) short_name = short_name + " EURX (green)";
   if (showJPY == true) short_name = short_name + " JPYX (blue)";
   
   if (showGBP == true) short_name = short_name + " GBPX (yellow)";
   if (showCHF == true) short_name = short_name + " CHFX (magenta)";
   
   IndicatorShortName(short_name);
   SetIndexLabel(0, "USDX (red)");   
   SetIndexLabel(1, "EURX (green)");
   SetIndexLabel(2, "JPYX (blue)");
   SetIndexLabel(3, "GBPX (yellow)");
   SetIndexLabel(4, "CHFX (magenta)");
      
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
   int    i, counted_bars = IndicatorCounted();
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
   double x, usdxOriginal;
   
   
   i = Bars - 1;
   while (i >= 0)
   {
      // EURX
      eurusd = iClose("EURUSD", 0, i);
      if (showEUR)
      {
         eurgbp = iClose("EURGBP", 0, i);
         eurjpy = iClose("EURJPY", 0, i);
         eurchf = iClose("EURCHF", 0, i);     
         x = mEUR * MathPow(eurusd, kEusd) *
            MathPow(eurgbp, kEgbp) * MathPow(eurjpy, kEjpy) *
            MathPow(eurchf, kEchf); 
         if (useSEK) x = x * MathPow(iClose("EURSEK", 0, i), kEsek);
         bufferEUR[i] = x + deltaEUR;
      }
         
      // USDX
      if (showUSD || showJPY || showGBP || showCHF)
      {
         gbpusd = iClose("GBPUSD", 0, i);
         usdjpy = iClose("USDJPY", 0, i);
         usdchf = iClose("USDCHF", 0, i);
         usdcad = iClose("USDCAD", 0, i);
         x = MathPow(eurusd, kUeur) *
            MathPow(gbpusd, kUgbp) * MathPow(usdjpy, kUjpy) *
            MathPow(usdchf, kUchf) * MathPow(usdcad, kUcad);
         if (useSEK) x = x * MathPow(iClose("USDSEK", 0, i), kUsek);                  
         usdxOriginal = originalKUSD * x;
         x = x * mUSD + deltaUSD;         
      }
      if (showUSD) bufferUSD[i] = x;
      
      if (showJPY)
      {
         if (usdjpy == 0) usdjpy = iClose("USDJPY", 0, i);
         if (usdjpy > 0)
            bufferJPY[i] = mJPY * usdxOriginal / usdjpy + deltaJPY;
      }
      if (showGBP)
      {
         if (gbpusd == 0) gbpusd = iClose("GBPUSD", 0, i);
         if (gbpusd > 0)
            bufferGBP[i] = mGBP * usdxOriginal * gbpusd + deltaGBP;
      }
      if (showCHF)
      {
         if (usdchf == 0) usdchf = iClose("USDCHF", 0, i);
         if (usdchf > 0)
            bufferCHF[i] = mCHF * usdxOriginal / usdchf + deltaCHF;
      }
         
      // next step
      i--;
   }
   
   return(0);
}
//+------------------------------------------------------------------+