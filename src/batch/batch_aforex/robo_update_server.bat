@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.100\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\..\TradeSharp.Server\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\Server *.dll
robocopy ..\..\TradeSharp.Server\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\Server *.exe
net use R: /delete
pause