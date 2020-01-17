#!/bin/bash

set -e

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

# Check env
if ! [ -x "$(command -v lintian)" ]; then
  echo 'Error: lintian is not installed.' >&2
  exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_debian
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_debian.deb

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
sudo rm -rf $TARGETDIR # root requested by dpkg

# Package dependencies
echo Step: Package dependencies - Build Mono
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_mono.tar.gz  
${SCRIPTDIR}/../linux_mono/build.sh ${PROJECT}
tar xvfp "${DEPPACKAGEPATH}" -C "${TARGETDIR}"
rm ${TARGETDIR}/eddie-${PROJECT}/eddie-${PROJECT} # old launcher not need

echo Step: Build Debian Structure

mkdir -p ${TARGETDIR}/usr/lib;
mv ${TARGETDIR}/eddie-${PROJECT}/bundle ${TARGETDIR}/usr/lib/eddie-${PROJECT}
rm -rf ${TARGETDIR}/eddie-${PROJECT}

mkdir -p ${TARGETDIR}/usr/share
mv ${TARGETDIR}/usr/lib/eddie-${PROJECT}/res ${TARGETDIR}/usr/share/eddie-${PROJECT}

# Copy bin
cp ${SCRIPTDIR}/../../deploy/linux_${ARCH}/* $TARGETDIR/usr/lib/eddie-${PROJECT}

# Resources
cp -r ${SCRIPTDIR}/bundle/eddie-${PROJECT}/* ${TARGETDIR}

# Add changelog
mkdir -p $TARGETDIR/usr/share/doc/eddie-${PROJECT}
curl "https://eddie.website/changelog/?software=client&format=debian&hidden=yes" -o $TARGETDIR/usr/share/doc/eddie-${PROJECT}/changelog
gzip -n -9 $TARGETDIR/usr/share/doc/eddie-${PROJECT}/changelog

# Auto-call to make man
mkdir -p $TARGETDIR/usr/share/man/man8
mono $TARGETDIR/usr/lib/eddie-${PROJECT}/eddie-${PROJECT}.exe --cli --path.resources="${TARGETDIR}/usr/share/eddie-${PROJECT}" --help --help.format=man >$TARGETDIR/usr/share/man/man8/eddie-${PROJECT}.8
gzip -n -9 $TARGETDIR/usr/share/man/man8/eddie-${PROJECT}.8

# Remove unneed
rm ${TARGETDIR}/usr/lib/eddie-${PROJECT}/openvpn
if test -f "${TARGETDIR}/usr/lib/eddie-${PROJECT}/hummingbird"; then
    rm ${TARGETDIR}/usr/lib/eddie-${PROJECT}/hummingbird
fi
rm ${TARGETDIR}/usr/lib/eddie-${PROJECT}/stunnel
rm ${TARGETDIR}/usr/lib/eddie-${PROJECT}/libgdiplus.so.0
rm ${TARGETDIR}/usr/lib/eddie-${PROJECT}/libMonoPosixHelper.so
if [ $PROJECT = "cli" ]; then
    rm $TARGETDIR/usr/lib/eddie-${PROJECT}/eddie-tray
    rm $TARGETDIR/usr/lib/eddie-${PROJECT}/libappindicator.so.1
elif [ $PROJECT = "ui" ]; then
    rm $TARGETDIR/usr/lib/eddie-${PROJECT}/libappindicator.so.1
elif [ $PROJECT = "ui3" ]; then
    rm $TARGETDIR/usr/lib/eddie-${PROJECT}/eddie-tray
    rm $TARGETDIR/usr/lib/eddie-${PROJECT}/libappindicator.so.1
else
    echo "Unexpected"
    exit 1
fi

# Owner and Permissions
echo Step: Owner and Permissions
chmod +x ${TARGETDIR}/usr/bin/eddie-${PROJECT}

# Permissions

sudo chown -R root:root ${TARGETDIR}/usr

# Debian control file
sed -i "s/{@version}/${VERSION}/g" ${TARGETDIR}/DEBIAN/control
if [ ${ARCH} = "x86" ]; then
    DEBARCH="i386" 
elif [ ${ARCH} = "x64" ]; then
    DEBARCH="amd64"
elif [ ${ARCH} = "armhf" ]; then
    DEBARCH="armhf"
else
    echo "Unknown debian arch";
    exit 1;
fi
sed -i "s/{@architecture}/${DEBARCH}/g" ${TARGETDIR}/DEBIAN/control

# dpkg
dpkg -b ${TARGETDIR} ${DEPLOYPATH}

lintian "${DEPLOYPATH}"

# Sign
if test -f "${SCRIPTDIR}/../signing/gpg.passphrase"; then # Staff AirVPN
    PASSPHRASE=$(cat ${SCRIPTDIR}/../signing/gpg.passphrase)
    export GPG_TTY=$(tty) # Fix for gpg: signing failed: Inappropriate ioctl for device
    dpkg-sig -m "maintainer@eddie.website" -g "--no-tty --passphrase ${PASSPHRASE}" --sign builder ${DEPLOYPATH}
    dpkg-sig --verify ${DEPLOYPATH}
fi

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
sudo rm -rf $TARGETDIR

echo Build linux_debian complete




