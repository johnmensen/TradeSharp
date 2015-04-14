net use R: /delete
net use R: \\10.5.237.10\d$ /USER:forexinvest\asitaev AndSit!qa

@rem ---- Quote ----
robocopy ..\News\TradeSharp.NewsGrabber\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\News *.dll
robocopy ..\News\TradeSharp.NewsGrabber\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\News *.exe


net use R: /delete
pause
