;NSIS Eddie Main Setup

Unicode True

!include "MUI.nsh"	

;--------------------------------
;General

	SetCompressor /SOLID /FINAL lzma  		
	
	Name "Eddie - VPN Tunnel"	
	VIAddVersionKey ProductName "Eddie - VPN Tunnel"
	VIProductVersion {@version}.0
	VIAddVersionKey ProductVersion "{@version}.0"
	VIAddVersionKey CompanyName "eddie.website & AirVPN"	
	VIAddVersionKey LegalCopyright "eddie.website & AirVPN"	
	VIAddVersionKey "FileVersion" "{@version}.0"
	VIFileVersion {@version}.0
	VIAddVersionKey "FileDescription" "OpenVPN/WireGuard UI with additional user-friendly features. Open-Source, GPLv3, developed by AirVPN"
	;VIAddVersionKey "Comments" ""	
	;VIAddVersionKey "LegalTrademarks" ""
	
	
	

	OutFile "{@out}"
	
	; Adds an XP manifest to the installer
	XPStyle on
	
	;Default installation folder
	InstallDir "$PROGRAMFILES\AirVPN"
	
	;Get installation folder from registry if available
	InstallDirRegKey HKLM "Software\AirVPN" ""

;--------------------------------
;Variables

	Var MUI_TEMP
	Var STARTMENU_FOLDER

;--------------------------------
;Interface Settings
	
	; MUI Settings / Icons
	!define MUI_ICON "{@resources}\box-install.ico"
	!define MUI_UNICON "{@resources}\box-uninstall.ico"

	; MUI Settings / Header
	!define MUI_HEADERIMAGE
	!define MUI_HEADERIMAGE_RIGHT
	!define MUI_HEADERIMAGE_BITMAP "{@resources}\header-inst.bmp" ; optional
	!define MUI_HEADERIMAGE_UNBITMAP "{@resources}\header-uninst.bmp" ; optional
	
	; MUI Settings / Wizard
	!define MUI_WELCOMEFINISHPAGE_BITMAP "{@resources}\page-inst.bmp"
	!define MUI_UNWELCOMEFINISHPAGE_BITMAP "{@resources}\page-uninst.bmp"
	
	; Other
	!define MUI_ABORTWARNING

;--------------------------------
;Language Selection Dialog Settings

	;Remember the installer language
	!define MUI_LANGDLL_REGISTRY_ROOT "HKLM" 
	!define MUI_LANGDLL_REGISTRY_KEY "Software\AirVPN" 
	!define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

;--------------------------------
;Pages
	
	!insertmacro MUI_PAGE_WELCOME
	!insertmacro MUI_PAGE_LICENSE $(MUILicense)
	;!insertmacro MUI_PAGE_COMPONENTS
	!insertmacro MUI_PAGE_DIRECTORY
  
	;Start Menu Folder Page Configuration
	!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKLM" 
	!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\AirVPN" 
	!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
	!define MUI_STARTMENUPAGE_DEFAULTFOLDER "AirVPN"
	
	!insertmacro MUI_PAGE_STARTMENU Application $STARTMENU_FOLDER
	
	!insertmacro MUI_PAGE_INSTFILES
	
	!define MUI_FINISHPAGE_NOAUTOCLOSE
	;!define MUI_FINISHPAGE_RUN
	;!define MUI_FINISHPAGE_RUN_CHECKED
	;!define MUI_FINISHPAGE_RUN_TEXT "Esegui ora"
	;!define MUI_FINISHPAGE_RUN_FUNCTION "LaunchLink"
	
	!insertmacro MUI_PAGE_FINISH
	
	!insertmacro MUI_UNPAGE_WELCOME
	!insertmacro MUI_UNPAGE_CONFIRM
	!insertmacro MUI_UNPAGE_INSTFILES
	!insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

	!insertmacro MUI_LANGUAGE "English" # first language is the default language
	;!insertmacro MUI_LANGUAGE "French"
	;!insertmacro MUI_LANGUAGE "German"
	;!insertmacro MUI_LANGUAGE "Spanish"
	;!insertmacro MUI_LANGUAGE "SimpChinese"
	;!insertmacro MUI_LANGUAGE "TradChinese"
	;!insertmacro MUI_LANGUAGE "Japanese"
	;!insertmacro MUI_LANGUAGE "Korean"
	;!insertmacro MUI_LANGUAGE "Italian"
	;!insertmacro MUI_LANGUAGE "Dutch"
	;!insertmacro MUI_LANGUAGE "Danish"
	;!insertmacro MUI_LANGUAGE "Swedish"
	;!insertmacro MUI_LANGUAGE "Norwegian"
	;!insertmacro MUI_LANGUAGE "Finnish"
	;!insertmacro MUI_LANGUAGE "Greek"
	;!insertmacro MUI_LANGUAGE "Russian"
	;!insertmacro MUI_LANGUAGE "Portuguese"
	;!insertmacro MUI_LANGUAGE "PortugueseBR"
	;!insertmacro MUI_LANGUAGE "Polish"
	;!insertmacro MUI_LANGUAGE "Ukrainian"
	;!insertmacro MUI_LANGUAGE "Czech"
	;!insertmacro MUI_LANGUAGE "Slovak"
	;!insertmacro MUI_LANGUAGE "Croatian"
	;!insertmacro MUI_LANGUAGE "Bulgarian"
	;!insertmacro MUI_LANGUAGE "Hungarian"
	;!insertmacro MUI_LANGUAGE "Thai"
	;!insertmacro MUI_LANGUAGE "Romanian"
	;!insertmacro MUI_LANGUAGE "Latvian"
	;!insertmacro MUI_LANGUAGE "Macedonian"
	;!insertmacro MUI_LANGUAGE "Estonian"
	;!insertmacro MUI_LANGUAGE "Turkish"
	;!insertmacro MUI_LANGUAGE "Lithuanian"
	;!insertmacro MUI_LANGUAGE "Catalan"
	;!insertmacro MUI_LANGUAGE "Slovenian"
	;!insertmacro MUI_LANGUAGE "Serbian"
	;!insertmacro MUI_LANGUAGE "SerbianLatin"
	;!insertmacro MUI_LANGUAGE "Arabic"
	;!insertmacro MUI_LANGUAGE "Farsi"
	;!insertmacro MUI_LANGUAGE "Hebrew"
	;!insertmacro MUI_LANGUAGE "Indonesian"
	;!insertmacro MUI_LANGUAGE "Mongolian"
	;!insertmacro MUI_LANGUAGE "Luxembourgish"
	;!insertmacro MUI_LANGUAGE "Albanian"
	;!insertmacro MUI_LANGUAGE "Breton"
	;!insertmacro MUI_LANGUAGE "Belarusian"
	;!insertmacro MUI_LANGUAGE "Icelandic"
	;!insertmacro MUI_LANGUAGE "Malay"
	;!insertmacro MUI_LANGUAGE "Bosnian"
	;!insertmacro MUI_LANGUAGE "Kurdish"

;--------------------------------
;License Language String

	LicenseLangString MUILicense ${LANG_ENGLISH} "{@resources}\license.txt"
	;LicenseLangString MUILicense ${LANG_FRENCH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_GERMAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SPANISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SIMPCHINESE} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_TRADCHINESE} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_JAPANESE} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_KOREAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_ITALIAN} "..\..\data\license\it\License.txt"
	;LicenseLangString MUILicense ${LANG_DUTCH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_DANISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SWEDISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_NORWEGIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_FINNISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_GREEK} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_RUSSIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_PORTUGUESE} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_PORTUGUESEBR} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_POLISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_UKRAINIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_CZECH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SLOVAK} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_CROATIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_BULGARIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_HUNGARIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_THAI} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_ROMANIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_LATVIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_MACEDONIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_ESTONIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_TURKISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_LITHUANIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_CATALAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SLOVENIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SERBIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_SERBIANLATIN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_ARABIC} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_FARSI} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_HEBREW} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_INDONESIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_MONGOLIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_LUXEMBOURGISH} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_ALBANIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_BRETON} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_BELARUSIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_ICELANDIC} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_MALAY} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_BOSNIAN} "..\..\data\license\en\License.txt"
	;LicenseLangString MUILicense ${LANG_KURDISH} "..\..\data\license\en\License.txt"

;--------------------------------
;Reserve Files
  
	;These files should be inserted before other files in the data block
	;Keep these lines before any File command
	;Only for solid compression (by default, solid compression is enabled for BZIP2 and LZMA)
	
	!insertmacro MUI_RESERVEFILE_LANGDLL
	
;--------------------------------
;Installer Sections

	InstType "Full"
	InstType "Minimal"


	Section "!Engine" SecEngine
	
		SectionIn RO
		SetOutPath "$INSTDIR"
		
		;Compatibility Clean
		Delete "$SMPROGRAMS\$STARTMENU_FOLDER\AirVPN.lnk"
		Delete "$SMPROGRAMS\$STARTMENU_FOLDER\Eddie-UI.lnk"
		Delete "$SMPROGRAMS\$STARTMENU_FOLDER\Website.lnk"
		Delete "$INSTDIR\CLI.exe"
		Delete "$INSTDIR\AirVPN.exe"
		Delete "$INSTDIR\ssleay32.dll"
		Delete "$INSTDIR\libeay32.dll"
		Delete "$INSTDIR\portable.txt"
		
		; DotNet
		
		File "{@resources}\VC_redist.{@arch}.exe"	
		call CheckAndInstallVCRuntime

		File "{@resources}\ndp48-web.exe"		
		call CheckAndInstallDotNet
		
		ExecWait '"$INSTDIR\Eddie-CLI-Elevated.exe" service=uninstall'		
		
		; Basic (required) Eddie files...
		{@files_add}
		
		; Restore base path
		SetOutPath "$INSTDIR"
		
		;Store installation folder
		WriteRegStr HKLM "Software\AirVPN" "" $INSTDIR
		
		; Write the uninstall keys for Windows
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "DisplayName" "Eddie - VPN Tunnel"
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "UninstallString" '"$INSTDIR\uninstall.exe"'
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "URLInfoAbout" "https://eddie.website"
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "HelpLink" "https://eddie.website"
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "Publisher" "AirVPN - https://airvpn.org"
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "RegCompany" "AirVPN - https://airvpn.org"
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "DisplayIcon" '"$INSTDIR\Eddie-UI.exe"'		 
		WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "SupportUpgrade" "yes"
		WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "NoModify" 1
		WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN" "NoRepair" 1
		
		;Create uninstaller
		WriteUninstaller "$INSTDIR\uninstall.exe"
		
		!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
		
		;Create shortcuts
		CreateDirectory "$SMPROGRAMS\$STARTMENU_FOLDER"
		CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\Eddie VPN - AirVPN.lnk" "$INSTDIR\Eddie-UI.exe" -path=home
		CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\Website.lnk" "https://airvpn.org"
		CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\Uninstall.lnk" "$INSTDIR\uninstall.exe"
		CreateShortcut "$Desktop\Eddie VPN.lnk" "$INSTDIR\Eddie-UI.exe" -path=home
				
		ExecWait '"$INSTDIR\Eddie-CLI-Elevated.exe" service=install'
		
		!insertmacro MUI_STARTMENU_WRITE_END
		
	SectionEnd	
	
	
;--------------------------------
;Uninstaller Section

Section "Uninstall"

	ExecWait '"$INSTDIR\Eddie-CLI-Elevated.exe" service=uninstall-full'

	{@files_delete}
		
	; Cancellazione files di installazione
	Delete "$INSTDIR\Uninstall.exe"	
			
	;Delete empty program directories
	;Loop because if someone specify C:\Program Files\Foo\Eddie, also Foo need to be removed if empty.
	StrCpy $MUI_TEMP "$INSTDIR"
	
	programDeleteLoop:
		ClearErrors
		;MessageBox MB_OK $MUI_TEMP
		RMDir $MUI_TEMP
		GetFullPathName $MUI_TEMP "$MUI_TEMP\.."
	
		IfErrors programDeleteLoopDone
	
		Call :programDeleteLoop		
		
	programDeleteLoopDone:  
	
	; Delete from Start Menu
    
	!insertmacro MUI_STARTMENU_GETFOLDER Application $MUI_TEMP
	
	Delete "$SMPROGRAMS\$MUI_TEMP\Eddie-UI.lnk"
	Delete "$SMPROGRAMS\$MUI_TEMP\Uninstall.lnk"
	
	
	;Delete empty start menu parent directories
	StrCpy $MUI_TEMP "$SMPROGRAMS\$MUI_TEMP"
	
	startMenuDeleteLoop:
		ClearErrors
		RMDir $MUI_TEMP
		GetFullPathName $MUI_TEMP "$MUI_TEMP\.."
	
		IfErrors startMenuDeleteLoopDone
	
		StrCmp $MUI_TEMP $SMPROGRAMS startMenuDeleteLoopDone startMenuDeleteLoop
		
	startMenuDeleteLoopDone:  
	
	
	; Remove registry keys
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AirVPN"
	DeleteRegKey HKLM "Software\AirVPN"
  
	; Remove AirVPN registry keys
	DeleteRegKey /ifempty HKLM "Software\AirVPN"

SectionEnd

;--------------------------------
;Installer Functions


Function .onInit

	# the plugins dir is automatically deleted when the installer exits
        InitPluginsDir
	
	File /oname=$PLUGINSDIR\splash.bmp "{@resources}\splash.bmp"
	advsplash::show 1000 600 400 0x04025C $PLUGINSDIR\splash
        Pop $0 
	
	!insertmacro MUI_LANGDLL_DISPLAY	
			
FunctionEnd

Function .onInstSuccess

FunctionEnd

Function un.onInit

	!insertmacro MUI_UNGETLANGUAGE
	
	# the plugins dir is automatically deleted when the installer exits
        InitPluginsDir
	
	File /oname=$PLUGINSDIR\splash.bmp "{@resources}\splash.bmp"
	advsplash::show 1000 600 400 0x04025C $PLUGINSDIR\splash
        Pop $0	
	
	
FunctionEnd

Function CheckAndInstallDotNet
    ; Magic numbers from http://msdn.microsoft.com/en-us/library/ee942965.aspx
    ClearErrors
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"

    IfErrors NotDetected

    ${If} $0 >= 528049
        DetailPrint "Microsoft .NET Framework 4.8 is installed ($0)"
    ${Else}
    NotDetected:
        DetailPrint "Installing Microsoft .NET Framework 4.8"
        SetDetailsPrint listonly
        ExecWait '"$INSTDIR\ndp48-web.exe" /passive /norestart' $0
        ${If} $0 == 3010 
        ${OrIf} $0 == 1641
            DetailPrint "Microsoft .NET Framework 4.8 installer requested reboot"
            SetRebootFlag true
        ${EndIf}
        SetDetailsPrint lastused
        DetailPrint "Microsoft .NET Framework 4.8 installer returned $0"
    ${EndIf}

FunctionEnd

Function CheckAndInstallVCRuntime
	ClearErrors
	ReadRegDWORD $0 HKLM "SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\{@arch}" "Major"
	ReadRegDWORD $1 HKLM "SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\{@arch}" "Minor"

	IfErrors NotDetected

	${If} $0 < 14
		Goto NotDetected
	${ElseIf} $0 = 14
		${AndIf} $1 < 28
			Goto NotDetected
	${EndIf}

	DetailPrint "VC++ runtime is installed ($0.$1)"
	Return

    NotDetected:
        DetailPrint "Installing VC++ runtime"
        SetDetailsPrint listonly
        ExecWait '"$INSTDIR\VC_redist.{@arch}.exe" /install /passive /norestart' $0
        SetDetailsPrint lastused
        DetailPrint "VC++ runtime installer returned $0"
FunctionEnd
 

