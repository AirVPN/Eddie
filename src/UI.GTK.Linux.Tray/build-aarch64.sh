#!/bin/sh

ROOTDIR="$PWD"

# Remove previous configuration
rm -r ./bin

BuildConfig()
{
	#$1 build path
	#$2 CMAKE_BUILD_ARCHITECTURE 32/64
	#$3 CMAKE_BUILD_TYPE Debug/Release
	#$4 CMAKE_LIBRARY_OUTPUT_DIRECTORY

	mkdir -p $1
	cd $1
	cmake -DCMAKE_BUILD_TYPE=$2 -DCMAKE_LIBRARY_OUTPUT_DIRECTORY=$3 "$ROOTDIR"
	make

	cd "$ROOTDIR"
}

BuildConfig ./build/aarch64/release Release
strip -S --strip-unneeded -o ../../deploy/linux_aarch64/eddie-tray ./build/aarch64/release/eddie_tray


