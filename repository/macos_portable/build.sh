
#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
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
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath "$0"))
ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_portable
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_portable.tar.gz 

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
rm -rf $TARGETDIR

# Compile
echo Step: Compile

ARCHCOMPILE=${ARCH}

${SCRIPTDIR}/../macos_common/compile.sh ${PROJECT}

mkdir -p ${TARGETDIR}

if [ ${PROJECT} = "cli" ]; then
    echo "Unsupported"
    exit 1
elif [ ${PROJECT} = "ui" ]; then
	cp -r ${SCRIPTDIR}/../../src/App.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/Eddie.app $TARGETDIR
elif [ ${PROJECT} = "u3" ]; then
	echo "Unsupported"
    exit 1
fi

# Resources
echo Step: Resources
cp ${SCRIPTDIR}/../../deploy/macos_${ARCH}/* $TARGETDIR/Eddie.App/Contents/MacOS/
cp -r ${SCRIPTDIR}/../../common/* $TARGETDIR/Eddie.App/Contents/Resources/

# Cleanup
echo Step: Cleanup
rm -f $TARGETDIR/Eddie.App/Contents/MacOS/*.profile
rm -f $TARGETDIR/Eddie.App/Contents/MacOS/*.pdb
rm -f $TARGETDIR/Eddie.App/Contents/MacOS/*.config
rm -rf $TARGETDIR/Eddie.App/Contents/Resources/providers

if [ $PROJECT = "cli" ]; then
	rm -rf $TARGETDIR/Eddie.App/Contents/Resources/webui
elif [ $PROJECT = "ui" ]; then
	rm -rf $TARGETDIR/Eddie.App/Contents/Resources/webui
elif [ $PROJECT = "ui3" ]; then
    rm -rf $TARGETDIR/Eddie.App/Contents/Resources/nothing
else
	echo "Unexpected"
	exit 1
fi

# Owner and Permissions
echo Step: Owner and Permissions

chmod -R 755 ${TARGETDIR}
find ${TARGETDIR} -type f -exec chmod 644 {} +;
if [ $PROJECT = "cli" ]; then
    chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/eddie-${PROJECT}
elif [ $PROJECT = "ui" ]; then
    chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie
elif [ $PROJECT = "ui3" ]; then
    chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie
else
    echo "Unexpected"
    exit 1
fi
chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/eddie-cli-elevated
chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/openvpn
chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/stunnel

# Signing
${SCRIPTDIR}/../macos_common/sign.sh ${TARGETDIR}/Eddie.App/Contents/MacOS/Eddie
${SCRIPTDIR}/../macos_common/sign.sh ${TARGETDIR}/Eddie.App/Contents/MacOS/eddie-cli-elevated
${SCRIPTDIR}/../macos_common/sign.sh ${TARGETDIR}/Eddie.App/Contents/MacOS/openvpn
${SCRIPTDIR}/../macos_common/sign.sh ${TARGETDIR}/Eddie.App/Contents/MacOS/stunnel
${SCRIPTDIR}/../macos_common/sign.sh ${TARGETDIR}/Eddie.App/Contents/MonoBundle/libLib.Platform.macOS.Native.dylib
${SCRIPTDIR}/../macos_common/sign.sh ${TARGETDIR}/Eddie.App

# Exception: sign.sh use "force" and "deep", and signing Eddie.App alter the sha256 of openvpn (already fetched by elevation).
# openvpn in deploy are already signed, so simply re-copy.
# Not sure why, but it doesn't invalidate the Eddie.App signature.
cp ${SCRIPTDIR}/../../deploy/macos_${ARCH}/openvpn $TARGETDIR/Eddie.App/Contents/MacOS/openvpn
chmod 755 ${TARGETDIR}/Eddie.App/Contents/MacOS/openvpn

# Build archive
echo Step: Build archive
cd "${TARGETDIR}/" 

if [ $PROJECT = "cli" ]; then
    tar cvpfz "${DEPLOYPATH}" "eddie-${PROJECT}"
elif [ $PROJECT = "ui" ]; then
    tar cvpfz "${DEPLOYPATH}" Eddie.app
elif [ $PROJECT = "ui3" ]; then
    tar cvpfz "${DEPLOYPATH}" Eddie.app
else
    echo "Unexpected"
    exit 1
fi

# Deploy to eddie.website
${SCRIPTDIR}/../macos_common/deploy.sh ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
rm -rf ${TARGETDIR}


