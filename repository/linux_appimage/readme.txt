Note 2024/10/23, Eddie 2.24.3

The newest Elevation security checks can't work with previous AppImage implementation.
So, now AppRun script extract all the files before run.
This also allow us to clean all the C#-side exception for AppImage (calls to Platform::RootExecutionOutside* are commented, and will be cleaned based on feedback).

Note 2026/06/29

Direct in-place execution was tested on Ubuntu 24.04.4 with libfuse2t64 installed and failed:
the app starts from the AppImage FUSE mount, but elevated cannot access
/tmp/.mount_*/opt/eddie-ui/eddie-cli-elevated (Permission denied).
AppRun therefore uses extract-and-run.
