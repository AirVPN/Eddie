@echo off
SETLOCAL

IF "%~1"=="" (
	echo "First param must be Debug or Release"
	GOTO error
)

IF "%~2"=="" (
	echo "Second param must be x86 or x64"
	GOTO error
)

rem set VARMSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe"
set VARMSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe"

set VARSCRIPTDIR=%~dp0

set VARCONFIG=%~1
set VARARCH=%~2
set VARARCHCOMPILE=%VARARCH%

set VARSOLUTIONPATH="%VARSCRIPTDIR%\App.Service.Windows.Elevated.sln"

%VARMSBUILD% /verbosity:minimal /property:CodeAnalysisRuleSet=%VARRULESETPATH% /p:Configuration=%VARCONFIG% /p:Platform=%VARARCHCOMPILE% /t:Rebuild %VARSOLUTIONPATH% || GOTO error

GOTO done

:error
echo Something wrong
EXIT /B 1

:done
EXIT /B 0

