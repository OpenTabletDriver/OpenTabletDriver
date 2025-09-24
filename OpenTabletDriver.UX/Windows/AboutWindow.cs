using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly TabPage _memoriamTabPage;

        private const string _jamesText = """
                                          In loving memory of jamesbt365.
                                          One of the biggest contributors to OpenTabletDriver and one of the kindest souls.

                                          James was with the OpenTabletDriver team for about 4 and a half years.

                                          In that time he sent over a quarter million messages on the OpenTabletDriver Discord server, added and improved hundreds of tablet configurations, commented thousands of times on Github issues, and pushed the project as a whole to new heights.

                                          The countless hours he spent helping and improving OpenTabletDriver for not only its users but also the developers will not be forgotten.

                                          His legacy will live on through the tablets he added support for, the features he merged into the driver, and the enormous impact he had on the community.
                                          """;

        public AboutWindow()
            : base(Application.Instance.MainForm)
        {
            Title = "About OpenTabletDriver";

            var tabControl = new TabControl();

            tabControl.Pages.Add(GenerateAboutTabPage());
            tabControl.Pages.Add(GenerateCreditsTabPage());
            tabControl.Pages.Add(GenerateLicenseTabPage());
            tabControl.Pages.Add(_memoriamTabPage = GenerateMemoriamTabPage());

            this.Content = tabControl;

            this.KeyDown += (sender, args) =>
            {
                if (args.Key == Keys.Escape)
                    this.Close();
                if (args.Key == Keys.J)
                    ShowMemoriamTab();
            };
        }

        #region Tab Pages

        private TabPage GenerateAboutTabPage()
        {
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
                        Font = SystemFonts.Bold(LARGE_FONTSIZE),
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
                    },
                    new CommandLabel
                    {
                        Text = "In memory of jamesbt365",
                        VerticalAlignment = VerticalAlignment.Center,
                        Command = new Command((s, e) => ShowMemoriamTab()),
                    },
                }
            };

            return new TabPage(aboutTabContent) { Text = "About" };
        }

        private TabPage GenerateCreditsTabPage()
        {
            var creditsTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = SPACING,
                Spacing = SPACING / 2,
            };

            var creditsTabContentControl = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = SPACING,
                Spacing = SPACING,
                Items =
                {
                    GenerateContributor(Developers, nameof(Developers)),
                    GenerateContributor(Designers, nameof(Designers)),
                    GenerateContributor(Documenters, nameof(Documenters)),
                }
            };

            GenerateGenericStackLayoutItems(ref creditsTabContent,
                $"OpenTabletDriver v{App.Version} Credits",
                creditsTabContentControl);

            return new TabPage(creditsTabContent) { Text = "Credits" };
        }

        private static TabPage GenerateLicenseTabPage()
        {
            var licenseTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Padding = SPACING,
                Spacing = SPACING / 2,
            };

            var licenseTabContentControl = new TextArea
            {
                ReadOnly = true,
                Text = App.License,
            };

            GenerateGenericStackLayoutItems(ref licenseTabContent,
                $"OpenTabletDriver v{App.Version} License",
                licenseTabContentControl);

            return new TabPage(licenseTabContent) { Text = "License" };
        }

        private static TabPage GenerateMemoriamTabPage()
        {
            var memoriamTabContent = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = SPACING,
                Spacing = SPACING / 2,
            };

            var memoriamTabContentControl = new Label
            {
                TextAlignment = TextAlignment.Center,
                Text = _jamesText
            };

            GenerateGenericStackLayoutItems(ref memoriamTabContent,
                "In Memory of James",
                memoriamTabContentControl);

            return new TabPage(memoriamTabContent) { Text = "Memoriam", Visible = false };
        }

        #endregion Tab Pages

        private void ShowMemoriamTab()
        {
            Debug.Assert(_memoriamTabPage != null);
            _memoriamTabPage.Visible = true;
        }

        private StackLayoutItem GenerateContributor(IEnumerable<string> contributors, string title) =>
            new()
            {
                Expand = true,
                Control = new StackLayout
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Items =
                    {
                        new GroupBox
                        {
                            Text = title,
                            Font = SystemFonts.Bold(LARGE_FONTSIZE),
                            Padding = SPACING,
                            Content = new LabelList(contributors, [
                                new CommandLabel
                                {
                                    Text = "jamesbt365",
                                    VerticalAlignment = VerticalAlignment.Center,
                                    TextAlignment = TextAlignment.Center,
                                    Command = new Command((s, e) => ShowMemoriamTab()),
                                }
                            ])
                            {
                                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                                VerticalContentAlignment = VerticalAlignment.Stretch,
                            }
                        }
                    }
                }
            };

        private static void GenerateGenericStackLayoutItems(ref StackLayout stackLayout, string title, Control control)
        {
            stackLayout.Items.Add(new Label
            {
                Text = title,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Font = SystemFonts.Bold(LARGE_FONTSIZE)
            });

            stackLayout.Items.Add(new StackLayoutItem
            {
                Expand = true,
                Control = control
            });
        }
    }
    class CommandLabel : Label
    {
        public required Command Command;

        private static readonly Cursor _pointerCursor = new(CursorType.Pointer);
        private static readonly Cursor _defaultCursor = new(CursorType.Default);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Command.Execute();
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this.Font = SystemFonts.Bold();
            this.Cursor = _pointerCursor;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Font = SystemFonts.Default();
            this.Cursor = _defaultCursor;
            base.OnMouseLeave(e);
        }
    }

    class LabelList : StackLayout
    {
        public LabelList(IEnumerable<string> textArray, CommandLabel[] commandLabels)
        {
            foreach (string text in textArray)
            {
                var commandLabel = Array.Find(commandLabels, (x) => x.Text == text);
                if (commandLabel != null)
                {
                    Items.Add(commandLabel);
                }
                else
                {
                    Items.Add(new Label
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        Text = text,
                    });
                }
            }
        }
    }
}
