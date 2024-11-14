#!/bin/bash

# Note 2.24.0, 2023-12-01
# The .deb and .rpm UI editions now include eddie-cli executable and all related files.
# It might be more appropriate (for package manager) for eddie-ui to not include eddie-cli, resources, etc., and to only have a package dependency on eddie-cli.
# Since the majority of our users prefer the UI edition, they would also need to manually download the eddie-cli package (if they download and install manually .deb files, for example with dpkg -i), which we aim to prevent.

set -euo pipefail

# Check args
if [ "${1-}" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

if [ "${2-}" == "" ]; then
	echo Second arg must be framework: net4, net8
	exit 1
fi

# Check env
if ! [ -x "$(command -v lintian)" ]; then
  echo 'Error: lintian is not installed.' >&2
  exit 1
fi

PROJECT=$1
FRAMEWORK=$2
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_debian
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_debian.deb
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_debian.deb

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
sudo rm -rf $TARGETDIR # root requested by dpkg

# Package dependencies
echo Step: Package dependencies - Build Portable
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable.tar.gz  
${SCRIPTDIR}/../linux_portable/build.sh ${PROJECT} ${FRAMEWORK}
tar xvfp "${DEPPACKAGEPATH}" -C "${TARGETDIR}"
rm -rf ${TARGETDIR}/eddie-${PROJECT}/portable.txt

echo Step: Build Debian Structure

mkdir -p ${TARGETDIR}/usr/lib;
mv ${TARGETDIR}/eddie-${PROJECT} ${TARGETDIR}/usr/lib/eddie-${PROJECT}
rm -rf ${TARGETDIR}/eddie-${PROJECT}

mkdir -p ${TARGETDIR}/usr/share
mv ${TARGETDIR}/usr/lib/eddie-${PROJECT}/res ${TARGETDIR}/usr/share/eddie-${PROJECT}

# Resources
cp -r ${SCRIPTDIR}/bundle/eddie-${PROJECT}/* ${TARGETDIR}
if [ $PROJECT = "ui" ]; then
    if [[ ${VERSION} =~ ^2 ]]; then
        cp -r ${SCRIPTDIR}/bundle/eddie-ui2/* ${TARGETDIR}
    fi
fi

mkdir -p $TARGETDIR/usr/share/doc/eddie-${PROJECT}
curl "https://eddie.website/changelog/?software=client&format=debian&hidden=yes" -o $TARGETDIR/usr/share/doc/eddie-${PROJECT}/changelog
gzip -n -9 $TARGETDIR/usr/share/doc/eddie-${PROJECT}/changelog

# Auto-call to make man
mkdir -p $TARGETDIR/usr/share/man/man8
$TARGETDIR/usr/lib/eddie-${PROJECT}/eddie-cli --path.resources="${TARGETDIR}/usr/share/eddie-${PROJECT}" --help --help.format=man >$TARGETDIR/usr/share/man/man8/eddie-${PROJECT}.8
gzip -n -9 $TARGETDIR/usr/share/man/man8/eddie-${PROJECT}.8

# Remove unneed
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/openvpn
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/liblzo*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/liblz4*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libcap-ng*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libnl*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libcrypto*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libssl*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libpkcs*
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/hummingbird
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/stunnel
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libgdiplus.so.0
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libMonoPosixHelper.so
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libdbusmenu-glib.so.4
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libdbusmenu-gtk3.so.4

rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libayatana-appindicator3.so.1
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libayatana-indicator3.so.7
rm -f "${TARGETDIR}"/usr/lib/eddie-${PROJECT}/libayatana-ido3-0.4.so.0

# Owner and Permissions
echo Step: Owner and Permissions

sudo chown -R root:root ${TARGETDIR}/usr

sudo chmod 0755 $TARGETDIR/usr
sudo chmod 0755 $TARGETDIR/usr/bin
sudo chmod 0755 $TARGETDIR/usr/lib
sudo chmod 0755 $TARGETDIR/usr/share
sudo chmod 0755 $TARGETDIR/usr/share/doc
sudo chmod 0755 $TARGETDIR/usr/share/doc/eddie-${PROJECT}
sudo chmod 0644 $TARGETDIR/usr/share/doc/eddie-${PROJECT}/copyright
sudo chmod 0644 $TARGETDIR/usr/share/doc/eddie-${PROJECT}/changelog.gz
sudo chmod 0755 $TARGETDIR/usr/share/man
sudo chmod 0755 $TARGETDIR/usr/share/man/man8
sudo chmod 0644 $TARGETDIR/usr/share/man/man8/eddie-${PROJECT}.8.gz
sudo chmod 0755 $TARGETDIR/usr/share/pixmaps
sudo chmod 0644 $TARGETDIR/usr/share/pixmaps/eddie-${PROJECT}.png
sudo chmod 0755 $TARGETDIR/usr/share/polkit-1
sudo chmod 0755 $TARGETDIR/usr/share/polkit-1/actions
sudo chmod 0644 $TARGETDIR/usr/share/polkit-1/actions/org.airvpn.eddie.${PROJECT}.elevated.policy
sudo chmod 0755 $TARGETDIR/usr/share/lintian
sudo chmod 0755 $TARGETDIR/usr/share/lintian/overrides
sudo chmod 0644 $TARGETDIR/usr/share/lintian/overrides/eddie-${PROJECT}

if [ $PROJECT = "cli" ]; then 
    sudo chmod 0755 $TARGETDIR/usr/bin/eddie-cli
elif [ $PROJECT = "ui" ]; then 
    sudo chmod 0755 $TARGETDIR/usr/share/applications
    sudo chmod 0644 $TARGETDIR/usr/share/applications/eddie-ui.desktop
    sudo chmod 0755 $TARGETDIR/usr/bin/eddie-ui    
fi

# Debian control file
sed -i "s/{@version}/${VERSION}/g" ${TARGETDIR}/DEBIAN/control
if [ ${ARCH} = "x86" ]; then
    DEBARCH="i386" 
elif [ ${ARCH} = "x64" ]; then
    DEBARCH="amd64"
elif [ ${ARCH} = "armv7l" ]; then
    DEBARCH="armhf"
elif [ ${ARCH} = "aarch64" ]; then
    DEBARCH="arm64"
else
    echo "Unknown debian arch";
    exit 1;
fi
sed -i "s/{@architecture}/${DEBARCH}/g" ${TARGETDIR}/DEBIAN/control

# dpkg
echo Step: dpkg
dpkg -b ${TARGETDIR} ${FINALPATH}

echo Step: Lintian
lintian "${FINALPATH}"

# Sign
if test -f "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase"; then # Staff AirVPN
    echo Step: Signing
    PASSPHRASE=$(cat ${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase)
    export GPG_TTY=$(tty) # Fix for gpg: signing failed: Inappropriate ioctl for device
    dpkg-sig -m "maintainer@eddie.website" -g "--no-tty --passphrase ${PASSPHRASE}" --sign builder ${FINALPATH}
    dpkg-sig --verify ${FINALPATH}
fi

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${FINALPATH}

# End
mv ${FINALPATH} ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
sudo rm -rf $TARGETDIR

echo Build linux_debian complete




