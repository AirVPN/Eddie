# eddie with amneziawg for windows

an unofficial fork of [airvpn/eddie](https://github.com/AirVPN/Eddie), based on tag `2.26.2`, adding amneziawg 1.0/2.0 profiles and a windows connection backend. the original eddie project is developed by airvpn and licensed under gplv3. this fork is not an official airvpn release.

## features

- create, edit, import, validate and export wireguard/amneziawg `.conf` profiles.
- edit jc/jmin/jmax, s1–s4, h1–h4 and i1–i5 with protocol version checks.
- generate client keys, junk parameters and cps signatures; choose from 34 eddie android cps presets.
- connect through `amneziawg-go v0.2.19` and the official wintun driver, using eddie's elevated helper and routing pipeline.
- russian and english labels, with english fallback for other languages.

connections require one peer. multiple peers can be edited and exported. awg 3.0 and a global awg switch for the built-in airvpn provider are not included.

## use

extract the complete windows x64 portable archive from this fork's releases and run `Eddie-UI.exe` inside the extracted folder. open the providers tab, add or edit a wireguard provider, then open **wg / awg profiles**. import the server configuration, save it and confirm the provider window. select the resulting server and connect normally.

server parameters s1–s4 and h1–h4 must match the server. register the client's public key on the server after generating a new private key.

## build

requires windows, .net 10 sdk, go 1.24.4 or a go installation supporting automatic toolchain download, visual studio 2022 c++ build tools with v143 and a windows sdk. dependency downloads require internet access.

from the repository root, powershell:

```powershell
.\repository\windows_common\build-amneziawg.ps1 -arch x64
.\repository\windows_common\package-amneziawg.ps1 -arch x64
```

the application folder is generated in `artifacts/windows_x64`; the portable zip and sha256 checksum are generated in `releases`. generated files and local caches are excluded from git. the github actions workflow builds the same x64 package and uploads it as a workflow artifact.

## verification and limitations

the windows x64 application was built locally. before repository cleanup, 2437 profile/ui checks passed, along with real localhost udp handshake and bidirectional encrypted packet tests on the awg engine. test sources were removed at the owner's request.

the full windows session through a wintun adapter remains unverified because the local test process lacked an administrator token. external server connections, network lock and reconnect during a live tunnel also require verification. the github actions workflow has been prepared but has not been run on github yet.

the backend uses one peer and windows only. scripts and unknown directives are rejected. windows runtime removes linux-only fwmark and uses table=off because eddie manages routes. dns search domains survive profile export but are not passed through the inherited connection builder. cps presets are saved in the profile; automatic rotation on each connection is not included. third-party notices are in `amneziawg-licenses` in the portable package and `deploy/amneziawg-licenses` in the source checkout.

## publish the fork

the local branch `amneziawg-windows` is based on the official `2.26.2` commit `010e33919e174c5146992dfb9f43a51b3606dbb9`, with `upstream` pointing to the original repository. create a fork of [airvpn/eddie](https://github.com/AirVPN/Eddie) on github, replace `your-account` below and push this prepared branch:

```powershell
git remote add origin https://github.com/your-account/Eddie.git
git push -u origin amneziawg-windows
```

set `amneziawg-windows` as the default branch of the fork and enable actions if github requests it. [setup-dotnet](https://github.com/actions/setup-dotnet/tree/v4), [setup-go](https://github.com/actions/setup-go/tree/v5) and [upload-artifact](https://github.com/actions/upload-artifact/tree/v4) handle sdk installation and artifact upload.

create a release from this branch with a new tag such as `2.26.2-awg-windows.1`. attach `eddie-2.26.2-amneziawg-windows-x64-portable.zip` and its `.sha256` file from `releases`. mark it as a prerelease until the full windows tunnel checks above are completed. keep portable archives in github releases, rather than source control. nothing has been published to github automatically.

## upstream and license

[original eddie website](https://eddie.website), [original desktop repository](https://github.com/AirVPN/Eddie), [android reference](https://gitlab.com/AirVPN/EddieAndroid).

the original gplv3 [license](LICENSE) and upstream copyright notices are preserved. third-party license texts are included in `deploy/amneziawg-licenses` and in portable packages. report issues with these modifications to this fork's issue tracker.
