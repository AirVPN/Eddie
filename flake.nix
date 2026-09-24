{
  description = "Eddie - VPN Tunnel Desktop Edition (CLI) by AirVPN";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    { self
    , nixpkgs
    , flake-utils
    }:
    flake-utils.lib.eachDefaultSystem (system:
    let
      pkgs = import nixpkgs { inherit system; };

      version = "2.24";

      # ------------------------------------------------------------------
      # C++ native components: eddie-cli-elevated, eddie-cli-elevated-service,
      # and libLib.Platform.Linux.Native.so
      # ------------------------------------------------------------------
      eddie-native = pkgs.stdenv.mkDerivation {
        pname = "eddie-cli-native";
        inherit version;

        src = self;

        nativeBuildInputs = with pkgs; [
          gcc
          gnumake
          curl.dev
        ];

        buildInputs = with pkgs; [
          curl
        ];

        buildPhase = ''
          runHook preBuild

          # Build libLib.Platform.Linux.Native.so
          chmod +x src/Lib.Platform.Linux.Native/build.sh
          bash src/Lib.Platform.Linux.Native/build.sh Release

          # Build eddie-cli-elevated
          chmod +x src/App.CLI.Linux.Elevated/build.sh
          bash src/App.CLI.Linux.Elevated/build.sh Release

          # Build eddie-cli-elevated-service
          chmod +x src/App.CLI.Linux.Elevated.Service/build.sh
          bash src/App.CLI.Linux.Elevated.Service/build.sh Release

          runHook postBuild
        '';

        installPhase = ''
          runHook preInstall
          install -Dm755 src/Lib.Platform.Linux.Native/bin/libLib.Platform.Linux.Native.so $out/lib/libLib.Platform.Linux.Native.so
          install -Dm755 src/App.CLI.Linux.Elevated/bin/eddie-cli-elevated $out/bin/eddie-cli-elevated
          install -Dm755 src/App.CLI.Linux.Elevated.Service/bin/eddie-cli-elevated-service $out/bin/eddie-cli-elevated-service
          runHook postInstall
        '';
      };

      # ------------------------------------------------------------------
      # .NET 8 CLI application: eddie-cli
      # ------------------------------------------------------------------
      eddie-cli = pkgs.buildDotnetModule {
        pname = "eddie-cli";
        inherit version;

        src = self;

        dotnet-sdk = pkgs.dotnet-sdk_8;
        dotnet-runtime = pkgs.dotnet-runtime_8;

        projectFile = "src/App.CLI.Linux/App.CLI.Linux.net8.csproj";

        # NuGet dependencies — generate with:
        #   nix build .#default.passthru.fetch-deps && ./result
        # then point this to the resulting deps.nix file.
        nugetDeps = ./nix/deps.nix;

        runtimeId =
          if system == "x86_64-linux" then "linux-x64"
          else if system == "aarch64-linux" then "linux-arm64"
          else "linux-${builtins.replaceStrings ["linux-"] [""] system}";

        executables = [ "eddie-cli" ];

        # The csproj's LinuxPostBuild target runs postbuild.sh which compiles
        # native C++ helpers. We build those separately via eddie-native, so
        # replace postbuild.sh with a no-op.
        preBuild = ''
          echo '#!/bin/sh' > src/App.CLI.Linux/postbuild.sh
          echo 'echo "PostBuild skipped (Nix build)"' >> src/App.CLI.Linux/postbuild.sh
          chmod +x src/App.CLI.Linux/postbuild.sh
        '';

        postInstall = ''
          # Copy native C++ binaries alongside the .NET app
          cp ${eddie-native}/bin/eddie-cli-elevated $out/lib/eddie-cli/
          cp ${eddie-native}/bin/eddie-cli-elevated-service $out/lib/eddie-cli/
          cp ${eddie-native}/lib/libLib.Platform.Linux.Native.so $out/lib/eddie-cli/

          # Eddie looks for resources in a 'res' directory relative to the binary.
          ln -s ${self}/resources $out/lib/eddie-cli/res
        '';

        meta = with pkgs.lib; {
          description = "OpenVPN/WireGuard VPN tunnel CLI with additional features by AirVPN";
          homepage = "https://eddie.website";
          license = licenses.gpl3;
          platforms = [ "x86_64-linux" "aarch64-linux" ];
          mainProgram = "eddie-cli";
        };
      };

    in
    {
      packages = {
        default = eddie-cli;
        eddie-cli = eddie-cli;
        eddie-cli-native = eddie-native;
      };

      devShells.default = pkgs.mkShell {
        name = "eddie-dev";

        packages = with pkgs; [
          dotnet-sdk_8
          gcc
          curl.dev
          patchelf
        ];

        shellHook = ''
          echo "Eddie development environment"
          echo "dotnet --version: $(dotnet --version)"
        '';
      };
    });
}
