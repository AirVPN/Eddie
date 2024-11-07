Note 2024/10/23, Eddie 2.24.3

The newest Elevation security checks can't work with previous AppImage implementation.
So, now AppRun script extract all the files before run.
This also allow us to clean all the C#-side exception for AppImage (calls to Platform::RootExecutionOutside* are commented, and will be cleaned based on feedback).
