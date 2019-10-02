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

set VARPROJECT=%1
set VARARCH=%2
set VAROS=%3
set VARCONFIG=Release

set VARSCRIPTDIR=%~dp0
FOR /F "tokens=*" %%g IN ('%VARSCRIPTDIR%\..\windows_common\get-version.exe') do (SET "VARVERSION=%%g")
set VARTARGETDIR=%TEMP%\eddie_deploy\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer
set VARDEPLOYPATH=%VARSCRIPTDIR%\..\files\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_installer.exe

rem Dependencies
CALL %VARSCRIPTDIR%\..\windows_portable\build.bat %VARPROJECT% %VARARCH% %VAROS% || goto :error

rem Cleanup
IF EXIST "%VARTARGETDIR%" (
	rmdir /s /q "%VARTARGETDIR%" 
)

rem Build
echo Build Windows Installer, Project: %VARPROJECT%, OS: %VAROS%, Arch: %VARARCH%

IF exist %VARDEPLOYPATH% (
	echo "Already builded: %VARDEPLOYPATH%"
	exit /b 0;
) 

%VARSCRIPTDIR%\..\windows_common\7za.exe -y x "%VARSCRIPTDIR%\..\files\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable.zip" -o"%VARTARGETDIR%" || goto :error
echo move /y "%VARTARGETDIR%\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable\*" "%VARTARGETDIR%" || goto :error
xcopy /s /e /i /Y "%VARTARGETDIR%\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable\*" "%VARTARGETDIR%\*" || goto :error
rmdir /s /q "%VARTARGETDIR%\eddie-%VARPROJECT%_%VARVERSION%_%VAROS%_%VARARCH%_portable" || goto :error

rem Build NSIS script
"%VARSCRIPTDIR%\nsis.exe" "%VARARCH%" "%VARTARGETDIR%" "%VARDEPLOYPATH%" || goto :error

rem NSIS
"c:\\Program Files (x86)\\NSIS\\makensis.exe" "%VARTARGETDIR%\Eddie.nsi" || goto :error

rem Signing

SET /p VARSIGNPASSWORD= < "%VARSCRIPTDIR%\..\signing\eddie.pfx.pwd"
IF exist %VARSCRIPTDIR%\..\signing\eddie.pfx (
		echo Step: Signing		
		%VARSCRIPTDIR%\..\windows_common\signtool.exe sign /fd sha256 /p "%VARSIGNPASSWORD%" /f "%VARSCRIPTDIR%\..\signing\eddie.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - OpenVPN UI" "%VARDEPLOYPATH%" || goto :error
)

rem Deploy
%VARSCRIPTDIR%\..\windows_common\deploy.bat "%VARDEPLOYPATH%" || goto :error

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