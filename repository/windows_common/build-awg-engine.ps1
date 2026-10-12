param([validateset('x64', 'x86')][string]$arch = 'x64')

$erroractionpreference = 'stop'
$root = (resolve-path (join-path $psscriptroot '../..')).path
$goarch = @{x64='amd64'; x86='386'}[$arch]
$env:GOPATH = join-path $root '.tools/gopath'
$env:GOCACHE = join-path $root '.tools/gocache'
$env:GOARCH = $goarch
$env:GOOS = 'windows'
$env:CGO_ENABLED = '0'
push-location (join-path $root 'src/awg.windows')
try {
    $erroractionpreference = 'continue'
    go build -trimpath '-ldflags=-s -w' -o "../../deploy/windows_$arch/amneziawg-eddie.exe" .
    $erroractionpreference = 'stop'
    if ($lastexitcode -ne 0) { throw 'amneziawg engine build failed' }
} finally { pop-location }

$driver = join-path $root "deploy/windows_$arch/wintun.dll"
if (!(test-path -literalpath $driver)) {
    $archive = join-path $root '.tools/wintun.zip'
    $unpacked = join-path $root '.tools/wintun'
    new-item -itemtype directory -force -path (split-path $archive) | out-null
    invoke-webrequest -usebasicparsing 'https://www.wintun.net/builds/wintun-0.14.1.zip' -outfile $archive
    if ((get-filehash -literalpath $archive -algorithm SHA256).hash -ne '07c256185d6ee3652e09fa55c0b673e2624b565e02c4b9091c79ca7d2f24ef51') { throw 'wintun archive checksum mismatch' }
    expand-archive -literalpath $archive -destinationpath $unpacked -force
    copy-item -literalpath (join-path $unpacked "wintun/bin/$goarch/wintun.dll") -destination $driver
}
