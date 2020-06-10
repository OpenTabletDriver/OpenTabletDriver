using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenTabletDriver.UX
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged([CallerMemberName] string memberName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }

        internal void RaiseAndSetIfChanged<T>(ref T refVal, T newVal, [CallerMemberName] string memberName = null)
        {
            refVal = newVal;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
    }
}