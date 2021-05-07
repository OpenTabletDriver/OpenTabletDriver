using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HidSharp;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver
{
    /// <summary>
    /// A container for a single tablet device that handles all information and functionality related to the tablet.
    /// </summary>
    public class TabletHandler : ITabletHandler
    {
        internal TabletHandler(HidDevice digitizerDevice, HidDevice auxilaryDevice)
        {
            this.digitizerDevice = digitizerDevice;
            this.auxilaryDevice = auxilaryDevice;
            InstanceID = GetHandlerID();
        }

        private static readonly Dictionary<string, Func<IReportParser<IDeviceReport>>> reportParserDict = new Dictionary<string, Func<IReportParser<IDeviceReport>>>
        {
            { typeof(TabletReportParser).FullName, () => new TabletReportParser() },
            { typeof(AuxReportParser).FullName, () => new AuxReportParser() },
            { typeof(TiltTabletReportParser).FullName, () => new TiltTabletReportParser() },
            { typeof(Vendors.SkipByteTabletReportParser).FullName, () => new Vendors.SkipByteTabletReportParser() },
            { typeof(Vendors.UCLogic.UCLogicReportParser).FullName, () => new Vendors.UCLogic.UCLogicReportParser() },
            { typeof(Vendors.Huion.GianoReportParser).FullName, () => new Vendors.Huion.GianoReportParser() },
            { typeof(Vendors.Wacom.BambooReportParser).FullName, () => new Vendors.Wacom.BambooReportParser() },
            { typeof(Vendors.Wacom.IntuosV2ReportParser).FullName, () => new Vendors.Wacom.IntuosV2ReportParser() },
            { typeof(Vendors.Wacom.IntuosV3ReportParser).FullName, () => new Vendors.Wacom.IntuosV3ReportParser() },
            { typeof(Vendors.Wacom.WacomDriverIntuosV2ReportParser).FullName, () => new Vendors.Wacom.WacomDriverIntuosV2ReportParser() },
            { typeof(Vendors.Wacom.WacomDriverIntuosV3ReportParser).FullName, () => new Vendors.Wacom.WacomDriverIntuosV3ReportParser() },
            { typeof(Vendors.XP_Pen.XP_PenReportParser).FullName, () => new Vendors.XP_Pen.XP_PenReportParser() },
            { typeof(Vendors.XP_Pen.XP_PenTiltReportParser).FullName, () => new Vendors.XP_Pen.XP_PenTiltReportParser() }
        };

        private static int id = 1;

        private bool initialized;
        private HidDevice digitizerDevice;
        private HidDevice auxilaryDevice;

        /// <summary>
        /// Is raised when tablet is disconnected.
        /// </summary>
        public event EventHandler<TabletHandlerID> Disconnected;

        /// <summary>
        /// Contains the whole information for the tablet.
        /// </summary>
        public TabletState TabletState { get; init; }

        /// <summary>
        /// Unique ID for identifying the correct <see cref="TabletHandler"/> during TabletHandler modifications.
        /// </summary>
        public TabletHandlerID InstanceID { get; init; }

        /// <summary>
        /// Input pipeline used by the tablet.
        /// </summary>
        public IOutputMode OutputMode { get; set; }

        /// <summary>
        /// Whether the tablet reports will be pushed to the input pipeline.
        /// </summary>
        public bool EnableInput { get; set; }

        /// <summary>
        /// Reader class for the tablet's digitizer component.
        /// </summary>
        public DeviceReader<IDeviceReport> DigitizerReader { get; private set; }

        /// <summary>
        /// Reader class for the tablet's auxilary component, an endpoint separate from the tablet's digitizer.
        /// </summary>
        public DeviceReader<IDeviceReport> AuxilaryReader { get; private set; }

        /// <summary>
        /// Determines how a report will be handled.
        /// </summary>
        public Action<IOutputMode, IDeviceReport> HandleReport { get; set; } = (outputMode, report) =>
        {
            outputMode.Read(report);
        };

        /// <summary>
        /// Retrieve and construct the report parser for an identifier.
        /// </summary>
        /// <param name="identifier">The identifier to retrieve the report parser path from.</param>
        public static Func<DeviceIdentifier, IReportParser<IDeviceReport>> GetReportParser { get; set; } = (identifier) =>
        {
            return reportParserDict[identifier.ReportParser].Invoke();
        };

        /// <summary>
        /// Retrieves a new instance ID to assign to the <see cref="TabletHandler"/>.
        /// </summary>
        /// <returns>An ID that is guaranteed unique during runtime.</returns>
        private static TabletHandlerID GetHandlerID()
        {
            return new TabletHandlerID { Value = id++ };
        }

        /// <summary>
        /// Initializes the tablet to their vendor interface mode if needed be, and opens a handle to the device for reading.
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            if (initialized)
                throw new Exception($"{this} is already initialized");

            if (TabletState.Digitizer is not null)
            {
                var digitizerParser = GetReportParser(TabletState.Digitizer) ?? new TabletReportParser();
                InitializeDigitizerDevice(digitizerParser);
            }

            if (TabletState.Auxiliary is not null)
            {
                var auxParser = GetReportParser(TabletState.Auxiliary) ?? new AuxReportParser();
                InitializeAuxDevice(auxParser);
            }

            initialized = true;
            return true;
        }

        private void InitializeDigitizerDevice(IReportParser<IDeviceReport> reportParser)
        {
            Log.Debug("Detect", $"{InstanceID}: Initializing {TabletState.Properties.Name} digitizer endpoint with path: {digitizerDevice.DevicePath}");
            Log.Debug("Detect", $"{InstanceID}: Using report parser type '{reportParser.GetType().FullName}'.");

            var identifier = TabletState.Digitizer;
            foreach (byte index in identifier.InitializationStrings)
            {
                Log.Debug("Device", $"{InstanceID}: Initializing index {index}");
                digitizerDevice.GetDeviceString(index);
            }

            DigitizerReader = new DeviceReader<IDeviceReport>(InstanceID, digitizerDevice, reportParser);
            DigitizerReader.Report += OnReportReceived;
            DigitizerReader.ReadingChanged += OnReadingChanged;

            if (identifier.FeatureInitReport != null)
            {
                foreach (var featureInitReport in identifier.FeatureInitReport.Where(f => f != null && f.Length > 0))
                {
                    try
                    {
                        DigitizerReader.ReportStream.SetFeature(featureInitReport);
                        Log.Debug("Device", $"{InstanceID}: Set digitizer feature: " + BitConverter.ToString(featureInitReport));
                    }
                    catch
                    {
                        Log.Debug("Device", $"{InstanceID}: Digitizer refused to process feature report: " + BitConverter.ToString(featureInitReport));
                    }
                }
            }

            if (identifier.OutputInitReport != null)
            {
                foreach (var outputInitReport in identifier.OutputInitReport.Where(o => o != null && o.Length > 0))
                {
                    try
                    {
                        DigitizerReader.ReportStream.Write(outputInitReport);
                        Log.Debug("Device", $"{InstanceID}: Set digitizer output: " + BitConverter.ToString(outputInitReport));
                    }
                    catch
                    {
                        Log.Debug("Device", $"{InstanceID}: Digitizer refused to process output report: " + BitConverter.ToString(outputInitReport));
                    }
                }
            }
        }

        private void InitializeAuxDevice(IReportParser<IDeviceReport> reportParser)
        {
            Log.Debug("Detect", $"{InstanceID}: Initializing {TabletState.Properties.Name} auxilary endpoint with path: {auxilaryDevice.DevicePath}");
            Log.Debug("Detect", $"{InstanceID}: Using report parser type '{reportParser.GetType().FullName}'.");

            var identifier = TabletState.Auxiliary;
            foreach (byte index in identifier.InitializationStrings)
            {
                Log.Debug("Device", $"{InstanceID}: Initializing index {index}");
                auxilaryDevice.GetDeviceString(index);
            }

            AuxilaryReader = new DeviceReader<IDeviceReport>(InstanceID, auxilaryDevice, reportParser);
            AuxilaryReader.Report += OnReportReceived;
            AuxilaryReader.ReadingChanged += OnReadingChanged;

            if (identifier.FeatureInitReport != null)
            {
                foreach (var featureInitReport in identifier.FeatureInitReport.Where(f => f != null && f.Length > 0))
                {
                    try
                    {
                        AuxilaryReader.ReportStream.SetFeature(featureInitReport);
                        Log.Debug("Device", $"{InstanceID}: Set auxilary feature: " + BitConverter.ToString(featureInitReport));
                    }
                    catch
                    {
                        Log.Debug("Device", $"{InstanceID}: Auxilary refused to process feature report: " + BitConverter.ToString(featureInitReport));
                    }
                }
            }

            if (identifier.OutputInitReport != null)
            {
                foreach (var outputInitReport in identifier.OutputInitReport.Where(o => o != null && o.Length > 0))
                {
                    try
                    {
                        AuxilaryReader.ReportStream.Write(outputInitReport);
                        Log.Debug("Device", $"{InstanceID}: Set auxilary output: " + BitConverter.ToString(outputInitReport));
                    }
                    catch
                    {
                        Log.Debug("Device", $"{InstanceID}: Auxilary refused to process output report: " + BitConverter.ToString(outputInitReport));
                    }
                }
            }
        }

        private void OnReportReceived(object _, IDeviceReport report)
        {
            if (EnableInput && OutputMode?.Tablet != null)
                HandleReport(OutputMode, report);
        }

        private void OnReadingChanged(object sender, bool isReading)
        {
            if (!isReading && sender is DeviceReader<IDeviceReport> reader)
            {
                switch (reader.ReadingChangeException)
                {
                    case ObjectDisposedException dex:
                        Log.Debug("Device", $"{(string.IsNullOrWhiteSpace(dex.ObjectName) ? "A device stream" : dex.ObjectName)} was disposed from {this}.");
                        break;
                    case IOException ioex when (ioex.Message == "I/O disconnected." || ioex.Message == "Operation failed after some time.") && reader == DigitizerReader:
                        Log.Write("Device", $"{this} has disconnected");
                        break;
                    case ArgumentOutOfRangeException:
                        Log.Write("Device", $"{this} has sent an incomplete report data. Was it disconnected?");
                        break;
                    case Exception ex:
                        Log.Exception(ex);
                        break;
                }

                if (sender == DigitizerReader)
                    Disconnected?.Invoke(this, InstanceID);
            }
        }

        public override string ToString()
        {
            return $"[{InstanceID.Value}]: '{TabletState.Properties.Name}'";
        }

        public void Dispose()
        {
            DigitizerReader?.Dispose();
            AuxilaryReader?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}