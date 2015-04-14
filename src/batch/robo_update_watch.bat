@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.WatchService\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Watch *.dll
robocopy ..\TradeSharp.WatchService\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Watch *.exe
net use R: /delete
pause