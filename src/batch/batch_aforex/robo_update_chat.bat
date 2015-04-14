@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.100\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\..\TradeSharp.Chat\TradeSharp.Chat.Service\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\Chat *.dll
robocopy ..\..\TradeSharp.Chat\TradeSharp.Chat.Service\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\Chat *.exe
net use R: /delete
pause