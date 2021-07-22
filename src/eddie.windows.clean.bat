@echo off

rem This exists because mixing already-builded net4.5 and net5 cause issues, so we launch this between switch.

RMDIR App.CLI.Windows\bin /S /Q
RMDIR App.CLI.Windows\obj /S /Q

RMDIR App.CLI.Windows.WireGuard\bin /S /Q
RMDIR App.CLI.Windows.WireGuard\obj /S /Q

RMDIR App.Forms.Windows\bin /S /Q
RMDIR App.Forms.Windows\obj /S /Q

RMDIR Lib.Core\bin /S /Q
RMDIR Lib.Core\obj /S /Q

RMDIR Lib.Forms\bin /S /Q
RMDIR Lib.Forms\obj /S /Q

RMDIR Lib.Platform.Windows\bin /S /Q
RMDIR Lib.Platform.Windows\obj /S /Q

RMDIR Checking\bin /S /Q
RMDIR Checking\obj /S /Q

GOTO done

:error
echo Something wrong
EXIT /B 1

:done
EXIT /B 0





