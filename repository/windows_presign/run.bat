@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem This is used to sign Windows /deploy directory when manually updated, for commit.
rem If not performed, anyway package build scripts will sign files.

set VARSCRIPTDIR=%~dp0
SET /p VARSIGNPASSWORD= < "%VARSCRIPTDIR%\..\signing\eddie.win-signing.pfx.pwd"
IF exist %VARSCRIPTDIR%\..\signing\eddie.win-signing.pfx (
	for /D %%d in (%VARSCRIPTDIR%\..\..\deploy\windows*) do (
		echo "Scan deploy windows dir %%~fd";
		for %%f in (%%~d\*.*) do (
			echo Check signature %%~ff 
			%VARSCRIPTDIR%\..\windows_common\signtool.exe verify /pa "%%~ff"
			if ERRORLEVEL 1 (
				echo Need sign
				%VARSCRIPTDIR%\..\windows_common\signtool.exe sign /fd sha256 /p "%VARSIGNPASSWORD%" /f "%VARSCRIPTDIR%\..\signing\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - OpenVPN UI" "%%~ff" || goto :error								
			) ELSE (
				rem Already signed
			)
		)
	)
	
)