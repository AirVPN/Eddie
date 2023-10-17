@echo off

echo This exists because mixing already-builded net4.8 and net7 cause issues, so we launch this between switch.

RMDIR App.CLI.Windows\bin /S /Q 2>nul
RMDIR App.CLI.Windows\obj /S /Q 2>nul

RMDIR App.Forms.Windows\bin /S /Q 2>nul
RMDIR App.Forms.Windows\obj /S /Q 2>nul

RMDIR Lib.Core\bin /S /Q 2>nul
RMDIR Lib.Core\obj /S /Q 2>nul

RMDIR Lib.Forms\bin /S /Q 2>nul
RMDIR Lib.Forms\obj /S /Q 2>nul

RMDIR Lib.Forms.Skin\bin /S /Q 2>nul
RMDIR Lib.Forms.Skin\obj /S /Q 2>nul

RMDIR Lib.Platform.Windows\bin /S /Q 2>nul
RMDIR Lib.Platform.Windows\obj /S /Q 2>nul

RMDIR Checking\bin /S /Q 2>nul
RMDIR Checking\obj /S /Q 2>nul

GOTO done

:error
echo Something wrong
EXIT /B 1

:done
EXIT /B 0





