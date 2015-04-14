net use R: /delete
net use R: \\10.5.237.100\c$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\..\TradeSharp.QuoteService\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\Quote *.dll
robocopy ..\..\TradeSharp.QuoteService\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\Quote *.exe

net use R: /delete
pause
