param (
    $output = "dist",
    $config = "Release",
    $framework = "net7.0",
    $netRuntime = "win-x64",
    $isRelease = $true,
    $isPackage = $true,
    $isPortable = $false,
    $selfContained = $false
)

$ErrorActionPreference = "Stop";
$PrevPath = $(Get-Location).Path;
$nl = [Environment]::NewLine;

$Projects = @(
    "OpenTabletDriver.Daemon",
    "OpenTabletDriver.UI"
    # "OpenTabletDriver.Console"
);

$UIProjects = @(
    # "OpenTabletDriver.UI"
);

$Options = @(
    "--configuration", "$config",
    "--runtime", "$netRuntime",
    "--self-contained", "$selfContained",
    "--output", "$output",
    "/p:PublishSingleFile=true",
    "/p:PublishTrimmed=false",
    "/p:DebugType=embedded",
    "/p:SuppressNETCoreSdkPreviewMessage=true",
    "/p:VersionSuffix=$env:VERSION_SUFFIX"
);

if (!($isRelease)) {
    $Options += "/p:SourceRevisionId=$(git rev-parse --short HEAD)";
}

function exitWithError {
    param ($message)

    Set-Location $PrevPath;
    Write-Error $message;
    exit 1;
}

# Change dir to repo root
Set-Location $PSScriptRoot/../..;

# Sanity check
if (!(Test-Path "./OpenTabletDriver")) {
    exitWithError "Could not find OpenTabletDriver folder!";
}

if (Test-Path "$output") {
    Write-Output "Cleaning old build outputs...";
    try {
        Get-ChildItem -Path "$output" | ForEach-Object {
            if ($_.Name -ne "userdata") {
                Remove-Item -Path $_.FullName -Recurse -Force;
            }
        }
    } catch {
        exitWithError "Could not clean old build dirs. Please manually remove contents of ./bin folder.";
    }
}

dotnet restore --verbosity quiet > $null
dotnet clean --configuration $config --verbosity quiet > $null;

Write-Output "Runtime = $netRuntime";
New-Item -ItemType Directory -Force -Path "$output" > $null;
if ($isPortable) {
    New-Item -ItemType Directory -Force -Path "$output/userdata" > $null;
}

foreach ($project in $Projects) {
    Write-Output "${nl}Building $project...$nl";
    dotnet publish $project $Options --framework $framework;
    if ($LASTEXITCODE -ne 0) {
        exitWithError "Build failed!";
    }
}

foreach ($project in $UIProjects) {
    Write-Output "${nl}Building $project...$nl";
    dotnet publish $project $Options --framework ${framework}-windows;
    if ($LASTEXITCODE -ne 0) {
        exitWithError "Build failed!";
    }
}

Write-Output "${nl}Build finished! Binaries created in $output";

if ($isPackage) {
    Write-Output "${nl}Creating package...";
    Copy-Item -Path $PSScriptRoot/convert_to_portable.bat -Destination $output;
    $zipPath = "$output/OpenTabletDriver-$netRuntime.zip";
    if (Test-Path $zipPath) {
        Remove-Item $zipPath;
    }
    Compress-Archive -Path "$output/*" -DestinationPath $zipPath;
    Write-Output "Packaging finished! Zip created in $zipPath";
}

Set-Location $PrevPath;
