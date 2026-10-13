using System;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Reflection;
using IBinding = OpenTabletDriver.Plugin.IBinding;

namespace OpenTabletDriver.UX.Windows.Bindings
{
    public class AdvancedBindingEditorDialog : Dialog<PluginSettingStore?>
    {
        public AdvancedBindingEditorDialog(PluginSettingStore? currentBinding = null)
        {
            Title = "Advanced Binding Editor";
            Result = currentBinding?.Clone();
            Padding = 5;
            base.Size = new Eto.Drawing.Size(500, 450);
            Resizable = true;

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new Group
                    {
                        Text = "Type",
                        Content = bindingTypeDropDown = new TypeDropDown<IBinding>()
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Scrollable
                        {
                            Padding = 5,
                            Border = BorderType.None,
                            Content = settingsScrollableContent = new StackLayout
                            {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                Width = 1, // forces HorizontalAlignment.Stretch to start at 1 instead of the default size which eliminates the Scrollable's horizontal scrollbar
                                Items =
                                {
                                    settingStoreEditor,
                                }
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
                            Padding = 5,
                            Orientation = Orientation.Horizontal,
                            VerticalContentAlignment = VerticalAlignment.Bottom,
                            Spacing = 5,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new Button(ClearBinding)
                                    {
                                        Text = "Clear"
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new Button(ApplyBinding)
                                    {
                                        Text = "Apply"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            bindingTypeDropDown.SelectedItemBinding.Convert(t => new PluginSettingStore(t)).Bind(settingStoreEditor.StoreBinding!);
            bindingTypeDropDown.SelectedItem = currentBinding?.GetTypeInfo();
            settingStoreEditor.Store = currentBinding;

            settingsScrollableContent.SizeChanged += (_, _) => settingStoreEditor.ResizeLayoutItems();

            // store starting value to be able to potentially recover it later
            initialStoreTypeInfo = Result?.GetTypeInfo(); // caching this saves a little bit of time on recoveries

            settingStoreEditor.StoreChanged += OnSettingStoreEditorOnStoreChanged;
        }

        private readonly TypeInfo? initialStoreTypeInfo;

        private readonly TypeDropDown<IBinding> bindingTypeDropDown;
        private readonly PluginSettingStoreEditor<IBinding> settingStoreEditor = new();
        private readonly StackLayout settingsScrollableContent;

        private bool _recoveringStore = false;

        private void OnSettingStoreEditorOnStoreChanged(object? sender, EventArgs args)
        {
            // restore binding settings when user goes back to the initial type
            if (!_recoveringStore // avoid recursion
                && Result != null && initialStoreTypeInfo != null) // avoid restoring null
            {
                var storeTypeInfo = settingStoreEditor.Store?.GetTypeInfo();

                if (storeTypeInfo == initialStoreTypeInfo)
                {
                    _recoveringStore = true;
                    settingStoreEditor.Store = Result.Clone();
                    _recoveringStore = false;
                }
            }
        }

        private void ClearBinding(object? sender, EventArgs e)
        {
            Close(null);
        }

        private void ApplyBinding(object? sender, EventArgs e)
        {
            if (bindingTypeDropDown.SelectedItem == null)
            {
                Close(null);
                return;
            }

            Close(settingStoreEditor.Store);
        }
    }
}
