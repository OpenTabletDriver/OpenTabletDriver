using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using TabletDriverPlugin.Attributes;

namespace OpenTabletDriver
{
    internal static class PropertyTools
    {
        public static IEnumerable<AvaloniaObject> GetPropertyControls(object obj, string bindingPath, Dictionary<string, string> pluginSettings = null)
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
                            Margin = new Thickness(5),
                            Padding = new Thickness(5),
                            Classes = { "r" },
                            Child = new StackPanel
                            {
                                Children =
                                {
                                    title,
                                    GetControl(bindingPath, property, attr)
                                }
                            }
                        };

                        yield return parent;
                    }
                }
            }
        }

        private static IControl GetControl(string bindingPath, PropertyInfo property, PropertyAttribute attr)
        {
            var path = bindingPath + "." + property.Name;
            if (attr is SliderPropertyAttribute sliderAttr)
            {
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions
                    {
                        new ColumnDefinition(),
                        new ColumnDefinition(125, GridUnitType.Pixel)
                    },
                    Children =
                    {
                        new Slider
                        {
                            Minimum = sliderAttr.Min,
                            Maximum = sliderAttr.Max,
                            [!Slider.ValueProperty] = new Binding(path, BindingMode.TwoWay)
                        },
                        new TextBox
                        {
                            [!TextBox.TextProperty] = new Binding(path, BindingMode.TwoWay)
                        }
                    }
                };
                for (int i = 0; i < grid.Children.Count; i++)
                    Grid.SetColumn(grid.Children[i] as Control, i);
                return grid;
            }
            else
            {
                return new TextBox()
                {
                    [!TextBox.TextProperty] = new Binding(path, BindingMode.TwoWay)
                };
            }
        }

        private static void SetValue(object obj, PropertyInfo property, Dictionary<string, string> pluginSettings)
        {
            if (pluginSettings.TryGetValue(property.Name, out var val))
            {
                var type = property.PropertyType;
                var converted = Convert.ChangeType(val, type);
                property.SetValue(obj, converted);
            }
        }
    }
}