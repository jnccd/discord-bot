# nix-shell --command "bash update_and_start.sh"
# export NIXPKGS_ALLOW_INSECURE=1 && nix-shell --impure --command "bash update_and_start.sh"
{ pkgs ? import <nixpkgs> {} }:
with pkgs;
mkShell {
  packages = [
    icu
    dotnet-sdk_10
    ffmpeg
    yt-dlp
    tesseract
  ];
}