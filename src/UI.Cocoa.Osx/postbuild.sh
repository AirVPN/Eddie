echo Eddie - Ensure provider directories
mkdir -p bin/Debug/Eddie.app/Contents/MacOS/Providers/
mkdir -p bin/Release/Eddie.app/Contents/MacOS/Providers/

echo Eddie - Copy providers *.xml
cp ../../Providers/*.xml bin/Debug/Eddie.app/Contents/MacOS/Providers/
cp ../../Providers/*.xml bin/Release/Eddie.app/Contents/MacOS/Providers/

echo Eddie - Update version in plist
sed "s/{@version}/$(cat ../../version.txt)/" ../../resources/macos/Info.plist > bin/Debug/Eddie.app/Contents/Info.plist
sed "s/{@version}/$(cat ../../version.txt)/" ../../resources/macos/Info.plist > bin/Release/Eddie.app/Contents/Info.plist

echo Eddie - Copy OpenVPN from deploy to release
chmod 755 ../../deploy/macos_x64/openvpn
cp ../../deploy/macos_x64/openvpn bin/Debug/Eddie.app/Contents/MacOS/
cp ../../deploy/macos_x64/openvpn bin/Release/Eddie.app/Contents/MacOS/

echo Eddie - Copy stunned from deploy to release
chmod 755 ../../deploy/macos_x64/stunnel
cp ../../deploy/macos_x64/stunnel bin/Debug/Eddie.app/Contents/MacOS/
cp ../../deploy/macos_x64/stunnel bin/Release/Eddie.app/Contents/MacOS/

echo Eddie - postbuild.sh done.
exit 0