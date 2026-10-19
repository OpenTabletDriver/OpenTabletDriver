param([switch] $SkipBuild)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
if (-not $SkipBuild) {
    Push-Location $root
    try {
        cargo build --release --locked
        if ($LASTEXITCODE -ne 0) { throw 'Release build failed' }
    } finally { Pop-Location }
}
$folder = Join-Path $root 'artifacts/OpenTabletDriverRust-S640-win-x64'
New-Item -ItemType Directory -Force $folder | Out-Null
Copy-Item -LiteralPath (Join-Path $root 'target/release/otd-rust.exe'),(Join-Path $root 'target/release/otd-rust-ui.exe') -Destination $folder
Copy-Item -LiteralPath (Join-Path $root '../LICENSE') -Destination $folder
Copy-Item -LiteralPath (Join-Path $root 'docs/PORTABLE.md') -Destination (Join-Path $folder 'README.md')
Copy-Item -LiteralPath (Join-Path $root 'docs/PERFORMANCE.md'),(Join-Path $root 'docs/MIGRATION.md') -Destination $folder
@'
@echo off
start "OpenTabletDriver Rust demo" "%~dp0otd-rust-ui.exe" --simulate
'@ | Set-Content -LiteralPath (Join-Path $folder 'Launch-Demo.cmd') -Encoding ascii
$zip = Join-Path $root 'artifacts/OpenTabletDriverRust-S640-win-x64.zip'
Compress-Archive -Path (Join-Path $folder '*') -DestinationPath $zip -Force
Get-FileHash -LiteralPath $zip -Algorithm SHA256 | Format-List Path,Hash
