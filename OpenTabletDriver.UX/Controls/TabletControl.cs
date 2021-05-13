using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.UX.Controls
{
    public class TabletControl : StackLayout
    {
        public TabletControl(Control control)
        {
            tabletControl = control;
            tabletControlPanel.Content = tabletControl;
            Padding = 5;
            Spacing = 5;
            Items.Clear();
            Items.Add(new StackLayoutItem(tabletDropDown, HorizontalAlignment.Center));
            Items.Add(new StackLayoutItem(tabletControlPanel, HorizontalAlignment.Stretch, true));

            App.Current.ProfileCache.HandlerInFocusChanged += OnHandlerChanged;

            tabletDropDown.Initialized += (_, _) => App.Current.ProfileCache.HandlerInFocusBinding.Bind(tabletDropDown.SelectedIDBinding);
        }

        private readonly TabletDropDown tabletDropDown = new TabletDropDown() { Width = 300 };
        private readonly Control noTablet = new StackLayout
        {
            Items =
            {
                new StackLayoutItem(null, true),
                new StackLayoutItem
                {
                    Control = new Bitmap(App.Logo.WithSize(256, 256)),
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                new StackLayoutItem
                {
                    Control = "No tablet detected...",
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                new StackLayoutItem(null, true)
            }
        };

        private Control tabletControl;
        private Panel tabletControlPanel = new Panel();

        private void OnHandlerChanged(object _, EventArgs args)
        {
            var show = App.Current.ProfileCache.HandlerInFocus != TabletHandlerID.Invalid;
            tabletDropDown.Visible = show;
            tabletControlPanel.Content = show ? tabletControl : noTablet;
        }
    }
}