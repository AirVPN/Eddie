#!/bin/bash

set -euo pipefail

if [ "$1" == "" ]; then
    echo First arg must be Project: cli,ui
    exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_appimage
#DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_appimage.AppImage
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_appimage.tar.gz

if test -f "${DEPLOYPATH}"; then
    echo "Already builded: ${DEPLOYPATH}"
    exit 0;
fi

if [ "$ARCH" = "armv7l" ]; then
    # https://github.com/linuxdeploy/linuxdeploy/releases armhf not exists
    echo "armv7l architecture not supported"; 
    exit 0;
fi

if [ "$ARCH" = "aarch64" ]; then
    # https://github.com/linuxdeploy/linuxdeploy/releases aarch64 not exists
    echo "aarch64 architecture not supported"; 
    exit 0;
fi

# Cleanup
rm -rf $TARGETDIR

# Package dependencies
echo Step: Package dependencies - Build Portable
"${SCRIPTDIR}/../linux_portable/build.sh" ${PROJECT}
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable.tar.gz  
tar xvfp "${DEPPACKAGEPATH}" -C "${TARGETDIR}"

# Adapt
echo Step: Adapt

cd $TARGETDIR
mkdir -p AppDir/opt
mv eddie-${PROJECT}/bundle AppDir/opt/eddie-${PROJECT}
mv eddie-${PROJECT}/eddie-${PROJECT} AppDir/AppRun
rm -r eddie-${PROJECT}

# Adapt Launcher
echo Step: AdaptLauncher

if [ $PROJECT = "cli" ]; then # if exists because issue using ${PROJECT} in sed expression
    sed -i 's/\/bundle/\/opt\/eddie-cli/g' AppDir/AppRun
    sed -i 's/ --path=\"\$MAINDIR\"/ --path=home/g' AppDir/AppRun
elif [ $PROJECT = "ui" ]; then
    sed -i 's/\/bundle/\/opt\/eddie-ui/g' AppDir/AppRun
    sed -i 's/ --path=\"\$MAINDIR\"/ --path=home/g' AppDir/AppRun
fi

# AppImage
echo Step: AppImage

if [ "$ARCH" = "x86" ]; then
	ARCHAPPIMAGE="i686"
elif [ "$ARCH" = "x64" ]; then
	ARCHAPPIMAGE="x86_64"
elif [ "$ARCH" = "armhf" ]; then
	ARCHAPPIMAGE="armhf"
else
	echo "Unknown arch for AppImage"
	exit 1
fi

wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-${ARCHAPPIMAGE}.AppImage
mv appimagetool*.AppImage appimagetool.AppImage
chmod +x appimagetool.AppImage


cp AppDir/opt/eddie-${PROJECT}/res/icon.png AppDir/eddie-${PROJECT}.png

if [ $PROJECT = "cli" ]; then
	printf "[Desktop Entry]\nName=Eddie - VPN UI\nExec=eddie-cli\nIcon=eddie-cli\nType=Application\nTerminal=true\nCategories=Network;\n" >AppDir/eddie-cli.desktop 
elif [ $PROJECT = "ui" ]; then
	printf "[Desktop Entry]\nName=Eddie - VPN UI\nExec=eddie-ui\nIcon=eddie-ui\nType=Application\nTerminal=false\nCategories=GNOME;Network;\n" >AppDir/eddie-ui.desktop 
elif [ $PROJECT = "ui3" ]; then
    printf "[Desktop Entry]\nName=Eddie - VPN UI\nExec=eddie-ui\nIcon=eddie-ui\nType=Application\nTerminal=false\nCategories=GNOME;Network;\n" >AppDir/eddie-ui.desktop 
fi

sudo ./appimagetool.AppImage AppDir

#mv Eddie*.AppImage ${DEPLOYPATH}
rm -rf Eddie
mv Eddie*.AppImage Eddie
tar cvfz "$DEPLOYPATH" "Eddie"

sudo chown $USER ${DEPLOYPATH}

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}

# Cleanup - with sudo because AppImage create files as root
sudo rm -rf $TARGETDIR

