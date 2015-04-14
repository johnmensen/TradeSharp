net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Server ----
robocopy ..\TradeSharp.Server\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server *.dll
robocopy ..\TradeSharp.Server\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server *.exe


@rem ---- Dealers ---
robocopy ..\Dealer\DemoDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers DemoDealer.dll
robocopy ..\Dealer\FixDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers FixDealer.dll
robocopy ..\Dealer\SignalDealer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\Server\dealers SignalDealer.dll
robocopy ..\Libraries \\10.5.237.10\d$\TradeSharp\Server\dealers BSEngine.SignalDelivery.Contract.dll


@rem ---- Quote ----
robocopy ..\TradeSharp.QuoteServer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\QuoteService *.dll
robocopy ..\TradeSharp.QuoteServer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\QuoteService *.exe


@rem ---- News ----
robocopy ..\News\TradeSharp.NewsGrabber\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\News *.dll
robocopy ..\News\TradeSharp.NewsGrabber\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\News *.exe


@rem ---- Reports ----
robocopy ..\Reports\TradeSharp.Reports.IndexGrabber\bin\Debug \\10.5.237.10\d$\Services\ReportIndexGrabber *.dll
robocopy ..\Reports\TradeSharp.Reports.IndexGrabber\bin\Debug \\10.5.237.10\d$\Services\ReportIndexGrabber *.exe

@rem ---- SiteBridgeService ----
robocopy ..\SiteBridge\TradeSharp.SiteBridge.Service\\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\SiteBridge *.dll
robocopy ..\SiteBridge\TradeSharp.SiteBridge.Service\\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\SiteBridge *.exe

net use R: /delete

pause
