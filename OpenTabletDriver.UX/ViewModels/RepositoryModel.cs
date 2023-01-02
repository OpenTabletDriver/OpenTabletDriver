using System.ComponentModel;

namespace OpenTabletDriver.UX.ViewModels
{
    public class RepositoryModel : NotifyPropertyChanged
    {
        private string _owner = "OpenTabletDriver";
        private string _name = "Plugin-Repository";
        private string _gitRef = "master";

        public string Owner
        {
            set => RaiseAndSetIfChanged(ref _owner!, value);
            get => _owner;
        }

        public string Name
        {
            set => RaiseAndSetIfChanged(ref _name!, value);
            get => _name;
        }

        [DisplayName("Git Ref")]
        public string GitRef
        {
            set => RaiseAndSetIfChanged(ref _gitRef!, value);
            get => _gitRef;
        }
    }
}
