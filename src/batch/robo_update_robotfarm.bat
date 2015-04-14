@rem ---- Server ----
net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\RobotFarm *.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\RobotFarm *.exe
net use R: /delete
pause