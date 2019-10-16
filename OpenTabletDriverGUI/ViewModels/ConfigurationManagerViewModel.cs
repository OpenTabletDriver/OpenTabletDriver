using System.Collections.Generic;
using TabletDriverLib.Class;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.IO;
using HidSharp;

namespace OpenTabletDriverGUI.ViewModels
{
    public class ConfigurationManagerViewModel : ViewModelBase
    {
        private ObservableCollection<HidDevice> _devices;
        public ObservableCollection<HidDevice> Devices
        {
            set => this.RaiseAndSetIfChanged(ref _devices, value);
            get => _devices;
        }

        private ObservableCollection<TabletProperties> _cfgs;
        public ObservableCollection<TabletProperties> Configurations
        {
            set => this.RaiseAndSetIfChanged(ref _cfgs, value);
            get => _cfgs;
        }

        private TabletProperties _selected;
        public TabletProperties Selected
        {
            set => this.RaiseAndSetIfChanged(ref _selected, value);
            get => _selected;
        }

        private HidDevice _device;
        public HidDevice SelectedDevice
        {
            set => this.RaiseAndSetIfChanged(ref _device, value);
            get => _device;
        }

        public void New()
        {
            if (Configurations == null)
                Configurations = new ObservableCollection<TabletProperties>();
            var config = new TabletProperties()
            {
                TabletName = "Tablet"
            };
            Configurations.Add(config);
            Selected = config;
        }
        
        public void CreateFrom(HidDevice device)
        {
            var config = new TabletProperties()
            {
                TabletName = $"{device.GetManufacturer()} {device.GetFriendlyName()}".Trim(),
                VendorID = device.VendorID,
                ProductID = device.ProductID,
            };
            Configurations.Add(config);
            Selected = config;
        }

        public void Delete(TabletProperties tablet)
        {
            Configurations.Remove(tablet);
        }

        public void DeleteSelected() => Delete(Selected);

        public async Task SaveAs(TabletProperties tablet)
        {
            var dialog = FileDialogs.CreateSaveFileDialog($"Saving tablet '{tablet.TabletName}'", "XML Document", "xml");
            var result = await dialog.ShowAsync(App.Current.MainWindow);
            if (result != null)
            {
                var file = new FileInfo(result);
                tablet.Write(file);
                Log.Info($"Saved tablet configuration to '{file.FullName}'.");
            }
        }
    }
}