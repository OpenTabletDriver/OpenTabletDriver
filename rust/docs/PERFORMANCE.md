# Measurements / 19 October 2026

Windows x64, Rust 1.99.0, GNU toolchain, optimized release build with LTO. Limit: **50 MB decimal (50,000,000 bytes)**. GUI executable has the Windows subsystem, so it does not create a console host. No working-set trimming or memory-limit tricks were used.

| Scenario | Peak working set | Peak sampled private bytes | Duration | Reports |
| --- | ---: | ---: | ---: | ---: |
| Native UI idle | 15.37 MB | 2.24 MB | 15.38 s | 0 |
| Native UI + synthetic 1000 Hz input | 14.46 MB | 2.21 MB | 15.39 s | 15,180 |
| Headless + synthetic 1000 Hz input | 7.08 MB | 1.23 MB | 15.34 s | 15,040 |

All scenarios passed the resident/private memory gate. UI controls were genuinely created, shown offscreen without activation, painted, and updated. Samples were taken every 250 ms; peak resident memory uses the process's peak working-set counter. Private memory is the maximum sampled value. These are short tests on one machine, not a long-duration guarantee.

The running C# daemon and WPF UI were observed at 87.41 MB and 202.20 MB working set respectively. Those processes remained running and their settings were untouched. Their workload, version, and complete feature set were not controlled, so this is a reference observation rather than a fair benchmark comparison.

## Core microbenchmark

One million synthetic S640 packets, decode + affine mapping + pressure/button mapping:

- Zero hot-path allocations.
- 37.53 ns mean in the throughput loop (about 26.6 million reports/s).
- Independently timed samples: 100 ns p50/p95/p99; these include clock overhead and have coarse timing resolution.
- In the UI simulation, the final telemetry interval averaged approximately 0.195 microseconds of core processing per report. Maximum observed was 88.4 microseconds under the host's scheduling noise.

These figures **exclude** USB transport, queueing in the OS, `SendInput`, compositor/display latency, and UI reaction time. A lower end-to-end latency than the existing driver has not been established. Do not present these nanosecond figures as pen-to-screen latency.

## Live S640 observation

The Rust reader opened the user's connected S640 using read access only, with the existing OpenTabletDriver still running. In 60 seconds it received **12,592 pen reports, zero malformed packets**, at about 8.44 MB working set / 1.35 MB private memory. No initialization reports or pointer events were sent by the Rust process. A preceding 20-second observation received no packets; the extended observation captured the user's pen movement.

This validates discovery, report-length matching, read completion, and decoding on the physical S640. It does not validate independent initialization, reconnect/suspend behavior, button injection, or application drawing pressure.

## Reproduce

```powershell
./tools/Measure-Performance.ps1 -Seconds 15
```

Raw results go to `artifacts/performance-summary.json`, per-scenario sample files, `core-benchmark.json`, `ui-self-test.json`, and `ui-panel.bmp`. The script fails if either memory metric exceeds the budget. Future parser/plugin/UI work must rerun the same gate. Add a longer hardware-driven soak before replacing the installed driver.
