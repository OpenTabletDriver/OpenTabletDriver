using System;
using System.Reflection;
using Eto.Drawing;
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
            Result = currentBinding;
            Padding = 5;

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    new Group
                    {
                        Text = "Binding Type",
                        Content = bindingTypeDropDown = new TypeDropDown<IBinding>()
                    },
                    new Group
                    {
                        Text = "Settings",
                        Content = new StackLayout
                        {
                            Size = new Size(400, -1),
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Items =
                            {
                                settingStoreEditor,
                            },
                        },
                    },
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
                            Padding = 5,
                            Orientation = Orientation.Horizontal,
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
            settingStoreEditor.StoreChanged += OnSettingStoreEditorOnStoreChanged;
            settingStoreEditor.Store = currentBinding;

            // store starting value to be able to potentially recover it later
            initialStore = currentBinding;
            initialStoreTypeInfo = initialStore?.GetTypeInfo(); // caching this saves a little bit of time on recoveries
        }

        private readonly PluginSettingStore? initialStore;
        private readonly TypeInfo? initialStoreTypeInfo;
        private readonly TypeDropDown<IBinding> bindingTypeDropDown;
        private readonly PluginSettingStoreEditor<IBinding> settingStoreEditor = new();

        private void OnSettingStoreEditorOnStoreChanged(object? sender, EventArgs args)
        {
            // restore binding settings in case user goes back to the initial type
            if (settingStoreEditor.Store != initialStore // avoid recursion
                && initialStore != null && initialStoreTypeInfo != null) // avoid restoring null
            {
                var storeTypeInfo = settingStoreEditor.Store?.GetTypeInfo();

                if (storeTypeInfo == initialStoreTypeInfo)
                    settingStoreEditor.Store = initialStore;
            }

            // Size -1 = Eto must rescale axis based on new contents
            settingStoreEditor.Parent.Size = new Size(settingStoreEditor.Parent.Size.Width, -1);
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
