using Newtonsoft.Json;

namespace OpenTabletDriver.Daemon.Contracts
{
    public enum VMultiDeviceStatusKind
    {
        Ready,
        Missing,
        Incomplete,
        OpenFailed
    }

    public sealed class VMultiDeviceStatusDto
    {
        [JsonConstructor]
        public VMultiDeviceStatusDto(
            VMultiDeviceStatusKind kind,
            bool isAvailable,
            bool isExtendedDigitizerAvailable,
            string message,
            string downloadUrl
        )
        {
            Kind = kind;
            IsAvailable = isAvailable;
            IsExtendedDigitizerAvailable = isExtendedDigitizerAvailable;
            Message = message;
            DownloadUrl = downloadUrl;
        }

        public VMultiDeviceStatusKind Kind { get; }
        public bool IsAvailable { get; }
        public bool IsExtendedDigitizerAvailable { get; }
        public string Message { get; }
        public string DownloadUrl { get; }
    }
}
