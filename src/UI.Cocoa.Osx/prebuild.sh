echo Eddie - Ensure directory structure
mkdir -p bin/Debug/Eddie.app/Contents/MacOS/
mkdir -p bin/Release/Eddie.app/Contents/MacOS/
echo Eddie - Remove *.pkg in release folder
rm -f bin/Release/*.pkg
echo Eddie - prebuild.sh done.
exit 0

