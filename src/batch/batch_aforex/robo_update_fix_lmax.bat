net use R: /delete
net use R: \\10.5.237.100\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\..\TradeSharp.ProviderProxy\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\FixProxyLMAX *.dll
robocopy ..\..\TradeSharp.ProviderProxy\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\FixProxyLMAX *.exe

net use R: /delete
pause
