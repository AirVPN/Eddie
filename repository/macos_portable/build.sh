
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
	echo Second arg must be Architecture: x64
	exit 1
fi

if [ "$3" == "" ]; then
	echo Third arg must be OS: 10.9,10.15
	exit 1
fi

# Check env

if ! [ -x "$(command -v tar)" ]; then
  echo 'Error: tar is not installed.' >&2
  exit 1
fi
if ! [ -x "$(command -v msbuild)" ]; then
  echo 'Error: msbuild is not installed. Install Mono, Xamarin, VisualStudio.' >&2
  exit 1
fi

PROJECT=$1
ARCH=$2
VAROS=$3
CONFIG=Release

VARSTAFF="no"
if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then # Staff AirVPN
    VARSTAFF="yes"
fi

# ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)

VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip 

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup

rm -rf /tmp/eddie_deploy

# Compile

echo Step: Compile

ARCHCOMPILE=${ARCH}

"${SCRIPTDIR}/../macos_common/compile.sh" ${PROJECT}

# Build

echo Step: Build

mkdir -p ${TARGETDIR}

if [ ${PROJECT} = "cli" ]; then
    cp -r "${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/Eddie.app" "${TARGETDIR}"
elif [ ${PROJECT} = "ui" ]; then
    cp -r "${SCRIPTDIR}/../../src/App.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/Eddie.app" "${TARGETDIR}"
elif [ ${PROJECT} = "ui3" ]; then
	cp -r "${SCRIPTDIR}/../../src/UI.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/Eddie.app" "${TARGETDIR}"
fi

# Touch to update main folder timestamp
touch "${TARGETDIR}/Eddie.App"

# Resources

echo Step: Resources
cp ${SCRIPTDIR}/../../deploy/macos_${ARCH}/* "${TARGETDIR}/Eddie.App/Contents/MacOS/"
cp -r ${SCRIPTDIR}/../../common/* "${TARGETDIR}/Eddie.App/Contents/Resources/"

# Cleanup

echo Step: Cleanup
rm -f ${TARGETDIR}/Eddie.App/Contents/MacOS/*.profile
rm -f ${TARGETDIR}/Eddie.App/Contents/MacOS/*.pdb
rm -f ${TARGETDIR}/Eddie.App/Contents/MacOS/*.config
rm -f ${TARGETDIR}/Eddie.App/Contents/MacOS/Recovery.xml
rm -rf ${TARGETDIR}/Eddie.App/Contents/Resources/providers

if [ $PROJECT = "cli" ]; then
	rm -rf "${TARGETDIR}/Eddie.App/Contents/Resources/webui"
elif [ $PROJECT = "ui" ]; then
	rm -rf "${TARGETDIR}/Eddie.App/Contents/Resources/webui"
elif [ $PROJECT = "ui3" ]; then
    rm -rf "${TARGETDIR}/Eddie.App/Contents/Resources/nothing"
else
	echo "Unexpected"
	exit 1
fi

if [ $PROJECT = "cli" ]; then
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libglib-2.0.0.dylib"
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libcairo.2.dylib"
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libfontconfig.1.dylib"
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libfreetype.6.dylib"
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libtiff.5.dylib"
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libgdiplus.dylib"
    rm -rf "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libgdiplus.0.dylib"
fi

# Owner and Permissions

echo Step: Owner and Permissions

chmod -R 755 "${TARGETDIR}"
find ${TARGETDIR} -type f -exec chmod 644 {} +;
if [ ${PROJECT} = "cli" ]; then
    chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie"
elif [ ${PROJECT} = "ui" ]; then
    chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie"
elif [ ${PROJECT} = "ui3" ]; then
    chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie"
else
    echo "Unexpected"
    exit 1
fi

chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/eddie-cli-elevated"
chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/openvpn"
chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/hummingbird"
chmod 755 "${TARGETDIR}/Eddie.App/Contents/MacOS/stunnel"

# Signing

# Exception:
# codesign --entitlements (required by notarization, otherwise --options=runtime don't allow JIT, required from Catalina 10.15.3)
# cause "SystemNetworkInterface threw an exception"
# Sometime 'LSOpenURLsWithRole() failed with error -47' is resolved with a reboot...
# See comment in /repository/build_macos.sh

VARHARDENING="yes"
if [ ${VAROS} = "macos-10.9" ]; then
    VARHARDENING="no"
fi

if [ ${VARSTAFF} = "yes" ]; then

    echo "Signing"
    
    # Remember: never sign openvpn again here, changing it invalidate the hash already compiled in Elevated
    
    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.App/Contents/MacOS/eddie-cli-elevated" no $VARHARDENING
    
    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.App/Contents/MonoBundle/libLib.Platform.macOS.Native.dylib" no $VARHARDENING

    # Exception: is already signed by Xamarin, but here force re-signing, otherwise under Catalina with notarization/hardening throw
    # "warning: exception inside UnhandledException handler: (null) assembly:/Users/user/Documents/eddie-air/repository/files/Eddie.app/Contents/MonoBundle/mscorlib.dll type:TypeInitializationException member:(null)
    # [ERROR] FATAL UNHANDLED EXCEPTION: System.TypeInitializationException: The type initializer for 'Eddie.Core.RandomGenerator' threw an exception. ---> System.TypeInitializationException: The type initializer for 'System.Random' threw an exception. ---> System.TypeInitializationException: The type initializer for 'Sys' threw an exception. ---> System.DllNotFoundException: mono-native assembly:<unknown assembly> type:<unknown type> member:(null)"

    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.App/Contents/MonoBundle/libmono-native.dylib" yes $VARHARDENING

    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.App/Contents/MonoBundle/libMonoPosixHelper.dylib" yes $VARHARDENING

    # Exception

    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MacOS/Eddie" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libcairo.2.dylib" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libglib-2.0.0.dylib" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libfreetype.6.dylib" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libfontconfig.1.dylib" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libtiff.5.dylib" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libgdiplus.0.dylib" yes $VARHARDENING
    #"${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.app/Contents/MonoBundle/libgdiplus.dylib" yes $VARHARDENING

    # This may have force=no, it's yes to be sure

    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie" yes $VARHARDENING

    # This may have force=no, it's yes to be sure

    "${SCRIPTDIR}/../macos_common/sign.sh" "${TARGETDIR}/Eddie.App" yes $VARHARDENING
fi

# Build archive

echo Step: Build archive
cd "${TARGETDIR}/" 
zip -r "${FINALPATH}" Eddie.app

# Sign archive

if [ ${VARSTAFF} = "yes" ]; then
    "${SCRIPTDIR}/../macos_common/sign.sh" "${FINALPATH}" yes $VARHARDENING
fi

# Notarization

if [ ${VARSTAFF} = "yes" ]; then
    if [ ${VARHARDENING} = "yes" ]; then    
        "${SCRIPTDIR}/../macos_common/notarize.sh" "${FINALPATH}" "${PROJECT}"
        echo "${SCRIPTDIR}/../macos_common/notarize.sh" "${FINALPATH}" "${PROJECT}"
    fi
fi

# Deploy to eddie.website

if [ ${VARSTAFF} = "yes" ]; then
    "${SCRIPTDIR}/../macos_common/deploy.sh" "${FINALPATH}"
fi

# End

mv ${FINALPATH} ${DEPLOYPATH}

# Cleanup

echo Step: Final cleanup
rm -rf ${TARGETDIR}


