@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\Dealer\DemoDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.dll
robocopy ..\Dealer\DemoDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.exe

robocopy ..\Dealer\FixDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.dll
robocopy ..\Dealer\FixDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.exe

robocopy ..\Dealer\SignalDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.dll
robocopy ..\Dealer\SignalDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.exe

robocopy ..\Dealer\SiteSignalDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.dll
robocopy ..\Dealer\SiteSignalDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers *.exe
net use R: /delete
pause


