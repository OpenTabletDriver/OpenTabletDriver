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

/// <summary>
/// Tablet debugger view model for visualizing and/or pulling statistics from <see cref="DebugReportData"/>, a JSON-RPC compatible aggregate class of <see cref="TabletReference"/> and an <see cref="IDeviceReport"/>.
/// Inject your report data into <see cref="ReportData"/> (or use the <see cref="HandleReport"/> event handler) to enable visualizing and collecting statistics.
/// </summary>
public sealed class TabletDebuggerViewModel : ViewModel, IDisposable
{
    private const DecodingMode _DEFAULT_DECODING_MODE = DecodingMode.Hex;

    private readonly HPETDeltaStopwatch _stopwatch = new();
    private readonly HashSet<string> _seenReports = [];

    private FileStream? _tabletRecordingFileStream;
    private TextWriter? _tabletRecordingStreamWriter;
    private TextWriter? _tabletRecordingDetailsStreamWriter;

    public NotifyCollectionChangedEventHandler? StatisticsCollectionChanged;

    // disable warning telling us to dispose these - these are provided directly by the user (e.g. stdout) and is undesirable to dispose.
#pragma warning disable CA2213
    private readonly TextWriter? _userDefinedTabletRecordingTextWriter;
    private readonly TextWriter? _userDefinedTabletRecordingTextDetailsWriter;
#pragma warning restore CA2213

    /// <summary>
    /// Tablet debugger view model with the default data recording writer (into <see cref="AppInfo.RecordingDirectory"/>).
    /// </summary>
    public TabletDebuggerViewModel()
    {
        AdditionalStatistics.ChildCollectionChanged += (sender, args) => StatisticsCollectionChanged?.Invoke(sender, args);
    }

    /// <summary>
    /// Tablet debugger view model with a custom data recording writer.
    /// </summary>
    /// <param name="tabletRecordingWriter">The writer to write decoded per-line tablet data output to.</param>
    /// <param name="tabletRecordingDetailsWriter">(optional) The writer to write finalized statistics to. If <c>null</c>, no finalized statistics will be emitted.</param>
    /// <remarks>Don't forget to enable <see cref="DataRecordingEnabled"/> to emit events. The class will NOT dispose your supplied <paramref name="tabletRecordingWriter"/> nor <paramref name="tabletRecordingDetailsWriter"/>.</remarks>
    public TabletDebuggerViewModel(TextWriter tabletRecordingWriter, TextWriter? tabletRecordingDetailsWriter = null) : this()
    {
        _userDefinedTabletRecordingTextWriter = tabletRecordingWriter;
        _userDefinedTabletRecordingTextDetailsWriter = tabletRecordingDetailsWriter;
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

    public Statistic AdditionalStatistics { get; } = new("Additional Statistics");

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

    public string DecodedTabletData
    {
        get;
        private set => RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public DecodingMode DecodingMode
    {
        get;
        [PublicAPI]
        set
        {
            RaiseAndSetIfChanged(ref field, value);
            RaiseChanged(nameof(RawTabletData));
        }
    } = _DEFAULT_DECODING_MODE;

    public int ReportsRecorded
    {
        get;
        private set => RaiseAndSetIfChanged(ref field, value);
    }

    private bool _dataRecordingEnabled;
    public bool DataRecordingEnabled
    {
        get => _dataRecordingEnabled;
        set
        {
            if (!(value ^ _dataRecordingEnabled)) return; // only handle state changes, avoids recreating files

            RaiseAndSetIfChanged(ref _dataRecordingEnabled, value);

            if (value)
            {
                ResetStatistics();

                if (_userDefinedTabletRecordingTextWriter == null)
                {
                    string fileName = $"tablet-data_{DateTimeOffset.UtcNow:yyyy-MM-dd_HH-mm-ss}.txt";
                    _tabletRecordingFileStream =
                        File.OpenWrite(Path.Join(AppInfo.Current.RecordingDirectory, fileName));
                    _tabletRecordingDetailsStreamWriter =
                        _tabletRecordingStreamWriter = new StreamWriter(_tabletRecordingFileStream);
                }
                else
                {
                    _tabletRecordingStreamWriter = _userDefinedTabletRecordingTextWriter;
                    _tabletRecordingDetailsStreamWriter = _userDefinedTabletRecordingTextDetailsWriter;
                }
            }
            else
                StopDataRecordingAsNeeded();
        }
    }

    // TODO: move into Eto, only used in Eto GUI
    public bool IsVisualizerEnabled
    {
        get;
        [PublicAPI]
        set => RaiseAndSetIfChanged(ref field, value);
    } = true;

    // TODO: move into Eto, only practically used in Eto GUI. Or repurpose so that disabling skips statistics parsing?
    public bool ShowAdditionalStatistics
    {
        get;
        [PublicAPI]
        set => RaiseAndSetIfChanged(ref field, value);
    } = true;

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

    private void ResetStatistics()
    {
        ReportsRecorded = 0;
        AdditionalStatistics.Children.Clear();
    }

    private void StopDataRecordingAsNeeded()
    {
        if (_tabletRecordingDetailsStreamWriter != null && AdditionalStatistics.Children.Count > 0)
            foreach (string s in AdditionalStatistics.DumpTreeAsStrings())
                _tabletRecordingDetailsStreamWriter.WriteLine(s);

        // we don't own the user-defined writers, skip disposal if the writers matches the users defined writers
        if (_userDefinedTabletRecordingTextDetailsWriter != _tabletRecordingDetailsStreamWriter)
            _tabletRecordingDetailsStreamWriter?.Dispose();
        if (_userDefinedTabletRecordingTextWriter != _tabletRecordingStreamWriter)
            _tabletRecordingStreamWriter?.Dispose();

        _tabletRecordingDetailsStreamWriter = null;
        _tabletRecordingStreamWriter = null;

        _tabletRecordingFileStream?.Dispose();
        _tabletRecordingFileStream = null;
    }

    public void Dispose()
    {
        StopDataRecordingAsNeeded();
        _dataRecordingEnabled = false;
        ResetStatistics();
    }

    #endregion
}

