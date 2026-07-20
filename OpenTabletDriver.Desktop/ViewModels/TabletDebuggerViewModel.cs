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
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.Plugin.Tablet.Wheel;
using OpenTabletDriver.Plugin.Timing;

namespace OpenTabletDriver.Desktop.ViewModels;

public sealed class TabletDebuggerViewModel : ViewModel, IDisposable
{
    private const DecodingMode _DEFAULT_DECODING_MODE = DecodingMode.Hex;

    private readonly HPETDeltaStopwatch _stopwatch = new();
    private readonly HashSet<string> _seenReports = [];

    private FileStream? _tabletRecordingFileStream;
    private StreamWriter? _tabletRecordingStreamWriter;

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

            var dataObject = value.ToObject();

            AdditionalStatistics["Tablets Parsed"].SaveCountAdd1(value.Tablet.Properties.Name)
                .HideAllChildren(); // probably nonsensical to keep in UI

            AdditionalStatistics["Report Parser"].SaveCountAdd1(value.Path)
                .HideAllChildren(); // goes too crazy in UI for now

            if (dataObject is IAbsolutePositionReport absolutePositionReport)
                AdditionalStatistics["Tablet Position"].SaveMinMax(absolutePositionReport.Position);

            if (dataObject is ITouchReport touchReport)
                AdditionalStatistics["Touch Position"].SaveMinMax(touchReport.Touches);

            if (dataObject is IEraserReport eraserReport)
                AdditionalStatistics["Eraser"].SaveButtons([eraserReport.Eraser], 1);

            if (dataObject is ITabletReport tabletReport)
            {
                AdditionalStatistics["Pressure"].SaveMinMax(tabletReport.Pressure);
                AdditionalStatistics["Pen Buttons"].SaveButtons(tabletReport.PenButtons, (int)value.Tablet.Properties.Specifications.Pen.ButtonCount);
            }

            if (dataObject is IMouseReport mouseReport)
            {
                AdditionalStatistics["Mouse Position"]
                    .SaveMinMax(mouseReport.Position);

                AdditionalStatistics["Mouse Scroll"]
                    .SaveMinMax(mouseReport.Scroll);

                AdditionalStatistics["Mouse Buttons"]
                    .SaveButtons(mouseReport.MouseButtons, (int)(value.Tablet.Properties.Specifications.MouseButtons?.ButtonCount ?? 0));
            }

            if (dataObject is IProximityReport proximityReport)
            {
                AdditionalStatistics["Hover Distance"]
                    .SaveMinMax(proximityReport.HoverDistance)
                    .HideAllChildren();

                AdditionalStatistics["Proximity"]
                    .SaveButtons([proximityReport.NearProximity], 1)
                    .HideAllChildren();
            }

            if (dataObject is IToolReport toolReport)
            {
                AdditionalStatistics["Tool ID"]
                    .SaveCountAdd1(toolReport.RawToolID.ToString())
                    .HideAllChildren();

                AdditionalStatistics["Tool Serial"]
                    .SaveCountAdd1(toolReport.Serial.ToString())
                    .HideAllChildren();

                AdditionalStatistics["Tool Type"]
                    .SaveCountAdd1(toolReport.Tool.ToString())
                    .HideAllChildren();
            }

            if (dataObject is IAuxReport auxReport)
                if (auxReport.AuxButtons.Length > 0)
                    AdditionalStatistics["Aux Buttons"]
                        .SaveButtons(auxReport.AuxButtons,
                            (int)(value.Tablet.Properties.Specifications.AuxiliaryButtons?.ButtonCount ?? 0));

            if (dataObject is ITiltReport tiltReport)
                AdditionalStatistics["Tilt Axes"]
                    .SaveMinMax(tiltReport.Tilt);

            if (dataObject is IAbsoluteWheelReport absoluteWheelReport)
                for (int i = 0; i < absoluteWheelReport.AnalogPositions.Length; i++)
                    if (absoluteWheelReport.AnalogPositions[i].HasValue)
                        AdditionalStatistics[$"Abs. Wheel {i} Position"]
                            .SaveMinMax(absoluteWheelReport.AnalogPositions[i]!.Value);

            if (dataObject is IRelativeWheelReport relativeWheelReport)
                for (int i = 0; i < relativeWheelReport.AnalogDeltas.Length; i++)
                    if (relativeWheelReport.AnalogDeltas[i] != 0)
                        AdditionalStatistics[$"Rel. Wheel {i} Position"].
                            SaveMinMax(relativeWheelReport.AnalogDeltas[i]);

            if (dataObject is IWheelButtonReport wheelButtonReport)
                for (int i = 0; i < wheelButtonReport.WheelButtons.Length; i++)
                    AdditionalStatistics["Wheel Buttons"].
                        SaveButtons(wheelButtonReport.WheelButtons[i],
                            (int)(value.Tablet.Properties.Specifications.Wheels?[i].ButtonCount ?? 0));

            if (dataObject is IDeviceReport deviceReport)
            {
                _deviceReport = deviceReport;
                RaiseChanged(nameof(RawTabletData));
                DecodedTabletData = ReportFormatter.GetStringFormat(deviceReport);
                HandleDataRecording(value, deviceReport, timeDelta);
            }
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

    private IDeviceReport? _deviceReport;

    public string RawTabletData
    {
        get
        {
            if (_deviceReport == null) return string.Empty;

            return DecodingMode switch
            {
                DecodingMode.Hex => ReportFormatter.GetStringRaw(_deviceReport),
                DecodingMode.Binary => ReportFormatter.GetStringRawAsBinary(_deviceReport),
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

                string fileName = "tablet-data_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".txt";
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

    private void HandleDataRecording(DebugReportData reportData, IDeviceReport report, TimeSpan timeDelta)
    {
        if (!DataRecordingEnabled || _tabletRecordingStreamWriter == null)
            return;

        string? output = ReportFormatter.GetStringFormatOneLine(reportData.Tablet.Properties,
            report,
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

