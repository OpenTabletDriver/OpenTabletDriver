using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Controls
{
    public class TabletSwitcherPanel : Panel
    {
        public TabletSwitcherPanel()
        {
            base.Content = new StackLayout
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
                                    Control = tabletSwitcher,
                                },
                                new StackLayoutItem(null, true),
                                new StackLayoutItem
                                {
                                    Control = commandsPanel,
                                }
                            }
                        }
                    }
                }
            };

            controlPanel.ProfileBinding.Bind(tabletSwitcher.SelectedValueBinding.Cast<Profile?>());

            tabletSwitcher.ProfilesBinding.BindDataContext<App>(a => a.Settings.Profiles);

            App.Driver.TabletsChanged += HandleTabletsChanged;
            // ReSharper disable once AsyncVoidMethod
            Application.Instance.AsyncInvoke(async void () => HandleTabletsChanged(this, await App.Driver.Instance!.GetTablets()));
        }

        private TabletSwitcher tabletSwitcher = new();
        private ControlPanel controlPanel;
        private Panel commandsPanel = new();

        public Control CommandsControl
        {
            set => commandsPanel.Content = value;
            get => commandsPanel.Content;
        }

        private void HandleTabletsChanged(object? sender, IEnumerable<TabletReference> tablets)
        {
            tabletSwitcher.HandleTabletsChanged(sender, tablets.ToImmutableArray());
        }

        private class TabletSwitcher : DropDown
        {
            public TabletSwitcher()
            {
                this.ItemTextBinding = Binding.Property<Profile, string>(t => t.Tablet);
                this.DataStore = visibleProfiles;
                this.SelectedIndex = 0;
            }

            private readonly ObservableCollection<Profile> visibleProfiles = new ObservableCollection<Profile>();

            private ProfileCollection profiles = [];
            public ProfileCollection Profiles
            {
                set
                {
                    this.profiles = value;
                    // ReSharper disable once AsyncVoidMethod
                    Application.Instance.AsyncInvoke(async void () => await this.OnProfilesChanged());
                }
                get => this.profiles;
            }

            public event EventHandler<EventArgs>? ProfilesChanged;

            protected virtual async Task OnProfilesChanged()
            {
                ProfilesChanged?.Invoke(this, EventArgs.Empty);
                var tablets = await App.Driver.Instance!.GetTablets();
                HandleTabletsChanged(this, tablets.ToImmutableArray());
            }

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

            public void HandleTabletsChanged(object? _, IList<TabletReference> tablets)
            {
                visibleProfiles.Clear();

                foreach (var tablet in tablets)
                    visibleProfiles.Add(Profiles[tablet]);

                if (visibleProfiles.Any())
                {
                    if (this.SelectedIndex < 0)
                    {
                        this.SelectedIndex = 0;
                        this.OnSelectedValueChanged(EventArgs.Empty);
                    }

                    this.Enabled = true;
                }
                else
                {
                    this.SelectedValue = null;
                    this.OnSelectedValueChanged(EventArgs.Empty);
                    this.Enabled = false;
                }
            }
        }
    }
}
