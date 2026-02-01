[Setup]
AppName=XScreensaver for Windows
AppVersion=2026.1.0
DefaultDirName={sys}
DisableDirPage=yes
PrivilegesRequired=admin
OutputBaseFilename=XScreensaver-setup
Compression=lzma
SolidCompression=yes
Uninstallable=yes

[Files]
Source: "Screensaver.exe"; DestDir: "{sys}"; DestName: "XScreensaver.scr"; Flags: ignoreversion
Source: "Models.dll"; DestDir: "{sys}"; Flags: ignoreversion
Source: "Screensaver.dll"; DestDir: "{sys}"; Flags: ignoreversion
Source: "Screensaver.runtimeconfig.json"; DestDir: "{sys}"; Flags: ignoreversion


; [Icons]
; Name: "{group}\Mi App"; Filename: "{app}\MiApp.exe"
; Name: "{commondesktop}\Mi App"; Filename: "{app}\MiApp.exe"

; [Run]
; Filename: "{app}\MiApp.exe"; Description: "Ejecutar Mi App"; Flags: nowait postinstall skipifsilent