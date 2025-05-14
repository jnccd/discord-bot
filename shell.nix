# nix-shell --command "bash update_and_start.sh"
# export NIXPKGS_ALLOW_INSECURE=1 && nix-shell --impure --command "bash update_and_start.sh"
{ pkgs ? import <nixpkgs> {} }:
with pkgs;
mkShell {
  packages = [
    dotnet-sdk_6
    libgdiplus
    ffmpeg_6
    youtube-dl
  ];
}