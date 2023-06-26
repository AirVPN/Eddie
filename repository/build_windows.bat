@echo off

if not exist "files" mkdir "files"

call windows_common\presign.bat

call windows_portable\build.bat cli x64 windows net4 || goto :error
call windows_portable\build.bat cli x64 windows-7 net4 || goto :error
call windows_portable\build.bat cli x64 windows-vista net4 || goto :error
call windows_portable\build.bat cli x64 windows-xp net4 || goto :error
call windows_portable\build.bat cli x86 windows net4 || goto :error
call windows_portable\build.bat cli x86 windows-7 net4 || goto :error
call windows_portable\build.bat cli x86 windows-vista net4 || goto :error
call windows_portable\build.bat cli x86 windows-xp net4 || goto :error
call windows_portable\build.bat ui x64 windows net4 || goto :error
call windows_portable\build.bat ui x64 windows-7 net4 || goto :error
call windows_portable\build.bat ui x64 windows-vista net4 || goto :error
call windows_portable\build.bat ui x64 windows-xp net4 || goto :error
call windows_portable\build.bat ui x86 windows net4 || goto :error
call windows_portable\build.bat ui x86 windows-7 net4 || goto :error
call windows_portable\build.bat ui x86 windows-vista net4 || goto :error
call windows_portable\build.bat ui x86 windows-xp net4 || goto :error

rem call windows_installer\build.bat cli x64 windows net4 || goto :error
rem call windows_installer\build.bat cli x64 windows-7 net4 || goto :error
rem call windows_installer\build.bat cli x64 windows-vista net4 || goto :error
rem call windows_installer\build.bat cli x64 windows-xp net4 || goto :error
rem call windows_installer\build.bat cli x86 windows net4 || goto :error
rem call windows_installer\build.bat cli x86 windows-7 net4 || goto :error
rem call windows_installer\build.bat cli x86 windows-vista net4 || goto :error
rem call windows_installer\build.bat cli x86 windows-xp net4 || goto :error
call windows_installer\build.bat ui x64 windows net4 || goto :error
call windows_installer\build.bat ui x64 windows-7 net4 || goto :error
call windows_installer\build.bat ui x64 windows-vista net4 || goto :error
call windows_installer\build.bat ui x64 windows-xp net4 || goto :error
call windows_installer\build.bat ui x86 windows net4 || goto :error
call windows_installer\build.bat ui x86 windows-7 net4 || goto :error
call windows_installer\build.bat ui x86 windows-vista net4 || goto :error
call windows_installer\build.bat ui x86 windows-xp net4 || goto :error

:done
echo Done
pause
exit /b

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%

