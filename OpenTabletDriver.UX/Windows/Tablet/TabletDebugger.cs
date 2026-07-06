using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.ViewModels;
using OpenTabletDriver.Desktop.ViewModels.Utility;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Tablet
{
    using TDVM = TabletDebuggerViewModel;

    public sealed class TabletDebugger : DesktopForm
    {
        private const int _LARGE_FONT_SIZE = 14;
        private const int _FONT_SIZE = _LARGE_FONT_SIZE - 4;
        private const int _SPACING = 5;

        // spacing + padding + content padding
        private const int _GROUP_BOX_EXTRA_WIDTH = _SPACING * 6;

        private static readonly Font s_LargeMonospaceFont = Fonts.Monospace(_LARGE_FONT_SIZE);
        private static readonly Font s_MonospaceFont = Fonts.Monospace(_FONT_SIZE);

        private readonly SizeF _rawTabletLineFontSizeHex = s_MonospaceFont.Measure("FF ", 8);
        private readonly SizeF _rawTabletLineFontSizeBinary = s_MonospaceFont.Measure("10101010 ", 4);

        private readonly TabletVisualizer _tabletVisualizer = new();
        private readonly Label _deviceName = new() { Font = s_LargeMonospaceFont };
        private readonly UnitLabel _reportRate = new(s_LargeMonospaceFont, "1234.56", "Hz");

        private readonly Label _reportsRecorded = new() { Font = s_MonospaceFont };
        private readonly Label _tabletData = new() { Font = s_MonospaceFont };

        private readonly Label _rawTabletData = new() { Font = s_MonospaceFont };

        private readonly DebuggerGroup _reportsRecordedGroup = new() { Text = "Reports Recorded" };
        private readonly DebuggerGroup _rawTabletDataGroup;
        private readonly DebuggerGroup _tabletVisualizerGroup = new()
        {
            Text = "Visualizer",
            MinimumSize = new Size(1, 200),
        };
        private readonly Group _additionalStatsGroup = new()
        {
            Text = "Additional Stats",
            Content = new Label { Text = "No stats observed yet" },
        };

        private readonly ButtonMenuItem _debuggedReports = new() { Text = "Debugged Reports", Visible = false };
        private readonly ButtonMenuItem _debuggedTablets = new() { Text = "Debugged Tablets", Visible = false };

        private int AdditionalStatColumnsPerRow { get; set; } = 3;

        public TabletDebugger()
            : base(Application.Instance.MainForm)
        {
            if (App.Driver.IsConnected)
                HandleTabletsChanged(null, App.Driver.Instance.GetTablets().Result);

            var recordingDir = new DirectoryInfo(AppInfo.Current.RecordingDirectory);

            if (!recordingDir.Exists)
            {
                recordingDir.Create();
                Log.Write("TabletDebugger", $"The recording directory '{recordingDir.FullName}' has been created");
            }

            var viewmodel = new TDVM();
            DataContext = viewmodel;

            Content = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Spacing = 5,
                Padding = 5,
                MinimumSize = new Size(1025, 560),
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Scrollable
                        {
                            Border = BorderType.None,
                            Content = new StackLayout
                            {
                                Padding = _SPACING,
                                Spacing = _SPACING,
                                Items =
                                {
                                    new StackLayoutItem
                                    {
                                        Expand = true,
                                        HorizontalAlignment = HorizontalAlignment.Stretch,
                                        Control = _tabletVisualizerGroup,
                                    },
                                    new StackLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Items =
                                        {
                                            new DebuggerGroup
                                            {
                                                Text = "Device",
                                                Content = _deviceName,
                                            },
                                            new DebuggerGroup
                                            {
                                                Text = "Report Rate",
                                                Content = _reportRate,
                                            },
                                        },
                                    },
                                    new StackLayout
                                    {
                                        Orientation = Orientation.Horizontal,
                                        Items = { _additionalStatsGroup },
                                    }
                                },
                            },
                        },
                    },
                    new StackLayout
                    {
                        Padding = _SPACING,
                        Spacing = _SPACING,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Items =
                        {
                            _reportsRecordedGroup,
                            new StackLayoutItem
                            {
                                Expand = true,
                                Control = new DebuggerGroup
                                {
                                    ExpandContent = false,
                                    MinimumSize = new Size(_FONT_SIZE * 22, _FONT_SIZE * 20),
                                    Text = "Tablet Report",
                                    Width = _FONT_SIZE * 22,
                                    Content = _tabletData,
                                },
                            },
                        },
                    },
                    {
                        _rawTabletDataGroup = new DebuggerGroup
                        {
                            Padding = _SPACING,
                            Text = "Raw Tablet Data",
                            Width = GetWidthOfRawTabletDataGroupBox(),
                            MinimumSize = new Size(_FONT_SIZE * 22, _FONT_SIZE * 1),
                            ExpandContent = false,
                            Content = _rawTabletData,
                        }
                    },
                },
            };

            _reportsRecordedGroup.Content = _reportsRecorded;
            _tabletVisualizerGroup.Content = _tabletVisualizer;

            SetupMenu();
            SetupBindings();

            KeyDown += (_, args) =>
            {
                if (args.Key == Keys.Escape)
                    Close();
            };

            viewmodel.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(TDVM.SeenReports) or nameof(TDVM.IgnoredReports):
                        RefreshActiveTabletsMenu();
                        break;
                    case nameof(TDVM.DecodingMode):
                        ResizeRawBasedOnDecodingMode();
                        break;
                    default:
                        return;
                }
            };

            CancellationTokenSource? cts = null;

            viewmodel.StatisticsCollectionChanged += void (sender, args) =>
            {
                cts?.Cancel();
                cts = new CancellationTokenSource();
                // debounce event and parse afterwards, this may fire a lot of times otherwise
                _ = Task.Delay(250, cts.Token).ContinueWith(void (t) =>
                {
                    if (t.IsCompletedSuccessfully)
                        UpdateAdditionalStatisticsFields().ConfigureAwait(false);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            };

            this.SizeChanged += async (_, _) =>
            {
                // readjust AdditionalStats box on window resize
                int oldValue = this.AdditionalStatColumnsPerRow;
                int newValue = this.Width switch
                {
                    >= 1300 => 5,
                    > 1160 => 4,
                    > 1000 => 3,
                    <= 1000 => 2,
                };

                if (oldValue == newValue) return;

                this.AdditionalStatColumnsPerRow = newValue;
                await UpdateAdditionalStatisticsFields().ConfigureAwait(false);
            };

            App.Driver.DeviceReport += viewmodel.HandleReport;
            App.Driver.TabletsChanged += HandleTabletsChanged;
            if (App.Driver.IsConnected)
                App.Driver.Instance.SetTabletDebug(true);
            App.Driver.Connected += SetTabletDebug;
        }

        private static void SetTabletDebug(object? sender, EventArgs e)
        {
            Debug.Assert(App.Driver.IsConnected);
            App.Driver.Instance.SetTabletDebug(true);
        }

        private void SetupBindings()
        {
            _deviceName.TextBinding.BindDataContext((TDVM vm) => vm.DeviceName, DualBindingMode.OneWay);
            _rawTabletData.TextBinding.BindDataContext((TDVM vm) => vm.RawTabletData, DualBindingMode.OneWay);
            _tabletData.TextBinding.BindDataContext((TDVM vm) => vm.DecodedTabletData, DualBindingMode.OneWay);
            _reportRate.TextBinding.BindDataContext((TDVM vm) => vm.ReportRateString, DualBindingMode.OneWay);
            _reportsRecorded.TextBinding.Convert(null, (int value) => $"{value}").BindDataContext((TDVM vm) => vm.ReportsRecorded, DualBindingMode.OneWay);
            _tabletVisualizer.ReportDataBinding.BindDataContext((TDVM vm) => vm.ReportData, DualBindingMode.OneWay);

            _tabletVisualizer.BindDataContext(x => x.Enabled, (TDVM vm) => vm.IsVisualizerEnabled);
            _tabletVisualizerGroup.BindDataContext(x => x.Visible, (TDVM vm) => vm.IsVisualizerEnabled);
            _reportsRecordedGroup.BindDataContext(x => x.Visible, (TDVM vm) => vm.HasReportsRecorded);
            _additionalStatsGroup.BindDataContext(x => x.Visible, (TDVM vm) => vm.ShowAdditionalStatistics);
        }

        private void SetupMenu()
        {
            var dataRecordingMenuItem = new CheckMenuItem
            {
                Text = "Data Recording",
            };

            dataRecordingMenuItem.BindDataContext(x => x.Checked, (TDVM vm) => vm.DataRecordingEnabled);

            var openDataRecordingDirectoryMenuItem = new Command
            {
                MenuText = "Open recordings directory...",
            };
            openDataRecordingDirectoryMenuItem.Executed += (sender, e) => DesktopInterop.OpenFolder(AppInfo.Current.RecordingDirectory);

            ButtonMenuItem recordingTab = new()
            {
                Text = "Recording",
                Visible = true,
                Items = {
                    dataRecordingMenuItem,
                    openDataRecordingDirectoryMenuItem,
                }
            };

            var visualizerEnabledMenuItem = new CheckMenuItem
            {
                Text = "Visualizer",
            };

            visualizerEnabledMenuItem.BindDataContext(x => x.Checked, (TDVM vm) => vm.IsVisualizerEnabled);


            var decodingSwitchMenuItem = new ButtonMenuItem
            {
                Text = "Raw Data Mode",
            };

            RadioMenuItem? rootRadioButton = null;
            foreach (var decodingMode in Enum.GetValues<DecodingMode>())
            {
                string modeName = decodingMode.ToString();

                var item = new RadioMenuItem(rootRadioButton);
                item.Text = modeName;
                item.BindDataContext(x => x.Checked,
                    Binding.Property((TDVM vm) => vm.DecodingMode).ToBool(decodingMode));

                rootRadioButton ??= item;

                decodingSwitchMenuItem.Items.Add(item);
            }


            var additionalStatisticsMenuItem = new CheckMenuItem()
            {
                Text = "Additional Statistics",
            };

            additionalStatisticsMenuItem.BindDataContext(x => x.Checked, (TDVM vm) => vm.ShowAdditionalStatistics);


            Menu = new MenuBar
            {
                ApplicationItems =
                {
                    visualizerEnabledMenuItem,
                    additionalStatisticsMenuItem,
                    decodingSwitchMenuItem,
                },
                QuitItem = new ButtonMenuItem((_, _) => Application.Instance.AsyncInvoke(Close))
                {
                    Text = "Close Window",
                },
                Items =
                {
                    recordingTab,
                    _debuggedTablets,
                    _debuggedReports,
                },
            };

#if DEBUG
            Menu.Items.Add(new ButtonMenuItem
            {
                Text = "Force Refresh",
                Items =
                {
                    new ButtonMenuItem(
                    (_, _) => Application.Instance.InvokeAsync(UpdateAdditionalStatisticsFields).ConfigureAwait(false))
                    {
                        Text = "Additional Statistics",
                    },
                },
            });
#endif
        }

        private void ResizeRawBasedOnDecodingMode()
        {
            if (DataContext is not TDVM viewmodel) return;

            _rawTabletDataGroup.Width = GetWidthOfRawTabletDataGroupBox(viewmodel.DecodingMode);
        }

        // TODO: move to utility class
        private static IEnumerable<CheckMenuItem> GenerateTabletFilterMenuItem(IReadOnlyCollection<string> seenIDs, HashSet<string> ignoredIDs)
        {
            foreach (string id in seenIDs)
            {
                bool isIgnored = ignoredIDs.Contains(id);

                var command = new CheckCommand(HandleCheckCommand)
                {
                    Checked = !isIgnored,
                    Tag = (id, seenIDs, ignoredIDs),
                };

                var menuItem = new CheckMenuItem(command)
                {
                    Text = id,
                };

                yield return menuItem;
            }
        }

        // TODO: move to utility class
        private static void HandleCheckCommand(object? sender, EventArgs e)
        {
            if (sender is not CheckCommand checkCommand) return;

            (string name, var seenIDs, var ignoredIDs) = (ValueTuple<string, IReadOnlyCollection<string>, HashSet<string>>)checkCommand.Tag;

            if (checkCommand.Checked)
                ignoredIDs.Remove(name);
            else if (seenIDs.Count > ignoredIDs.Count + 1) // don't allow removing last entry
                ignoredIDs.Add(name);
            else
                checkCommand.Checked = true;
        }

        // TODO: can GeneratedItemList<T> be used?
        private void RefreshActiveTabletsMenu()
        {
            if (DataContext is not TDVM viewmodel) return;

            UpdateFilterList(_debuggedReports, viewmodel.SeenReports, viewmodel.IgnoredReports);
        }

        // TODO: move to utility class
        private static void UpdateFilterList(ButtonMenuItem menuItem, IReadOnlyCollection<string> seenIDs, HashSet<string> ignoredIDs)
        {
            int menuItemCount = menuItem.Items.Count;
            menuItem.Items.AddRange(GenerateTabletFilterMenuItem(seenIDs, ignoredIDs));
            for (int i = 0; i < menuItemCount; i++)
            {
                menuItem.Items.RemoveAt(0);
            }

            // hide if only 1 tablet active
            menuItem.Visible = menuItem.Items.Count > 1;
        }

        private async Task UpdateAdditionalStatisticsFields()
        {
            if (DataContext is not TDVM viewmodel) return;

            // don't do anything if there isn't anything to do anyway
            if (viewmodel.AdditionalStatistics.Children.Count == 0) return;

            await Application.Instance.InvokeAsync(async () =>
            {
                var outerContainer = new StackLayout
                {
                    Spacing = _SPACING,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                };

                var relevantGroups =
                    viewmodel.AdditionalStatistics.Children.Where(x =>
                            x.Children.Count > 0 && x.Children.Any(c => !c.Hidden))
                        .ToImmutableSortedSet();

                int[] groupsPerRow = Helpers.SplitIntoBuckets(relevantGroups.Count, AdditionalStatColumnsPerRow);

                int takenElements = 0;

                foreach (int groupsForThisRow in groupsPerRow)
                {
                    var container = new StackLayout
                    {
                        Orientation = Orientation.Horizontal,
                    };

                    foreach (var group in relevantGroups.Skip(takenElements).Take(groupsForThisRow))
                    {
                        takenElements += 1;

                        var children = new StackLayout();

                        foreach (var groupChild in group.Children.Where(x => !x.Hidden))
                        {
                            string exampleValue = groupChild.Value switch
                            {
                                Vector2 => new Vector2(123456).ToString(),
                                short => "255",
                                int => "10000",
                                float => "1000.000",
                                double => "1000.000",
                                string => "not too short",
                                _ => "1234",
                            };

                            var label = new UnitLabel(s_MonospaceFont, exampleValue, groupChild.Unit);

                            if (groupChild.Value is string)
                                label.TextAlignment = TextAlignment.Left;

                            var item = new DebuggerGroup
                            {
                                Text = groupChild.Name,
                                Content = label,
                            };

                            label.TextBinding.Bind(groupChild, nameof(groupChild.ValueString));
                            children.Items.Add(item);
                            await Task.Yield();
                        }

                        var groupContainer = new Group
                        {
                            Text = group.Name,
                            Padding = _SPACING,
                            Content = children,
                        };

                        container.Items.Add(groupContainer);
                    }

                    outerContainer.Items.Add(container);
                }

                await Application.Instance.InvokeAsync(() => _additionalStatsGroup.Content = outerContainer);
            }).ConfigureAwait(false);
        }


        private int GetWidthOfRawTabletDataGroupBox(
            DecodingMode decodingMode = DecodingMode.Hex) =>
            decodingMode switch
            {
                DecodingMode.Binary => (int)_rawTabletLineFontSizeBinary.Width + _GROUP_BOX_EXTRA_WIDTH,
                DecodingMode.Hex => (int)_rawTabletLineFontSizeHex.Width + _GROUP_BOX_EXTRA_WIDTH,
                _ => throw new ArgumentOutOfRangeException(nameof(decodingMode)),
            };

        protected override async void OnClosing(CancelEventArgs e)
        {
            var viewmodel = DataContext as TDVM ?? throw new InvalidOperationException("Invalid data context");

            if (App.Driver.IsConnected)
                await App.Driver.Instance.SetTabletDebug(false);

            App.Driver.DeviceReport -= viewmodel.HandleReport;
            App.Driver.TabletsChanged -= HandleTabletsChanged;
            App.Driver.Connected -= SetTabletDebug;

            if (DataContext is IDisposable disposable)
                disposable.Dispose();

            base.OnClosing(e);
        }

        private void SetTitle(IEnumerable<TabletReference> tablets)
        {
            StringBuilder sb = new StringBuilder("Tablet Debugger");
            var tabletReferenceArr = tablets as TabletReference[] ?? tablets.ToArray();

            if (tabletReferenceArr.Length != 0)
            {
                int numTablets = Math.Min(tabletReferenceArr.Length, 3);
                sb.Append(" - ");
                sb.Append(string.Join(", ", tabletReferenceArr.Take(numTablets).Select(t => t.Properties.Name)));
            }
            Title = sb.ToString();
        }

        private void HandleTabletsChanged(object? sender, IEnumerable<TabletReference> tablets)
        {
            Application.Instance.AsyncInvoke(() => SetTitle(tablets));
            Application.Instance.AsyncInvoke(() => UpdateTablets(tablets));
        }

        private void UpdateTablets(IEnumerable<TabletReference> tablets)
        {
            if (DataContext is not TDVM viewmodel) return;

            var tabletNames = tablets.Select(x => x.Properties.Name);
            UpdateFilterList(_debuggedTablets, tabletNames.ToArray(), viewmodel.IgnoredTablets);
        }

        private class DebuggerGroup : Group
        {
            protected override Color VerticalBackgroundColor => base.HorizontalBackgroundColor;
        }

        private class TabletVisualizer : ScheduledDrawable
        {
            private static readonly Color s_AccentColor = SystemColors.Highlight;

            private DebugReportData? ReportData { set; get; }
            private readonly List<int> _warnedDigitizers = [];

            public BindableBinding<TabletVisualizer, DebugReportData?> ReportDataBinding
            {
                get
                {
                    return new BindableBinding<TabletVisualizer, DebugReportData?>(
                        this,
                        c => c.ReportData,
                        (c, v) => c.ReportData = v
                    );
                }
            }

            protected override void OnNextFrame(PaintEventArgs e)
            {
                if (ReportData?.Tablet is not { } tablet) return;

                var graphics = e.Graphics;
                using (graphics.SaveTransformState())
                {
                    var digitizer = tablet.Properties.Specifications.Digitizer;
                    float yScale = (ClientSize.Height - _SPACING) / digitizer.Height;
                    float xScale = (ClientSize.Width - _SPACING) / digitizer.Width;
                    float finalScale = Math.Min(yScale, xScale);

                    var clientCenter = new PointF(ClientSize.Width, ClientSize.Height) / 2;
                    var tabletCenter = new PointF(digitizer.Width, digitizer.Height) / 2 * finalScale;

                    graphics.TranslateTransform(clientCenter - tabletCenter);

                    DrawBackground(graphics, finalScale, tablet);
                    DrawPosition(graphics, finalScale);
                }
            }

            private static void DrawBackground(Graphics graphics, float scale, TabletReference tablet)
            {
                var digitizer = tablet.Properties.Specifications.Digitizer;
                var bg = new RectangleF(0, 0, digitizer.Width, digitizer.Height) * scale;

                graphics.FillRectangle(SystemColors.WindowBackground, bg);
                graphics.DrawRectangle(s_AccentColor, bg);
            }

            private void DrawPosition(Graphics graphics, float scale)
            {
                Debug.Assert(ReportData != null); // ReportData should already be checked by callers
                object? report = ReportData!.ToObject();
                var specifications = ReportData.Tablet.Properties.Specifications;
                string tabletName = ReportData.Tablet.Properties.Name;
                var touchDigitizerSpecification = specifications.Touch;
                var absDigitizerSpecification = specifications.Digitizer;

                if (report is IAbsolutePositionReport absReport)
                {
                    var tabletScale = CalculateTabletScale(absDigitizerSpecification, scale);
                    var position = new PointF(absReport.Position.X, absReport.Position.Y) * tabletScale;

                    var drawRect = RectangleF.FromCenter(position, new SizeF(_SPACING, _SPACING));
                    graphics.FillEllipse(s_AccentColor, drawRect);
                }

                // touch reports
                if (report is ITouchReport touchReport)
                {
                    if (touchDigitizerSpecification != null)
                    {
                        var tabletScale = CalculateTabletScale(touchDigitizerSpecification, scale);

                        foreach (var touchPoint in touchReport.Touches.Where(t => t != null).Cast<TouchPoint>())
                        {
                            var position = new PointF(touchPoint.Position.X, touchPoint.Position.Y) * tabletScale;
                            var drawPen = new Pen(s_AccentColor, _SPACING / 2);
                            var drawRect = RectangleF.FromCenter(position, new SizeF(_SPACING * 2, _SPACING * 2));
                            graphics.DrawEllipse(drawPen, drawRect);
                        }
                    }
                    else
                    {
                        string touchHashName = tabletName + "touch";
                        int touchHash = touchHashName.GetHashCode();
                        if (!_warnedDigitizers.Contains(touchHash))
                        {
                            _warnedDigitizers.Add(touchHash);
                            Log.Write("TabletDebugger",
                                "Touch undefined in tablet configuration - unable to draw touch points",
                                LogLevel.Warning);
                        }
                    }
                }
            }

            private static SizeF CalculateTabletScale(DigitizerSpecifications digitizer, float scale)
            {
                var tabletMm = new SizeF(digitizer.Width, digitizer.Height);
                var tabletPx = new SizeF(digitizer.MaxX, digitizer.MaxY);
                return tabletMm / tabletPx * scale;
            }
        }
    }
}
