@echo off

if not exist "files" mkdir "files"

call windows_common\presign.bat

rem call windows_portable\build.bat ui x64 windows || goto :error
rem call windows_installer\build.bat ui x64 windows || goto :error
call windows_installer\build.bat ui3 x64 windows || goto :error

rem call windows_portable\build.bat ui x86 windows || goto :error
rem call windows_installer\build.bat ui x86 windows || goto :error


rem call windows_portable\build.bat ui x86 windows-7 || goto :error
rem call windows_installer\build.bat ui x86 windows-7 || goto :error

rem call windows_portable\build.bat ui x64 windows-7 || goto :error
rem call windows_installer\build.bat ui x64 windows-7 || goto :error

:done
echo Done
pause
exit /b

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%

