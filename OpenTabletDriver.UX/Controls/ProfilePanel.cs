using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.UX.Components;

namespace OpenTabletDriver.UX.Controls
{
    public class ProfilePanel : DesktopPanel
    {
        private readonly IControlBuilder _controlBuilder;

        public ProfilePanel(IDriverDaemon daemon, IControlBuilder controlBuilder, App app)
        {
            _controlBuilder = controlBuilder;

            var outputPage = CreatePage<DigitizerPanel>("Digitizer");
            var filtersPage = CreatePage<FiltersPanel>("Filters");
            var penPage = CreatePage<PenPanel>("Pen");
            var auxPage = CreatePage<AuxPanel>("Auxiliary");
            var mousePage = CreatePage<MousePanel>("Mouse");
            var toolsPage = CreatePage<ToolsPanel>("Tools");
            var logPage = CreatePage<LogViewer>("Log");
            var placeholderPage = new TabPage
            {
                Text = "Info",
                Padding = 5,
                Content = new Placeholder("No tablet is selected. Make sure your tablet is connected.")
            };

            var tabControl = new TabControl
            {
                Pages =
                {
                    logPage
                }
            };

            daemon.Message += (_, m) => Application.Instance.AsyncInvoke(() =>
            {
                if (m.Level > LogLevel.Info)
                    tabControl.SelectedPage = logPage;
            });

            Content = new Panel
            {
                Padding = new Padding(5, 5, 5, 0),
                Content = tabControl
            };

            DataContextChanged += delegate
            {
                var pages = tabControl.Pages;
                pages.Clear();

                if (DataContext is Profile profile && app.Tablets.Any())
                {
                    var tablet = app.Tablets.First(t => t.Name == profile.Tablet);
                    var specifications = tablet.Specifications;

                    if (specifications.Digitizer != null)
                    {
                        pages.Add(outputPage);
                        pages.Add(filtersPage);
                    }

                    if (specifications.Pen != null)
                        pages.Add(penPage);

                    if (specifications.AuxiliaryButtons != null)
                        pages.Add(auxPage);

                    if (specifications.MouseButtons != null)
                        pages.Add(mousePage);

                    pages.Add(toolsPage);
                }
                else
                {
                    pages.Add(placeholderPage);
                    tabControl.SelectedPage = placeholderPage;
                }

                pages.Add(logPage);
            };
        }

        private TabPage CreatePage<TControl>(string text) where TControl : Control
        {
            return new TabPage
            {
                Text = text,
                Padding = 5,
                Content = _controlBuilder.Build<TControl>()
            };
        }
    }
}
