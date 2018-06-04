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
	#cp -r ../res $1
	cd $1
	cmake -DCMAKE_BUILD_ARCHITECTURE=$2 -DCMAKE_BUILD_TYPE=$3 -DCMAKE_LIBRARY_OUTPUT_DIRECTORY=$4 "$ROOTDIR"
	make

	cd "$ROOTDIR"
}

BuildConfig ./build/x64/release -m64 Release
strip -S --strip-unneeded -o ../../deploy/linux_x64/eddie-tray ./build/x64/release/eddie_tray

