@rem ----MTS.Live.Client ----
net use T: /delete
net use T: \\10.5.237.10\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal Candlechart.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal Entity.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal FastChart.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal FastGrid.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal FastMultiChart.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Contract.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Contract.Util.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Robot.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.TradeLib.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Util.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.UI.Util.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.Util.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.Subscription.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.SiteBridge.Lib.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.QuoteHistory.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Chat.Client.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Chat.Contract.dll
robocopy ..\TradeSharp.Client.Tooltip\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.Tooltip.dll
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.exe


net use T: /delete
pause
