@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.Server\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server *.dll
robocopy ..\TradeSharp.Server\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server *.exe
net use R: /delete
pause