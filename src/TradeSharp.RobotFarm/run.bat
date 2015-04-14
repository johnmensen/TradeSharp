for /F "tokens=3 delims=: " %%H in ('sc query "TradeSharp.RobotFarm" ^| findstr "        STATE"') do (
  if /I "%%H" NEQ "RUNNING" (
   net start "TradeSharp.RobotFarm"
 )   
)