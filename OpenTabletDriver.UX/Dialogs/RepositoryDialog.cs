using System;
using System.Net;
using System.Net.Http;
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

            var owner = new TextBoxGroup("Owner")
            {
                DefaultInputText = PluginMetadataCollection.REPOSITORY_OWNER
            };

            var repo = new TextBoxGroup("Name")
            {
                DefaultInputText = PluginMetadataCollection.REPOSITORY_NAME
            };

            var gitRef = new TextBoxGroup("Ref")
            {
                DefaultInputText = "master"
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
                        Control = new Button((sender, e) => Return(owner, repo, gitRef))
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
                    new StackLayoutItem(owner),
                    new StackLayoutItem(repo),
                    new StackLayoutItem(gitRef),
                    new StackLayoutItem(null, true),
                    new StackLayoutItem(actions)
                }
            };
        }

        protected async void Return(TextBoxGroup owner, TextBoxGroup repo, TextBoxGroup gitRef)
        {
            try
            {
                var collection = await PluginMetadataCollection.DownloadAsync(owner.InputText, repo.InputText, gitRef.InputText);
                Close(collection);
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                MessageBox.Show(
                    "Unable to find the repository source that was requested.",
                    "Error switching repository",
                    MessageBoxButtons.OK,
                    MessageBoxType.Error
                );
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
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
                get => this.inputText ?? DefaultInputText;
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
        }
    }
}
