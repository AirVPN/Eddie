echo Eddie - Ensure directory structure
mkdir -p bin/Debug/Eddie.app/Contents/MacOS/
mkdir -p bin/Release/Eddie.app/Contents/MacOS/
echo Eddie - Copy deploy files, need only for direct debugging
cp ../../common/* bin/Debug/Eddie.app/Contents/MacOS/
cp ../../common/* bin/Release/Eddie.app/Contents/MacOS/
cp ../../deploy/macos_x64/* bin/Debug/Eddie.App/Contents/MacOS/
cp ../../deploy/macos_x64/* bin/Release/Eddie.App/Contents/MacOS/
echo Eddie - prebuild.sh done.
exit 0

