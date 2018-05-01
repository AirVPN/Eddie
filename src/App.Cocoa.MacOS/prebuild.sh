# Eddie 2.14.0 - deprecated. Can be removed from csproj custom build actions.
exit 0

echo Eddie - Ensure directory structure
mkdir -p bin/Debug/Eddie.app/Contents/MacOS/
mkdir -p bin/Release/Eddie.app/Contents/MacOS/
mkdir -p bin/Debug/Eddie.app/Contents/Resources/
mkdir -p bin/Release/Eddie.app/Contents/Resources/
echo Eddie - Copy deploy files, need only for direct debugging
cp -r ../../common/* bin/Debug/Eddie.app/Contents/Resources/
cp -r ../../common/* bin/Release/Eddie.app/Contents/Resources/
cp ../../deploy/macos_x64/* bin/Debug/Eddie.App/Contents/MacOS/
cp ../../deploy/macos_x64/* bin/Release/Eddie.App/Contents/MacOS/

echo Eddie - prebuild.sh done.
exit 0

