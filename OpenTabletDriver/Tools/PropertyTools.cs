using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using TabletDriverPlugin.Attributes;

namespace OpenTabletDriver.Tools
{
    internal static class PropertyTools
    {
        public static IEnumerable<IControl> GetPropertyControls(object obj, string bindingPath, Dictionary<string, string> pluginSettings = null)
        {
            if (obj != null)
            {
                foreach (var property in obj.GetType().GetProperties())
                {
                    var attributes = from attr in property.GetCustomAttributes(false)
                                     where attr is PropertyAttribute
                                     select attr as PropertyAttribute;

                    if (pluginSettings != null || pluginSettings?.Count == 0)
                        SetValue(obj, property, pluginSettings);

                    foreach (var attr in attributes)
                    {
                        var title = new TextBlock
                        {
                            Text = attr.DisplayName
                        };

                        var parent = new Border
                        {
                            Classes = { "r" },
                            Child = new StackPanel
                            {
                                Children =
                                {
                                    title,
                                    GetSubcontrol(bindingPath, property, attr)
                                }
                            }
                        };

                        yield return parent;
                    }
                }
            }
        }

        private static IControl GetSubcontrol(string bindingPath, PropertyInfo property, PropertyAttribute attr)
        {
            var subPath = $"{bindingPath}.{property.Name}";
            IControl control;
            if (attr is SliderPropertyAttribute sliderAttr)
            {
                var slider = new Slider
                {
                    Minimum = sliderAttr.Min,
                    Maximum = sliderAttr.Max,
                    TickFrequency = (sliderAttr.Max - sliderAttr.Min) / 10,
                    SmallChange = (sliderAttr.Max - sliderAttr.Min) / 25,
                    LargeChange = (sliderAttr.Max - sliderAttr.Min) / 10,
                    [!Slider.ValueProperty] = new Binding(subPath, BindingMode.TwoWay),
                };
                var tb = GetTextBox(subPath);

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions
                    {
                        new ColumnDefinition(),
                        new ColumnDefinition(125, GridUnitType.Pixel)
                    },
                    Children =
                    {
                        slider,
                        tb
                    }
                };
                for (int i = 0; i < grid.Children.Count; i++)
                    Grid.SetColumn(grid.Children[i] as Control, i);

                control = grid;
            }
            else if (attr is UnitPropertyAttribute unitAttr)
            {
                var tb = GetTextBox(subPath);
                var label = GetLabel(unitAttr.Unit);
                control = new Grid
                {
                    Children = 
                    {
                        tb,
                        label
                    }
                };
            }
            else
            {
                control = GetTextBox(subPath);
            }
            return control;
        }

        private static IControl GetTextBox(string bindingPath)
        {
            return new TextBox
            {
                [!TextBox.TextProperty] = new Binding(bindingPath, BindingMode.TwoWay),
            };
        }

        private static IControl GetLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                IsHitTestVisible = false,
                Margin = new Thickness(0,0,5,0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static void SetValue(object obj, PropertyInfo property, Dictionary<string, string> pluginSettings)
        {
            if (pluginSettings.TryGetValue(property.Name, out var val))
            {
                var converted = Convert.ChangeType(val, property.PropertyType);
                property.SetValue(obj, converted);
            }
        }
    }
}