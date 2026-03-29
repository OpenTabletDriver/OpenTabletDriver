using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public sealed class AuxiliaryBindingEditor : BindingEditor
    {
        public AuxiliaryBindingEditor()
        {
            this.Content = new Scrollable
            {
                Border = BorderType.None,
                Content = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Spacing = 5,
                    Items =
                    {
                        new Group
                        {
                            Text = "Auxiliary",
                            Content = auxButtons = new BindingDisplayList
                            {
                                Prefix = "Auxiliary Binding"
                            }
                        }
                    }
                }
            };

            auxButtons.ItemSourceBinding.Bind(SettingsBinding.Child(c => (IList<PluginSettingStore>)c.AuxButtons));
        }

        public void SetButtonNames(ButtonSpecifications specs)
        {
            auxButtons.ButtonNames = specs?.ButtonNames;
            _hasAuxButtons = specs != null;
        }

        public void SetVisible(bool visible)
        {
            if (visible && _hasAuxButtons)
                EnableDebug();
            else
                DisableDebug();
        }

        protected override void OnUnLoad(EventArgs e)
        {
            DisableDebug();
            base.OnUnLoad(e);
        }

        private void EnableDebug()
        {
            if (!_debugActive)
            {
                _debugActive = true;
                App.Driver.DeviceReport += OnDeviceReport;
                _ = App.Driver.Instance.SetTabletDebug(true);
            }
        }

        private void DisableDebug()
        {
            if (_debugActive)
            {
                _debugActive = false;
                App.Driver.DeviceReport -= OnDeviceReport;
                _ = App.Driver.Instance.SetTabletDebug(false);
            }
        }

        private void OnDeviceReport(object sender, DebugReportData data)
        {
            if (data.ToObject() is IAuxReport auxReport)
            {
                for (int i = 0; i < auxReport.AuxButtons.Length; i++)
                {
                    if (auxReport.AuxButtons[i])
                        auxButtons.HighlightItem(i);
                }
            }
        }

        private BindingDisplayList auxButtons;
        private bool _hasAuxButtons;
        private bool _debugActive;
    }
}
