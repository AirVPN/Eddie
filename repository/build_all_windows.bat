@echo off

if not exist "files" mkdir "files"

call windows_common\presign.bat

call windows_portable\build.bat cli x64 windows l || goto :error
rem [deprecated in 2.26.0] call windows_portable\build.bat cli x64 windows-7 l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat cli x64 windows-vista l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat cli x64 windows-xp l || goto :error
call windows_portable\build.bat cli x86 windows l || goto :error
rem [deprecated in 2.26.0] call windows_portable\build.bat cli x86 windows-7 l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat cli x86 windows-vista l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat cli x86 windows-xp l || goto :error
call windows_portable\build.bat ui x64 windows l || goto :error
rem [deprecated in 2.26.0] call windows_portable\build.bat ui x64 windows-7 l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat ui x64 windows-vista l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat ui x64 windows-xp l || goto :error
call windows_portable\build.bat ui x86 windows l || goto :error
rem [deprecated in 2.26.0] call windows_portable\build.bat ui x86 windows-7 l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat ui x86 windows-vista l || goto :error
rem [deprecated in 2.24.0] call windows_portable\build.bat ui x86 windows-xp l || goto :error

rem call windows_installer\build.bat cli x64 windows l || goto :error
rem call windows_installer\build.bat cli x64 windows-7 l || goto :error
rem [deprecated in 2.24.0] rem call windows_installer\build.bat cli x64 windows-vista l || goto :error
rem [deprecated in 2.24.0] rem call windows_installer\build.bat cli x64 windows-xp l || goto :error
rem call windows_installer\build.bat cli x86 windows l || goto :error
rem call windows_installer\build.bat cli x86 windows-7 l || goto :error
rem [deprecated in 2.24.0] rem call windows_installer\build.bat cli x86 windows-vista l || goto :error
rem [deprecated in 2.24.0] rem call windows_installer\build.bat cli x86 windows-xp l || goto :error
call windows_installer\build.bat ui x64 windows l || goto :error
rem [deprecated in 2.26.0] call windows_installer\build.bat ui x64 windows-7 l || goto :error
rem [deprecated in 2.24.0] call windows_installer\build.bat ui x64 windows-vista l || goto :error
rem [deprecated in 2.24.0] call windows_installer\build.bat ui x64 windows-xp l || goto :error
call windows_installer\build.bat ui x86 windows l || goto :error
rem [deprecated in 2.26.0] call windows_installer\build.bat ui x86 windows-7 l || goto :error
rem [deprecated in 2.24.0] call windows_installer\build.bat ui x86 windows-vista l || goto :error
rem [deprecated in 2.24.0] call windows_installer\build.bat ui x86 windows-xp l || goto :error

rem WIP
rem call windows_portable\build.bat ui x64 windows u || goto :error
rem call windows_portable\build.bat ui x86 windows u || goto :error
rem call windows_installer\build.bat ui x64 windows u || goto :error
rem call windows_installer\build.bat ui x86 windows u || goto :error

:done
echo Done
pause
exit /b

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%

