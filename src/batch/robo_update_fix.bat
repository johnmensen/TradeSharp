net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\TradeSharp.ProviderProxy\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\FixProxy *.dll
robocopy ..\TradeSharp.ProviderProxy\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\FixProxy *.exe

net use R: /delete
pause
