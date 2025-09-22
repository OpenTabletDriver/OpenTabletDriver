using System;
using System.Linq;
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

            var jamesLabelDevelopers = new HoverLabel
            {
                Text = "jamesbt365",
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };

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
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new StackLayout
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                            VerticalContentAlignment = VerticalAlignment.Stretch,
                            Items =
                            {
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new StackLayout
                                    {
                                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                        VerticalContentAlignment = VerticalAlignment.Stretch,
                                        Items =
                                        {
                                            new Label
                                            {
                                                Text = "Developers",
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                                Font = SystemFonts.Bold(FONTSIZE),
                                            },
                                            new Label
                                            {
                                                Text = string.Join(Environment.NewLine, Developers.Take(Array.IndexOf(Developers, "jamesbt365"))),
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                            },
                                            jamesLabelDevelopers,
                                            new Label
                                            {
                                                Text = string.Join(Environment.NewLine, Developers.TakeLast(Developers.Length - Array.IndexOf(Developers, "jamesbt365") - 1)),
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                            },
                                        }
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new StackLayout
                                    {
                                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                        VerticalContentAlignment = VerticalAlignment.Stretch,
                                        Items =
                                        {
                                            new Label
                                            {
                                                Text = "Designers",
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                                Font = SystemFonts.Bold(FONTSIZE),
                                            },
                                            new Label
                                            {
                                                Text = string.Join(Environment.NewLine, Designers),
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                            },
                                        }
                                    }
                                },
                                new StackLayoutItem
                                {
                                    Expand = true,
                                    Control = new StackLayout
                                    {
                                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                        VerticalContentAlignment = VerticalAlignment.Stretch,
                                        Items =
                                        {
                                            new Label
                                            {
                                                Text = "Documenters",
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                                Font = SystemFonts.Bold(FONTSIZE),
                                            },
                                            new Label
                                            {
                                                Text = string.Join(Environment.NewLine, Documenters),
                                                VerticalAlignment = VerticalAlignment.Center,
                                                TextAlignment = TextAlignment.Center,
                                            },
                                        }
                                    }
                                },
                            }
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

            var memoriamTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = SPACING,
                Spacing = SPACING / 2,
                Items =
                {
                    new Label
                    {
                        Text = "In Memory of James",
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Font = SystemFonts.Bold(FONTSIZE),
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new TextArea
                        {
                            TextAlignment = TextAlignment.Center,
                            Text = "In loving memory of Jamesbt365. One of the biggest contributors to OpenTabletDriver and one of the kindest souls.\n\n"
                                + "James was with the OpenTabletDriver team for about 4 and a half years. In that time he sent over a quarter million messages on the OpenTabletDriver Discord server, added and improved hundreds of tablet configurations, commented thousands of times on Github issues, and pushed the project as a whole to new heights.\n\n"
                                + "The countless hours he spent helping and improving OpenTabletDriver for not only its users but also the developers will not be forgotten.\n\n"
                                + "His legacy will live on through the tablets he added support for, the features he merged into the driver, and the enormous impact he had on the community.",
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
                if (args.Key == Keys.J)
                    if (!tabControl.Pages.Any((x) => x.Text == "Memoriam"))
                    {
                        tabControl.Pages.Add(new TabPage(memoriamTabContent) { Text = "Memoriam" });
                    }
            };

            jamesLabelDevelopers.MouseDown += (sender, args) =>
            {
                if (!tabControl.Pages.Any((x) => x.Text == "Memoriam"))
                {
                    tabControl.Pages.Add(new TabPage(memoriamTabContent) { Text = "Memoriam" });
                }
            };
        }
    }
    class HoverLabel : Label
    {
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this.Font = SystemFonts.Bold();
            this.Cursor = new Cursor(CursorType.Pointer);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Font = SystemFonts.Default();
            this.Cursor = new Cursor(CursorType.Default);
            base.OnMouseLeave(e);
        }
    }
}
