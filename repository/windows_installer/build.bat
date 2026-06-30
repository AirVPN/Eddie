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
	echo "OS missing"
	goto :error
)

if "%~4"=="" (
	echo "Line missing"
	goto :error
)

set VARPROJECT=%1
set VARARCH=%2
set VAROS=%3
set VARLINE=%4
set VARCONFIG=Release

if not "%VARLINE%"=="l" if not "%VARLINE%"=="u" (
	echo "Line must be l or u"
	goto :error
)

set VARPACKAGEPROJECT=%VARPROJECT%
if "%VARLINE%"=="u" set VARPACKAGEPROJECT=%VARPROJECT%-u

set VARSCRIPTDIR=%~dp0
FOR /F "tokens=*" %%g IN ('%VARSCRIPTDIR%\..\windows_common\get-version.exe') do (SET "VARVERSION=%%g")
set VARTARGETDIR=%TEMP%\eddie_deploy\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer
set VARFINALPATH=%TEMP%\eddie_deploy\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer.exe
set VARDEPLOYPATH=%VARSCRIPTDIR%\..\files\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer.exe

echo Step: Dependencies
CALL %VARSCRIPTDIR%\..\windows_portable\build.bat %VARPROJECT% %VARARCH% %VAROS% %VARLINE% || goto :error

echo Step: Cleanup
IF EXIST "%VARTARGETDIR%" (
	rmdir /s /q "%VARTARGETDIR%" 
)

echo Build Windows Installer, Project: %VARPROJECT%, OS: %VAROS%, Arch: %VARARCH%, Line: %VARLINE%

IF exist %VARDEPLOYPATH% (
	echo "Already builded: %VARDEPLOYPATH%"
	exit /b 0;
) 

%VARSCRIPTDIR%\..\windows_common\7za.exe -y x "%VARSCRIPTDIR%\..\files\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable.zip" -o"%VARTARGETDIR%" || goto :error
del "%VARTARGETDIR%\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable\portable.txt"
xcopy /s /e /i /Y "%VARTARGETDIR%\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable\*" "%VARTARGETDIR%\*" || goto :error
rmdir /s /q "%VARTARGETDIR%\eddie-%VARPACKAGEPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable" || goto :error

rem Build NSIS script
"%VARSCRIPTDIR%\nsis-helper.exe" "%VARARCH%" "%VARTARGETDIR%" "%VARFINALPATH%" "%VARLINE%" || goto :error

rem NSIS
echo "Shell NSIS"
"c:\\Program Files (x86)\\NSIS\\makensis.exe" "%VARTARGETDIR%\Eddie.nsi" || goto :error

rem Staff Deploy
if defined EDDIESIGNINGDIR (
	echo Step: Signing		
	SET /p VARSIGNPASSWORD= < "%EDDIESIGNINGDIR%\eddie.win-signing.pfx.pwd"
	%VARSCRIPTDIR%\..\windows_common\signtool.exe sign /fd sha256 /p "!VARSIGNPASSWORD!" /f "%EDDIESIGNINGDIR%\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - VPN Tunnel" "%VARFINALPATH%" || goto :error

	CALL %VARSCRIPTDIR%\..\windows_common\deploy.bat "%VARFINALPATH%" || goto :error
	call %VARSCRIPTDIR%\..\windows_common\sign-openpgp.bat "%VARFINALPATH%" || goto :error
	if exist "%VARFINALPATH%.asc" call %VARSCRIPTDIR%\..\windows_common\deploy.bat "%VARFINALPATH%.asc" || goto :error
)

rem End
move "%VARFINALPATH%" "%VARDEPLOYPATH%"
if exist "%VARFINALPATH%.asc" move "%VARFINALPATH%.asc" "%VARDEPLOYPATH%.asc"

rem Cleanup
echo Step: Final cleanup
rmdir /s /q %VARTARGETDIR%

:done
echo Done
exit /b 0

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%