#!/bin/sh

ROOTDIR="$PWD"

# Remove previous configuration
rm -r ./build

BuildConfig() 
{
 #$1 build path
 #$2 CMAKE_BUILD_TYPE Debug/Release
 #$3 CMAKE_LIBRARY_OUTPUT_DIRECTORY

 mkdir -p $1
 cd $1
 cmake -DCMAKE_BUILD_TYPE=$2 -DCMAKE_LIBRARY_OUTPUT_DIRECTORY=$3 "$ROOTDIR"
 make

 cd "$ROOTDIR"
}

#BuildConfig ./build/x64/debug Debug ../../../../bin/x86/Debug/
BuildConfig ./build/x64/release Release ../../../../../deploy/linux_armhf/
strip -S --strip-unneeded -o ../../deploy/linux_armhf/libLib.Platform.Linux.Native.so ../../deploy/linux_armhf/libLib.Platform.Linux.Native.so