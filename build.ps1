# Simple powershell script to easily build on Windows and verify functionality.

$ErrorActionPreference = "Stop";
$NetRuntime = "win-x64";
$PrevPath = $(Get-Location).Path;
$nl = [Environment]::NewLine;

$Config = "Release";

$Options = @("--configuration", "$Config", "--self-contained=false", "--output=./bin", "/p:PublishSingleFile=true", "/p:DebugType=embedded",`
    "/p:SuppressNETCoreSdkPreviewMessage=true", "/p:PublishTrimmed=false", "--runtime=$NetRuntime", "-p:SourceRevisionId=$(git rev-parse --short HEAD)");

# Change dir to script root, in case people run the script outside of the folder.
Set-Location $PSScriptRoot;

# Sanity check
if (!(Test-Path "./OpenTabletDriver")) {
    Write-Error "Could not find OpenTabletDriver folder. Please run this script from the root of the OpenTabletDriver repository.";
    exit 1;
}

Write-Output "Cleaning old build dirs...";
if (Test-Path "./bin") {
    try {
        Get-ChildItem -Path "./bin" | ForEach-Object {
            if ($_.Name -ne "userdata") {
                Remove-Item -Path $_.FullName -Recurse -Force;
            }
        }
    } catch {
        Write-Error "Could not clean old build dirs. Please manually remove contents of ./bin folder.";
        exit 1;
    }
}

dotnet clean --configuration $Config;

Write-Output "Building OpenTabletDriver with runtime $NetRuntime...";
New-Item -ItemType Directory -Force -Path "./bin";
New-Item -ItemType Directory -Force -Path "./bin/userdata";

Write-Output "${nl}Building Daemon...$nl";
dotnet publish .\OpenTabletDriver.Daemon $Options;
if ($LASTEXITCODE -ne 0) { exit 1; }

Write-Output "${nl}Building Console...$nl";
dotnet publish .\OpenTabletDriver.Console $Options;
if ($LASTEXITCODE -ne 0) { exit 2; }

Write-Output "${nl}Building WPF UX...$nl";
dotnet publish .\OpenTabletDriver.UX.Wpf $Options;
if ($LASTEXITCODE -ne 0) { exit 3; }

Write-Output "${nl}Build finished. Binaries created in ./bin";
Set-Location $PrevPath;
