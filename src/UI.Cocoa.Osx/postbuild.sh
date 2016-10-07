mkdir -p bin/Debug/AirVPN.app/Contents/MacOS/Providers/
mkdir -p bin/Release/AirVPN.app/Contents/MacOS/Providers/

cp ../../Providers/*.xml bin/Debug/AirVPN.app/Contents/MacOS/Providers/
cp ../../Providers/*.xml bin/Release/AirVPN.app/Contents/MacOS/Providers/

sed "s/{@version}/$(cat ../../version.txt)/" ../../resources/osx/Info.plist > bin/Debug/AirVPN.app/Contents/Info.plist
sed "s/{@version}/$(cat ../../version.txt)/" ../../resources/osx/Info.plist > bin/Release/AirVPN.app/Contents/Info.plist

chmod 755 ../../deploy/osx_x64/openvpn
cp ../../deploy/osx_x64/openvpn bin/Debug/AirVPN.app/Contents/MacOS/
cp ../../deploy/osx_x64/openvpn bin/Release/AirVPN.app/Contents/MacOS/

chmod 755 ../../deploy/osx_x64/stunnel
cp ../../deploy/osx_x64/stunnel bin/Debug/AirVPN.app/Contents/MacOS/
cp ../../deploy/osx_x64/stunnel bin/Release/AirVPN.app/Contents/MacOS/

codesign -v --force --sign "Developer ID Application: Fabrizio Carimati (SQ9X79YUY3)" "/Users/clodo/Documents/airvpn-client/src/UI.Cocoa.Osx/bin/Release/AirVPN.app/Contents/MonoBundle/libMonoPosixHelper.dylib"
codesign -v --force --sign "Developer ID Application: Fabrizio Carimati (SQ9X79YUY3)" "/Users/clodo/Documents/airvpn-client/src/UI.Cocoa.Osx/bin/Release/AirVPN.app/Contents/MacOS/openvpn"
codesign -v --force --sign "Developer ID Application: Fabrizio Carimati (SQ9X79YUY3)" "/Users/clodo/Documents/airvpn-client/src/UI.Cocoa.Osx/bin/Release/AirVPN.app/Contents/MacOS/stunnel"
codesign -v --force --sign "Developer ID Application: Fabrizio Carimati (SQ9X79YUY3)" "/Users/clodo/Documents/airvpn-client/src/UI.Cocoa.Osx/bin/Release/AirVPN.app/Contents/ResourceRules.plist"
codesign -v -d --deep --force --sign "Developer ID Application: Fabrizio Carimati (SQ9X79YUY3)" "/Users/clodo/Documents/airvpn-client/src/UI.Cocoa.Osx/bin/Release/AirVPN.app"

exit 0