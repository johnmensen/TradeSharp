net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\Report\TradeSharp.Reports.IndexGrabber\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\ReportIndexGrabber *.dll
robocopy ..\Report\TradeSharp.Reports.IndexGrabber\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\ReportIndexGrabber *.exe


net use R: /delete
pause
