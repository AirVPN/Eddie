@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

"C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\Roslyn\csc.exe" get-version.cs

exit /b 0