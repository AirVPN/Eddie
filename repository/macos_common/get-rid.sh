#!/bin/bash

# Return current runtime identifier

set -euo pipefail

ARCHOS=$(uname -m)
if [ $ARCHOS = "arm64" ]; then
	echo "osx-arm64"
elif [ $ARCHOS = "x86_64" ]; then
	echo "osx-x64"
else
	echo "Unknown RID for -$ARCHOS-"
	exit 1;
fi

exit 0

