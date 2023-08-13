@echo off

pushd "%~dp0"

if exist "userdata" (
    echo OpenTabletDriver is already in portable mode.
    pause
    popd
    exit
)

echo Converting to portable mode...

if exist "%LOCALAPPDATA%\OpenTabletDriver" (
    robocopy "%LOCALAPPDATA%\OpenTabletDriver" "userdata" /E /R:0 /W:0 > nul
) else (
    mkdir "userdata" > nul
)

if ERRORLEVEL == 0 (
    echo Done! If you want to start fresh, delete the contents of userdata folder and restart OpenTabletDriver.
) else (
    echo An error occured while converting to portable mode.
    rmdir "userdata" /S /Q > nul
)

pause
popd
