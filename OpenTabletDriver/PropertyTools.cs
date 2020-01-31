using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using TabletDriverPlugin.Attributes;

namespace OpenTabletDriver
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
            if (attr is SliderPropertyAttribute sliderAttr)
            {
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions
                    {
                        new ColumnDefinition(),
                        new ColumnDefinition(125, GridUnitType.Pixel)
                    },
                };
                var slider = new Slider
                {
                    Minimum = sliderAttr.Min,
                    Maximum = sliderAttr.Max,
                    [!Slider.ValueProperty] = new Binding(subPath, BindingMode.TwoWay),                    
                };
                var tb = GetTextBox(subPath);
                grid.Children.Add(slider);
                grid.Children.Add(tb);
                for (int i = 0; i < grid.Children.Count; i++)
                    Grid.SetColumn(grid.Children[i] as Control, i);

                return grid;
            }
            else
            {
                return GetTextBox(subPath);
            }
        }

        private static IControl GetTextBox(string bindingPath)
        {
            return new TextBox
            {
                [!TextBox.TextProperty] = new Binding(bindingPath, BindingMode.TwoWay),
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