@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.Delivery\TradeSharp.Delivery.Service\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Delivery *.dll
robocopy ..\TradeSharp.Delivery\TradeSharp.Delivery.Service\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Delivery *.exe
net use R: /delete
pause