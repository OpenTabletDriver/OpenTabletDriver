param([int] $Seconds = 15, [switch] $SkipBuild)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$output = Join-Path $root 'artifacts'
New-Item -ItemType Directory -Force $output | Out-Null
if (-not $SkipBuild) {
    Push-Location $root
    try {
        cargo build --release --locked
        if ($LASTEXITCODE -ne 0) { throw 'Release build failed' }
    } finally { Pop-Location }
}
$cli = Join-Path $root 'target/release/otd-rust.exe'
$gui = Join-Path $root 'target/release/otd-rust-ui.exe'
$baseline = @(Get-Process -ErrorAction SilentlyContinue | Where-Object ProcessName -in @('OpenTabletDriver.Daemon','OpenTabletDriver.UX.Wpf') | ForEach-Object {
    @{ name=$_.ProcessName; pid=$_.Id; working_set_bytes=$_.WorkingSet64; private_bytes=$_.PrivateMemorySize64 }
})
$results = [Collections.Generic.List[object]]::new()
foreach ($scenario in @('idle-ui', 'simulated-input-ui', 'simulated-input-headless')) {
    $report = Join-Path $output ($scenario + '.json')
    $settings = Join-Path $output 'performance-unused-settings.json'
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = if ($scenario -eq 'simulated-input-headless') { $cli } else { $gui }
    $startInfo.WorkingDirectory = $root
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.WindowStyle = [Diagnostics.ProcessWindowStyle]::Hidden
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @('--seconds',"$Seconds",'--report',$report,'--settings',$settings)) { $startInfo.ArgumentList.Add($argument) }
    if ($scenario -ne 'idle-ui') { $startInfo.ArgumentList.Add('--simulate') }
    if ($scenario -eq 'simulated-input-headless') { $startInfo.ArgumentList.Add('--headless') } else { $startInfo.ArgumentList.Add('--hidden') }
    $process = [Diagnostics.Process]::Start($startInfo)
    $samples = [Collections.Generic.List[object]]::new()
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $lastCpu = 0.0
    try {
        while (-not $process.HasExited) {
            $process.Refresh()
            if ($process.HasExited) { break }
            $lastCpu = $process.TotalProcessorTime.TotalMilliseconds
            $samples.Add(@{ elapsed_ms=$watch.ElapsedMilliseconds; working_set_bytes=$process.WorkingSet64; private_bytes=$process.PrivateMemorySize64; peak_working_set_bytes=$process.PeakWorkingSet64; handle_count=$process.HandleCount })
            if ($watch.Elapsed.TotalSeconds -gt ($Seconds + 15)) { $process.Kill(); throw "Scenario timed out: $scenario" }
            Start-Sleep -Milliseconds 250
        }
        $process.WaitForExit()
        $stderr = $process.StandardError.ReadToEnd()
        if ($process.ExitCode -ne 0) { throw "Scenario failed: $scenario ($stderr)" }
        $state = Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
        $peakResident = ($samples | Measure-Object -Property peak_working_set_bytes -Maximum).Maximum
        $peakPrivate = ($samples | Measure-Object -Property private_bytes -Maximum).Maximum
        $entry = @{ scenario=$scenario; seconds=$watch.Elapsed.TotalSeconds; samples=$samples.Count;
            peak_working_set_bytes=$peakResident; peak_private_bytes=$peakPrivate;
            cpu_one_core_percent=100.0*$lastCpu/($watch.Elapsed.TotalMilliseconds);
            reports=$state.snapshot.reports; malformed=$state.snapshot.malformed;
            core_average_us=$state.snapshot.processing_us; core_max_us=$state.snapshot.max_processing_us;
            under_50mb=($peakResident -lt 50000000 -and $peakPrivate -lt 50000000) }
        $results.Add($entry)
        Write-Output ("{0}: peak resident {1:N2} MB / private {2:N2} MB; {3} reports" -f $scenario,($peakResident/1e6),($peakPrivate/1e6),$state.snapshot.reports)
        $samples | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $output ($scenario+'-samples.json'))
    } finally {
        if (-not $process.HasExited) { $process.Kill(); $process.WaitForExit() }
        $process.Dispose()
    }
}
& $cli --benchmark --iterations 1000000 --report (Join-Path $output 'core-benchmark.json') | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Core benchmark allocation check failed' }
& $cli --simulate --hidden --ui-self-test --seconds 6 --settings (Join-Path $output 'ui-test-settings.json') --snapshot (Join-Path $output 'ui-panel.bmp') --report (Join-Path $output 'ui-self-test.json') | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'UI interaction test failed' }
$summary = @{ recorded_at_utc=[DateTime]::UtcNow.ToString('o'); limit_bytes=50000000; scenarios=$results;
    existing_csharp_processes=$baseline;
    baseline_note='Observed existing processes; not a controlled workload or feature-parity comparison.';
    scope='Release binaries, one process including native UI controls. UI rendered offscreen for non-intrusive tests. Synthetic 1000 Hz input has no USB or SendInput. Peak resident and private memory sampled, without working-set trimming.' }
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $output 'performance-summary.json')
if (@($results | Where-Object { -not $_.under_50mb }).Count -gt 0) { throw '50 MB memory budget exceeded' }
Write-Output 'PASS: memory budget, zero-allocation core, UI Apply/Stop/Start/Save and validation checks.'
