@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\Processing\TradeSharp.Processing.WebMoney\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\ProcessingWebMoney *.dll
robocopy ..\Processing\TradeSharp.Processing.WebMoney\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\ProcessingWebMoney *.exe
net use R: /delete
pause


