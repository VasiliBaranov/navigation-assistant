#ifndef AppVersion
    #define AppVersion "1.0.0"
    #define Revision "0"
    #define SourceDir "..\NavigationAssistant\bin\Debug"
#endif

#define ProductName "Navigation Assistant"
#define AppSupportURL "http://code.google.com/p/navigation-assistant/"

[Setup]
AppPublisher="Vasili Baranov"
AppCopyright="Copyright © 2011 Vasili Baranov"
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
LicenseFile={#SourceDir}\License.txt

; AppVerName and AppVersion are shown in Programs and Features in Control Panel
AppVersion={#AppVersion}.{#Revision}
AppVerName={#ProductName}

DefaultGroupName={#ProductName}
OutputBaseFilename={#ProductName} {#AppVersion}.{#Revision}
UninstallDisplayIcon={app}\NavigationAssistant.exe
VersionInfoVersion={#AppVersion}.{#Revision}

; This will be the {app} variable
DefaultDirName={pf}\{#ProductName}

[CustomMessages]
ApplicationAlreadyInstalled=Navigation Assistant seems to have already been installed on your computer. Would you like to continue?
RunExecutableDescription=Run Navigation Assistant
DotNetMissing=Navigation Assistant requires Microsoft .Net Framework 3.5, which is not yet installed. Would you like to download it now?

[Registry]
Root: HKLM; Subkey: "Software\{#ProductName}"; Flags: uninsdeletekey

[Files]
Source: "{#SourceDir}\Interop.SHDocVw.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\NavigationAssistant.Core.dll"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\NavigationAssistant.exe"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\License.txt"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#SourceDir}\NLog.config"; DestDir: "{app}"; Flags: ignoreversion;

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Icons]
Name: "{userprograms}\{#ProductName}\{#ProductName}"; Comment: "Run Navigation Assistant"; Filename: "{app}\NavigationAssistant.exe"; IconFileName: {app}\NavigationAssistant.exe;
Name: "{userprograms}\{#ProductName}\Uninstall"; Comment: "Uninstall Navigation Assistant"; Filename: "{app}\unins000.exe"; IconFileName: {app}\unins000.exe;

[Run]
; .Net 2.0 will definetely be installed, as we require .Net 3.5 and check for it on the installer startup.
Filename: "{dotnet20}\ngen.exe"; Parameters: "install ""{app}\NavigationAssistant.exe"" /nologo /silent"; StatusMsg: "Optimizing performance..."; Flags: runascurrentuser runhidden;
Filename: "{app}\NavigationAssistant.exe"; Parameters: "/install"; StatusMsg: "Performing startup actions"; Flags: runascurrentuser runhidden;
; Add a checkbox to the success window for running the Navigation Assistant
Filename: "{app}\NavigationAssistant.exe"; Description: "{cm:RunExecutableDescription}";  Flags: postinstall nowait;

[UninstallRun]
Filename: "{dotnet20}\ngen.exe"; Parameters: "uninstall ""{app}\NavigationAssistant.exe"" /nologo /silent"; StatusMsg: "Removing natively compiled binaries..."; Flags: runascurrentuser runhidden;
Filename: "{app}\NavigationAssistant.exe"; Parameters: "/uninstall"; StatusMsg: "Removing cache and settings..."; Flags: runascurrentuser runhidden;

[Code]
const dotNet35Url = 'http://www.microsoft.com/download/en/details.aspx?DisplayLang=en&id=21';

function ProductNotInstalled() : Boolean;
begin
    result := (not RegKeyExists(HKLM, 'Software\{#ProductName}'));

    //Show a warning that this app has already been installed.
    if not result then
    begin
        result := (MsgBox(CustomMessage('ApplicationAlreadyInstalled'), mbConfirmation, MB_YESNO) = IDYES);
    end;
end;

function DotNetFrameworkInstalled() : Boolean;
var
    errorCode: Integer;
    netFrameWorkInstalled : Boolean;
    isInstalled: Cardinal;
begin
    result := true;

    // Check for the .Net 3.5 framework
    isInstalled := 0;
    netFrameworkInstalled := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5', 'Install', isInstalled);
    netFrameworkInstalled := (netFrameworkInstalled and (isInstalled = 1));

    if (not netFrameworkInstalled) then
    begin
        if (MsgBox(CustomMessage('DotNetMissing'), mbError, MB_YESNO) = idYes) then
        begin
            ShellExec('open', dotNet35Url, '', '', SW_SHOWNORMAL, ewNoWait, errorCode);
        end;
        result := false;
    end;
end;

function InitializeSetup(): Boolean;
begin
    // If the first function returns false, doesn't execute a second one
    result := (DotNetFrameworkInstalled() and ProductNotInstalled());
end;