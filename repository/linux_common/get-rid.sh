#!/bin/bash

set -euo pipefail

ARCHOS=$(uname -m)
if [ $ARCHOS = "i686" ]; then
	echo "linux-x86" # Not exists in dotnet
elif [ $ARCHOS = "x86_64" ]; then
	echo "linux-x64"
elif [ $ARCHOS = "aarch64" ]; then
	echo "linux-arm64" # Not sure
elif [ $ARCHOS = "armv7l" ]; then
	echo "linux-arm" # Not sure
else
	echo "Unknown RID -$ARCHOS-"
	exit 1
fi

exit 0

