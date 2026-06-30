@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem This is used to sign Windows /deploy directory when manually updated, for commit.
rem If not performed, anyway package build scripts will sign files.

set VARSCRIPTDIR=%~dp0
IF exist "%EDDIESIGNINGDIR%\eddie.win-signing.pfx" (
	SET /p VARSIGNPASSWORD= < "%EDDIESIGNINGDIR%\eddie.win-signing.pfx.pwd"
	for /D %%d in (%VARSCRIPTDIR%\..\..\deploy\windows*) do (
		echo "Scan deploy windows dir %%~fd";
		for %%f in (%%~d\*.exe %%~d\*.dll %%~d\*.sys %%~d\*.cat) do (
			echo Check signature %%~ff 
			%VARSCRIPTDIR%\..\windows_common\signtool.exe verify /pa "%%~ff"
			if ERRORLEVEL 1 (
				echo Signing %%~ff
				%VARSCRIPTDIR%\..\windows_common\signtool.exe sign /fd sha256 /p "!VARSIGNPASSWORD!" /f "%EDDIESIGNINGDIR%\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - VPN Tunnel" "%%~ff" || goto :error								
			) ELSE (
				rem Already signed
			)
		)
	)
	
)

goto :done

:error
echo presign failed with error #%errorlevel%.
exit /b 1

:done
exit /b 0
