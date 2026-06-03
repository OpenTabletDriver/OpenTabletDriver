using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection.Metadata;
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
                    // Get the plugin's updated metadata from the repo
                    updatedMetadata = GetRepoMetadataForPlugin(PluginMetadataList.Repository, Metadata!, CurrentDriverVersion).FirstOrDefault() ?? Metadata;
                    return updatedMetadata != Metadata;
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
                            Text = "Creator",
                            Content = creator = new Label()
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
                            Text = "Max Supported Driver Version",
                            Content = maxDriverVersion = new Label()
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

            name.TextBinding.Bind(MetadataBinding.Child(c => c!.Name));
            owner.TextBinding.Bind(MetadataBinding.Child(c => c!.Owner));
            creator.TextBinding.Bind(MetadataBinding.Child(c => c!.Creator ?? c.Owner));
            description.TextBinding.Bind(MetadataBinding.Child(c => c!.Description));
            driverVersion.TextBinding.Bind(MetadataBinding.Child(c => c!.SupportedDriverVersion).Convert(v => v?.ToString()));
            maxDriverVersion.TextBinding.Bind(MetadataBinding.Child(c => c!.MaxSupportedDriverVersion).Convert(v => v?.ToString() ?? "N/A"));
            pluginVersion.TextBinding.Bind(MetadataBinding.Child(c => c!.PluginVersion).Convert(v => v?.ToString()));
            license.TextBinding.Bind(MetadataBinding.Child(c => c!.LicenseIdentifier));

            sourceCode.GetEnabledBinding().Bind(MetadataBinding.Child(c => c!.RepositoryUrl).Convert(c => c != null));
            sourceCode.Click += (_, _) => DesktopInterop.Open(Metadata!.RepositoryUrl!);

            wiki.GetEnabledBinding().Bind(MetadataBinding.Child(c => c!.WikiUrl).Convert(c => c != null));
            wiki.Click += (_, _) => DesktopInterop.Open(Metadata!.WikiUrl!);

            AppInfo.PluginManager.AssembliesChanged += HandleAssembliesChanged;
        }

        private Control content;
        private StackLayout actions;
        private Placeholder placeholder = new()
        {
            Text = "No plugin selected.",
        };

        private Label name, owner, creator, description, driverVersion, maxDriverVersion, pluginVersion, license;
        private Button sourceCode, wiki;

        private Button uninstallButton, installButton;

        // TODO: CurrentDriverVersion should look up version from daemon?
        private readonly Version CurrentDriverVersion = Assembly.GetExecutingAssembly().GetName().Version!;

        public event Func<PluginMetadata, Task<bool>>? RequestPluginInstall;
        public event Func<PluginMetadata, Task<bool>>? RequestPluginUninstall;

        private PluginMetadata? updatedMetadata;
        private PluginMetadata? metadata;
        public PluginMetadata? Metadata
        {
            set
            {
                this.metadata = value;
                this.OnMetadataChanged();
            }
            get => this.metadata;
        }

        public event EventHandler<EventArgs>? MetadataChanged;

        protected virtual void OnMetadataChanged()
        {
            MetadataChanged?.Invoke(this, new EventArgs());

            this.Content = Metadata != null ? content : placeholder;
        }

        public BindableBinding<MetadataViewer, PluginMetadata?> MetadataBinding
        {
            get
            {
                return new BindableBinding<MetadataViewer, PluginMetadata?>(
                    this,
                    c => c.Metadata,
                    (c, v) => c.Metadata = v,
                    (c, h) => c.MetadataChanged += h,
                    (c, h) => c.MetadataChanged -= h
                );
            }
        }

        private void HandleAssembliesChanged(object? sender, EventArgs e) => Application.Instance.AsyncInvoke(() =>
        {
            MetadataBinding.Update();
        });

        private async void InstallHandler(object? sender, EventArgs e)
        {
            Debug.Assert(updatedMetadata != null);
            this.ParentWindow.Enabled = false;

            if (RequestPluginInstall != null)
                await RequestPluginInstall.Invoke(updatedMetadata);

            this.ParentWindow.Enabled = true;
        }

        private async void UninstallHandler(object? sender, EventArgs e)
        {
            Debug.Assert(Metadata != null);
            this.ParentWindow.Enabled = false;

            if (RequestPluginUninstall != null)
                await RequestPluginUninstall.Invoke(Metadata);

            this.ParentWindow.Enabled = true;
        }

        private static IEnumerable<PluginMetadata> GetRepoMetadataForPlugin(PluginMetadataCollection? repo, PluginMetadata? localMetadata, Version currentDriverVersion)
        {
            if (repo == null)
                return Enumerable.Empty<PluginMetadata>();

            return from meta in repo
                   where PluginMetadata.Match(meta, localMetadata) // find plugin match
                   where localMetadata == null || // assume valid if no local metadata
                         localMetadata.PluginVersion == null || // assume valid if no local plugin version
                         meta.PluginVersion > localMetadata.PluginVersion // only show actual upgrades where data is available
                   where currentDriverVersion >= meta.SupportedDriverVersion // must be supported by current driver version
                   where meta.MaxSupportedDriverVersion == null || // assume valid if upstream doesn't define a version max
                         currentDriverVersion <= meta.MaxSupportedDriverVersion // current driver must be equal or less to the version max
                   orderby meta.PluginVersion descending // newest first
                   select meta;
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
    }
}
