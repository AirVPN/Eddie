@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem This is used to sign Windows /deploy directory when manually updated, for commit.
rem If not performed, anyway package build scripts will sign files.

set VARSCRIPTDIR=%~dp0
for /D %%d in (%VARSCRIPTDIR%\..\..\deploy\windows*) do (
	echo "Scan deploy windows dir %%~fd";
	call "%VARSCRIPTDIR%\sign-authenticode.bat" "%%~fd" || goto :error
)

goto :done

:error
echo presign failed with error #%errorlevel%.
exit /b 1

:done
exit /b 0
