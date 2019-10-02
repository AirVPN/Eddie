#!/bin/bash

set -e

if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable.tar.gz 

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
rm -rf $TARGETDIR

# Package dependencies
echo Step: Package dependencies - Build Mono
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_mono.tar.gz  
${SCRIPTDIR}/../linux_mono/build.sh ${PROJECT}
tar xvfp "${DEPPACKAGEPATH}" -C "${TARGETDIR}"
rm ${TARGETDIR}/eddie-${PROJECT}/eddie-${PROJECT} # old launcher not need

echo Step: Build portable

if [ $ARCH = "armhf" ]; then
	MKBUNDLECROSSTARGET="mono-6.0.0-raspbian-9-arm"
else
	MKBUNDLECROSSTARGET="mono-5.10.0-debian-7-${ARCH}"
fi

# Issue here, check with 'mkbundle --list-targets'
#mkdir -p /home/pi/.mono/targets/${MKBUNDLECROSSTARGET}/lib/mono # Not sure if need
if [[ ! -d ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET} ]]; then
    echo Download mkbundle target. If break here, check with 'mkbundle --list-targets' and fix build.sh MKBUNDLECROSSTARGET
    mkbundle --fetch-target ${MKBUNDLECROSSTARGET}

    # Fix issue 'ERROR: Unable to load assembly `Mono.WebBrowser' referenced by'
    cp /usr/lib/mono/4.5/Mono.WebBrowser.dll ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/lib/mono/4.5/

    # Fix issue 'ERROR: Unable to load assembly `Mono.WebBrowser' referenced by'
    cp /usr/lib/mono/4.5/Mono.Data.Tds.dll ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/lib/mono/4.5/
fi


mv ${TARGETDIR}/eddie-${PROJECT}/bundle ${TARGETDIR}/bundle
rm -rf ${TARGETDIR}/eddie-${PROJECT}
cd ${TARGETDIR}/bundle

# Copy bin
cp ${SCRIPTDIR}/../../deploy/linux_${ARCH}/* $TARGETDIR/bundle/

# Update config
cp ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/etc/mono/config ${SCRIPTDIR}/mkbundle.config
sed -i 's/\$mono_libdir\///g' ${SCRIPTDIR}/mkbundle.config

echo mkbundle run
if [ $PROJECT = "cli" ]; then
	mkbundle eddie-${PROJECT}.exe -o eddie-${PROJECT} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so
elif [ $PROJECT = "ui" ]; then
	mkbundle eddie-${PROJECT}.exe -o eddie-${PROJECT} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so --library ${TARGETDIR}/bundle/libgdiplus.so.0
elif [ $PROJECT = "u3" ]; then
	mkbundle eddie-${PROJECT}.exe -o eddie-${PROJECT} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so
fi

# Remove unneed
rm ${TARGETDIR}/bundle/*.exe # Linked with mkbundle
rm ${TARGETDIR}/bundle/*.dll # Linked with mkbundle
rm ${TARGETDIR}/bundle/libgdiplus.so.0 # Linked with mkbundle
rm ${TARGETDIR}/bundle/libMonoPosixHelper.so # Linked with mkbundle
if [ $PROJECT = "cli" ]; then
    rm $TARGETDIR/bundle/eddie-tray
    rm $TARGETDIR/bundle/libappindicator.so.1
elif [ $PROJECT = "ui" ]; then
    rm $TARGETDIR/bundle/libappindicator.so.1
elif [ $PROJECT = "ui3" ]; then
    rm $TARGETDIR/bundle/eddie-tray
    rm $TARGETDIR/bundle/libappindicator.so.1
else
    echo "Unexpected"
    exit 1
fi


# Create Launcher
echo Step: Launcher

#printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\ncd \$MAINDIR/bundle/\nLD_LIBRARY_PATH=\"\$MAINDIR/bundle/lib\" MONO_PATH=\"\$MAINDIR/bundle/\" ./mono-sgen --config config \"eddie-${PROJECT}.exe\" --path=\"\$MAINDIR\" \"\$@\"\n" > ${TARGETDIR}/eddie-${PROJECT}
printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\ncd \$MAINDIR/bundle/\nLD_LIBRARY_PATH=\"\$MAINDIR/bundle/\" MONO_PATH=\"\$MAINDIR/bundle/\" ./eddie-${PROJECT} --path=\"\$MAINDIR\" \"\$@\"\n" > ${TARGETDIR}/eddie-${PROJECT}

# Owner and Permissions
echo Step: Owner and Permissions

chmod 755 $TARGETDIR/eddie-${PROJECT}

# Build archive
echo Step: Build archive
mkdir -p "$TARGETDIR/tar/eddie-${PROJECT}"
mv "$TARGETDIR/bundle" "$TARGETDIR/tar/eddie-${PROJECT}/bundle"
mv "$TARGETDIR/eddie-${PROJECT}" "$TARGETDIR/tar/eddie-${PROJECT}" 
cd "$TARGETDIR/tar/" 
tar cvfz "$DEPLOYPATH" "eddie-${PROJECT}"

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
#rm -rf $TARGETDIR


