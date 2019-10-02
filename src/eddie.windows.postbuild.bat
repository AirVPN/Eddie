@echo off

@if "%~1"=="" goto error
@if "%~2"=="" goto error
@if "%~3"=="" goto error
@if "%~4"=="" goto error

SET basepath=%~dp0
SET targetdir=%1
SET project=%2
SET arch=%3
SET config=%4

rem Remove quote
set targetdir=%targetdir:"=%

echo BasePath: %basepath%
echo TargetDir: %targetdir%
echo Project: %project%
echo Arch: %arch%
echo Config: %config%

copy "%basepath%\..\deploy\windows_%arch%\Lib.Platform.Windows.Native.dll" "%targetdir%" /Y /V
copy "%basepath%\Lib.Platform.Windows.Elevated\bin\%arch%\%config%\Lib.Platform.Windows.Elevated.dll" "%targetdir%\Lib.Platform.Windows.Elevated.dll" /Y /V
copy "%basepath%\App.CLI.Windows.Elevated\bin\%arch%\%config%\App.CLI.Windows.Elevated.exe" "%targetdir%\Eddie-CLI-Elevated.exe" /Y /V
copy "%basepath%\App.Service.Windows.Elevated\bin\%arch%\%config%\App.Service.Windows.Elevated.exe" "%targetdir%\Eddie-Service-Elevated.exe" /Y /V

goto done

:error
echo Wrong arguments

:done





