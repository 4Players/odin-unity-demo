[Setup]
AppName=ODIN Sample Project
AppVersion=0.10.0
AppVerName=ODIN Sample Project
AppPublisher=4Players GmbH
AppPublisherURL=https://www.4players.io/
ArchitecturesAllowed=x64
WizardStyle=modern
DefaultDirName={commonpf64}\ODIN Sample Project
DefaultGroupName=4Players GmbH
UninstallDisplayIcon={app}\ODIN-SampleProject.exe
Compression=zip
SolidCompression=yes
OutputBaseFilename=ODIN-SampleProject_Setup
OutputDir="dist"

[Files]
Source: "bin/*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\ODIN Sample Project"; Filename: "{app}\ODIN-SampleProject.exe"
