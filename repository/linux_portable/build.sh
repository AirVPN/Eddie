#!/bin/bash

set -euo pipefail

if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

PROJECT=$1
PROJECTP=$1
if [ ${PROJECTP} = "ui3" ]; then
    PROJECTP="ui"
fi
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR="/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable"
DEPLOYPATH="${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable.tar.gz" 

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
rm -rf $TARGETDIR

# Package dependencies
echo Step: Package dependencies - Build Mono
mkdir -p "${TARGETDIR}"
DEPPACKAGEPATH="${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_mono.tar.gz"
"${SCRIPTDIR}/../linux_mono/build.sh" ${PROJECT}
tar xvfp "${DEPPACKAGEPATH}" -C "${TARGETDIR}"
rm "${TARGETDIR}/eddie-${PROJECTP}/eddie-${PROJECTP}" # old launcher not need

echo Step: Build portable

if [ $ARCH = "armv7l" ]; then
	MKBUNDLECROSSTARGET="mono-6.0.0-raspbian-9-arm"
elif [ $ARCH = "aarch64" ]; then
	MKBUNDLECROSSTARGET="mono-6.6.0-debian-10-arm64"
else
	#MKBUNDLECROSSTARGET="mono-5.10.0-debian-7-${ARCH}"
    MKBUNDLECROSSTARGET="mono-6.8.0-debian-10-${ARCH}"
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


mv ${TARGETDIR}/eddie-${PROJECTP}/bundle ${TARGETDIR}/bundle
rm -rf ${TARGETDIR}/eddie-${PROJECTP}
cd ${TARGETDIR}/bundle

# Copy bin
cp ${SCRIPTDIR}/../../deploy/linux_${ARCH}/* $TARGETDIR/bundle/

# Update config
cp ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/etc/mono/config ${SCRIPTDIR}/mkbundle.config
sed -i 's/\$mono_libdir\///g' ${SCRIPTDIR}/mkbundle.config

echo mkbundle run
# Remember: libMonoPosixHelper.so and libmono-native.so are required for System.IO.File operations
if [ $PROJECT = "cli" ]; then
    if [ $ARCH = "armv7l" ]; then
        mkbundle eddie-${PROJECTP}.exe -o eddie-${PROJECTP} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so
    else
        mkbundle eddie-${PROJECTP}.exe -o eddie-${PROJECTP} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so --library /usr/lib/libmono-native.so
    fi
elif [ $PROJECT = "ui" ]; then
    if [ $ARCH = "armv7l" ]; then
        mkbundle eddie-${PROJECTP}.exe -o eddie-${PROJECTP} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so --library ${TARGETDIR}/bundle/libgdiplus.so.0
    else
        mkbundle eddie-${PROJECTP}.exe -o eddie-${PROJECTP} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so --library ${TARGETDIR}/bundle/libgdiplus.so.0 --library /usr/lib/libmono-native.so
    fi
elif [ $PROJECT = "ui3" ]; then
	mkbundle eddie-${PROJECTP}.exe -o eddie-${PROJECTP} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${TARGETDIR}/bundle/libMonoPosixHelper.so --library /usr/lib/libmono-native.so
fi

# Remove unneed
rm ${TARGETDIR}/bundle/*.exe # Linked with mkbundle
rm ${TARGETDIR}/bundle/*.dll # Linked with mkbundle
rm ${TARGETDIR}/bundle/*.pdb
rm ${TARGETDIR}/bundle/*.config
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

#printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\ncd \$MAINDIR/bundle/\nLD_LIBRARY_PATH=\"\$MAINDIR/bundle/lib\" MONO_PATH=\"\$MAINDIR/bundle/\" ./mono-sgen --config config \"eddie-${PROJECTP}.exe\" --path=\"\$MAINDIR\" \"\$@\"\n" > ${TARGETDIR}/eddie-${PROJECTP}
printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\ncd \$MAINDIR/bundle/\nLD_LIBRARY_PATH=\"\$MAINDIR/bundle/\" MONO_PATH=\"\$MAINDIR/bundle/\" ./eddie-${PROJECTP} --path=\"\$MAINDIR\" \"\$@\"\n" > ${TARGETDIR}/eddie-${PROJECTP}

# Owner and Permissions
echo Step: Owner and Permissions

chmod 755 "$TARGETDIR/eddie-${PROJECTP}"

# Build archive
echo Step: Build archive
mkdir -p "$TARGETDIR/tar/eddie-${PROJECTP}"
mv "$TARGETDIR/bundle" "$TARGETDIR/tar/eddie-${PROJECTP}/bundle"
mv "$TARGETDIR/eddie-${PROJECTP}" "$TARGETDIR/tar/eddie-${PROJECTP}" 
cd "$TARGETDIR/tar/" 
tar cvfz "$DEPLOYPATH" "eddie-${PROJECTP}"

# Deploy to eddie.website
"${SCRIPTDIR}/../linux_common/deploy.sh" "${DEPLOYPATH}"

# Cleanup
echo Step: Final cleanup
rm -rf "${TARGETDIR}"


