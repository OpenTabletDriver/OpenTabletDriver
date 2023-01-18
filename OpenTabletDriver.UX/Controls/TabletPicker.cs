using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public class TabletPicker : DropDown
    {
        private readonly App _app;

        public TabletPicker(App app)
        {
            _app = app;

            ItemTextBinding = Binding.Property<TabletHandler, string>(tablet => tablet.Name);
            UpdateItems();

            app.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(App.Tablets))
                    UpdateItems();
            };
        }

        private void UpdateItems()
        {
            var index = SelectedIndex;
            var handlers = _app.Tablets;

            Application.Instance.AsyncInvoke(() =>
            {
                DataStore = handlers;

                if (SelectedIndex == -1 && handlers.Any())
                    SelectedIndex = Math.Clamp(index, 0, handlers.Length - 1);

                OnSelectedValueChanged(EventArgs.Empty);
            });
        }
    }
}
