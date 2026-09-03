@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
rem Authenticode signature for Windows artifacts. Accepts a file or a directory.
rem With a directory, signs *.exe *.dll *.sys *.cat that are not already validly signed.
rem Signing identity is the certificate whose SHA-1 thumbprint is in
rem %EDDIESIGNINGDIR%\eddie.win-signing.thumbprint, which must be available in the user store.
rem No-op if EDDIESIGNINGDIR or the thumbprint file are not found.

if "%~1"=="" exit /b 0
if not defined EDDIESIGNINGDIR exit /b 0
if not exist "%EDDIESIGNINGDIR%\eddie.win-signing.thumbprint" exit /b 0

set /p VARTHUMBPRINT=<"%EDDIESIGNINGDIR%\eddie.win-signing.thumbprint"
if not defined VARTHUMBPRINT exit /b 0

set VARSIGNTOOL=%~dp0signtool.exe
set VARTIMESTAMP=http://time.certum.pl
set VARDESCRIPTION=Eddie - VPN Tunnel

if exist "%~1\" (
	for %%f in ("%~1\*.exe" "%~1\*.dll" "%~1\*.sys" "%~1\*.cat") do (
		call :sign "%%~ff" || exit /b 1
	)
) else (
	call :sign "%~f1" || exit /b 1
)

exit /b 0

:sign
"%VARSIGNTOOL%" verify /pa "%~1" >nul 2>&1
if not errorlevel 1 exit /b 0
echo Signing %~1
"%VARSIGNTOOL%" sign /fd sha256 /sha1 %VARTHUMBPRINT% /tr %VARTIMESTAMP% /td sha256 /d "%VARDESCRIPTION%" "%~1"
if errorlevel 1 (
	echo Signing failed. Certificate %VARTHUMBPRINT% must be present in the user store and the signing session must be active.
	exit /b 1
)
exit /b 0
