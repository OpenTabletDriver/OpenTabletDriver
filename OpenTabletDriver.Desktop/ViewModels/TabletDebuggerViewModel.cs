using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.ViewModels.Utility;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Desktop.ViewModels;

public sealed class TabletDebuggerViewModel : ViewModel, IDisposable
{
    private const DecodingMode _DEFAULT_DECODING_MODE = DecodingMode.Hex;

    private readonly HPETDeltaStopwatch _stopwatch = new();
    private readonly HashSet<string> _seenReports = [];

    private FileStream? _tabletRecordingFileStream;
    private StreamWriter? _tabletRecordingStreamWriter;

    private byte[]? _lastBytes;

    public NotifyCollectionChangedEventHandler? StatisticsCollectionChanged;

    public TabletDebuggerViewModel()
    {
        _additionalStatistics.ChildCollectionChanged += (sender, args) => StatisticsCollectionChanged?.Invoke(sender, args);
    }

    public void HandleReport(object? sender, DebugReportData data) => ReportData = data;

    #region View Model Properties (and backing fields)
    private DebugReportData? _reportData;
    public DebugReportData? ReportData
    {
        get => _reportData;
        // TODO: only gather AdditionalStatistics if enabled
        private set
        {
            // early exit if ignored
            if (value != null && IgnoredReports.Contains(GetNameKeyForFilter(value.Tablet, value.Path))) return;
            if (value != null && IgnoredTablets.Contains(value.Tablet.Properties.Name)) return;

            RaiseAndSetIfChanged(ref _reportData, value);
            if (value == null) return;
            RaiseChanged(nameof(DeviceName));

            if (_seenReports.Add(GetNameKeyForFilter(value.Tablet, value.Path)))
                RaiseChanged(nameof(SeenReports));

            var timeDelta = _stopwatch.Restart();
            AdditionalStatistics["Report Rate"].SaveMinMax(timeDelta.TotalMilliseconds, "ms");
            HandleReportInterval(timeDelta);

            var report = new DeviceReport();

            if (!value.Reinit(ref report))
            {
                Log.Write(nameof(TabletDebuggerViewModel), $"Could not parse report. Skipping statistics for bytes '{report.Raw.PrettyPrintHex()}' on parser path {value.Path}", LogLevel.Error);
                AdditionalStatistics["Failed Reports"].SaveCountAdd1(value.Path);
                return;
            }

            AdditionalStatistics["Tablets Parsed"].SaveCountAdd1(value.Tablet.Properties.Name)
                .HideAllChildren(); // probably nonsensical to keep in UI

            AdditionalStatistics["Report Parser"].SaveCountAdd1(value.Path)
                .HideAllChildren(); // goes too crazy in UI for now

            if (report.PenPosition.HasValue)
                AdditionalStatistics["Tablet Position"].SaveMinMax(report.PenPosition.GetValueOrDefault());

            if (report.Touches != null)
                AdditionalStatistics["Touch Position"].SaveMinMax(report.Touches.Span);

            if (report.Eraser.HasValue)
                AdditionalStatistics["Eraser"].SaveButtons([report.Eraser.GetValueOrDefault()]);

            if (report.PenPressure.HasValue)
                AdditionalStatistics["Pressure"].SaveMinMax(report.PenPressure.GetValueOrDefault());

            // TODO: what happens if button count is higher than parser expects?
            if (report.PenButtons != null)
                AdditionalStatistics["Pen Buttons"].SaveButtons(report.PenButtons.Span[..(int)value.Tablet.Properties.Specifications.Pen.ButtonCount]);

            if (report.MousePosition.HasValue)
                AdditionalStatistics["Mouse Position"]
                    .SaveMinMax(report.MousePosition.GetValueOrDefault());

            if (report.MouseScroll.HasValue)
                AdditionalStatistics["Mouse Scroll"]
                    .SaveMinMax(report.MouseScroll.GetValueOrDefault());

            // TODO: what happens if button count is higher than parser expects?
            if (report.MouseButtons != null)
                AdditionalStatistics["Mouse Buttons"]
                    .SaveButtons(report.MouseButtons.Span[..(int)(value.Tablet.Properties.Specifications.MouseButtons?.ButtonCount ?? 0)]);

            if (report.HoverDistance.HasValue)
                AdditionalStatistics["Hover Distance"]
                    .SaveMinMax(report.HoverDistance.GetValueOrDefault())
                    .HideAllChildren();

            if (report.NearProximity.HasValue)
                AdditionalStatistics["Proximity"]
                    .SaveButtons([report.NearProximity.GetValueOrDefault()])
                    .HideAllChildren();

            if (report.ToolType.HasValue)
                AdditionalStatistics["Tool ID"]
                    .SaveCountAdd1(report.ToolType.GetValueOrDefault().ToString())
                    .HideAllChildren();

            if (report.ToolSerial.HasValue)
                AdditionalStatistics["Tool Serial"]
                    .SaveCountAdd1(report.ToolSerial.GetValueOrDefault().ToString())
                    .HideAllChildren();

            if (report.ToolType.HasValue)
                AdditionalStatistics["Tool Type"]
                    .SaveCountAdd1(report.ToolType.GetValueOrDefault().ToString())
                    .HideAllChildren();

            if (report.AuxButtons is { Length: > 0 })
                AdditionalStatistics["Aux Buttons"]
                    .SaveButtons(report.AuxButtons.Span[..(int)(value.Tablet.Properties.Specifications.AuxiliaryButtons?.ButtonCount ?? 0)]);

            if (report.Tilt.HasValue)
                AdditionalStatistics["Tilt Axes"]
                    .SaveMinMax(report.Tilt.GetValueOrDefault());

            if (report.WheelPositions is { Span: var absWheelPosSpan })
                for (int i = 0; i < absWheelPosSpan.Length; i++)
                    if (absWheelPosSpan[i].HasValue)
                        AdditionalStatistics[$"Abs. Wheel {i} Position"]
                            .SaveMinMax(absWheelPosSpan[i]!.GetValueOrDefault());

            if (report.WheelDeltas is { Span: var relWheelPosSpan })
                for (int i = 0; i < relWheelPosSpan.Length; i++)
                    if (relWheelPosSpan[i] != 0)
                        AdditionalStatistics[$"Rel. Wheel {i} Delta"].
                            SaveMinMax(relWheelPosSpan[i]);

            if (report.WheelButtons is { Span: var wheelButtonSpan })
                for (int i = 0; i < wheelButtonSpan.Length; i++)
                    AdditionalStatistics[$"Wheel {i} Buttons"].
                        SaveButtons(wheelButtonSpan[i].Span[..(int)(value.Tablet.Properties.Specifications.Wheels?[i].ButtonCount ?? 0)]);

            _lastBytes = [.. report.Raw];
            RaiseChanged(nameof(RawTabletData));
            DecodedTabletData = ReportFormatter.GetStringFormat(in report);
            HandleDataRecording(value, in report, timeDelta);
        }
    }

    public string DeviceName => ReportData?.Tablet.Properties.Name ?? string.Empty;

    private readonly Queue<double> _reportRates = new();
    private void HandleReportInterval(TimeSpan timeDelta)
    {
        _reportRates.Enqueue(timeDelta.TotalMilliseconds);
        if (_reportRates.Count > 100)
            _reportRates.Dequeue();

        RaiseChanged(nameof(ReportRateString));
    }

    private readonly Statistic _additionalStatistics = new("Additional Statistics");

    public Statistic AdditionalStatistics
    {
        get => _additionalStatistics;
        init => RaiseAndSetIfChanged(ref _additionalStatistics, value);
    }

    private double ReportRateAverage => 1000 / (_reportRates.Count > 0 ? _reportRates.Average() : 0);
    public string ReportRateString => $"{ReportRateAverage:0.00}";

    public string RawTabletData
    {
        get
        {
            if (_lastBytes == null) return string.Empty;

            return DecodingMode switch
            {
                DecodingMode.Hex => ReportFormatter.GetStringRawAsHex(_lastBytes),
                DecodingMode.Binary => ReportFormatter.GetStringRawAsBinary(_lastBytes),
                _ => throw new ArgumentOutOfRangeException(nameof(DecodingMode)),
            };
        }
    }

    private string _decodedTabletData = string.Empty;
    public string DecodedTabletData
    {
        get => _decodedTabletData;
        private set => RaiseAndSetIfChanged(ref _decodedTabletData, value);
    }

    private DecodingMode _decodingMode = _DEFAULT_DECODING_MODE;
    public DecodingMode DecodingMode
    {
        get => _decodingMode;
        set
        {
            RaiseAndSetIfChanged(ref _decodingMode, value);
            RaiseChanged(nameof(RawTabletData));
        }
    }

    private int _reportsRecorded;
    public int ReportsRecorded
    {
        get => _reportsRecorded;
        private set
        {
            RaiseAndSetIfChanged(ref _reportsRecorded, value);

            if (value > 0 && !HasReportsRecorded)
                HasReportsRecorded = true;
        }
    }

    private bool _hasReportsRecorded;
    public bool HasReportsRecorded
    {
        get => _hasReportsRecorded;
        private set => RaiseAndSetIfChanged(ref _hasReportsRecorded, value);
    }

    private bool _dataRecordingEnabled;
    public bool DataRecordingEnabled
    {
        get => _dataRecordingEnabled;
        [UsedImplicitly]
        set
        {
            RaiseAndSetIfChanged(ref _dataRecordingEnabled, value);

            if (value)
            {
                ReportsRecorded = 0;
                ResetStatistics();

                string fileName = $"tablet-data_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm-ss}.txt";
                _tabletRecordingFileStream = File.OpenWrite(Path.Join(AppInfo.Current.RecordingDirectory, fileName));
                _tabletRecordingStreamWriter = new StreamWriter(_tabletRecordingFileStream);
            }
            else
                StopDataRecordingAsNeeded();
        }
    }

    private bool _isVisualizerEnabled = true;
    public bool IsVisualizerEnabled
    {
        get => _isVisualizerEnabled;
        set => RaiseAndSetIfChanged(ref _isVisualizerEnabled, value);
    }

    private bool _showAdditionalStatistics = true;
    public bool ShowAdditionalStatistics
    {
        get => _showAdditionalStatistics;
        set => RaiseAndSetIfChanged(ref _showAdditionalStatistics, value);
    }

    public ReadOnlyCollection<string> SeenReports => _seenReports.ToArray().AsReadOnly();
    // TODO: make this an ObservableHashSet so that we can reset AdditionalStats on changes
    public HashSet<string> IgnoredReports { get; } = [];

    public HashSet<string> IgnoredTablets { get; } = [];

    #endregion

    #region Class Functions

    private void HandleDataRecording(DebugReportData reportData, scoped in DeviceReport report, TimeSpan timeDelta)
    {
        if (!DataRecordingEnabled || _tabletRecordingStreamWriter == null)
            return;

        string output = ReportFormatter.GetStringFormatOneLine(reportData.Tablet.Properties,
            in report,
            timeDelta,
            reportData.Path);

        _tabletRecordingStreamWriter.WriteLine(output);
        ReportsRecorded++;
    }

    #endregion

    #region Static Functions

    private static string GetNameKeyForFilter(TabletReference tabletReference, string path) =>
        $"{tabletReference.Properties.Name}: {path}";

    #endregion

    #region Cleanup/Management

    public void ResetStatistics()
    {
        _additionalStatistics.Children.Clear();
    }

    private void StopDataRecordingAsNeeded()
    {
        // dump stats
        if (_tabletRecordingStreamWriter != null && _additionalStatistics.Children.Count > 0)
            foreach (string s in _additionalStatistics.DumpTreeAsStrings())
                _tabletRecordingStreamWriter?.WriteLine(s);

        _tabletRecordingStreamWriter?.Dispose();
        _tabletRecordingStreamWriter = null;
        _tabletRecordingFileStream?.Dispose();
        _tabletRecordingFileStream = null;
    }

    public void Dispose() => StopDataRecordingAsNeeded();

    #endregion
}

