using System;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection.Metadata;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Dialogs
{
    public class RepositoryDialog : Dialog<PluginMetadataCollection>
    {
        public RepositoryDialog()
        {
            this.Size = new Size(350, 250);
        }

        public RepositoryDialog(string title)
            : this()
        {
            this.Title = title;
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            var repo = new TextBoxGroup("Name")
            {
                DefaultInputText = PluginMetadataCollection.REPOSITORY_NAME
            };

            var branchRef = new TextBoxGroup("Git Reference")
            {
                ToolTip = "{Owner}:{GitRef}",
                DefaultInputText = $"{PluginMetadataCollection.REPOSITORY_OWNER}:master"
            };

            var actions = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Button((sender, e) => Close(null))
                        {
                            Text = "Cancel"
                        }
                    },
                    new StackLayoutItem
                    {
                        Expand = true,
                        Control = new Button((sender, e) => Return(branchRef, repo))
                        {
                            Text = "Apply"
                        }
                    }
                }
            };

            this.Content = new StackLayout
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(repo),
                    new StackLayoutItem(branchRef),
                    new StackLayoutItem(null, true),
                    new StackLayoutItem(actions)
                }
            };
        }

        protected async void Return(TextBoxGroup branchRef, TextBoxGroup repoName)
        {
            var collection = await PluginMetadataCollection.DownloadAsync(branchRef, repoName);
            Close(collection);
        }

        protected class TextBoxGroup : Group
        {
            public TextBoxGroup(string text)
            {
                base.Text = text;
                base.Orientation = Orientation.Horizontal;
            }

            private string inputText;
            public string InputText
            {
                protected set => this.inputText = value;
                get => string.IsNullOrWhiteSpace(this.inputText) ? DefaultInputText : this.inputText;
            }

            public string DefaultInputText { set; get; }

            protected const int TEXTBOX_WIDTH = 200;

            protected override void OnLoadComplete(EventArgs e)
            {
                base.OnLoadComplete(e);

                var textbox = new TextBox
                {
                    Width = TEXTBOX_WIDTH,
                    PlaceholderText = DefaultInputText
                };
                textbox.TextChanged += (sender, e) => InputText = textbox.Text;

                base.Content = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Items =
                    {
                        new StackLayoutItem(null, true),
                        new StackLayoutItem(textbox)
                    }
                };
            }

            public static implicit operator string(TextBoxGroup grp) => grp.InputText;
        }
    }
}