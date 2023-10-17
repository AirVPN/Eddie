@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

if "%~1"=="" (
	echo "Project missing"
	goto :error
)

if "%~2"=="" (
	echo "Arch missing"
	goto :error
)

if "%~3"=="" (
	echo "Framework missing"
	goto :error
)

set VARSCRIPTDIR=%~dp0

set VARPROJECT=%1
set VARARCH=%2
set VARFRAMEWORK=%3
set VARCONFIG=Release
set VARARCHCOMPILE=!VARARCH!

echo Compilation

IF "!VARFRAMEWORK!"=="net7" (
	dotnet build "!VARSCRIPTDIR!\..\..\src\App.CLI\App.CLI.net7.csproj" --verbosity normal --runtime win-!VARARCH! --configuration Release
) ELSE IF "!VARFRAMEWORK!"=="net4" (

	set VARMSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe"
	set VARTARGETFRAMEWORK="v4.8"
	
	set VARRULESETPATH="!VARSCRIPTDIR!\..\..\src\ruleset\norules.ruleset"

	IF "!VARPROJECT!"=="cli" (
		set VARSOLUTIONPATH="!VARSCRIPTDIR!\..\..\src\eddie.windows.cli.sln"
	) ELSE IF "!VARPROJECT!"=="ui" (
		set VARSOLUTIONPATH="!VARSCRIPTDIR!\..\..\src\eddie2.windows.ui.sln"
	)

	!VARMSBUILD! /verbosity:minimal /property:CodeAnalysisRuleSet=!VARRULESETPATH! /p:Configuration=!VARCONFIG! /p:Platform=!VARARCHCOMPILE! /p:TargetFrameworkVersion=!VARTARGETFRAMEWORK! /t:Rebuild !VARSOLUTIONPATH! /p:DefineConstants="EDDIENET4" || goto :error	
)

:done
exit /b 0

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%