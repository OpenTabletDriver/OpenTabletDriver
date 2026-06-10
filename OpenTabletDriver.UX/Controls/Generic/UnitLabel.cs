using System.Diagnostics.CodeAnalysis;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public sealed class UnitLabel : Label
    {
        [SetsRequiredMembers]
        public UnitLabel(Font font, string exampleValue, string? unit, bool spaced = false)
        {
            this.Font = font;
            this.TextAlignment = TextAlignment.Right;
            this.Unit = unit ?? string.Empty;

            if (spaced)
                Spacer = " ";

            this.Width = (int)font.Measure(GetFormattedText(exampleValue)).Width;
        }

        public required string Unit { get; set; }
        public string Spacer = string.Empty;

        public override string Text
        {
            set => base.Text = GetFormattedText(value);
            get => base.Text;
        }

        private string GetFormattedText(string text) => $"{text}{Spacer}{Unit}";
    }
}
