$hasUserData = $false;
if (!(Test-Path -Path $PSScriptRoot\userdata)) {
    $null = New-Item -Path $PSScriptRoot\userdata -ItemType Directory;
    if (Test-Path -Path $env:LOCALAPPDATA) {
        Write-Output "Copying userdata from $env:LOCALAPPDATA\OpenTabletDriver to $PSScriptRoot\userdata..."
        Copy-Item -Path $env:LOCALAPPDATA\OpenTabletDriver\* -Destination $PSScriptRoot\userdata -Recurse;
        $hasUserData = $true;
    }

    if ($hasUserData) {
        Write-Output "Done! If you want to start fresh, delete the contents of $PSScriptRoot\userdata and restart OpenTabletDriver."
    } else {
        Write-Output "Done!"
    }
} else {
    Write-Output "This OpenTabletDriver is already in portable mode. Skipping..."
}
