using HidSharp;

namespace OpenTabletDriver.Tests.Fakes
{
    public class FakeHidDevice : HidDevice
    {
        private string _devicePath = "/dev/fake/hidraw0";
        private bool _canOpen = true;
        private string _manufacturer = "Fake Manufacturer";
        private string _productName = "Fake Product";
        private string _serialNumber = "12345";
        private int _maxInputReportLength = 64;
        private int _maxOutputReportLength = 64;
        private int _maxFeatureReportLength = 64;
        private int _productId;
        private int _vendorId;
        private int _releaseNumberBcd;

        public override string DevicePath => _devicePath;
        public override bool CanOpen => _canOpen;
        public override int ProductID => _productId;
        public override int VendorID => _vendorId;
        public override int ReleaseNumberBcd => _releaseNumberBcd;

        public FakeHidDevice WithDevicePath(string devicePath)
        {
            _devicePath = devicePath;
            return this;
        }

        public FakeHidDevice WithCanOpen(bool canOpen)
        {
            _canOpen = canOpen;
            return this;
        }

        public FakeHidDevice WithManufacturer(string manufacturer)
        {
            _manufacturer = manufacturer;
            return this;
        }

        public FakeHidDevice WithProductName(string productName)
        {
            _productName = productName;
            return this;
        }

        public FakeHidDevice WithSerialNumber(string serialNumber)
        {
            _serialNumber = serialNumber;
            return this;
        }

        public FakeHidDevice WithMaxInputReportLength(int length)
        {
            _maxInputReportLength = length;
            return this;
        }

        public FakeHidDevice WithMaxOutputReportLength(int length)
        {
            _maxOutputReportLength = length;
            return this;
        }

        public FakeHidDevice WithMaxFeatureReportLength(int length)
        {
            _maxFeatureReportLength = length;
            return this;
        }

        public FakeHidDevice WithProductId(int productId)
        {
            _productId = productId;
            return this;
        }

        public FakeHidDevice WithVendorId(int vendorId)
        {
            _vendorId = vendorId;
            return this;
        }

        public FakeHidDevice WithReleaseNumberBcd(int releaseNumberBcd)
        {
            _releaseNumberBcd = releaseNumberBcd;
            return this;
        }

        protected override DeviceStream OpenDeviceDirectly(OpenConfiguration openConfig)
        {
            return null!;
        }

        public override string GetFileSystemName() => _devicePath;

        public override string GetManufacturer() => _manufacturer;

        public override string GetProductName() => _productName;

        public override string GetSerialNumber() => _serialNumber;

        public override int GetMaxInputReportLength() => _maxInputReportLength;

        public override int GetMaxOutputReportLength() => _maxOutputReportLength;

        public override int GetMaxFeatureReportLength() => _maxFeatureReportLength;

        public override string GetDeviceString(int index) => $"DeviceString{index}";
    }
}
