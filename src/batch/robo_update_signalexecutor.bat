@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.TradeSignalExecutor\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\SignalExecutor *.dll
robocopy ..\TradeSharp.TradeSignalExecutor\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\SignalExecutor *.exe
net use R: /delete
pause