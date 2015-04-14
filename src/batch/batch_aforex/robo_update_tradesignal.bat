@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.100\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\..\TradeSharp.TradeSignal\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\TradeSignal *.dll
robocopy ..\..\TradeSharp.TradeSignal\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\TradeSignal *.exe
net use R: /delete
pause