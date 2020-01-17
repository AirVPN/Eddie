
#!/bin/bash

set -e

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
if ! [ -x "$(command -v xbuild)" ]; then
  echo 'Error: xbuild is not installed. Install mono-complete.' >&2
  exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_mono
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_mono.tar.gz 

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
if [ $ARCHCOMPILE = "armhf" ]; then
	ARCHCOMPILE="x64" # Arm pick x64 executable (that are anyway CIL).
fi

${SCRIPTDIR}/../linux_common/compile.sh ${PROJECT}

mkdir -p ${TARGETDIR}
mkdir -p ${TARGETDIR}/bundle

if [ ${PROJECT} = "cli" ]; then
	cp ${SCRIPTDIR}/../../src/App.CLI.Linux/bin/${ARCHCOMPILE}/${CONFIG}/* $TARGETDIR/bundle
	mv $TARGETDIR/bundle/App.CLI.Linux.exe $TARGETDIR/bundle/eddie-cli.exe
elif [ ${PROJECT} = "ui" ]; then
	cp ${SCRIPTDIR}/../../src/App.Forms.Linux/bin/${ARCHCOMPILE}/${CONFIG}/* $TARGETDIR/bundle
	mv $TARGETDIR/bundle/App.Forms.Linux.exe $TARGETDIR/bundle/eddie-ui.exe
elif [ ${PROJECT} = "u3" ]; then
	cp ${SCRIPTDIR}/../../src/UI.GTK.Linux/bin/${ARCHCOMPILE}/${CONFIG}/* $TARGETDIR/bundle
	mv $TARGETDIR/bundle/UI.GTK.Linux.exe $TARGETDIR/bundle/eddie-ui.exe
fi

# Resources
echo Step: Resources
cp ${SCRIPTDIR}/../../deploy/linux_${ARCH}/* $TARGETDIR/bundle/
cp -r ${SCRIPTDIR}/../../common $TARGETDIR/bundle/res

# Cleanup
echo Step: Cleanup
rm -f $TARGETDIR/bundle/*.profile
rm -f $TARGETDIR/bundle/*.pdb
rm -f $TARGETDIR/bundle/*.config
rm -f $TARGETDIR/bundle/mscorlib.dll
rm -f $TARGETDIR/bundle/temp.*
rm -f $TARGETDIR/bundle/mono_crash.*
rm -rf $TARGETDIR/bundle/res/providers

rm $TARGETDIR/bundle/libgdiplus.so.0
rm $TARGETDIR/bundle/libMonoPosixHelper.so
if [ $PROJECT = "cli" ]; then
	rm -rf $TARGETDIR/bundle/res/webui
	rm $TARGETDIR/bundle/eddie-tray
	rm $TARGETDIR/bundle/libappindicator.so.1
elif [ $PROJECT = "ui" ]; then
	rm -rf $TARGETDIR/bundle/res/webui
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

printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\nmono \"\${MAINDIR}/bundle/eddie-${PROJECT}.exe\" --path=\"\${MAINDIR}\" \"\$@\"\n" > $TARGETDIR/eddie-${PROJECT}

# Owner and Permissions
echo Step: Owner and Permissions

chmod 755 -R ${TARGETDIR}
find ${TARGETDIR} -type f -exec chmod 644 {} +;
chmod 755 ${TARGETDIR}/eddie-${PROJECT}
chmod 755 ${TARGETDIR}/bundle/eddie-cli-elevated
chmod 755 ${TARGETDIR}/bundle/openvpn
if test -f "${TARGETDIR}/bundle/hummingbird"; then
    chmod 755 ${TARGETDIR}/bundle/hummingbird
fi
chmod 755 ${TARGETDIR}/bundle/stunnel
if [ $PROJECT = "ui" ]; then
    chmod 755 ${TARGETDIR}/bundle/eddie-tray
fi

# Build archive
echo Step: Build archive
mkdir -p "${TARGETDIR}/tar/eddie-${PROJECT}"
mv "${TARGETDIR}/bundle" "${TARGETDIR}/tar/eddie-${PROJECT}/bundle"
mv "${TARGETDIR}/eddie-${PROJECT}" "${TARGETDIR}/tar/eddie-${PROJECT}" 
cd "${TARGETDIR}/tar/" 
tar cvpfz "${DEPLOYPATH}" "eddie-${PROJECT}"

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
rm -rf ${TARGETDIR}


