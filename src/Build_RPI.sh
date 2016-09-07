#!/bin/bash
xbuild /p:Configuration=Release /p:Platform=x64 /p:TargetFrameworkVersion="v4.0" /t:Rebuild Eddie_VS2015.sln 
cd bin/x64/Release
mono Deploy.exe
