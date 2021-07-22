
#!/bin/bash

set -euo pipefail

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

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_mono
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_mono.tar.gz 

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
mkdir -p ${TARGETDIR}/bundle

if [ ${PROJECT} = "cli" ]; then
    rm -rf ${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/App.CLI.macOS.app
	cp ${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/* $TARGETDIR/bundle
    mv $TARGETDIR/bundle/App.CLI.MacOS.exe $TARGETDIR/bundle/eddie-cli.exe
elif [ ${PROJECT} = "ui" ]; then
	echo "Unsupported"
    exit 1
elif [ ${PROJECT} = "ui3" ]; then
	echo "Unsupported"
    exit 1
fi

# Resources
echo Step: Resources
cp ${SCRIPTDIR}/../../deploy/macos_${ARCH}/* $TARGETDIR/bundle/
cp -r ${SCRIPTDIR}/../../common $TARGETDIR/bundle/res

# Cleanup
echo Step: Cleanup
rm -f $TARGETDIR/bundle/*.profile
rm -f $TARGETDIR/bundle/*.pdb
rm -f $TARGETDIR/bundle/*.config
rm -f $TARGETDIR/bundle/mscorlib.dll
rm -f $TARGETDIR/bundle/temp.*
rm -f $TARGETDIR/bundle/mono_crash.*
rm -f $TARGETDIR/Eddie.App/Contents/MacOS/Recovery.xml

if [ $PROJECT = "cli" ]; then
	rm -rf $TARGETDIR/bundle/res/webui
elif [ $PROJECT = "ui" ]; then
	rm -rf $TARGETDIR/bundle/res/webui
elif [ $PROJECT = "ui3" ]; then
    rm -rf $TARGETDIR/bundle/res/nothing
else
	echo "Unexpected"
	exit 1
fi

# Create Launcher
echo Step: Launcher

printf "#!/bin/sh\nrealpath() {\n    [[ \$1 = /* ]] && echo \"\$1\" || echo \"\$PWD/\${1#./}\"\n}\nMAINDIR=\$(dirname \$(realpath \"\$0\"))\nmono \"\${MAINDIR}/bundle/eddie-${PROJECT}.exe\" --path=\"\${MAINDIR}\" \"\$@\"\n" > $TARGETDIR/eddie-${PROJECT}

# Owner and Permissions
echo Step: Owner and Permissions

chmod -R 755 ${TARGETDIR}
find ${TARGETDIR} -type f -exec chmod 644 {} +;
chmod 755 ${TARGETDIR}/eddie-${PROJECT}
chmod 755 ${TARGETDIR}/bundle/eddie-cli-elevated
chmod 755 ${TARGETDIR}/bundle/openvpn
chmod 755 ${TARGETDIR}/bundle/hummingbird
chmod 755 ${TARGETDIR}/bundle/stunnel

# Build archive
echo Step: Build archive
mkdir -p "${TARGETDIR}/tar/eddie-${PROJECT}"
mv "${TARGETDIR}/bundle" "${TARGETDIR}/tar/eddie-${PROJECT}/bundle"
mv "${TARGETDIR}/eddie-${PROJECT}" "${TARGETDIR}/tar/eddie-${PROJECT}" 
cd "${TARGETDIR}/tar/" 
tar cvpfz "${DEPLOYPATH}" "eddie-${PROJECT}"

# Deploy to eddie.website
${SCRIPTDIR}/../macos_common/deploy.sh ${DEPLOYPATH}

# Cleanup
echo Step: Final cleanup
rm -rf ${TARGETDIR}


