@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
rem Detached OpenPGP signature for release artifacts. Creates %1.asc
rem Uses staff/dependencies/gpg-windows/ (portable gpg.exe). No-op if not found or no passphrase.

if "%~1"=="" exit /b 0
if not exist "%~1" exit /b 0

set VARSCRIPTDIR=%~dp0
set VARREPO=!VARSCRIPTDIR!..\..\
set VARGPGDIR=!VARREPO!staff\dependencies\gpg-windows
set VARGPG=!VARGPGDIR!\gpg.exe
if not exist "!VARGPG!" set VARGPG=!VARGPGDIR!\bin\gpg.exe
if not exist "!VARGPG!" exit /b 0
if not exist "%EDDIESIGNINGDIR%\eddie_gpg_2026.passphrase" exit /b 0

set "GNUPGHOME=%EDDIESIGNINGDIR%\gpg-home"
set "PATH=!VARGPGDIR!;!VARGPGDIR!\bin;!PATH!"
set /p PASSPHRASE=<"%EDDIESIGNINGDIR%\eddie_gpg_2026.passphrase"
"!VARGPG!" --batch --yes --pinentry-mode loopback --passphrase "!PASSPHRASE!" --detach-sign --armor -u "apt@eddie.website" -o "%~1.asc" "%~1"
if errorlevel 1 exit /b 1
"!VARGPG!" --verify "%~1.asc" "%~1"
exit /b 0
