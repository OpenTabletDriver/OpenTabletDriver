using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.Interop;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Plugins
{
    public class MetadataViewer : Panel
    {
        public MetadataViewer()
        {
            actions = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = uninstallButton = new Button(UninstallHandler)
                        {
                            Text = "Uninstall"
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = installButton = new Button(InstallHandler)
                    }
                }
            };

            var installedBinding = new DelegateBinding<bool>(
                () =>
                {
                    var contexts = AppInfo.PluginManager.GetLoadedPlugins();
                    return contexts.Any(t => PluginMetadata.Match(t.GetMetadata(), Metadata));
                },
                addChangeEvent: (e) => MetadataChanged += e,
                removeChangeEvent: (e) => MetadataChanged -= e
            );

            var updateableBinding = new DelegateBinding<bool>(
                () =>
                {
                    var repo = PluginMetadataList.Repository;
                    if (repo == null)
                        return false;

                    var updatableFromRepository = from meta in repo
                        where PluginMetadata.Match(meta, Metadata)
                        where meta.PluginVersion > Metadata.PluginVersion
                        where CurrentDriverVersion >= meta.SupportedDriverVersion
                        orderby meta.PluginVersion descending
                        select meta;

                    return updatableFromRepository.Any();
                },
                addChangeEvent: (e) => MetadataChanged += e,
                removeChangeEvent: (e) => MetadataChanged -= e
            );

            var installableBinding = new DelegateBinding<bool>(
                () => updateableBinding.GetValue() || !installedBinding.GetValue(),
                addChangeEvent: (e) => MetadataChanged += e,
                removeChangeEvent: (e) => MetadataChanged += e
            );

            uninstallButton.GetEnabledBinding().Bind(installedBinding);
            installButton.TextBinding.Bind(updateableBinding.Convert(c => c ? "Update" : "Install"));
            installButton.GetEnabledBinding().Bind(installableBinding);

            content = new Scrollable
            {
                Content = new StackLayout
                {
                    Padding = 5,
                    Spacing = 5,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new AlignedGroup
                        {
                            Text = "Name",
                            Content = name = new Label()
                        },
                        new AlignedGroup
                        {
                            Text = "Owner",
                            Content = owner = new Label()
                        },
                        new AlignedGroup
                        {
                            Text = "Description",
                            Content = description = new Label
                            {
                                Wrap = WrapMode.Word
                            }
                        },
                        new AlignedGroup
                        {
                            Text = "Driver Version",
                            Content = driverVersion = new Label()
                        },
                        new AlignedGroup
                        {
                            Text = "Plugin Version",
                            Content = pluginVersion = new Label()
                        },
                        new AlignedGroup
                        {
                            Text = "Source Code Repository",
                            Content = sourceCode = new Button
                            {
                                Width = 175,
                                Text = "Show source code"
                            }
                        },
                        new AlignedGroup
                        {
                            Text = "Wiki",
                            Content = wiki = new Button
                            {
                                Width = 175,
                                Text = "Show plugin wiki"
                            }
                        },
                        new AlignedGroup
                        {
                            Text = "License",
                            Content = license = new Label()
                        },
                        new StackLayoutItem(null, true),
                        actions
                    }
                }
            };

            name.TextBinding.Bind(MetadataBinding.Child(c => c.Name));
            owner.TextBinding.Bind(MetadataBinding.Child(c => c.Owner));
            description.TextBinding.Bind(MetadataBinding.Child(c => c.Description));
            driverVersion.TextBinding.Bind(MetadataBinding.Child(c => c.SupportedDriverVersion).Convert(v => v?.ToString()));
            pluginVersion.TextBinding.Bind(MetadataBinding.Child(c => c.PluginVersion).Convert(v => v?.ToString()));
            license.TextBinding.Bind(MetadataBinding.Child(c => c.LicenseIdentifier));
            
            sourceCode.GetEnabledBinding().Bind(MetadataBinding.Child(c => c.RepositoryUrl).Convert(c => c != null));
            sourceCode.Click += (sender, e) => SystemInterop.Open(Metadata.RepositoryUrl);

            wiki.GetEnabledBinding().Bind(MetadataBinding.Child(c => c.WikiUrl).Convert(c => c != null));
            wiki.Click += (sender, e) => SystemInterop.Open(Metadata.WikiUrl);

            AppInfo.PluginManager.AssembliesChanged += HandleAssembliesChanged;
        }

        private Control content;
        private StackLayout actions;
        private Placeholder placeholder;

        private Label name, owner, description, driverVersion, pluginVersion, license;
        private Button sourceCode, wiki;
        
        private Version CurrentDriverVersion = Assembly.GetExecutingAssembly().GetName().Version;
        private Button uninstallButton, installButton;

        public event Func<PluginMetadata, Task<bool>> RequestPluginInstall;
        public event Func<PluginMetadata, Task<bool>> RequestPluginUninstall;

        private PluginMetadata metadata;
        public PluginMetadata Metadata
        {
            set
            {
                this.metadata = value;
                this.OnMetadataChanged();
            }
            get => this.metadata;
        }

        public event EventHandler<EventArgs> MetadataChanged;

        protected virtual void OnMetadataChanged()
        {
            MetadataChanged?.Invoke(this, new EventArgs());

            var contexts = AppInfo.PluginManager.GetLoadedPlugins();

            bool isInstalled = contexts.Any(t => PluginMetadata.Match(t.GetMetadata(), metadata));

            this.Content = Metadata != null ? content : placeholder ??= new Placeholder
            {
                Text = "No plugin selected."
            };
        }

        public BindableBinding<MetadataViewer, PluginMetadata> MetadataBinding
        {
            get
            {
                return new BindableBinding<MetadataViewer, PluginMetadata>(
                    this,
                    c => c.Metadata,
                    (c, v) => c.Metadata = v,
                    (c, h) => c.MetadataChanged += h,
                    (c, h) => c.MetadataChanged -= h
                );
            }
        }

        private void HandleAssembliesChanged(object sender, EventArgs e) => Application.Instance.AsyncInvoke(() =>
        {
            MetadataBinding.Update();
        });

        private async void InstallHandler(object sender, EventArgs e)
        {
            this.ParentWindow.Enabled = false;

            await RequestPluginInstall?.Invoke(Metadata);

            this.ParentWindow.Enabled = true;
        }

        private async void UninstallHandler(object sender, EventArgs e)
        {
            this.ParentWindow.Enabled = false;
            
            await RequestPluginUninstall?.Invoke(Metadata);

            this.ParentWindow.Enabled = true;
        }

        private class AlignedGroup : Group
        {
            public AlignedGroup()
            {
                base.Content = panel = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Padding = 5,
                    Items =
                    {
                        new StackLayoutItem
                        {
                            Control = container = new Panel()
                        }
                    }
                };
                this.Orientation = Orientation.Horizontal;
            }

            private StackLayout panel;
            private Panel container;

            public new Control Content
            {
                set => container.Content = value;
                get => container.Content;
            }
        }

        private class LinkButtonGroup : Group
        {
            public LinkButtonGroup(string header, string link, string text = null)
            {
                var linkButton = new Button
                {
                    Text = text ?? header,
                    Width = 175,
                    Enabled = !string.IsNullOrEmpty(link)
                };
                linkButton.Click += (sender, e) => SystemInterop.Open(link);

                this.Text = header;
                this.Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Items =
                    {
                        linkButton
                    }
                };
                this.Orientation = Orientation.Horizontal;
            }
        }
    }
}
