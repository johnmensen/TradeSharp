@rem ----MTS.Live.Client ----
net use T: /delete
net use T: \\10.5.237.10\c$ /USER:forexinvest\asitaev AndSit!qa
robocopy ..\TradeSharp.Client\bin\Debug \\10.5.237.10\d$\Services\TradeSharp\UpdateServer\terminal TradeSharp.Client.exe.config


net use T: /delete
pause
