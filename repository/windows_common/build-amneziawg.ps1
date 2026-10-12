param(
    [validateset('x64', 'x86')][string]$arch = 'x64',
    [string]$dotnet = 'dotnet',
    [string]$toolset = 'v143'
)

$erroractionpreference = 'stop'
$root = (resolve-path (join-path $psscriptroot '../..')).path
$destination = join-path $root "artifacts/windows_$arch"
& (join-path $psscriptroot 'build-awg-engine.ps1') -arch $arch

$vswhere = join-path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuild = & $vswhere -latest -products '*' -find 'MSBuild/Current/Bin/MSBuild.exe'
if (!$msbuild) { throw 'visual studio msbuild was not found' }
foreach ($project in @('Lib.Platform.Windows.Native', 'App.CLI.Windows.Elevated', 'App.CLI.Windows.Elevated.Service')) {
    & $msbuild (join-path $root "src/$project/$project.sln") /p:Configuration=Release "/p:Platform=$arch" "/p:PlatformToolset=$toolset" /p:WindowsTargetPlatformVersion=10.0 /v:minimal /nologo
    if ($lastexitcode -ne 0) { throw "native build failed: $project" }
}

& $dotnet publish (join-path $root 'src/App.Forms.Windows/App.Forms.Windows.net10.csproj') -c Release -r "win-$arch" -o $destination -p:skipwindowspostbuild=true -p:PublishReadyToRun=false -p:EnableNETAnalyzers=false -p:DebugType=None -p:DebugSymbols=false --nologo -v:minimal
if ($lastexitcode -ne 0) { throw 'managed publish failed' }
copy-item -path (join-path $root "deploy/windows_$arch/*") -destination $destination -force
copy-item -literalpath (join-path $root "src/Lib.Platform.Windows.Native/bin/$arch/Release/Lib.Platform.Windows.Native.dll") -destination $destination -force
copy-item -literalpath (join-path $root "src/App.CLI.Windows.Elevated/bin/$arch/Release/App.CLI.Windows.Elevated.exe") -destination (join-path $destination 'Eddie-CLI-Elevated.exe') -force
copy-item -literalpath (join-path $root "src/App.CLI.Windows.Elevated.Service/bin/$arch/Release/App.CLI.Windows.Elevated.Service.exe") -destination (join-path $destination 'Eddie-CLI-Elevated-Service.exe') -force
copy-item -literalpath (join-path $root 'resources') -destination $destination -recurse -force
copy-item -literalpath (join-path $root 'deploy/amneziawg-licenses') -destination $destination -recurse -force
copy-item -literalpath (join-path $root 'LICENSE') -destination $destination -force
copy-item -literalpath (join-path $root 'README.md') -destination $destination -force
set-content -literalpath (join-path $destination 'portable.txt') -value ''
write-output "built $destination"
