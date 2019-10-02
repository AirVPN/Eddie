#!/bin/sh

ROOTDIR="$PWD"

# Remove previous configuration
rm -r ./build

BuildConfig() 
{
	#$1 build path
	#$2 CMAKE_BUILD_ARCHITECTURE 32/64
	#$3 CMAKE_BUILD_TYPE Debug/Release
	#$4 CMAKE_LIBRARY_OUTPUT_DIRECTORY

	mkdir -p $1
	cd $1
	/Applications/CMake.app/Contents/bin/cmake -DCMAKE_BUILD_ARCHITECTURE=$2 -DCMAKE_OSX_DEPLOYMENT_TARGET=10.9 -DCMAKE_BUILD_TYPE=$3 -DCMAKE_LIBRARY_OUTPUT_DIRECTORY=$4 "$ROOTDIR"
	make

	cd "$ROOTDIR"
}

#BuildConfig ./build/x64/debug -m64 Debug ../../../../bin/x86/Debug/
#BuildConfig ./build/x86/release -m32 Release ../../../../../deploy/macos_x64/
BuildConfig ./build/x64/release -m64 Release ../../../../../deploy/macos_x64/

