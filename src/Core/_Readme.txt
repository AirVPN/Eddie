------------------------------------
Tags in Code
------------------------------------

TOTRANSLATE: Need a message in Messages.cs
TODO: Need an implementation
TOOPTIMIZE : May need optimization
TOCLEAN : Maybe removed
TOFIX: Need a fix
TOCHECK: Need an investigation
TOTEST: Need a detailed test


------------------------------------
Windows development - VS2010
------------------------------------

- warning CS1607: Assembly generation -- Referenced assembly 'mscorlib.dll' targets a different processor
It won't be a problem at runtime because on a 64-bit machine, the GAC stores a 64-bit specific version of mscorlib.dll.

- x86 / x64 / Any CPU
Targeting x86/x64 only really matters on the EXE assembly since that is the one that determines the bitness of the process. 
A DLL (Eddie.Core) assembly doesn't have a choice, the appropriate build setting for them is Any CPU so they can work both ways.