using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Controls
{
    internal static class WheelTabLayout
    {
        public static IReadOnlyList<WheelTabDefinition> Create(IReadOnlyList<WheelSpecifications> wheels)
        {
            ArgumentNullException.ThrowIfNull(wheels);

            if (!wheels.Any(HasValidGroup))
                return CreateStandaloneTabs(wheels);

            var tabs = new List<WheelTabDefinition>();
            var groupedTabs = new Dictionary<string, WheelTabDefinition>(StringComparer.Ordinal);

            for (int wheelIndex = 0; wheelIndex < wheels.Count; wheelIndex++)
            {
                var wheel = wheels[wheelIndex];
                var wheelTitle = GetWheelTitle(wheel, wheelIndex);

                if (TryGetGroupName(wheel, out var groupName))
                {
                    if (!groupedTabs.TryGetValue(groupName, out var tab))
                    {
                        tab = WheelTabDefinition.CreateGrouped(groupName);
                        groupedTabs.Add(groupName, tab);
                        tabs.Add(tab);
                    }

                    tab.AddWheel(wheelIndex, wheelTitle);
                }
                else
                {
                    tabs.Add(WheelTabDefinition.CreateStandalone(wheelIndex, wheelTitle));
                }
            }

            return tabs;
        }

        private static List<WheelTabDefinition> CreateStandaloneTabs(IReadOnlyList<WheelSpecifications> wheels)
        {
            var tabs = new List<WheelTabDefinition>(wheels.Count);

            for (int wheelIndex = 0; wheelIndex < wheels.Count; wheelIndex++)
                tabs.Add(WheelTabDefinition.CreateStandalone(wheelIndex, GetWheelTitle(wheels[wheelIndex], wheelIndex)));

            return tabs;
        }

        private static bool HasValidGroup(WheelSpecifications wheel) => TryGetGroupName(wheel, out _);

        private static bool TryGetGroupName(WheelSpecifications wheel, out string groupName)
        {
            groupName = wheel.Group?.Trim() ?? string.Empty;
            return groupName.Length > 0;
        }

        private static string GetWheelTitle(WheelSpecifications wheel, int wheelIndex) =>
            string.IsNullOrWhiteSpace(wheel.Name) ? $"Wheel {wheelIndex + 1}" : wheel.Name!;
    }

    internal sealed class WheelTabDefinition
    {
        private readonly List<WheelTabItem> _wheels = [];

        private WheelTabDefinition(string text, bool isGrouped)
        {
            Text = text;
            IsGrouped = isGrouped;
        }

        public string Text { get; }
        public bool IsGrouped { get; }
        public IReadOnlyList<WheelTabItem> Wheels => _wheels;

        public static WheelTabDefinition CreateStandalone(int wheelIndex, string wheelTitle)
        {
            var tab = new WheelTabDefinition($"{wheelTitle} Bindings", false);
            tab.AddWheel(wheelIndex, wheelTitle);
            return tab;
        }

        public static WheelTabDefinition CreateGrouped(string groupName) => new(groupName, true);

        public void AddWheel(int wheelIndex, string title) => _wheels.Add(new WheelTabItem(wheelIndex, title));
    }

    internal sealed class WheelTabItem
    {
        public WheelTabItem(int wheelIndex, string title)
        {
            WheelIndex = wheelIndex;
            Title = title;
        }

        public int WheelIndex { get; }
        public string Title { get; }
    }
}
