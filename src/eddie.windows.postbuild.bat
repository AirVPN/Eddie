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

rem Compile and copy Elevated - CLI Edition
call %basepath%\App.CLI.Windows.Elevated\build.bat %config% %arch% || goto :error
copy %basepath%\App.CLI.Windows.Elevated\bin\%arch%\%config%\App.CLI.Windows.Elevated.exe "%targetdir%"\Eddie-CLI-Elevated.exe /Y /V || goto :error

rem Compile and copy Elevated - Service Edition
call %basepath%\App.Service.Windows.Elevated\build.bat %config% %arch% || goto :error
copy %basepath%\App.Service.Windows.Elevated\bin\%arch%\%config%\App.Service.Windows.Elevated.exe "%targetdir%"\Eddie-Service-Elevated.exe /Y /V || goto :error

rem Compile and copy native library

rem Workaround, issue in building debug
rem call %basepath%\Lib.Platform.Windows.Native\build.bat %config% %arch% || goto :error
rem copy %basepath%\Lib.Platform.Windows.Native\bin\%arch%\%config%\Lib.Platform.Windows.Native.dll "%targetdir%" /Y /V || goto :error
call %basepath%\Lib.Platform.Windows.Native\build.bat Release %arch% || goto :error
copy %basepath%\Lib.Platform.Windows.Native\bin\%arch%\Release\Lib.Platform.Windows.Native.dll "%targetdir%" /Y /V || goto :error

goto done

:error
echo Something wrong
exit 1

:done
exit 0





