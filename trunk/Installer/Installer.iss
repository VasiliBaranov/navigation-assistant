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
AppId="23F9BDAD-95C4-4A6B-AC83-C864CEF28696"
AppName={#ProductName}
AppMutex="44F16610-EF84-47B6-8536-33C0D754F41E"
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

[Messages]
FinishedLabel=Setup has finished installing [name] on your computer. The application will be launched automatically and will be registered for launching on Windows startup.

[CustomMessages]
ApplicationAlreadyInstalled=Navigation Assistant seems to be already installed on your computer. Would you like to continue?

[Registry]
Root: HKLM; Subkey: "Software\{#ProductName}"; Flags: uninsdeletekey

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

[Code]

function InitializeSetup(): Boolean;
begin

    Result := (not RegKeyExists(HKLM, 'Software\{#ProductName}'));

    //Show a warning that this app has already been installed.
    if not Result then
    begin
        Result := (MsgBox(CustomMessage('ApplicationAlreadyInstalled'), mbError, MB_YESNO) = IDYES);
    end;
end;