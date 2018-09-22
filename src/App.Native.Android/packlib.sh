#!/bin/sh

# pack native library [ProMIND]

OUTPUTFILE=EddieNativeLibrary.7z

if [[ -f $OUTPUTFILE ]]; then
    rm $OUTPUTFILE
fi

7z a -xr'!.*' $OUTPUTFILE native_library

echo "Done"
