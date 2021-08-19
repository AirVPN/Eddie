Deploy scripts

These scripts create packages in 'files' subdirectory.

There isn't any cross-platform compilation, we build from native arch.

Main script like "build_linux.sh" build Eddie in every supported package formats and fill the "files" subdirectory.
It's incremental, clean "files" subdirectory for a full rebuild.

There are subdirectory {platform}_{package} with build script, simply launch "build.sh ui"
Some package have dependencies from other, deps will be builded automatically if need.





-- Compatibility

Win7 use a specific build of tap-windows-9.24.2 (win7).