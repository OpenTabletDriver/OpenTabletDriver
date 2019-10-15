using System.Collections.Generic;
using TabletDriverLib.Class;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using System.IO;

namespace OpenTabletDriverGUI.ViewModels
{
    public class ConfigurationManagerViewModel : ViewModelBase
    {
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

        public void Delete(TabletProperties tablet)
        {
            Configurations.Remove(tablet);
        }

        public void DeleteSelected() => Delete(Selected);

        public async Task SaveAs(TabletProperties tablet)
        {
            var dialog = DialogTools.CreateSaveFileDialog($"Saving tablet '{tablet.TabletName}'", "XML Document", "xml");
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