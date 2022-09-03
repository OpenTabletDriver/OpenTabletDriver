using Eto.Forms;

namespace OpenTabletDriver.UX.ViewModels
{
    public class DialogModel<T> where T : class
    {
        public DialogModel(DialogResult result, T? model = null)
        {
            Result = result;
            Model = model;
        }

        public DialogResult Result { get; }
        public T? Model { get; }
    }
}
