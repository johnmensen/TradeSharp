; general
Name "Trade#"
OutFile "TradeSharp_Client.exe"
InstallDir $PROGRAMFILES\TradeSharp
InstallDirRegKey HKLM Software\TradeSharp" "Install_Dir"

; pages
Page components
Page directory
Page instfiles
UninstPage uninstConfirm
UninstPage instfiles

Section "Trade#-клиент (необходимые файлы)"
  SectionIn RO
  SetOutPath $INSTDIR
  File D:\TradeSharp\InstallShield\*.*
  File /r D:\TradeSharp\InstallShield\mt4
  File /r D:\TradeSharp\InstallShield\sounds
  WriteRegStr HKLM SOFTWARE\TradeSharp "Install_Dir" "$INSTDIR"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TradeSharp" "DisplayName" "TradeSharp"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TradeSharp" "DisplayIcon" "$INSTDIR\TradeSharp.Client.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TradeSharp" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TradeSharp" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TradeSharp" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
SectionEnd

Section "јрхив котировок (рекомендуемые файлы)"
  SetOutPath $INSTDIR\quotes
  File D:\TradeSharp\InstallShield\quotes\*.*
SectionEnd

Section "ярлыки"
  CreateDirectory "$SMPROGRAMS\TradeSharp"
  CreateShortCut "$SMPROGRAMS\TradeSharp\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\TradeSharp\Trade#.lnk" "$INSTDIR\TradeSharp.UpdateManager.exe" "" "$INSTDIR\TradeSharp.Client.exe" 0
  CreateShortCut "$DESKTOP\Trade#.lnk" "$INSTDIR\TradeSharp.UpdateManager.exe" "" "$INSTDIR\TradeSharp.Client.exe" 0
SectionEnd

Section "Uninstall"
  RMDir /r "$INSTDIR"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TradeSharp"
  DeleteRegKey HKLM SOFTWARE\TradeSharp
  RMDir /r "$SMPROGRAMS\TradeSharp"
  Delete $DESKTOP\Trade#.lnk
SectionEnd
