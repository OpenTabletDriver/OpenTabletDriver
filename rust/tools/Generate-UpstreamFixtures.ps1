# Compile the actual upstream C# parser files with minimal interface/attribute stubs.
# No SDK, driver, device initialization, or input injection is involved.
param([int] $CasesPerVariant = 1024)
$ErrorActionPreference = 'Stop'
$repo = Split-Path (Split-Path $PSScriptRoot)
$temp = Join-Path ([IO.Path]::GetTempPath()) ('otd-parity-' + [Guid]::NewGuid().ToString('N') + '.cs')
@'
using System;
using System.Numerics;
namespace OpenTabletDriver {
    public static class BitExtensions { public static bool IsBitSet(this byte b, int bit) => (b & (1 << bit)) != 0; }
}
namespace OpenTabletDriver.Plugin.Tablet {
    public interface IDeviceReport { byte[] Raw { get; set; } }
    public interface IReportParser<T> { T Parse(byte[] report); }
    public interface ITabletReport : IDeviceReport { Vector2 Position { get; set; } uint Pressure { get; set; } bool[] PenButtons { get; set; } }
    public interface IAuxReport : IDeviceReport { bool[] AuxButtons { get; set; } }
    public struct DeviceReport(byte[] raw) : IDeviceReport { public byte[] Raw { get; set; } = raw; }
    public struct OutOfRangeReport(byte[] raw) : IDeviceReport { public byte[] Raw { get; set; } = raw; }
}
namespace OpenTabletDriver.Plugin.Tablet.Wheel { public interface IRelativeWheelReport : IDeviceReport { int[] AnalogDeltas { get; set; } } }
'@ | Set-Content -LiteralPath $temp -Encoding utf8
try {
    $paths = @(
        'OpenTabletDriver.Configurations/Parsers/Veikk/VeikkV1ReportParser.cs',
        'OpenTabletDriver.Configurations/Parsers/Veikk/VeikkReportParser.cs',
        'OpenTabletDriver.Configurations/Parsers/Veikk/VeikkTabletReport.cs',
        'OpenTabletDriver.Configurations/Parsers/Veikk/VeikkAuxV1Report.cs',
        'OpenTabletDriver.Configurations/Parsers/Veikk/VeikkAuxReport.cs',
        'OpenTabletDriver.Configurations/Parsers/Veikk/VeikkRelativeWheelReport.cs',
        'OpenTabletDriver.Plugin/Tablet/TabletReport.cs'
    ) | ForEach-Object { Join-Path $repo $_ }
    Add-Type -Path (@($temp) + $paths)
    $random = [Random]::new(640)
    $lines = [Collections.Generic.List[string]]::new()
    foreach ($variant in @('S640','S640V2')) {
        $parser = if ($variant -eq 'S640') { [OpenTabletDriver.Configurations.Parsers.Veikk.VeikkV1ReportParser]::new() } else { [OpenTabletDriver.Configurations.Parsers.Veikk.VeikkReportParser]::new() }
        for ($index = 0; $index -lt $CasesPerVariant; $index++) {
            $bytes = [byte[]]::new($(if ($variant -eq 'S640') { 9 } else { 13 }))
            $random.NextBytes($bytes)
            if ($variant -eq 'S640') {
                $bytes[0] = if ($index % 5 -eq 0) { 3 } else { 8 }
                $bytes[1] = if ($bytes[0] -eq 3) { $index % 2 } else { 0x41 }
                $bytes[2] = if ($index % 7 -eq 0) { 0xc0 } else { 0xa0 + ($index % 16) }
                if ($bytes[0] -eq 3) { $bytes[2] = @(0x3e,0x0c,0x2c,0x19,0x06,0x1d,0x16,0x2e,0x2d,0x30,0x2f)[$index % 11] }
            } else {
                $bytes[1] = if ($index % 17 -eq 0) { 0x43 } else { 0 }
                $bytes[2] = $index % 256
                if ($bytes[2] -eq 1 -or $bytes[2] -eq 3) { $bytes[3] = $index % 2; $bytes[4] = ($index / 256) % 4 }
            }
            $report = $parser.Parse($bytes)
            $expected = if ($report -is [OpenTabletDriver.Plugin.Tablet.ITabletReport]) {
                $buttons = 0
                for ($bit = 0; $bit -lt $report.PenButtons.Length; $bit++) { if ($report.PenButtons[$bit]) { $buttons = $buttons -bor (1 -shl $bit) } }
                @{ type='pen'; x=[uint32]$report.Position.X; y=[uint32]$report.Position.Y; pressure=$report.Pressure; buttons=$buttons }
            } elseif ($report -is [OpenTabletDriver.Plugin.Tablet.OutOfRangeReport]) { @{ type='out_of_range' }
            } elseif ($report -is [OpenTabletDriver.Plugin.Tablet.IAuxReport]) {
                $buttons = 0
                for ($bit = 0; $bit -lt $report.AuxButtons.Length; $bit++) { if ($report.AuxButtons[$bit]) { $buttons = $buttons -bor (1 -shl $bit) } }
                @{ type='auxiliary'; buttons=$buttons }
            } elseif ($report -is [OpenTabletDriver.Plugin.Tablet.Wheel.IRelativeWheelReport]) { @{ type='wheel'; delta=$report.AnalogDeltas[0] }
            } else { @{ type='ignore' } }
            $lines.Add((@{ variant=$variant; packet=[Convert]::ToHexString($bytes); expected=$expected } | ConvertTo-Json -Depth 5 -Compress))
        }
    }
    $destination = Join-Path $PSScriptRoot '../tests/fixtures/upstream-veikk.jsonl'
    $lines | Set-Content -LiteralPath $destination -Encoding utf8
    Write-Output "Generated $($lines.Count) cases from actual C# parsers: $destination"
} finally {
    Remove-Item -LiteralPath $temp
}
