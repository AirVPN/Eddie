@echo off

if not exist "files" mkdir "files"

rem Test dotnet6
rem call windows_portable\build.bat cli x64 windows net6 || goto :error

call windows_installer\build.bat ui x64 windows net4 || goto :error


:done
echo Done
pause
exit /b

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%

