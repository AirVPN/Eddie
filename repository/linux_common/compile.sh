#!/bin/bash

set -e

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

# Check env
if ! [ -x "$(command -v xbuild)" ]; then
  echo 'Error: tar is not installed.' >&2
  exit 1
fi

# Work
PROJECT=$1
CONFIG="Release"

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
TARGETFRAMEWORK="v4.5";
SOLUTIONPATH="";
if [ $PROJECT = "cli" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie2.linux.sln"
elif [ $PROJECT = "ui" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie2.linux.sln"
elif [ $PROJECT = "ui3" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie3.linux.sln"
fi
RULESETPATH="${SCRIPTDIR}/../../src/ruleset/norules.ruleset"

ARCHCOMPILE=${ARCH}
if [ $ARCHCOMPILE = "armhf" ]; then
	ARCHCOMPILE="x64" # Arm pick x64 executable (that are anyway CIL).
fi

# msbuild is recommended, but generally not available (Debian10 for example)
xbuild /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=${ARCHCOMPILE} /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIENET4"


if [ $PROJECT = "cli" ]; then
	${SCRIPTDIR}/../../src/eddie.linux.postbuild.sh ${SCRIPTDIR}/../../src/App.CLI.Linux/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}	
elif [ $PROJECT = "ui" ]; then
	${SCRIPTDIR}/../../src/eddie.linux.postbuild.sh ${SCRIPTDIR}/../../src/App.Forms.Linux/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}	
elif [ $PROJECT = "u3" ]; then
	${SCRIPTDIR}/../../src/eddie.linux.postbuild.sh ${SCRIPTDIR}/../../src/UI.GTK.Linux/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}	
fi
