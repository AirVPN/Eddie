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

if "%~3"=="" (
	echo "OS missing"
	goto :error
)

if "%~4"=="" (
	echo "Framework missing"
	goto :error
)

set VARPROJECT=%1
set VARARCH=%2
set VAROS=%3
set VARFRAMEWORK=%4
set VARCONFIG=Release

set VARSCRIPTDIR=%~dp0
FOR /F "tokens=*" %%g IN ('%VARSCRIPTDIR%\..\windows_common\get-version.exe') do (SET "VARVERSION=%%g")
set VARTARGETDIR=%TEMP%\eddie_deploy\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer
set VARFINALPATH=%TEMP%\eddie_deploy\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer.exe
set VARDEPLOYPATH=%VARSCRIPTDIR%\..\files\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer.exe

echo Step: Dependencies
CALL %VARSCRIPTDIR%\..\windows_portable\build.bat %VARPROJECT% %VARARCH% %VAROS% %VARFRAMEWORK% || goto :error

echo Step: Cleanup
IF EXIST "%VARTARGETDIR%" (
	rmdir /s /q "%VARTARGETDIR%" 
)

echo Build Windows Installer, Project: %VARPROJECT%, OS: %VAROS%, Arch: %VARARCH%, Framework: %VARFRAMEWORK%

IF exist %VARDEPLOYPATH% (
	echo "Already builded: %VARDEPLOYPATH%"
	exit /b 0;
) 

%VARSCRIPTDIR%\..\windows_common\7za.exe -y x "%VARSCRIPTDIR%\..\files\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable.zip" -o"%VARTARGETDIR%" || goto :error
echo move /y "%VARTARGETDIR%\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable\*" "%VARTARGETDIR%" || goto :error
xcopy /s /e /i /Y "%VARTARGETDIR%\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable\*" "%VARTARGETDIR%\*" || goto :error
rmdir /s /q "%VARTARGETDIR%\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable" || goto :error

rem Build NSIS script
"%VARSCRIPTDIR%\nsis.exe" "%VARARCH%" "%VARTARGETDIR%" "%VARFINALPATH%" || goto :error

rem NSIS
echo "Shell NSIS"
"c:\\Program Files (x86)\\NSIS\\makensis.exe" "%VARTARGETDIR%\Eddie.nsi" || goto :error

rem Signing

SET /p VARSIGNPASSWORD= < "%VARSCRIPTDIR%\..\signing\eddie.win-signing.pfx.pwd"
IF exist %VARSCRIPTDIR%\..\signing\eddie.win-signing.pfx (
	echo Step: Signing		
	%VARSCRIPTDIR%\..\windows_common\signtool.exe sign /fd sha256 /p "%VARSIGNPASSWORD%" /f "%VARSCRIPTDIR%\..\signing\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - VPN Tunnel" "%VARFINALPATH%" || goto :error
)

rem Deploy
CALL %VARSCRIPTDIR%\..\windows_common\deploy.bat "%VARFINALPATH%" || goto :error

rem End
move "%VARFINALPATH%" "%VARDEPLOYPATH%"

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