@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

set VARSCRIPTDIR=%~dp0
call "%VARSCRIPTDIR%\..\windows_common\locate_msbuild.bat" || exit /b 1

for %%i in ("%VARMSBUILD%") do set "VARMSBUILDDIR=%%~dpi"
set "VARCSC=%VARMSBUILDDIR%Roslyn\csc.exe"

if not exist "%VARCSC%" (
	echo csc.exe not found
	exit /b 1
)

"%VARCSC%" "%VARSCRIPTDIR%\nsis-helper.cs" /out:"%VARSCRIPTDIR%\nsis-helper.exe" || exit /b 1

exit /b 0