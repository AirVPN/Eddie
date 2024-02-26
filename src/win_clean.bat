@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

set VARSCRIPTDIR=%~dp0

rem This exists because mixing already-builded net4.8 and net7 cause issues, so we launch this between switch.

echo Clean - Start

rem -----------------

RMDIR "!VARSCRIPTDIR!\Lib.Core\bin" /S /Q 2>nul
RMDIR "!VARSCRIPTDIR!\Lib.Core\obj" /S /Q 2>nul

RMDIR "!VARSCRIPTDIR!\Lib.Platform.Windows\bin" /S /Q 2>nul
RMDIR "!VARSCRIPTDIR!\Lib.Platform.Windows\obj" /S /Q 2>nul

RMDIR "!VARSCRIPTDIR!\App.CLI.Windows\bin" /S /Q 2>nul
RMDIR "!VARSCRIPTDIR!\App.CLI.Windows\obj" /S /Q 2>nul

rem -----------------

RMDIR "!VARSCRIPTDIR!\Lib.Forms\bin" /S /Q 2>nul
RMDIR "!VARSCRIPTDIR!\Lib.Forms\obj" /S /Q 2>nul

RMDIR "!VARSCRIPTDIR!\Lib.Forms.Skin\bin" /S /Q 2>nul
RMDIR "!VARSCRIPTDIR!\Lib.Forms.Skin\obj" /S /Q 2>nul

RMDIR "!VARSCRIPTDIR!\App.Forms.Windows\bin" /S /Q 2>nul
RMDIR "!VARSCRIPTDIR!\App.Forms.Windows\obj" /S /Q 2>nul

rem -----------------

GOTO done

:error
echo Clean - Something wrong
EXIT /B 1

:done
echo Clean - Done
EXIT /B 0





