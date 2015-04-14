net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\TradeSharp.UpdateServer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer *.dll
robocopy ..\TradeSharp.UpdateServer\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer *.exe


net use R: /delete
pause
