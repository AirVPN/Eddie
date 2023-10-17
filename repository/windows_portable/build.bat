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

rmdir "!VARTARGETDIR!" /S /Q 2>nul

rmdir "!VARSCRIPTDIR!\..\..\src\Lib.Core\bin" /S /Q 2>nul
rmdir "!VARSCRIPTDIR!\..\..\src\Lib.Core\obj" /S /Q 2>nul
rmdir "!VARSCRIPTDIR!\..\..\src\Lib.Platform.Windows\bin" /S /Q 2>nul
rmdir "!VARSCRIPTDIR!\..\..\src\Lib.Platform.Windows\obj" /S /Q 2>nul

IF "!VARFRAMEWORK!"=="net7" (
	rmdir "!VARSCRIPTDIR!\..\..\App.CLI\bin" /S /Q 2>nul
	rmdir "!VARSCRIPTDIR!\..\..\App.CLI\obj" /S /Q 2>nul	
) ELSE IF "!VARFRAMEWORK!"=="net4" (
	rmdir "!VARSCRIPTDIR!\..\..\App.Forms.Windows\bin" /S /Q 2>nul
	rmdir "!VARSCRIPTDIR!\..\..\App.Forms.Windows\obj" /S /Q 2>nul

	rmdir "!VARSCRIPTDIR!\..\..\Lib.Forms\bin" /S /Q 2>nul
	rmdir "!VARSCRIPTDIR!\..\..\Lib.Forms\obj" /S /Q 2>nul

	rmdir "!VARSCRIPTDIR!\..\..\Lib.Forms.Skin\bin" /S /Q 2>nul
	rmdir "!VARSCRIPTDIR!\..\..\Lib.Forms.Skin\obj" /S /Q 2>nul
)
	
echo Step: Compilation

set VARARCHCOMPILE=!VARARCH!

call "!VARSCRIPTDIR!\..\windows_common\compile.bat" !VARPROJECT! !VARARCH! !VARFRAMEWORK! || goto :error

echo Step: Copying

mkdir !VARTARGETDIR!

IF "!VARFRAMEWORK!"=="net7" (
	IF "!VARPROJECT!"=="cli" (
		echo copy !VARSCRIPTDIR!\..\..\src\App.CLI\bin\!VARCONFIG!\net7.0\win-x64\publish\* !VARTARGETDIR! || goto :error
		copy !VARSCRIPTDIR!\..\..\src\App.CLI\bin\!VARCONFIG!\net7.0\win-x64\publish\* !VARTARGETDIR! || goto :error		
	) ELSE IF "!VARPROJECT!"=="ui" (
		echo WIP		
	)	
) ELSE IF "!VARFRAMEWORK!"=="net4" (
	IF "!VARPROJECT!"=="cli" (
		copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARARCHCOMPILE!\!VARCONFIG!\* !VARTARGETDIR! || goto :error
		move !VARTARGETDIR!\App.CLI.Windows.exe !VARTARGETDIR!\Eddie-CLI.exe || goto :error	
	) ELSE IF "!VARPROJECT!"=="ui" (
		copy !VARSCRIPTDIR!\..\..\src\App.CLI.Windows\bin\!VARARCHCOMPILE!\!VARCONFIG!\APP.CLI.Windows.exe !VARTARGETDIR!\Eddie-CLI.exe || goto :error
		copy !VARSCRIPTDIR!\..\..\src\App.Forms.Windows\bin\!VARARCHCOMPILE!\!VARCONFIG!\* !VARTARGETDIR! || goto :error
		move !VARTARGETDIR!\App.Forms.Windows.exe !VARTARGETDIR!\Eddie-UI.exe || goto :error
	)
)

rem Resources
echo Step: Resources
copy !VARSCRIPTDIR!\..\..\deploy\!VAROS!_!VARARCH!\* !VARTARGETDIR! || goto :error
robocopy !VARSCRIPTDIR!\..\..\common !VARTARGETDIR!\res /E
 
rem Cleanup
echo Step: Cleanup
del !VARTARGETDIR!\*.profile 2>nul
del !VARTARGETDIR!\*.pdb 2>nul
del !VARTARGETDIR!\*.config 2>nul
del !VARTARGETDIR!\temp.* 2>nul
del !VARTARGETDIR!\Recovery.xml 2>nul
del !VARTARGETDIR!\mono_crash.* 2>nul
del !VARTARGETDIR!\*.xml 2>nul

rmdir "!VARTARGETDIR!\res\webui" /S /Q 2>nul

rem Signing

SET /p VARSIGNPASSWORD= < "!VARSCRIPTDIR!\..\signing\eddie.win-signing.pfx.pwd"
IF exist !VARSCRIPTDIR!\..\signing\eddie.win-signing.pfx (
	echo Step: Signing

	for %%f in (!VARTARGETDIR!\*.*) do (
		IF NOT "%%~nxf"=="openvpn.exe" (
			echo Check signature %%~ff 
			rem !VARSCRIPTDIR!\..\windows_common\signtool.exe verify /pa "%%~ff" | find /i "No signature found"
			!VARSCRIPTDIR!\..\windows_common\signtool.exe verify /pa "%%~ff"
			if ERRORLEVEL 1 (
				echo !VARSCRIPTDIR!\..\windows_common\signtool.exe sign /fd sha256 /p "!VARSIGNPASSWORD!" /f "!VARSCRIPTDIR!\..\signing\eddie.win-signing.pfx" /t http://timestamp.comodoca.com/authenticode /d "Eddie - VPN Tunnel" "%%~ff" || goto :error
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