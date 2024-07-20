{
  description = "OpenTabletDriver is an open source, cross platform, user mode tablet driver.";

  inputs.nixpkgs.url = "nixpkgs/nixos-unstable";

  outputs = inputs @ { self, nixpkgs }: let
    inherit (nixpkgs.lib) attrValues genAttrs;

    supportedSystems = [ "x86_64-linux" "aarch64-linux" ];

    # Helper function to generate an attrset '{ x86_64-linux = f "x86_64-linux"; ... }'.
    forAllSystems = f: genAttrs supportedSystems (system: f system);

  in rec {
    # Nixpkgs instantiated for supported system types.
    nixpkgsFor = forAllSystems (system: import nixpkgs {
      inherit system;
      overlays = attrValues self.overlays;
    });

    # Nixpkgs overlay
    overlays.default = final: prev: {
      opentabletdriver = final.callPackage ./default.nix {};
    };

    # Provide binary packages for all system types.
    packages = forAllSystems (system: rec {
      inherit (nixpkgsFor.${system}) opentabletdriver;
      default = opentabletdriver;
    });

    # Provide a 'nix develop' environment for interactive hacking.
    devShells = forAllSystems (system: {
      default = import ./shell.nix {
        inherit system;
        flake = self;
      };
    });
  };
}
