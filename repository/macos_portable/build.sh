#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}
SCRIPTDIR=$(dirname $(realpath "$0"))

# Check args

if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

if [ "$2" == "" ]; then
	echo Second arg must be Arch: x86_64, arm64
	exit 1
fi

if [ "$3" == "" ]; then
	echo Third arg must be OS: 10.9,10.15
	exit 1
fi

if [ "$4" == "" ]; then
	echo Fourth arg must be framework: net4, net7
	exit 1
fi

PROJECT=$1
ARCH=$2
VAROS=$3
FRAMEWORK=$4
RID="osx-$ARCH"
CONFIG=Release
VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)

# We compile native on each MacOS architecture, cross-compiling not supported.
ARCHOS=$($SCRIPTDIR/../macos_common/get-arch.sh)
if [ ${ARCH} != ${ARCHOS} ]; then
    echo "Portable build for '${ARCH}' arch build skipped on this OS, cross-compiling not supported."
    exit 0;
fi

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip     

# Check env

if ! [ -x "$(command -v tar)" ]; then
  echo 'Error: tar is not installed.' >&2
  exit 1
fi

if [ $FRAMEWORK = "net7" ]; then
    if ! [ -x "$(command -v dotnet)" ]; then
        echo 'Error: dotnet is not installed.' >&2
        exit 1  
    fi
else
    if ! [ -x "$(command -v /Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild)" ]; then
        echo 'Error: msbuild is not installed. Install Mono, Xamarin, VisualStudioForMac.' >&2
        exit 1  
    fi
fi

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Clean, ensure build from zero
"${SCRIPTDIR}/../../src/macos_clean.sh"
rm -rf "${TARGETDIR}"

# Build

echo Step: Build Portable

mkdir -p ${TARGETDIR}

if [ $PROJECT = "cli" ]; then

    # Build
    TARGETMAINDIR="Eddie-CLI"
    TARGETBINDIR="${TARGETDIR}/Eddie-CLI"
    mkdir "${TARGETBINDIR}"        

    cd "${SCRIPTDIR}/../../src/App.CLI.MacOS/"
    dotnet publish App.CLI.MacOS.net7.csproj --configuration Release --runtime ${RID} --self-contained true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true
    cp "${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${CONFIG}/net7.0/${RID}/publish"/* "${TARGETBINDIR}"
    cp "${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${CONFIG}/net7.0/${RID}/eddie-cli-elevated" "${TARGETBINDIR}"
    cp "${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${CONFIG}/net7.0/${RID}/libLib.Platform.MacOS.Native.dylib" "${TARGETBINDIR}"

    # Resources
    echo Step: Resources
    cp "${SCRIPTDIR}/../../deploy/macos_${ARCH}"/* "${TARGETBINDIR}"
    cp -r "${SCRIPTDIR}/../../resources" "${TARGETBINDIR}/Resources"
    if [[ ${VERSION} =~ ^2 ]]; then
        rm -rf "${TARGETBINDIR}/Resources/webui"
    fi

elif [ $PROJECT = "ui" ]; then 
    TARGETMAINDIR="Eddie.app"
    TARGETBINDIR="${TARGETDIR}/Eddie.app/Contents/MacOS"

    echo Step: Dependencies        
    "${SCRIPTDIR}/../macos_portable/build.sh" cli ${ARCH} ${VAROS} net7
    DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-cli_${VERSION}_${VAROS}_${ARCH}_portable.zip
    cp "${DEPPACKAGEPATH}" "${TARGETDIR}"
    cd "${TARGETDIR}"
    unzip *.zip
    rm -rf "${TARGETDIR}/Eddie-CLI/_CodeSignature" # Otherwise issue after "codesign subcomponent error"

    if [ $FRAMEWORK = "net7" ]; then
        echo Step: Compile
        "${SCRIPTDIR}/../../src/App.UI.MacOS/build.sh" ${CONFIG}
    
        cp -r "${SCRIPTDIR}/../../src/App.UI.MacOS/bin/Eddie.app" .        
    elif [ $FRAMEWORK = "net4" ]; then
        # Note: libLib.Platform.MacOS.Native.dylib will be in both MacOS (used by Eddie-CLI) and MonoBundle (used by Eddie-UI Xamarin)        
        echo Step: Compile
        TARGETFRAMEWORK="v4.8";
        RULESETPATH="${SCRIPTDIR}/../../src/ruleset/norules.ruleset"
        SOLUTIONPATH="${SCRIPTDIR}/../../src/App.Cocoa.MacOS/App.Cocoa.MacOS.sln"
        "${SCRIPTDIR}/../../src/macos_clean.sh"
        echo /Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild /verbosity:minimal /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=x64 /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIENET4"        
        /Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild /verbosity:minimal /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=x64 /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIENET4"        
        cp -r "${SCRIPTDIR}/../../src/App.Cocoa.MacOS/bin/x64/${CONFIG}/Eddie-UI.app" "${TARGETDIR}/Eddie.app"
    fi

    mv "Eddie-CLI/Resources"/* "${TARGETBINDIR}/../Resources/"
    rm -r "Eddie-CLI/Resources"
    mv "Eddie-CLI/"* "${TARGETBINDIR}"
fi

# Touch to update main folder timestamp
touch "${TARGETDIR}/${TARGETMAINDIR}"

# Cleanup

echo Step: Cleanup
rm -f "${TARGETBINDIR}/"*.profile
rm -f "${TARGETBINDIR}/"*.pdb
rm -f "${TARGETBINDIR}/"*.config
rm -f "${TARGETBINDIR}/"Recovery.xml

# Owner and Permissions

echo Step: Owner and Permissions

chmod -R 755 "${TARGETBINDIR}"
find ${TARGETBINDIR} -type f -exec chmod 644 {} +;
chmod 755 "${TARGETBINDIR}/Eddie-CLI"
if [ $PROJECT = "ui" ]; then
    chmod 755 "${TARGETBINDIR}/Eddie-UI"
fi
chmod 755 "${TARGETBINDIR}/eddie-cli-elevated"
chmod 755 "${TARGETBINDIR}/openvpn"
chmod 755 "${TARGETBINDIR}/hummingbird"
chmod 755 "${TARGETBINDIR}/stunnel"
chmod 755 "${TARGETBINDIR}/wireguard-go"
chmod 755 "${TARGETBINDIR}/wg"

# Signing

# Exception:
# codesign --entitlements (required by notarization, otherwise --options=runtime don't allow JIT, required from Catalina 10.15.3)
# cause "SystemNetworkInterface threw an exception"
# Sometime 'LSOpenURLsWithRole() failed with error -47' is resolved with a reboot...
# See comment in /repository/build_macos.sh

VARHARDENING="yes"
# 2023-09-21, Ventura Notarization fail without hardening
#if [ ${VAROS} = "macos-10.9" ]; then
#    VARHARDENING="no"
#fi

echo "Signing"

# Remember: never sign openvpn/wg again here, changing it invalidate the hash already compiled in Elevated

"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/eddie-cli-elevated" no $VARHARDENING

if [ $FRAMEWORK = "net7" ]; then
    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/libLib.Platform.macOS.Native.dylib" no $VARHARDENING
else
    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/../MonoBundle/libLib.Platform.macOS.Native.dylib" no $VARHARDENING
    
    # Exception: is already signed by Xamarin, but here force re-signing, otherwise under Catalina with notarization/hardening throw
    # "warning: exception inside UnhandledException handler: (null) assembly:/Users/user/Documents/eddie-air/repository/files/Eddie.app/Contents/MonoBundle/mscorlib.dll type:TypeInitializationException member:(null)
    # [ERROR] FATAL UNHANDLED EXCEPTION: System.TypeInitializationException: The type initializer for 'Eddie.Core.RandomGenerator' threw an exception. ---> System.TypeInitializationException: The type initializer for 'System.Random' threw an exception. ---> System.TypeInitializationException: The type initializer for 'Sys' threw an exception. ---> System.DllNotFoundException: mono-native assembly:<unknown assembly> type:<unknown type> member:(null)"

    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/../MonoBundle/libmono-native.dylib" yes $VARHARDENING

    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/../MonoBundle/libMonoPosixHelper.dylib" yes $VARHARDENING
fi

"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/Eddie-CLI" no $VARHARDENING

if [ $PROJECT = "ui" ]; then
    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETBINDIR}/Eddie-UI" yes $VARHARDENING
fi

"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/${TARGETMAINDIR}" yes $VARHARDENING

# Build archive

echo Step: Build archive
cd "${TARGETDIR}/" 
zip -r "${FINALPATH}" ${TARGETMAINDIR}

# Sign archive
"${SCRIPTDIR}/../macos_common/sign.sh" "${FINALPATH}" yes $VARHARDENING

# Notarization
if [ ${VARHARDENING} = "yes" ]; then    
    "${SCRIPTDIR}/../macos_common/notarize.sh" "${FINALPATH}"
fi

# Deploy to eddie.website
"${SCRIPTDIR}/../macos_common/deploy.sh" "${FINALPATH}" "internal"

# End
mv ${FINALPATH} ${DEPLOYPATH}

# Cleanup

echo Step: Final cleanup
rm -rf ${TARGETDIR}


