# Run this script to use a local copy of HidSharp rather than fetching it from nuget.
# It expects the HIDSharpCore directory to be in the same level as the OpenTabletDriver directory.
#
# based on: https://github.com/ppy/osu-framework/blob/master/UseLocalVeldrid.ps1

$OTD_CSPROJ="OpenTabletDriver/OpenTabletDriver.csproj"
$SLN="OpenTabletDriver.sln"

dotnet remove $OTD_CSPROJ reference HidSharpCore

dotnet sln $SLN add ../HIDSharpCore/HidSharp/HidSharp.csproj `
    ../HIDSharpCore/HidSharp.Test/HidSharp.Test.csproj

dotnet add $OTD_CSPROJ reference ../HIDSharpCore/HidSharp/HidSharp.csproj
