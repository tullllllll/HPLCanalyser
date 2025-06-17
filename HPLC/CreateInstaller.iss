[Setup]
AppName=HPLC
AppVersion=1.0
DefaultDirName={pf}\HPLC
DefaultGroupName=HPLC
OutputDir=.\Installer
OutputBaseFilename=SundaeX_Installer

[Files]
Source: "bin\Release\net8.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\HPLC"; Filename: "{app}\HPLC.exe"