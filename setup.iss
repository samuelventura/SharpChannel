; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Channel Manager"
#define MyAppVersion "1.0.4"
#define MyAppPublisher "Samuel Ventura"
#define MyAppURL "https://github.com/samuelventura/SharpChannel"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{FAE6FBFF-B512-4DA7-B7EA-9A2A7C453DEB}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppPublisher}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=ChannelManager.Setup-{#MyAppVersion}
SetupIconFile=favicon.ico
UninstallDisplayIcon={uninstallexe}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "SharpChannel.Manager.Service\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "SharpChannel.Manager.WebUI\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName} {#MyAppVersion}"; Filename: "http://127.0.0.1:2017"; IconFilename: "{app}\Content\favicon.ico"
Name: "{commondesktop}\{#MyAppName} {#MyAppVersion}"; Filename: "http://127.0.0.1:2017"; IconFilename: "{app}\Content\favicon.ico"

[Run]
Filename: "{app}\PostInstall.bat";

[UninstallRun]
Filename: "{app}\PreUninstall.bat";


