#!/bin/bash

set -e

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

# Check env
if ! [ -x "$(command -v rpmbuild)" ]; then
  echo 'Error: rpmbuild is not installed. In Debian, apt-get install rpm' >&2
  exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_opensuse
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_opensuse.rpm
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_opensuse.rpm

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

echo Step: Build RPM Structure

LIBPATH="lib"; # TOCLEAN
if [ ${ARCH} = "x64" ]; then
    LIBPATH="lib64"
fi

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
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/openvpn
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/liblzo*
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/libcrypto*
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/libssl*
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/libpkcs*
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/hummingbird
rm -f ${TARGETDIR}/usr/lib/eddie-${PROJECT}/stunnel
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



# RPM spec file

cp "${SCRIPTDIR}/rpmbuild-eddie-${PROJECT}.spec" "${TARGETDIR}/../rpmbuild.spec"

sed -i "s/{@version}/${VERSION}/g" ${TARGETDIR}/../rpmbuild.spec

REQUIRES="mono-core sudo openvpn stunnel curl libsecret-tools" # Diff between OpenSuse and Fedora
if [ $PROJECT = "cli" ]; then
    REQUIRES="${REQUIRES}"
elif [ $PROJECT = "ui" ]; then
    REQUIRES="${REQUIRES} mono-winforms libgdiplus-devel libnotify libappindicator1" # Diff between OpenSuse and Fedora
elif [ $PROJECT = "ui3" ]; then
    REQUIRES="${REQUIRES} gtk-sharp3 libnotify libappindicator1" # Diff between OpenSuse and Fedora
fi
sed -i "s/{@requires}/${REQUIRES}/g" ${TARGETDIR}/../rpmbuild.spec

sed -i "s/{@files}//g" ${TARGETDIR}/../rpmbuild.spec
find ${TARGETDIR} -type f | sed "s|$TARGETDIR||g" | sed "s/^/\"\//g" | sed "s/$/\"/g" >>${TARGETDIR}/../rpmbuild.spec
cat ${TARGETDIR}/../rpmbuild.spec

# RPM Build
cd ${TARGETDIR} # Unable to specify an output path
if test -f "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase"; then # Staff AirVPN
    echo if requested, enter $(cat "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase") as passphrase
    echo -e "%_signature gpg\n%_gpg_name Eddie <maintainer@eddie.website>\n%__gpg /usr/bin/gpg\n" >~/.rpmmacros
    export GPG_TTY=$(tty) # Fix for gpg: signing failed: Inappropriate ioctl for device
    rpmbuild -bb "${TARGETDIR}/../rpmbuild.spec" --buildroot "${TARGETDIR}" -sign
else
    # clodotemp rimuovere
    echo "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase" not found, unexpected, temp 
    exit 1 # clodotemp rimuovere

    rpmbuild -bb "${TARGETDIR}/../rpmbuild.spec" --buildroot "${TARGETDIR}" 
    #--define "_rpmdir ${TARGETDIR}/../"
fi
cd ..
ARCHRPM=${ARCH}
if [ ${ARCH} = "armv7l" ]; then
    ARCHRPM="armv7hnl"
elif [ ${ARCH} = "x86" ]; then
    ARCHRPM="i386"
elif [ ${ARCH} = "x64" ]; then
    ARCHRPM="x86_64"
fi
mv eddie-${PROJECT}-${VERSION}-0.${ARCHRPM}.rpm ${FINALPATH}

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${FINALPATH}

# End
mv ${FINALPATH} ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
rm -rf $TARGETDIR
echo Build linux_opensuse complete

