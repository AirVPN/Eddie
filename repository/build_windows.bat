@echo off

call windows_portable\build.bat cli x64 windows-10 || goto :error
call windows_portable\build.bat cli x64 windows-7 || goto :error
call windows_portable\build.bat cli x64 windows-vista || goto :error
call windows_portable\build.bat cli x64 windows-xp || goto :error
call windows_portable\build.bat cli x86 windows-10 || goto :error
call windows_portable\build.bat cli x86 windows-7 || goto :error
call windows_portable\build.bat cli x86 windows-vista || goto :error
call windows_portable\build.bat cli x86 windows-xp || goto :error
call windows_portable\build.bat ui x64 windows-10 || goto :error
call windows_portable\build.bat ui x64 windows-7 || goto :error
call windows_portable\build.bat ui x64 windows-vista || goto :error
call windows_portable\build.bat ui x64 windows-xp || goto :error
call windows_portable\build.bat ui x86 windows-10 || goto :error
call windows_portable\build.bat ui x86 windows-7 || goto :error
call windows_portable\build.bat ui x86 windows-vista || goto :error
call windows_portable\build.bat ui x86 windows-xp || goto :error

rem call windows_installer\build.bat cli x64 windows-10 || goto :error
rem call windows_installer\build.bat cli x64 windows-7 || goto :error
rem call windows_installer\build.bat cli x64 windows-vista || goto :error
rem call windows_installer\build.bat cli x64 windows-xp || goto :error
rem call windows_installer\build.bat cli x86 windows-10 || goto :error
rem call windows_installer\build.bat cli x86 windows-7 || goto :error
rem call windows_installer\build.bat cli x86 windows-vista || goto :error
rem call windows_installer\build.bat cli x86 windows-xp || goto :error
call windows_installer\build.bat ui x64 windows-10 || goto :error
call windows_installer\build.bat ui x64 windows-7 || goto :error
call windows_installer\build.bat ui x64 windows-vista || goto :error
call windows_installer\build.bat ui x64 windows-xp || goto :error
call windows_installer\build.bat ui x86 windows-10 || goto :error
call windows_installer\build.bat ui x86 windows-7 || goto :error
call windows_installer\build.bat ui x86 windows-vista || goto :error
call windows_installer\build.bat ui x86 windows-xp || goto :error

:done
echo Done
pause
exit /b

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%

