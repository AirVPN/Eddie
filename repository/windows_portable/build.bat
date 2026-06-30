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
	echo "Line missing"
	goto :error
)

set VARPROJECT=%1
set VARARCH=%2
set VAROS=%3
set VARLINE=%4
set VARRID="win-!VARARCH!"
set VARCONFIG=Release

if not "!VARLINE!"=="l" if not "!VARLINE!"=="u" (
	echo "Line must be l or u"
	goto :error
)

set VARPACKAGEPROJECT=!VARPROJECT!
if "!VARLINE!"=="u" set VARPACKAGEPROJECT=!VARPROJECT!-u
set VARDEPPACKAGEPROJECT=cli
if "!VARLINE!"=="u" set VARDEPPACKAGEPROJECT=cli-u

set VARSCRIPTDIR=%~dp0
FOR /F "tokens=*" %%g IN ('!VARSCRIPTDIR!\..\windows_common\get-version.exe') do (SET "VARVERSION=%%g")
set VARTARGETDIR=%TEMP%\eddie_deploy\eddie-!VARPACKAGEPROJECT!_!VARVERSION!_!VAROS!_!VARARCH!_portable
set VARFINALPATH=%TEMP%\eddie_deploy\eddie-!VARPACKAGEPROJECT!_!VARVERSION!_!VAROS!_!VARARCH!_portable.zip
set VARDEPLOYPATH=!VARSCRIPTDIR!\..\files\eddie-!VARPACKAGEPROJECT!_!VARVERSION!_!VAROS!_!VARARCH!_portable.zip

echo Build Windows Portable, Project: !VARPROJECT!, OS: !VAROS!, Arch: !VARARCH!, Line: !VARLINE!

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
	dotnet publish App.CLI.Windows.net10.csproj --configuration Release --runtime !VARRID! --self-contained true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net10.0\!VARRID!\publish\* !VARTARGETDIR! || goto :error		
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net10.0\!VARRID!\Eddie-CLI-Elevated.exe !VARTARGETDIR! || goto :error		
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net10.0\!VARRID!\Eddie-CLI-Elevated-Service.exe !VARTARGETDIR! || goto :error		
	copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARCONFIG!\net10.0\!VARRID!\Lib.Platform.Windows.Native.dll !VARTARGETDIR! || goto :error				

	rem Resources
	echo Step: Resources
	copy !VARSCRIPTDIR!\..\..\deploy\!VAROS!_!VARARCH!\* !VARTARGETDIR! || goto :error	
	robocopy !VARSCRIPTDIR!\..\..\resources !VARTARGETDIR!\Resources /E
	IF "!VARLINE!"=="l" (
		rmdir "!VARTARGETDIR!\Resources\webui" /S /Q 2>nul
	)
	
) 

IF "!VARPROJECT!"=="ui" (
	echo Step: Dependencies
	CALL %VARSCRIPTDIR%\..\windows_portable\build.bat cli %VARARCH% %VAROS% %VARLINE% || goto :error
	%VARSCRIPTDIR%\..\windows_common\7za.exe -y x "%VARSCRIPTDIR%\..\files\eddie-!VARDEPPACKAGEPROJECT!_%VARVERSION%_%VAROS%_%VARARCH%_portable.zip" -o"%VARTARGETDIR%" || goto :error
	robocopy "%VARTARGETDIR%\eddie-!VARDEPPACKAGEPROJECT!_%VARVERSION%_%VAROS%_%VARARCH%_portable" "%VARTARGETDIR%" *.* /E /MOVE

	IF "!VARLINE!"=="u" (
		echo Build UI App
		call "%VARSCRIPTDIR%\..\..\src\App.UI.Windows\build.bat" %VARCONFIG% %VARARCH% || GOTO error
		copy "%VARSCRIPTDIR%\..\..\src\App.UI.Windows\bin\*" "%VARTARGETDIR%\" /Y /V || GOTO error	
	) ELSE IF "!VARLINE!"=="l" (
		call "!VARSCRIPTDIR!\..\..\src\win_clean.bat"		
		call "!VARSCRIPTDIR!\..\windows_common\locate_msbuild.bat" || exit /b 1
		set VARTARGETFRAMEWORK="v4.8.1"	
		set VARRULESETPATH="!VARSCRIPTDIR!\..\..\src\ruleset\norules.ruleset"
		set VARSOLUTIONPATH="!VARSCRIPTDIR!\..\..\src\App.Forms.Windows\App.Forms.Windows.sln"
		"!VARMSBUILD!" /verbosity:minimal /property:CodeAnalysisRuleSet=!VARRULESETPATH! /p:Configuration=!VARCONFIG! /p:Platform=!VARARCH! /p:TargetFrameworkVersion=!VARTARGETFRAMEWORK! /t:Rebuild !VARSOLUTIONPATH! /p:DefineConstants="EDDIENET4" || goto :error	
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

if defined EDDIESIGNINGDIR (
	echo Step: Signing		
	
	SET /p VARSIGNPASSWORD= < "%EDDIESIGNINGDIR%\eddie.win-signing.pfx.pwd"

	for %%f in (!VARTARGETDIR!\*.exe !VARTARGETDIR!\*.dll !VARTARGETDIR!\*.sys !VARTARGETDIR!\*.cat) do (
		echo Check signature %%~ff 
		!VARSCRIPTDIR!\..\windows_common\signtool.exe verify /pa "%%~ff"
		if ERRORLEVEL 1 (
			!VARSCRIPTDIR!\..\windows_common\signtool.exe sign /fd sha256 /p "!VARSIGNPASSWORD!" /f "%EDDIESIGNINGDIR%\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - VPN Tunnel" "%%~ff" || goto :error
		) ELSE (
			rem Already signed
		)
	)
)

rem Build archive
echo Step: Build archive

!VARSCRIPTDIR!\..\windows_common\7za.exe a -mx9 -tzip "!VARFINALPATH!" "!VARTARGETDIR!" || goto :error

rem Staff Deploy
if defined EDDIESIGNINGDIR (
	call !VARSCRIPTDIR!\..\windows_common\deploy.bat "!VARFINALPATH!" || goto :error
	call !VARSCRIPTDIR!\..\windows_common\sign-openpgp.bat "!VARFINALPATH!" || goto :error
	if exist "!VARFINALPATH!.asc" call !VARSCRIPTDIR!\..\windows_common\deploy.bat "!VARFINALPATH!.asc" || goto :error
)

rem End
move "!VARFINALPATH!" "!VARDEPLOYPATH!"
if exist "!VARFINALPATH!.asc" move "!VARFINALPATH!.asc" "!VARDEPLOYPATH!.asc"

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