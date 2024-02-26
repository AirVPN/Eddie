#!/bin/bash

set -euo pipefail

# Check args
if [ "${1-}" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

if [ "${2-}" == "" ]; then
	echo Second arg must be framework: net4, net7
	exit 1
fi

if [ "${3-}" == "" ]; then
	echo Third arg must be distro: opensuse, fedora
	exit 1
fi

# Check env
if ! [ -x "$(command -v rpmbuild)" ]; then
  echo 'Error: rpmbuild is not installed. In Debian, apt-get install rpm' >&2
  exit 1
fi

PROJECT=$1
FRAMEWORK=$2
DISTRO=$3
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_${DISTRO}
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_${DISTRO}.rpm
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_${DISTRO}.rpm

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
sudo rm -rf $TARGETDIR

# Package dependencies
echo Step: Package dependencies - Build Portable
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable.tar.gz  
${SCRIPTDIR}/../linux_portable/build.sh ${PROJECT} ${FRAMEWORK}
tar xvfp "${DEPPACKAGEPATH}" -C "${TARGETDIR}"
rm -rf ${TARGETDIR}/eddie-${PROJECT}/portable.txt

echo Step: Build RPM Structure

mkdir -p ${TARGETDIR}/usr/lib;
mv ${TARGETDIR}/eddie-${PROJECT} ${TARGETDIR}/usr/lib/eddie-${PROJECT}
rm -rf ${TARGETDIR}/eddie-${PROJECT}

mkdir -p ${TARGETDIR}/usr/share
mv ${TARGETDIR}/usr/lib/eddie-${PROJECT}/res ${TARGETDIR}/usr/share/eddie-${PROJECT}

# Resources
cp -r ${SCRIPTDIR}/bundle/eddie-${PROJECT}/* ${TARGETDIR}

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
# Note: don't remove libayatana

# Owner and Permissions
echo Step: Owner and Permissions

#sudo chown -R root:root ${TARGETDIR}/usr

chmod 0755 $TARGETDIR/usr
chmod 0755 $TARGETDIR/usr/bin
chmod 0755 $TARGETDIR/usr/lib
chmod 0755 $TARGETDIR/usr/share
chmod 0755 $TARGETDIR/usr/share/doc
chmod 0755 $TARGETDIR/usr/share/doc/eddie-${PROJECT}
chmod 0644 $TARGETDIR/usr/share/doc/eddie-${PROJECT}/copyright
chmod 0644 $TARGETDIR/usr/share/doc/eddie-${PROJECT}/changelog.gz
chmod 0755 $TARGETDIR/usr/share/man
chmod 0755 $TARGETDIR/usr/share/man/man8
chmod 0644 $TARGETDIR/usr/share/man/man8/eddie-${PROJECT}.8.gz
chmod 0755 $TARGETDIR/usr/share/pixmaps
chmod 0644 $TARGETDIR/usr/share/pixmaps/eddie-${PROJECT}.png
chmod 0755 $TARGETDIR/usr/share/polkit-1
chmod 0755 $TARGETDIR/usr/share/polkit-1/actions
chmod 0644 $TARGETDIR/usr/share/polkit-1/actions/org.airvpn.eddie.${PROJECT}.elevated.policy
#chmod 0755 $TARGETDIR/usr/share/lintian
#chmod 0755 $TARGETDIR/usr/share/lintian/overrides
#chmod 0644 $TARGETDIR/usr/share/lintian/overrides/eddie-${PROJECT}

if [ $PROJECT = "cli" ]; then 
    chmod 0755 $TARGETDIR/usr/bin/eddie-cli
elif [ $PROJECT = "ui" ]; then 
    chmod 0755 $TARGETDIR/usr/share/applications
    chmod 0644 $TARGETDIR/usr/share/applications/eddie-ui.desktop
    chmod 0755 $TARGETDIR/usr/bin/eddie-ui    
fi

# RPM spec file

cp "${SCRIPTDIR}/rpmbuild-eddie-${PROJECT}.spec" "${TARGETDIR}/../rpmbuild.spec"

sed -i "s/{@version}/${VERSION}/g" ${TARGETDIR}/../rpmbuild.spec

REQUIRES=""

# Old: mono-core (all)
# Old: mono-winforms libgdiplus-devel (ui)

if [ $PROJECT = "cli" ]; then 
    REQUIRES="${REQUIRES} openvpn stunnel polkit"
elif [ $PROJECT = "ui" ]; then
    if [ $PROJECT = "net7" ]; then
        REQUIRES="${REQUIRES} openvpn stunnel polkit libnotify"
    elif [ $PROJECT = "net4" ]; then 
        REQUIRES="${REQUIRES} openvpn stunnel polkit libnotify"
    fi
fi

if [ $DISTRO = "opensuse" ]; then
    REQUIRES="${REQUIRES} libcurl4 libsecret-tools"
elif [ $DISTRO = "fedora" ]; then
    REQUIRES="${REQUIRES} libcurl libsecret"
fi

sed -i "s/{@requires}/${REQUIRES}/g" ${TARGETDIR}/../rpmbuild.spec

sed -i "s/{@files}//g" ${TARGETDIR}/../rpmbuild.spec
find ${TARGETDIR} -type f | sed "s|$TARGETDIR||g" | sed "s/^/\"\//g" | sed "s/$/\"/g" >>${TARGETDIR}/../rpmbuild.spec
cat ${TARGETDIR}/../rpmbuild.spec

# RPM Build
cd ${TARGETDIR} # Unable to specify an output path
rpmbuild -bb "${TARGETDIR}/../rpmbuild.spec" --buildroot "${TARGETDIR}" 

cd ..
ARCHRPM=${ARCH}
if [ ${ARCH} = "armv7l" ]; then
    ARCHRPM="armv7hnl"
elif [ ${ARCH} = "x86" ]; then
    ARCHRPM="i386"
elif [ ${ARCH} = "x64" ]; then
    ARCHRPM="x86_64"
fi

# Signing
if test -f "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase"; then # Staff AirVPN
    echo if requested, enter $(cat "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase") as passphrase
    echo -e "%_signature gpg\n%_gpg_name Eddie <maintainer@eddie.website>\n%__gpg /usr/bin/gpg\n" >~/.rpmmacros
    export GPG_TTY=$(tty) # Fix for gpg: signing failed: Inappropriate ioctl for device
    rpmsign --addsign eddie-${PROJECT}-${VERSION}-0.${ARCHRPM}.rpm
fi

# Final move
mv eddie-${PROJECT}-${VERSION}-0.${ARCHRPM}.rpm ${FINALPATH}

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${FINALPATH}

# End
mv ${FINALPATH} ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
rm -rf $TARGETDIR
echo Build linux_${DISTRO} complete

