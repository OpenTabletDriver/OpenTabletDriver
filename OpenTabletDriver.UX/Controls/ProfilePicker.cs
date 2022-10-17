using System.Collections.Immutable;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;

namespace OpenTabletDriver.UX.Controls
{
    public class ProfilePicker : DropDown
    {
        private readonly App _app;

        public ProfilePicker(App app)
        {
            _app = app;

            ItemTextBinding = Binding.Property<Profile, string>(profile => profile.Tablet);
            UpdateItems();

            app.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(App.Tablets):
                    case nameof(App.Settings):
                        UpdateItems();
                        break;
                }
            };
        }

        private void UpdateItems()
        {
            var index = SelectedIndex;

            var profiles = _app.Settings.Profiles;
            var tablets = _app.Tablets.Select(t => t.ToString());

            var visibleProfiles = profiles.Where(p => tablets.Contains(p.Tablet)).ToImmutableArray();

            Application.Instance.AsyncInvoke(() =>
            {
                DataStore = visibleProfiles;

                if (SelectedIndex == -1 && visibleProfiles.Any())
                    SelectedIndex = Math.Clamp(index, 0, visibleProfiles.Length - 1);

                OnSelectedValueChanged(EventArgs.Empty);
            });
        }
    }
}
