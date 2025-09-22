using System;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows
{
    public class AboutWindow : DesktopForm
    {
        const int LARGE_FONTSIZE = 14;
        const int FONTSIZE = LARGE_FONTSIZE - 4;
        const int SPACING = 10;

        private readonly string[] Developers = ["InfinityGhost", "X9VoiD", "gonX", "jamesbt365", "Kuuube", "AkiSakurai"];
        private readonly string[] Designers = ["InfinityGhost"];
        private readonly string[] Documenters = ["InfinityGhost", "gonX", "jamesbt365", "Kuuube"];

        public AboutWindow()
            : base(Application.Instance.MainForm)
        {
            Title = "About OpenTabletDriver";

            var aboutTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Width = 500,
                Padding = SPACING,
                Spacing = SPACING / 2,
                Items =
                {
                    new ImageView
                    {
                        Image = new Bitmap(App.Logo.WithSize(256, 256)),
                    },
                    new Label
                    {
                        Text = "OpenTabletDriver",
                        VerticalAlignment = VerticalAlignment.Center,
                        Font = SystemFonts.Bold(FONTSIZE),
                    },
                    new Label
                    {
                        Text = $"v{App.Version}",
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new Label
                    {
                        Text = "Open source, cross-platform tablet configurator",
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                    new LinkButton
                    {
                        Text = "OpenTabletDriver Github Repository",
                        Command = new Command((s, e) => Application.Instance.Open(App.Website.ToString())),
                    }
                }
            };

            var creditsTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = SPACING,
                Spacing = SPACING / 2,
                Items =
                {
                    new Label
                    {
                        Text = $"OpenTabletDriver v{App.Version} Credits",
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Font = SystemFonts.Bold(FONTSIZE),
                    },
                    new StackLayoutItem {
                        Expand = true,
                        Control = new TextArea
                        {
                            ReadOnly = true,
                            Text = "Developers:" + Environment.NewLine + string.Join(Environment.NewLine, Developers) + Environment.NewLine + Environment.NewLine
                                    + "Designers:" + Environment.NewLine + string.Join(Environment.NewLine, Designers) + Environment.NewLine + Environment.NewLine
                                    + "Documenters:" + Environment.NewLine + string.Join(Environment.NewLine, Documenters)
                        }
                    }
                }
            };

            var licenseTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = SPACING,
                Spacing = SPACING / 2,
                Items =
                {
                    new Label
                    {
                        Text = $"OpenTabletDriver v{App.Version} License",
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Font = SystemFonts.Bold(FONTSIZE),
                    },
                    new StackLayoutItem {
                        Expand = true,
                        Control = new TextArea
                        {
                            ReadOnly = true,
                            Text = App.License,
                        }
                    }
                }
            };

            var tabControl = new TabControl();
            tabControl.Pages.Add(new TabPage(aboutTabContent) { Text = "About" });
            tabControl.Pages.Add(new TabPage(creditsTabContent) { Text = "Credits" });
            tabControl.Pages.Add(new TabPage(licenseTabContent) { Text = "License" });

            this.Content = tabControl;

            this.KeyDown += (sender, args) =>
            {
                if (args.Key == Keys.Escape)
                    this.Close();
            };
        }
    }
}
