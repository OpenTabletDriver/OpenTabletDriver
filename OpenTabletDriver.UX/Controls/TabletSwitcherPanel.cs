using System.Collections.Generic;
using System.Linq;
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
                            Padding = new Eto.Drawing.Padding(5, 5, 0, 5),
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
        }

        private TabletSwitcher tabletSwitcher;
        private ControlPanel controlPanel;
        private Panel commandsPanel;

        public Control CommandsControl
        {
            set => commandsPanel.Content = value;
            get => commandsPanel.Content;
        }

        private class TabletSwitcher : DropDown
        {
            public TabletSwitcher()
            {
                App.Driver.Instance.TabletsChanged += HandleTabletsChanged;

                this.ItemTextBinding = Binding.Property<Profile, string>(t => t.Tablet);

                InitializeAsync();
            }

            private async void InitializeAsync()
            {
                HandleTabletsChanged(this, await App.Driver.Instance.GetTablets());
            }

            private void HandleTabletsChanged(object sender, IEnumerable<TabletReference> tablets)
            {
                var profiles = (this.DataContext as App).Settings.Profiles;
                this.DataStore = profiles.Where(p => tablets.Any(t => t.Properties.Name == p.Tablet));

                if (tablets.Any())
                {
                    var tabletsWithoutProfile = tablets.Where(t => profiles.Any(p => p.Tablet == t.Properties.Name));
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