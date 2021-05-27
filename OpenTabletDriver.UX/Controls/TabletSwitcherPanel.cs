using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Controls
{
    public class TabletSwitcherPanel : Panel
    {
        public TabletSwitcherPanel()
        {
            base.Content = layout = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = controlPanel = new ControlPanel()
                    },
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            Padding = new Padding(5, 5, 0, 5),
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Control = tabletSwitcher = new TabletSwitcher()
                                },
                                new StackLayoutItem(null, true),
                                new StackLayoutItem
                                {
                                    Control = commandsPanel = new Panel()
                                }
                            }
                        }
                    }
                }
            };

            controlPanel.ProfileBinding.Bind(tabletSwitcher.SelectedValueBinding.Cast<Profile>());

            tabletSwitcher.ProfilesBinding.BindDataContext<App>(a => a.Settings.Profiles);

            App.Driver.Instance.TabletsChanged += HandleTabletsChanged;
            Application.Instance.AsyncInvoke(async () => HandleTabletsChanged(this, await App.Driver.Instance.GetTablets()));
        }

        private StackLayout layout;
        private Placeholder placeholder;
        private TabletSwitcher tabletSwitcher;
        private ControlPanel controlPanel;
        private Panel commandsPanel;

        public Control CommandsControl
        {
            set => commandsPanel.Content = value;
            get => commandsPanel.Content;
        }

        private void HandleTabletsChanged(object sender, IEnumerable<TabletReference> tablets)
        {
            this.Content = tablets.Any() ? layout : placeholder ??= new Placeholder
            {
                Text = "No tablets are detected."
            };
        }

        private class TabletSwitcher : DropDown
        {
            public TabletSwitcher()
            {
                App.Driver.Instance.TabletsChanged += HandleTabletsChanged;

                this.ItemTextBinding = Binding.Property<Profile, string>(t => t.Tablet);

                App.Current.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(App.Settings):
                        {
                            UpdateAsync();
                            break;
                        }
                    }
                };

                UpdateAsync();
            }

            private ProfileCollection profiles;
            public ProfileCollection Profiles
            {
                set
                {
                    this.profiles = value;
                    this.OnProfilesChanged();
                }
                get => this.profiles;
            }

            public event EventHandler<EventArgs> ProfilesChanged;

            protected virtual void OnProfilesChanged() => ProfilesChanged?.Invoke(this, new EventArgs());

            public BindableBinding<TabletSwitcher, ProfileCollection> ProfilesBinding
            {
                get
                {
                    return new BindableBinding<TabletSwitcher, ProfileCollection>(
                        this,
                        c => c.Profiles,
                        (c, v) => c.Profiles = v,
                        (c, h) => c.ProfilesChanged += h,
                        (c, h) => c.ProfilesChanged -= h
                    );
                }
            }

            private async void UpdateAsync()
            {
                HandleTabletsChanged(this, await App.Driver.Instance.GetTablets());
            }

            private void HandleTabletsChanged(object sender, IEnumerable<TabletReference> tablets)
            {
                if (Profiles is ProfileCollection profiles)
                {
                    this.DataStore = profiles.Where(p => tablets.Any(t => t.Properties.Name == p.Tablet));

                    if (tablets.Any())
                    {
                        var tabletsWithoutProfile = tablets.Where(t => !profiles.Any(p => p.Tablet == t.Properties.Name));
                        foreach (var tablet in tabletsWithoutProfile)
                            profiles.Generate(tablet);

                        if (this.SelectedIndex == -1)
                            this.SelectedIndex = 0;
                    }
                    else
                    {
                        // Deselect when no tablets are detected
                        this.SelectedIndex = -1;
                    }
                }
            }
        }
    }
}