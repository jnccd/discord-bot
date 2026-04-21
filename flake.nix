# nix develop . --experimental-features 'nix-command flakes'
# nix develop --experimental-features 'nix-command flakes' --command bash -c "bash update_and_start.sh"
# export NIXPKGS_ALLOW_INSECURE=1 && nix develop --impure --experimental-features 'nix-command flakes' --command bash -c "bash update_and_start.sh"
{
  description = "Nix Shell Wrapper";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-25.11";
    numtide-utils.url = "github:numtide/flake-utils";
    jnccd-utils.url = "github:jnccd/nix-utils";
  };

  outputs = { self, nixpkgs, numtide-utils, jnccd-utils }:
    numtide-utils.lib.eachDefaultSystem
      (system: let
          pkgs = import nixpkgs {inherit system;};
        in {
          devShells = rec {
            service = inputs.jnccd-utils.lib.mkUnfrozenDotnetShell {
              inherit system nixpkgs;
              dotnetVersion = "10.0";
              includeAndroidSdk = false;
            };
            default = service;
          };
        }
      );
}