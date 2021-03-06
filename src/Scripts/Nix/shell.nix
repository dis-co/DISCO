{ pkgs ? import <nixpkgs> {} }:

with pkgs;
let
  mono = mono46;

  inputs = [
    stdenv
    stdenv.cc.cc.lib
    curl
    openssl
    zlib
    phantomjs2
    e2fsprogs
    libidn
    keyutils
    yarn
  ];

  libpath= lib.makeLibraryPath inputs;

  hook = ''
    export TMPDIR=/tmp
    export TMPDIR=$(mktemp -d)
    export LD_LIBRARY_PATH="$libpath":$LD_LIBRARY_PATH
    export PHANTOMJS_PATH=${phantomjs2}/bin/phantomjs
  '';

  service = stdenv.mkDerivation {
    name = "Disco";
    version = "1.0";
    phases = [ "unpackPhase" "buildPhase" "installPhase" ];

    buildInputs = [ fsharp ];

    src = ./dotnet;

    unpackPhase = ''
      cp -r $src .
      chmod -R +rwx .
      cd $src
      substituteInPlace ./build.sh \
        --replace /usr/bin/env bash ${stdenv.shell}
    '';

    buildPhase = ''
      ./build.sh DebugDocker
    '';

    installPhase = ''
      mkdir -p $out/bin
      cp src/Disco/bin/Debug/Disco/* $out/bin
    '';
  };

  image = mono: dockerTools.buildImage {
    name = "disco-image";
    tag = "0.1";

    runAsRoot = ''
      #!${stdenv.shell}
      export PATH=/bin:/usr/bin:/sbin:/usr/sbin:$PATH
      ${dockerTools.shadowSetup}
      mkdir /data
    '';

    contents = [ mono myApp ];

    config = {
      Cmd = [ "${mono}/bin/mono" "${myApp}/bin/Main.exe"];
      ExposedPorts = {
        "6379/tcp" = {};
      };
      WorkingDir = "/data";
      Volumes = {
        "/data" = {};
      };
    };
  };

  env = stdenv.mkDerivation {
    name = "discoEnv";
    buildInputs = inputs;
    libpath = libpath;
    shellHook = hook;
  };

in rec {
  discoService = service;
  dockerImage = image;
  discoEnv = env;
}
