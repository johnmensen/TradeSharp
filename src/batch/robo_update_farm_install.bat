@rem ----MTS.Live.Client ----
net use T: /delete
net use T: \\10.5.237.10\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal Candlechart.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal Entity.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal FastChart.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal FastGrid.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal ICSharpCode.SharpZipLib.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.Contract.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.Client.Util.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.Contract.Util.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.QuoteHistory.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.Robot.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.TradeLib.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.Util.dll
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.RobotFarm.exe
robocopy ..\TradeSharp.RobotFarm\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateFarmServer\terminal TradeSharp.RobotFarm.config

net use T: /delete
pause
