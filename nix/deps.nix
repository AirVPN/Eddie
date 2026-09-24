# NuGet dependencies for Eddie CLI
#
# To regenerate, run:
#   nix build .#default.passthru.fetch-deps && ./result
#
# Or manually add packages below with the correct sha256 from:
#   nix-prefetch-url "https://api.nuget.org/v3-flatcontainer/<pname>/<version>/<pname>.<version>.nupkg"
{ fetchNuGet }: [
  (fetchNuGet {
    pname = "Microsoft.CSharp";
    version = "4.7.0";
    hash = "sha256-Enknv2RsFF68lEPdrf5M+BpV1kHoLTVRApKUwuk/pj0=";
  })
]
