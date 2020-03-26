@echo off
SETLOCAL

if "%~1"=="" (
	echo "First param must be Debug or Release"
	goto :end
)

if "%~2"=="" (
	echo "Second param must be x86 or x64"
	goto :end
)

set VARMSBUILD="c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"

set VARSCRIPTDIR=%~dp0

set VARCONFIG=%~1
set VARARCH=%~2
set VARARCHCOMPILE=%VARARCH%

set VARSOLUTIONPATH="%VARSCRIPTDIR%\Lib.Platform.Windows.Native.sln"

%VARMSBUILD% /verbosity:minimal /property:CodeAnalysisRuleSet=%VARRULESETPATH% /p:Configuration=%VARCONFIG% /p:Platform=%VARARCHCOMPILE% /t:Rebuild %VARSOLUTIONPATH% || goto :error

:end
