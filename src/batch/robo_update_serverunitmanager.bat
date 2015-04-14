net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\ServerUnitManager.Service\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\ServerUnitManager *.dll
robocopy ..\ServerUnitManager.Service\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\ServerUnitManager *.exe

net use R: /delete
pause
