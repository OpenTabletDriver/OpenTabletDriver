using System;
using System.Collections.Generic;
using System.Text;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public static class ControlBuilder
    {
        public static GroupBox MakeGroup(string groupName, Control content)
        {
            return new GroupBox
            {
                Text = groupName,
                Padding = App.GroupBoxPadding,
                Content = content
            };
        }

        public static StackLayout MakeStack(params Control[] controls) => MakeStack((IEnumerable<Control>)controls);
        public static StackLayout MakeStack(IEnumerable<Control> controls)
        {
            var stack = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Spacing = 5
            };
            
            foreach (var ctrl in controls)
                stack.Items.Add(MakeStackItem(ctrl));

            return stack;
        }

        public static StackLayoutItem MakeStackItem(Control ctrl, HorizontalAlignment alignment = HorizontalAlignment.Stretch)
        {
            return new StackLayoutItem
            {
                HorizontalAlignment = alignment,
                Control = ctrl
            };
        }

        public static void AddControl(this StackLayout stack, Control control, HorizontalAlignment alignment = HorizontalAlignment.Stretch)
        {
            stack.Items.Add(MakeStackItem(control, alignment));
        }

        public static void AddControls(this StackLayout stack, IEnumerable<Control> controls, HorizontalAlignment alignment = HorizontalAlignment.Stretch)
        {
            foreach (var ctrl in controls)
                stack.AddControl(ctrl, alignment);
        }

        public static GroupBox MakeInput(string groupName, Func<string> getValue, Action<string> setValue, string placeholder = null)
        {
            var textBox = new TextBox
            {
                PlaceholderText = placeholder
            };
            textBox.TextBinding.Bind(getValue, setValue);
            return MakeGroup(groupName, textBox);
        }

        public static Control MakeButton(string text, Action action)
        {
            return new Button((s, e) => action())
            {
                Text = text
            };
        }

        public static Expander MakeExpander(string groupName, bool isExpanded, params Control[] controls) => MakeExpanderGeneric(groupName, isExpanded, controls);
        public static Expander MakeExpanderGeneric(string groupName, bool isExpanded, IEnumerable<Control> controls)
        {
            return new Expander
            {
                Header = groupName,
                Content = MakeStack(controls),
                Expanded = isExpanded,
                Padding = new Padding(0, 5, 0, 0)
            };
        }

        public static Control MakeList(
            string groupName,
            Func<IEnumerable<string>> getValue,
            Action<IEnumerable<string>> setValue
        )
        {
            var textArea = new TextArea();
            textArea.TextBinding.Bind(
                () => 
                {
                    StringBuilder buffer = new StringBuilder();
                    foreach (string value in getValue())
                        buffer.AppendLine(value);
                    return buffer.ToString();
                },
                (o) => 
                {
                    setValue(o.Split(Environment.NewLine));
                }
            );
            return MakeGroup(groupName, textArea);
        }

        public static GroupBox MakeDictionary(
            string groupName,
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue
        )
        {
            var entries = new StackLayout
            {
                Spacing = 5,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            
            foreach (var pair in getValue())
                entries.Items.Add(MakeDictionaryEntry(getValue, setValue, pair.Key, pair.Value));

            var actions = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items = 
                {
                    new Button((sender, e) => entries.Items.Add(MakeDictionaryEntry(getValue, setValue)))
                    {
                        Text = "+"
                    }
                }
            };

            var layout = new TableLayout
            {
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow
                    {
                        ScaleHeight = true,
                        Cells = 
                        {
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = entries
                            }
                        }
                    },
                    new TableRow
                    {
                        ScaleHeight = false,
                        Cells =
                        {
                            new TableCell
                            {
                                ScaleWidth = true,
                                Control = actions
                            }
                        }
                    }
                }
            };

            return MakeGroup(groupName, layout);
        }

        public static StackLayoutItem MakeDictionaryEntry(
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue,
            string startKey = null,
            string startValue = null
        )
        {
            var keyBox = new TextBox
            {
                PlaceholderText = "Key",
                ToolTip = 
                    "The dictionary entry's key. This is what is indexed to find a value." + Environment.NewLine +
                    "If left empty, the entry will be removed on save or apply."
            };

            var valueBox = new TextBox
            {
                PlaceholderText = "Value",
                ToolTip = "The dictionary entry's value. This is what is retrieved when indexing with the specified key."
            };

            string oldKey = startKey;
            keyBox.TextBinding.Bind(
                () => startKey,
                (key) =>
                {
                    var dict = getValue();
                    var value = valueBox.Text;

                    if (string.IsNullOrWhiteSpace(key))
                        dict.Remove(key);
                    else if (!dict.TryAdd(key, value))
                        dict[key] = value;
                    
                    if (oldKey != null)
                        dict.Remove(oldKey);
                    oldKey = key;

                    setValue(dict);
                }
            );

            valueBox.TextBinding.Bind(
                () => startValue,
                (value) =>
                {
                    var dict = getValue();
                    var key = keyBox.Text;

                    if (!dict.TryAdd(key, value))
                        dict[key] = value;

                    setValue(dict);
                }
            );

            var stackLayout = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = keyBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = valueBox,
                        Expand = true
                    }
                }
            };
            
            return new StackLayoutItem(stackLayout, true);
        }
    }
}