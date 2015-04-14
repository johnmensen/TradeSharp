//+------------------------------------------------------------------+
//|                                                      EURUSDx.mq4 |
//|                      Copyright © 2009, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#property copyright "Copyright © 2009, MetaQuotes Software Corp."
#property link      "http://www.metaquotes.net"

#property indicator_chart_window
#property indicator_buffers 1
#property indicator_color1 Red
//---- input parameters
extern double    scale = 1.0;
//---- buffers
double bufferEURUSDX[];


double GetEURUSDx(int i)
{    
    // коэффициенты USDX
    double kUeur = -0.601, kUjpy = 0.142, kUgbp = -0.124, kUcad = 0.095, kUchf = 0.038, kUsek = 0;      
    // коэффициенты EURX   
    double kEusd = 0.342377, kEgbp = 0.331633, kEjpy = 0.205209, kEchf = 0.120781, kEsek = 0;   

    double eurusd, eurgbp, eurjpy, eurchf;
    double gbpusd, usdjpy, usdchf, usdcad;
    double x, usdxOriginal, eurx, usdx;
      
    // EURX
    eurusd = iClose("EURUSD", 0, i);
    eurgbp = iClose("EURGBP", 0, i);
    eurjpy = iClose("EURJPY", 0, i);
    eurchf = iClose("EURCHF", 0, i);     
    eurx = MathPow(eurusd, kEusd) *
        MathPow(eurgbp, kEgbp) * MathPow(eurjpy, kEjpy) * MathPow(eurchf, kEchf);     
     
    // USDX
    gbpusd = iClose("GBPUSD", 0, i);
    usdjpy = iClose("USDJPY", 0, i);
    usdchf = iClose("USDCHF", 0, i);
    usdcad = iClose("USDCAD", 0, i);
    usdx = MathPow(eurusd, kUeur) *
        MathPow(gbpusd, kUgbp) * MathPow(usdjpy, kUjpy) *
        MathPow(usdchf, kUchf) * MathPow(usdcad, kUcad);
    
    Print("usdx=", usdx, " eurx=", eurx);
        
    if (usdx == 0) return (1);
    if (scale == 0) return (eurx / usdx);
    return (scale * eurx / usdx);
}

//+------------------------------------------------------------------+
//| Custom indicator initialization function                         |
//+------------------------------------------------------------------+
int init()
{
    SetIndexStyle(0, DRAW_LINE);
    SetIndexBuffer(0, bufferEURUSDX);
    return (0);
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
    int    limit = Bars - IndicatorCounted();
    for (int i = 0; i < limit; i++)
    {
        bufferEURUSDX[i] = GetEURUSDx(i);
    }
    
    return (0);
}
//+------------------------------------------------------------------+