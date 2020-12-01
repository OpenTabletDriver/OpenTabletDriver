using System;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls
{
    public class SensitivityEditor : StackView
    {
        public SensitivityEditor()
        {
            base.Orientation = Orientation.Horizontal;
            base.VerticalContentAlignment = VerticalAlignment.Top;

            UpdateBindings();
            App.SettingsChanged += (s) => UpdateBindings();
        }

        public void UpdateBindings()
        {
            this.Items.Clear();
            
            var xSensBox = new SensitivityEditorBox(
                "X Sensitivity",
                (s) => App.Settings.XSensitivity = float.TryParse(s, out var val) ? val : 0f,
                () => App.Settings?.XSensitivity.ToString(),
                "px/mm"
            );
            AddControl(xSensBox, true);

            var ySensBox = new SensitivityEditorBox(
                "Y Sensitivity",
                (s) => App.Settings.YSensitivity = float.TryParse(s, out var val) ? val : 0f,
                () => App.Settings?.YSensitivity.ToString(),
                "px/mm"
            );
            AddControl(ySensBox, true);

            var rotationBox = new SensitivityEditorBox(
                "Rotation",
                (s) => App.Settings.RelativeRotation = float.TryParse(s, out var val) ? val : 0f,
                () => App.Settings?.RelativeRotation.ToString(),
                "°"
            );
            AddControl(rotationBox, true);

            var resetTimeBox = new SensitivityEditorBox(
                "Reset Time",
                (s) => App.Settings.ResetTime = TimeSpan.TryParse(s, out var val) ? val : TimeSpan.FromMilliseconds(100),
                () => App.Settings?.ResetTime.ToString()
            );
            AddControl(resetTimeBox, true);
        }

        private class SensitivityEditorBox : GroupBoxBase
        {
            public SensitivityEditorBox(
                string header,
                Action<string> setValue,
                Func<string> getValue,
                string unit = null
            )
            {
                this.Text = header;
                this.setValue = setValue;
                this.getValue = getValue;

                var layout = new StackView
                {
                    Orientation = Orientation.Horizontal,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                layout.AddControl(textBox, true);

                if (unit != null)
                {
                    var unitControl = new Label
                    {
                        Text = unit,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    layout.AddControl(unitControl);
                }
                
                UpdateBindings();
                App.SettingsChanged += (Settings) => UpdateBindings();
                this.Content = layout;
            }

            private Action<string> setValue;
            private Func<string> getValue;

            private TextBox textBox = new TextBox();

            private void UpdateBindings()
            {
                textBox.TextBinding.Bind(getValue, setValue);
            }
        }
    }
}
