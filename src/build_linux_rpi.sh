#!/bin/bash
xbuild /p:Configuration=Release /p:Platform=x64 /p:TargetFrameworkVersion="v4.0" /t:Rebuild Eddie_VS2015.sln 
mono bin/x64/Release/Deploy.exe
