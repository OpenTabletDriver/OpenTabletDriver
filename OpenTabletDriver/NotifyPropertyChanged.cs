using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenTabletDriver
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaiseAndSetIfChanged<T>(ref T? obj, T? newValue, [CallerMemberName] string propertyName = "")
        {
            obj = newValue;
            RaiseChanged(propertyName);
        }

        private void RaiseChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
