#!/bin/bash

set -euo pipefail

if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

if ! [ -x "$(command -v dotnet)" ]; then
  echo 'Error: dotnet is not installed.' >&2
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

# Compile

SOLUTIONPATH="";
if [ $PROJECT = "cli" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie.linux.cli.net6.sln"
elif [ $PROJECT = "ui" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie2.linux.ui.net6.sln"
elif [ $PROJECT = "ui3" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie3.linux.ui.net6.sln"
fi

dotnet publish ${SOLUTIONPATH} --configuration ${CONFIG} --runtime linux-x64 --self-contained true -p:DefineConstants="EDDIENET6"

# ?

mv ${TARGETDIR}/eddie-${PROJECTP}/bundle ${TARGETDIR}/bundle
rm -rf ${TARGETDIR}/eddie-${PROJECTP}
cd ${TARGETDIR}/bundle

# Copy bin
cp ${SCRIPTDIR}/../../deploy/linux_${ARCH}/* $TARGETDIR/bundle/


# Create Launcher
echo Step: Launcher

# LD_LIBRARY_PATH removed in 2.21.7, issue with OpenSUSE
#printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\ncd \$MAINDIR/bundle/\nLD_LIBRARY_PATH=\"\$MAINDIR/bundle/\" MONO_PATH=\"\$MAINDIR/bundle/\" ./eddie-${PROJECTP} --path=\"\$MAINDIR\" \"\$@\"\n" > ${TARGETDIR}/eddie-${PROJECTP}
printf "#!/bin/sh\nMAINDIR=\"\$(dirname \"\$(readlink -f \"\$0\")\")/\"\ncd \$MAINDIR/bundle/\nMONO_PATH=\"\$MAINDIR/bundle/\" ./eddie-${PROJECTP} --path=\"\$MAINDIR\" \"\$@\"\n" > ${TARGETDIR}/eddie-${PROJECTP}

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


