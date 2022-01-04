using System;
using System.Collections;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class TextContent : Label, IEnumerable<string>
    {
        public TextContent()
        {
            this.TextAlignment = TextAlignment.Center;
        }

        public TextContent(params string[] lines)
            : this()
        {
            foreach (var line in lines)
                Add(line);
        }

        public IList<string> Lines => base.Text.Split(Environment.NewLine);

        public void Add(string line)
        {
            if (!string.IsNullOrWhiteSpace(base.Text))
                base.Text += Environment.NewLine;
            base.Text += line;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        public static implicit operator TextContent(string[] lines)
        {
            return new TextContent(lines);
        }
    }
}
