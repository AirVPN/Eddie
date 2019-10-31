#!/bin/bash

set -e

ARCHOS=$(uname -m)
if [ $ARCHOS = "i686" ]; then
	echo "x86"
elif [ $ARCHOS = "x86_64" ]; then
	echo "x64"
elif [ $ARCHOS = "armv7l" ]; then # rPI 32
	echo "armhf"
elif [ $ARCHOS = "aarch64" ]; then # rPI 64
    echo "armhf"
else
	echo $ARCHOS
fi

exit 0

