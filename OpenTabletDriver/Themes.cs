using System;
using Avalonia.Markup.Xaml.Styling;

namespace OpenTabletDriver
{
    public class Themes
    {
        public static StyleInclude Parse(string name)
        {
            switch (name?.ToLower())
            {
                case "light":
                    return LightTheme;
                case "dark":
                    return DarkTheme;
                default:
                    throw new ArgumentException("Invalid style name: " + name);
            }
        }

        public static StyleInclude LightTheme = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
        {
            Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default")
        };

        public static StyleInclude DarkTheme = new StyleInclude(new Uri("resm:Styles?assembly=ControlCatalog"))
        {
            Source = new Uri("resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default")
        };
    }
}