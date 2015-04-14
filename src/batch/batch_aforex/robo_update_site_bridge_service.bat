@rem ---- SiteBridgeService ----
net use R: /delete
net use R: \\10.5.237.100\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\..\SiteBridge\TradeSharp.SiteBridge.Service\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\SiteBridge *.dll
robocopy ..\..\SiteBridge\TradeSharp.SiteBridge.Service\bin\Debug \\10.5.237.100\c$\Services\TradeSharp\SiteBridge *.exe
net use R: /delete
pause