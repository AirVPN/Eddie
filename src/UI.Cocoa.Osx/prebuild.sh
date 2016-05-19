mkdir -p bin/Debug/AirVPN.app/Contents/MacOS/
mkdir -p bin/Release/AirVPN.app/Contents/MacOS/

mkdir -p bin/Debug/AirVPN.app/Contents/MacOS/Providers/
mkdir -p bin/Release/AirVPN.app/Contents/MacOS/Providers/
cp ../../providers/AirVPN.xml bin/Debug/AirVPN.app/Contents/MacOS/Providers/
cp ../../providers/AirVPN.xml bin/Release/AirVPN.app/Contents/MacOS/Providers/

chmod 755 ../../deploy/osx_x64/openvpn
cp ../../deploy/osx_x64/openvpn bin/Debug/AirVPN.app/Contents/MacOS/
cp ../../deploy/osx_x64/openvpn bin/Release/AirVPN.app/Contents/MacOS/

chmod 755 ../../deploy/osx_x64/stunnel
cp ../../deploy/osx_x64/stunnel bin/Debug/AirVPN.app/Contents/MacOS/
cp ../../deploy/osx_x64/stunnel bin/Release/AirVPN.app/Contents/MacOS/