using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    using static ParseTools;

    public class DeviceIdentifierEntry<T> : CollectionEntry<List<T>> where T : DeviceIdentifier
    {
        public DeviceIdentifierEntry(
            Func<List<T>> getValue,
            Action<List<T>> setValue,
            string name,
            int index,
            Type reportParser
        ) : base(getValue, setValue)
        {
            this.name = name;
            this.Index = index;
            this.reportParser = reportParser;
            Build();
        }

        public int Index { set; get; }

        private string name;
        protected Type reportParser;

        protected override void Build()
        {
            var container = new StackView();
            foreach (var control in GetControls())
                container.AddControl(control);

            base.controlContainer = new ExpanderBase(name, false)
            {
                Content = container
            };

            base.contentAlignment = Eto.Forms.VerticalAlignment.Top;
            base.deleteButtonAlignment = Eto.Forms.VerticalAlignment.Top;

            base.Build();
        }

        protected override void OnDestroy()
        {
            ModifyValue(source =>
            {
                if (source.Count > this.Index)
                    source.RemoveAt(this.Index);
            });
            base.OnDestroy();
        }

        protected T GetCurrent()
        {
            var source = base.getValue();
            if (source.Count > this.Index)
                return source[this.Index];
            else
                return null;
        }

        protected void ModifyCurrent(Action<T> modify)
        {
            var current = GetCurrent();
            modify(current);
            ModifyValue(source =>
            {
                if (source.Count > this.Index)
                    source[this.Index] = current;
                else
                    source.Add(current);
            });
        }

        protected virtual IEnumerable<Control> GetControls()
        {
            yield return new InputBox("Vendor ID",
                () => GetCurrent().VendorID.ToString(),
                (o) => ModifyCurrent(id => id.VendorID = ToInt(o))
            );
            yield return new InputBox("Product ID",
                () => GetCurrent().ProductID.ToString(),
                (o) => ModifyCurrent(id => id.ProductID = ToInt(o))
            );
            yield return new InputBox("Input Report Length",
                () => GetCurrent().InputReportLength.ToString(),
                (o) => ModifyCurrent(id => id.InputReportLength = ToNullableUInt(o))
            );
            yield return new InputBox("Output Report Length",
                () => GetCurrent().OutputReportLength.ToString(),
                (o) => ModifyCurrent(id => id.OutputReportLength = ToNullableUInt(o))
            );

            var reportParser = new TypeDropDown<IReportParser<IDeviceReport>>();
            reportParser.SelectedKeyBinding.Bind(
                () => GetCurrent().ReportParser,
                (o) => ModifyCurrent(id => id.ReportParser = o)
            );
            yield return new Group
            {
                Orientation = Orientation.Horizontal,
                Text = "Report Parser",
                Content = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalContentAlignment = Eto.Forms.HorizontalAlignment.Right,
                    Items =
                    {
                        new PaddingSpacerItem(),
                        reportParser
                    }
                }
            };

            yield return new InputBox("Button Count",
                () => GetCurrent().ButtonCount.ToString(),
                (o) => ModifyCurrent(id => id.ButtonCount = ToUInt(o))
            );
            yield return new InputBox("Feature Initialization Report",
                () => ToHexString(GetCurrent().FeatureInitReport),
                (o) => ModifyCurrent(id => id.FeatureInitReport = ToByteArray(o))
            );
            yield return new InputBox("Output Initialization Report",
                () => ToHexString(GetCurrent().OutputInitReport),
                (o) => ModifyCurrent(id => id.OutputInitReport = ToByteArray(o))
            );
            yield return new DictionaryEditor("Device Strings",
                () =>
                {
                    var dictionaryBuffer = new Dictionary<string, string>();
                    foreach (var pair in GetCurrent().DeviceStrings)
                        dictionaryBuffer.Add($"{pair.Key}", pair.Value);
                    return dictionaryBuffer;
                },
                (o) =>
                {
                    ModifyCurrent(id =>
                    {
                        id.DeviceStrings.Clear();
                        foreach (KeyValuePair<string, string> pair in o)
                            if (byte.TryParse(pair.Key, out var keyByte))
                                id.DeviceStrings.Add(keyByte, pair.Value);
                    });
                }
            );
            yield return new ListEditor("Initialization String Indexes",
                () =>
                {
                    var listBuffer = new List<string>();
                    foreach (var value in GetCurrent().InitializationStrings)
                        listBuffer.Add($"{value}");
                    return listBuffer;
                },
                (o) =>
                {
                    ModifyCurrent(id =>
                    {
                        id.InitializationStrings.Clear();
                        foreach (string value in o)
                            if (byte.TryParse(value, out var byteValue))
                                id.InitializationStrings.Add(byteValue);
                            else
                                id.InitializationStrings.Add(0);
                    });
                }
            );
        }
    }
}