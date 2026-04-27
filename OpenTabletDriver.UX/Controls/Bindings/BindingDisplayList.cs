using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public class BindingDisplayList : GeneratedItemList<PluginSettingStore>
    {
        public string Prefix { set; get; }

        private IList<string> _buttonNames;
        public IList<string> ButtonNames
        {
            set
            {
                _buttonNames = value;
                if (ItemSource != null)
                    HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            get => _buttonNames;
        }

        protected virtual string GetTextForIndex(int index)
        {
            if (_buttonNames != null && index < _buttonNames.Count
                && !string.IsNullOrEmpty(_buttonNames[index]))
                return _buttonNames[index];
            return $"{Prefix} {index + 1}";
        }

        protected override Control CreateControl(int index, DirectBinding<PluginSettingStore> itemBinding)
        {
            BindingDisplay display = new BindingDisplay();
            display.StoreBinding.Bind(itemBinding);

            return new Group
            {
                Text = GetTextForIndex(index),
                Orientation = Orientation.Horizontal,
                ExpandContent = false,
                Content = display
            };
        }

        #region Highlight Animation

        private const float DecayRate = 0.03f;

        private float[] _highlightAlpha;
        private Color[] _originalColors;
        private bool _animating;

        public void HighlightItem(int index)
        {
            if (index < 0 || index >= layout.Items.Count)
                return;

            if (_highlightAlpha == null || _highlightAlpha.Length != layout.Items.Count)
                _highlightAlpha = new float[layout.Items.Count];

            _highlightAlpha[index] = 1.0f;
            UpdateItemColor(index);

            if (!_animating)
            {
                _animating = true;
                CompositionScheduler.Register(OnHighlightTick);
            }
        }

        private void OnHighlightTick(object sender, EventArgs e)
        {
            if (_highlightAlpha == null)
            {
                StopAnimation();
                return;
            }

            bool anyActive = false;
            for (int i = 0; i < _highlightAlpha.Length && i < layout.Items.Count; i++)
            {
                if (_highlightAlpha[i] > 0)
                {
                    _highlightAlpha[i] = Math.Max(0, _highlightAlpha[i] - DecayRate);
                    UpdateItemColor(i);
                    if (_highlightAlpha[i] > 0)
                        anyActive = true;
                }
            }

            if (!anyActive)
                StopAnimation();
        }

        private Control GetColorTarget(Control control)
        {
            if (control is Panel panel && panel.Content is GroupBox innerBox)
                return innerBox;
            if (control is Panel panel2 && panel2.Content is Panel innerPanel)
                return innerPanel;
            return control;
        }

        private void CaptureOriginalColors()
        {
            _originalColors = new Color[layout.Items.Count];
            for (int i = 0; i < layout.Items.Count; i++)
                _originalColors[i] = GetColorTarget(layout.Items[i].Control).BackgroundColor;
        }

        private void UpdateItemColor(int index)
        {
            if (_originalColors == null || _originalColors.Length != layout.Items.Count)
                CaptureOriginalColors();

            var target = GetColorTarget(layout.Items[index].Control);
            var bg = _originalColors[index];

            if (_highlightAlpha[index] > 0.01f)
            {
                var a = _highlightAlpha[index] * 0.5f;

                // Pre-blend against background — GTK ignores alpha on BackgroundColor
                target.BackgroundColor = new Color(
                    bg.R + (1f - bg.R) * a,
                    bg.G + (1f - bg.G) * a,
                    bg.B + (1f - bg.B) * a
                );
            }
            else
            {
                target.BackgroundColor = bg;
            }
        }

        private void StopAnimation()
        {
            if (_animating)
            {
                _animating = false;
                CompositionScheduler.Unregister(OnHighlightTick);
            }
        }

        #endregion
    }
}
