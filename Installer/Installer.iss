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

[CustomMessages]
ExecutableDescription=Run Navigation Assistant

[Icons]
Name: "{userprograms}\{#ProductName}\{#ProductName}"; Comment: "{cm:ExecutableDescription}"; Filename: "{app}\NavigationAssistant.exe"; IconFileName: {app}\NavigationAssistant.exe;
; [Code]
