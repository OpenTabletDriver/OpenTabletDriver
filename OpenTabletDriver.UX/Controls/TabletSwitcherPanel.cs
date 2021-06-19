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

            App.Driver.AddConnectionHook(i => i.TabletsChanged += HandleTabletsChanged);
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

        private void HandleTabletsChanged(object sender, IEnumerable<TabletReference> tablets) => Application.Instance.AsyncInvoke(() =>
        {
            this.Content = tablets.Any() ? layout : placeholder ??= new Placeholder
            {
                Text = "No tablets are detected."
            };
            tabletSwitcher.HandleTabletsChanged(sender, tablets);
        });

        private class TabletSwitcher : DropDown
        {
            public TabletSwitcher()
            {
                this.ItemTextBinding = Binding.Property<Profile, string>(t => t.Tablet);
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

            protected virtual async void OnProfilesChanged()
            {
                ProfilesChanged?.Invoke(this, new EventArgs());
                HandleTabletsChanged(this, await App.Driver.Instance.GetTablets());
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

            public void HandleTabletsChanged(object sender, IEnumerable<TabletReference> tablets) => Application.Instance.AsyncInvoke(() =>
            {
                if (Profiles is ProfileCollection profiles)
                {
                    this.DataStore = from profile in profiles
                        where tablets.Any(t => t.Properties.Name == profile.Tablet)
                        orderby profile.Tablet
                        select profile;

                    if (tablets.Any())
                    {
                        var tabletsWithoutProfile = from tablet in tablets 
                            where !profiles.Any(p => p.Tablet == tablet.Properties.Name)
                            select tablet;

                        foreach (var tablet in tabletsWithoutProfile)
                            profiles.Generate(tablet);

                        this.SelectedValue ??= this.DataStore.FirstOrDefault();
                    }
                    else
                    {
                        this.SelectedIndex = -1;
                    }
                }
            });
        }
    }
}