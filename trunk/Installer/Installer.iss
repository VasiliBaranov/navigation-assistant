#ifndef AppVersion
    #define AppVersion "1.0.0"
    #define Revision "0"
    #define SourceDir "..\NavigationAssistant\bin\Release"
#endif

#define ProductName "Navigation Assistant"
#define AppSupportURL "http://code.google.com/p/navigation-assistant/"

[Setup]
AppPublisher="Vasili Baranov"
; AppCopyright="Copyright © 2011 Vasili Baranov"
AppPublisherURL={#AppSupportURL}
AppSupportURL={#AppSupportURL}
AppUpdatesURL={#AppSupportURL}
Compression=lzma
DisableProgramGroupPage=yes
OutputDir={#SourceDir}
SolidCompression=yes
ShowLanguageDialog=no
WizardImageStretch=no
AppId={#ProductName}
AppName={#ProductName}
WizardImageFile=WizardImage.bmp
WizardSmallImageFile=WizardSmallImage.bmp

; AppVerName and AppVersion are shown in Programs and Features in Control Panel
AppVersion={#AppVersion}.{#Revision}
AppVerName={#ProductName}

DefaultGroupName={#ProductName}
OutputBaseFilename={#ProductName} {#AppVersion}
UninstallDisplayIcon={app}\NavigationAssistant.exe
VersionInfoVersion={#AppVersion}.{#Revision}

; This will be the {app} variable
DefaultDirName={pf}\{#ProductName}

[Files]
Source: "{#SourceDir}\Interop.SHDocVw.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\NavigationAssistant.Core.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\NavigationAssistant.exe"; DestDir: "{app}"; Flags: ignoreversion;

[UninstallDelete]
; Removing product directory if it is empty
Type: dirifempty; Name: "{app}"

[Icons]
Name: "{userprograms}\{#ProductName}\{#ProductName}"; Comment: "Run Navigation Assistant"; Filename: "{app}\NavigationAssistant.exe"; IconFileName: {app}\NavigationAssistant.exe;
Name: "{userprograms}\{#ProductName}\Uninstall"; Comment: "Uninstall Navigation Assistant"; Filename: "{app}\unins000.exe"; IconFileName: {app}\unins000.exe;

[Run]
Filename: "{app}\NavigationAssistant.exe"; Parameters: "/install"; StatusMsg: "Parsing file system (this may take several minutes)..."; Flags: runascurrentuser runhidden;

; We run the application at once (no postinstall flag) to prevent changing filesystem between parsing the file system and running the app manually.
Filename: "{app}\NavigationAssistant.exe"; Parameters: "/startup"; Flags: nowait;

[UninstallRun]
Filename: "{app}\NavigationAssistant.exe"; Parameters: "/uninstall"; StatusMsg: "Removing cache and settings..."; Flags: runascurrentuser runhidden;