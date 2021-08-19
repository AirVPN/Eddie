@echo off

if not exist "files" mkdir "files"

rem temp call windows_portable\build.bat cli x64 windows || goto :error
rem temp call windows_portable\build.bat cli x64 windows-7 || goto :error
rem temp call windows_portable\build.bat cli x64 windows-vista || goto :error
rem temp call windows_portable\build.bat cli x64 windows-xp || goto :error
rem temp call windows_portable\build.bat cli x86 windows || goto :error
rem temp call windows_portable\build.bat cli x86 windows-7 || goto :error
rem temp call windows_portable\build.bat cli x86 windows-vista || goto :error
rem temp call windows_portable\build.bat cli x86 windows-xp || goto :error
call windows_portable\build.net5.bat ui x64 windows || goto :error

goto :done

call windows_portable\build.bat ui x64 windows-7 || goto :error
call windows_portable\build.bat ui x64 windows-vista || goto :error
call windows_portable\build.bat ui x64 windows-xp || goto :error
call windows_portable\build.bat ui x86 windows || goto :error
call windows_portable\build.bat ui x86 windows-7 || goto :error
call windows_portable\build.bat ui x86 windows-vista || goto :error
call windows_portable\build.bat ui x86 windows-xp || goto :error

rem call windows_installer\build.bat cli x64 windows || goto :error
rem call windows_installer\build.bat cli x64 windows-7 || goto :error
rem call windows_installer\build.bat cli x64 windows-vista || goto :error
rem call windows_installer\build.bat cli x64 windows-xp || goto :error
rem call windows_installer\build.bat cli x86 windows || goto :error
rem call windows_installer\build.bat cli x86 windows-7 || goto :error
rem call windows_installer\build.bat cli x86 windows-vista || goto :error
rem call windows_installer\build.bat cli x86 windows-xp || goto :error
call windows_installer\build.bat ui x64 windows || goto :error
call windows_installer\build.bat ui x64 windows-7 || goto :error
call windows_installer\build.bat ui x64 windows-vista || goto :error
call windows_installer\build.bat ui x64 windows-xp || goto :error
call windows_installer\build.bat ui x86 windows || goto :error
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

