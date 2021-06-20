using System;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Generic.Reflection;
using IBinding = OpenTabletDriver.Plugin.IBinding;

namespace OpenTabletDriver.UX.Windows.Bindings
{
    public class AdvancedBindingEditorDialog : Dialog<PluginSettingStore>
    {
        public AdvancedBindingEditorDialog(PluginSettingStore currentBinding = null)
        {
            Title = "Advanced Binding Editor";
            Result = currentBinding;
            Padding = 5;

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Width = 500,
                Height = 450,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new StackLayout
                        {
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            Items =
                            {
                                new Group
                                {
                                    Text = "Type",
                                    Content = bindingTypeDropDown = new TypeDropDown<IBinding>()
                                },
                                settingStoreEditor
                            }
                        }
                    },
                    new StackLayoutItem
                    {
                        Control = new StackLayout
                        {
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

            bindingTypeDropDown.SelectedItemBinding.Convert(t => new PluginSettingStore(t)).Bind(settingStoreEditor.StoreBinding);
            bindingTypeDropDown.SelectedItem = currentBinding?.GetPluginReference().GetTypeReference();
        }

        private TypeDropDown<IBinding> bindingTypeDropDown;
        private PluginSettingStoreEditor<IBinding> settingStoreEditor = new PluginSettingStoreEditor<IBinding>();

        private void ClearBinding(object sender, EventArgs e)
        {
            Close(null);
        }

        private void ApplyBinding(object sender, EventArgs e)
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
