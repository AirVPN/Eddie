@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

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
set VARRID="win-!VARARCH!"
set VARCONFIG=Release

set VARSCRIPTDIR=%~dp0
FOR /F "tokens=*" %%g IN ('!VARSCRIPTDIR!\..\windows_common\get-version.exe') do (SET "VARVERSION=%%g")
set VARTARGETDIR=%TEMP%\eddie_deploy\eddie-!VARPROJECT!_!VARVERSION!_!VAROS!_!VARARCH!_portable
set VARFINALPATH=%TEMP%\eddie_deploy\eddie-!VARPROJECT!_!VARVERSION!_!VAROS!_!VARARCH!_portable.zip
set VARDEPLOYPATH=!VARSCRIPTDIR!\..\files\eddie-!VARPROJECT!_!VARVERSION!_!VAROS!_!VARARCH!_portable.zip

echo Build Windows Portable, Project: !VARPROJECT!, OS: !VAROS!, Arch: !VARARCH!, Framework: !VARFRAMEWORK!

IF EXIST "!VARDEPLOYPATH!" (
	echo "Already builded: !VARDEPLOYPATH!"
	exit /b 0;
) 

echo Step: Cleanup

call "!VARSCRIPTDIR!\..\..\src\win_clean.bat"
rmdir "!VARTARGETDIR!" /S /Q 2>nul

echo Step: Compile and Copying

mkdir !VARTARGETDIR!

copy !VARSCRIPTDIR!\portable.txt !VARTARGETDIR!

IF "!VARPROJECT!"=="cli" (
	cd /d "!VARSCRIPTDIR!\..\..\src\App.CLI.Windows\"
	dotnet publish App.CLI.Windows.net8.csproj --configuration Release --runtime !VARRID! --self-contained true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net8.0\!VARRID!\publish\* !VARTARGETDIR! || goto :error		
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net8.0\!VARRID!\Eddie-CLI-Elevated.exe !VARTARGETDIR! || goto :error		
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net8.0\!VARRID!\Eddie-CLI-Elevated-Service.exe !VARTARGETDIR! || goto :error		
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net8.0\!VARRID!\Lib.Platform.Windows.Native.dll !VARTARGETDIR! || goto :error				

	rem Resources
	echo Step: Resources
	copy !VARSCRIPTDIR!\..\..\deploy\!VAROS!_!VARARCH!\* !VARTARGETDIR! || goto :error	
	robocopy !VARSCRIPTDIR!\..\..\resources !VARTARGETDIR!\Resources /E
	IF "%VARVERSION:~0,1%"=="2" (		
		rmdir "!VARTARGETDIR!\Resources\webui" /S /Q 2>nul
	)
	
) 

IF "!VARPROJECT!"=="ui" (
	echo Step: Dependencies
	CALL %VARSCRIPTDIR%\..\windows_portable\build.bat cli %VARARCH% %VAROS% %VARFRAMEWORK% || goto :error
	%VARSCRIPTDIR%\..\windows_common\7za.exe -y x "%VARSCRIPTDIR%\..\files\eddie-cli_%VARVERSION%_%VAROS%_%VARARCH%_portable.zip" -o"%VARTARGETDIR%" || goto :error
	robocopy "%VARTARGETDIR%\eddie-cli_%VARVERSION%_%VAROS%_%VARARCH%_portable" "%VARTARGETDIR%" *.* /E /MOVE

	IF "!VARFRAMEWORK!"=="net8" (
		echo Build UI App
		call "%VARSCRIPTDIR%\..\..\src\App.UI.Windows\build.bat" %VARCONFIG% %VARARCH% || GOTO error
		copy "%VARSCRIPTDIR%\..\..\src\App.UI.Windows\bin\*" "%VARTARGETDIR%\" /Y /V || GOTO error	
	) ELSE IF "!VARFRAMEWORK!"=="net4" (		
		call "!VARSCRIPTDIR!\..\..\src\win_clean.bat"		
		set VARMSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe"
		set VARTARGETFRAMEWORK="v4.8"	
		set VARRULESETPATH="!VARSCRIPTDIR!\..\..\src\ruleset\norules.ruleset"
		set VARSOLUTIONPATH="!VARSCRIPTDIR!\..\..\src\App.Forms.Windows\App.Forms.Windows.sln"
		echo !VARMSBUILD! /verbosity:minimal /property:CodeAnalysisRuleSet=!VARRULESETPATH! /p:Configuration=!VARCONFIG! /p:Platform=!VARARCH! /p:TargetFrameworkVersion=!VARTARGETFRAMEWORK! /t:Rebuild !VARSOLUTIONPATH! /p:DefineConstants="EDDIENET4" || goto :error	
		!VARMSBUILD! /verbosity:minimal /property:CodeAnalysisRuleSet=!VARRULESETPATH! /p:Configuration=!VARCONFIG! /p:Platform=!VARARCH! /p:TargetFrameworkVersion=!VARTARGETFRAMEWORK! /t:Rebuild !VARSOLUTIONPATH! /p:DefineConstants="EDDIENET4" || goto :error	
		copy !VARSCRIPTDIR!\..\..\src\App.Forms.Windows\bin\!VARARCH!\!VARCONFIG!\* !VARTARGETDIR! || goto :error
		move !VARTARGETDIR!\App.Forms.Windows.exe !VARTARGETDIR!\Eddie-UI.exe || goto :error
	)
)
 
rem Cleanup
echo Step: Cleanup
del !VARTARGETDIR!\*.profile 2>nul
del !VARTARGETDIR!\*.pdb 2>nul
del !VARTARGETDIR!\*.config 2>nul
del !VARTARGETDIR!\temp.* 2>nul
del !VARTARGETDIR!\Recovery.xml 2>nul
del !VARTARGETDIR!\mono_crash.* 2>nul
del !VARTARGETDIR!\*.xml 2>nul

rem Signing

SET /p VARSIGNPASSWORD= < "!VARSCRIPTDIR!\..\signing\eddie.win-signing.pfx.pwd"
IF exist !VARSCRIPTDIR!\..\signing\eddie.win-signing.pfx (
	echo Step: Signing

	for %%f in (!VARTARGETDIR!\*.*) do (
		IF NOT "%%~nxf"=="portable.txt" (
			echo Check signature %%~ff 
			!VARSCRIPTDIR!\..\windows_common\signtool.exe verify /pa "%%~ff"
			if ERRORLEVEL 1 (
				!VARSCRIPTDIR!\..\windows_common\signtool.exe sign /fd sha256 /p "!VARSIGNPASSWORD!" /f "!VARSCRIPTDIR!\..\signing\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - VPN Tunnel" "%%~ff" || goto :error
			) ELSE (
				rem Already signed
			)
		)
	)
)

rem Build archive
echo Step: Build archive

!VARSCRIPTDIR!\..\windows_common\7za.exe a -mx9 -tzip "!VARFINALPATH!" "!VARTARGETDIR!" || goto :error

rem Deploy
call !VARSCRIPTDIR!\..\windows_common\deploy.bat "!VARFINALPATH!" || goto :error

rem End
move "!VARFINALPATH!" "!VARDEPLOYPATH!"

rem Cleanup
echo Step: Final cleanup
rmdir /s /q !VARTARGETDIR!

:done
echo Done
exit /b 0

:error
echo Failed with error #%errorlevel%.
pause
exit /b %errorlevel%