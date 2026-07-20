using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet.Touch;

namespace OpenTabletDriver.Desktop.ViewModels.Utility
{
    public static class StatisticSubGroup
    {
        public const string Min = "Minimum Value";
        public const string Max = "Maximum Value";
        public const string Status = "Status";
    }

    public class Statistic : INotifyPropertyChanged, IComparable
    {
        private object? _value;
        private string? _unit;
        private string _valueStringFormat;
        private bool _hidden;

        // null: valid (full 'false -> true -> false' transition happened)
        // false: only seen false
        // true: have seen false and then true but haven't seen false after true
        private readonly Dictionary<int, bool?> _seenButtons = new();

        internal Statistic(string name, object? value = null, string? unit = null, string? valueStringFormat = null)
        {
            Name = name;
            Value = value;
            _unit = unit;
            _valueStringFormat = valueStringFormat ?? "{0}";
            Children.CollectionChanged += ChildCollectionChangedHandler;
        }

        #region Properties

        /// <summary>
        /// If <c>Children</c> has any elements, this instance effectively becomes a group
        /// </summary>
        public ObservableCollection<Statistic> Children { get; } = [];

        /// <summary>
        /// The key name of the instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The optional value of the instance
        /// </summary>
        public object? Value
        {
            get => _value;
            set
            {
                SetField(ref _value, value);
                OnPropertyChanged(nameof(ValueString));
            }
        }

        /// <summary>
        /// The unit intended to be appended to the string. Consumed by clients.
        /// </summary>
        public string? Unit
        {
            get => _unit;
            set => SetField(ref _unit, value);
        }

        /// <summary>
        /// The string format to use. See <see cref="string.Format(string, object?[])"/>.
        /// Used when retrieving <see cref="ValueString"/>
        /// </summary>
        public string ValueStringFormat
        {
            get => _valueStringFormat;
            set
            {
                SetField(ref _valueStringFormat, value);
                OnPropertyChanged(nameof(ValueString));
            }
        }

        /// <summary>
        /// Should the value normally be displayed/designed for in a UI
        /// </summary>
        public bool Hidden
        {
            get => _hidden;
            set => SetField(ref _hidden, value);
        }

        /// <summary>
        /// Formatted string of the value using the specified <see cref="ValueStringFormat"/>
        /// </summary>
        public string ValueString => Value != null
            ? string.Format(ValueStringFormat, Value)
            : Children.Count == 0 // don't show <null> on groups
                ? "<null>"
                : string.Empty;

        #endregion

        #region Class Functions

        /// <summary>
        /// Retrieve the child group <see cref="Statistic"/> from the current instance
        /// </summary>
        /// <param name="childName">The <see cref="Name"/> of the child</param>
        public Statistic this[string childName]
        {
            get
            {
                var rv = Children.FirstOrDefault(x => x.Name == childName);

                if (rv == null)
                {
                    Children.Add(rv = new Statistic(childName));
                    ChildCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, rv));
                }

                return rv;
            }
        }

        /// <summary>
        /// Return this object and its children's full string representation
        /// </summary>
        /// <returns>A string per this and per child</returns>
        public IEnumerable<string> DumpTreeAsStrings()
        {
            yield return $"{Name}: {ValueString} {Unit}".TrimEnd();

            foreach (var child in Children)
                foreach (var s in child.DumpTreeAsStrings())
                    yield return $"  {s}";
        }

        #endregion

        #region Class "Extension Methods"

        public Statistic SaveMinMax(double source, string? unit = null, int precision = 2) => SaveMinMax(source, Math.Min, Math.Max, unit, precision);
        public Statistic SaveMinMax(uint source, string? unit = null) => SaveMinMax(source, Math.Min, Math.Max, unit, null);
        public Statistic SaveMinMax(Vector2 source, string? unit = null, int precision = 0) => SaveMinMax(source, Vector2.Min, Vector2.Max, unit, precision);
        public Statistic SaveMinMax(TouchPoint?[] touchPoints)
        {
            var validTouchPoints = touchPoints.Where(x => x != null).Select(x => x!.Position).ToArray();
            if (validTouchPoints.Length == 0) return this;

            // Vector2 doesn't implement IComparable, do naive method:
            foreach (var touchPoint in validTouchPoints)
                SaveMinMax(touchPoint);

            return this;
        }

        private Statistic SaveMinMax<T>(T source, Func<T, T, T> minFunc, Func<T, T, T> maxFunc, string? unit, int? precision)
        {
            var min = this[StatisticSubGroup.Min];
            min.Value = minFunc(source, (T)(min.Value ?? source)!);
            min.Unit = unit;
            var max = this[StatisticSubGroup.Max];
            max.Value = maxFunc(source, (T)(max.Value ?? source)!);
            max.Unit = unit;

            if (precision != null)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(precision.Value, nameof(precision));
                string format;

                if (precision.Value == 0)
                    format = "{0:0}";
                else
                    format = "{0:0." + string.Concat(Enumerable.Repeat("0", precision.Value)) + "}";

                min.ValueStringFormat = max.ValueStringFormat = format;
            }

            return this;
        }

        /// <summary>
        /// Save up to <paramref name="expectedButtons"/> values from <paramref name="buttons"/> into this group.
        /// This function also tracks the complete state range of the button,
        ///   to ensure a full <c>false</c> -> <c>true</c> -> <c>false</c> transition has happened
        /// </summary>
        /// <param name="buttons">The buttons to save</param>
        /// <param name="expectedButtons">The maximum number of buttons to save</param>
        /// <returns></returns>
        public Statistic SaveButtons(bool[] buttons, int expectedButtons)
        {
            // no buttons expected, don't log anything
            if (expectedButtons == 0) return this;

            // skip if more than 1 button pressed
            if (buttons.Take(expectedButtons).Count(x => x) > 1) return this;

            for (int i = 0; i < expectedButtons; i++)
            {
                var buttonStatistic = this[$"{i}"];
                buttonStatistic.Hidden = true;

                bool buttonState = buttons[i];

                if (!_seenButtons.TryGetValue(i, out bool? seenButton))
                {
                    // don't add true-first buttons (click another button first, unless there's no other button to click)
                    if (buttonState && expectedButtons > 1) continue;

                    _seenButtons.Add(i, buttonState);
                    buttonStatistic.Value = "Press Down";
                    continue;
                }

                // null means this button has already been successfully processed
                if (seenButton == null) continue;

                if (buttonState && !seenButton.Value) // if button pressed and only seen false
                {
                    _seenButtons[i] = true;
                    buttonStatistic.Value = "Release Button";
                    continue;
                }

                if (!buttonState && seenButton.Value) // if button released and last known state was true
                {
                    _seenButtons[i] = null;
                    buttonStatistic.Value = "PASS";
                }
            }

            var status = this[StatisticSubGroup.Status];
            status.Value = string.Join(" ", _seenButtons.Select(SelectEmojisFromButtonBool));

            return this;

            string SelectEmojisFromButtonBool(KeyValuePair<int, bool?> button) =>
                button.Value switch
                { // add 1 to key number to display human-friendly number
                    null => $"{button.Key + 1}✓",
                    false => $"{button.Key + 1}↓",
                    true => $"{button.Key + 1}↑",
                };
        }

        /// <summary>
        /// Save the <paramref name="valuePath"/> as a subgroup and increment its count if it has been seen before.
        /// </summary>
        /// <param name="valuePath">The value of the group</param>
        /// <returns><c>this</c> (for LINQ-style chaining)</returns>
        public Statistic SaveCountAdd1(string valuePath)
        {
            valuePath = valuePath.Replace("OpenTabletDriver.Configurations.Parsers.", "")
                .Replace("OpenTabletDriver.Plugin.Tablet.", "");

            var key = this[valuePath];
            key.Value ??= 0;
            key.Value = (int)key.Value + 1;

            return this;
        }

        /// <summary>
        /// Recursively enable <see cref="Hidden"/> on everything in this hierarchy
        /// </summary>
        /// <returns><c>this</c> (for LINQ-style chaining)</returns>
        public Statistic HideAllChildren()
        {
            this.Hidden = true;

            foreach (var child in Children)
                child.HideAllChildren();

            return this;
        }

        #endregion

        public override string ToString()
        {
            string hiddenText = Hidden ? "Hidden" : string.Empty;
            string groupText = Children.Count > 0 ? "Group" : string.Empty;
            return $"{hiddenText} {groupText} {Name} {ValueString} {Unit}".Trim();
        }

        // to support sorting, e.g. SortedHashMap
        public int CompareTo(object? obj) =>
            obj is Statistic statistic
                ? string.CompareOrdinal(this.Name, statistic.Name)
                : throw new ArgumentException("Invalid type of object", nameof(obj));

        #region Event Handling

        private void ChildCollectionChangedHandler(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ChildCollectionChanged?.Invoke(sender, e);

            // TODO: only subscribe if it's our child

            if (e is { OldItems: not null })
                foreach (Statistic item in e.OldItems)
                    item.ChildCollectionChanged -= ChildCollectionChangedHandler;
            if (e is { NewItems: not null })
                foreach (Statistic item in e.NewItems)
                    item.ChildCollectionChanged += ChildCollectionChangedHandler;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event NotifyCollectionChangedEventHandler? ChildCollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
    }
}
