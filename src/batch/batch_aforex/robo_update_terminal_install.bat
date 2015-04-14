@rem ----MTS.Live.Client ----
net use T: /delete
net use T: \\10.5.237.100\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal Candlechart.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal Entity.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal FastChart.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal FastGrid.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal FastMultiChart.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Contract.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Contract.Util.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Robot.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.TradeLib.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Util.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.UI.Util.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.Subscription.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.SiteBridge.Lib.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.QuoteHistory.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Chat.Client.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Chat.Contract.dll
robocopy ..\..\TradeSharp.Client\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.exe

robocopy ..\..\TradeSharp.Localization.AForex\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Localization.dll

robocopy ..\..\Help \\10.5.237.100\c$\Services\TradeSharp\UpdateServer\terminal terminal.chm


net use T: /delete
pause
