param([validateset('x64', 'x86')][string]$arch = 'x64')

$erroractionpreference = 'stop'
$root = (resolve-path (join-path $psscriptroot '../..')).path
$package = join-path $root "artifacts/windows_$arch"
$destination = join-path $root 'releases'
$name = "eddie-2.26.2-amneziawg-windows-$arch-portable.zip"
$archive = join-path $destination $name
foreach ($file in @('Eddie-UI.exe', 'Eddie-CLI-Elevated.exe', 'Eddie-CLI-Elevated-Service.exe', 'Lib.Platform.Windows.Native.dll', 'amneziawg-eddie.exe', 'wintun.dll', 'resources/locales/inv.json', 'resources/locales/ru.json', 'amneziawg-licenses/sources.txt', 'LICENSE')) {
    if (!(test-path -literalpath (join-path $package $file))) { throw "missing package file: $file" }
}
new-item -itemtype directory -force -path $destination | out-null
add-type -assemblyname System.IO.Compression
add-type -assemblyname System.IO.Compression.FileSystem
$output = [System.IO.File]::Open($archive, [System.IO.FileMode]::Create)
$zip = [System.IO.Compression.ZipArchive]::new($output, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in (get-childitem -literalpath $package -file -recurse | sort-object FullName)) {
        if ($file.Extension -in @('.pdb', '.log', '.tmp')) { continue }
        $relative = $file.FullName.Substring($package.Length + 1).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, "windows_$arch/$relative", [System.IO.Compression.CompressionLevel]::Optimal) | out-null
    }
} finally { $zip.Dispose(); $output.Dispose() }
$hash = (get-filehash -literalpath $archive -algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText("$archive.sha256", "$hash  $name`n", [System.Text.UTF8Encoding]::new($false))
write-output "packaged $archive"
