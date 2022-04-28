@echo off
SETLOCAL

if "%~1"=="" (
	echo "Project missing"
	goto :error
)

if "%~2"=="" (
	echo "Arch missing"
	goto :error
)

set VARSCRIPTDIR=%~dp0

set VARPROJECT=%1
set VARARCH=%2
set VARCONFIG=Release

rem set VARMSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
set VARMSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe"
set VARTARGETFRAMEWORK="v4.8"
set VARARCHCOMPILE=%VARARCH%
set VARRULESETPATH="%VARSCRIPTDIR%\..\..\tools\ruleset\norules.ruleset"

echo Compilation

IF "%VARPROJECT%"=="cli" (
	set VARSOLUTIONPATH="%VARSCRIPTDIR%\..\..\src\eddie.windows.cli.sln"
) ELSE IF "%VARPROJECT%"=="ui" (
	set VARSOLUTIONPATH="%VARSCRIPTDIR%\..\..\src\eddie2.windows.ui.sln"
) ELSE IF "%VARPROJECT%"=="ui3" (
	set VARSOLUTIONPATH="%VARSCRIPTDIR%\..\..\src\eddie3.windows.ui.sln"
)

%VARMSBUILD% /verbosity:minimal /property:CodeAnalysisRuleSet=%VARRULESETPATH% /p:Configuration=%VARCONFIG% /p:Platform=%VARARCHCOMPILE% /p:TargetFrameworkVersion=%VARTARGETFRAMEWORK% /t:Rebuild %VARSOLUTIONPATH% /p:DefineConstants="EDDIENET4" || goto :error

rem Dont need, VS already launch postbuild.bat event (under Linux / macOS , xbuild/msbuild dont do the same)
rem IF "%VARPROJECT%"=="cli" (
rem 	CALL %VARSCRIPTDIR%\..\..\src\eddie.windows.postbuild.bat %VARSCRIPTDIR%\..\..\src\App.CLI.Windows\bin\%VARARCHCOMPILE%\%VARCONFIG%/ %VARPROJECT% %VARARCH% %VARCONFIG% || goto :error
rem ) ELSE IF "%VARPROJECT%"=="ui" (
rem 	CALL %VARSCRIPTDIR%\..\..\src\eddie.windows.postbuild.bat %VARSCRIPTDIR%\..\..\src\App.Forms.Windows\bin\%VARARCHCOMPILE%\%VARCONFIG%/ %VARPROJECT% %VARARCH% %VARCONFIG% || goto :error
rem ) ELSE IF "%VARPROJECT%"=="ui3" (
rem 	CALL %VARSCRIPTDIR%\..\..\src\eddie.windows.postbuild.bat %VARSCRIPTDIR%\..\..\src\UI.WPF.Windows\bin\%VARARCHCOMPILE%\%VARCONFIG%/ %VARPROJECT% %VARARCH% %VARCONFIG% || goto :error
rem )

:done
exit /b 0

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%