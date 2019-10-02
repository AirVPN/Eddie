@echo off
SETLOCAL

if "%~1"=="" (
	echo "File missing"
	goto :error
)

set VARSCRIPTDIR=%~dp0

IF exist "%VARSCRIPTDIR%/../signing/eddie.website_deploy.ppk" (
	%VARSCRIPTDIR%\pscp.exe -P 46333 -i "%VARSCRIPTDIR%/../signing/eddie.website_deploy.ppk" "%~1"  deploy@eddie.website:/home/www/repository/eddie/internal
)

:done
exit /b 0

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%