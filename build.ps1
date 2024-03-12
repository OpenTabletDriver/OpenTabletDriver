# Simple powershell script to easily build on Windows and verify functionality.

. ./eng/windows/package.ps1 -output bin -config Release -isPackage $false -isPortable $true
