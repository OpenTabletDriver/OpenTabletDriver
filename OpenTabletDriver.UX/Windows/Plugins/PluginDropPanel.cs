using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;

namespace OpenTabletDriver.UX.Windows.Plugins
{
    public class PluginDropPanel : Panel
    {
        public PluginDropPanel()
        {
            AllowDrop = true;

            DropContent = new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem
                    {
                        Control = dropTextLabel,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackLayoutItem(null, true)
                }
            };
        }

        private const string DRAG_DROP_SUPPORTED = "Drop plugin here...";
        private const string DRAG_DROP_UNSUPPORTED = "Drag and drop is not supported on this platform.";

        public event Func<string, Task> RequestPluginInstall;

        private Control content;
        public new Control Content
        {
            set
            {
                this.content = value;
                base.Content = this.Content;
            }
            get => this.content;
        }

        private readonly Label dropTextLabel = new Label
        {
            Text = DRAG_DROP_SUPPORTED
        };

        protected StackLayout DropContent { get; }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            base.Content = this.Content;
        }

        protected override void OnDragEnter(DragEventArgs args)
        {
            base.OnDragEnter(args);
            try
            {
                if (args.Data.ContainsUris)
                {
                    // Skip if running on bugged platform
                    // https://github.com/picoe/Eto/issues/1812
                    if (args.Data.Uris != null && args.Data.Uris?.Length > 0)
                    {
                        var uriList = args.Data.Uris;
                        var supportedType = uriList.All(uri =>
                        {
                            if (uri.IsFile && File.Exists(uri.LocalPath))
                            {
                                var fileInfo = new FileInfo(uri.LocalPath);
                                return fileInfo.Extension switch
                                {
                                    ".zip" => true,
                                    ".dll" => true,
                                    _ => false
                                };
                            }
                            return false;
                        });
                        if (supportedType)
                        {
                            dropTextLabel.Text = DRAG_DROP_SUPPORTED;
                            args.Effects = DragEffects.Copy;
                        }
                    }
                    else
                    {
                        dropTextLabel.Text = DRAG_DROP_UNSUPPORTED;
                        args.Effects = DragEffects.None;
                    }
                    base.Content = DropContent;
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        protected override void OnDragLeave(DragEventArgs args)
        {
            base.OnDragLeave(args);
            base.Content = this.Content;
        }

        protected override async void OnDragDrop(DragEventArgs args)
        {
            base.OnDragDrop(args);
            try
            {
                if (args.Data.ContainsUris && args.Data.Uris != null && args.Data.Uris.Length > 0)
                {
                    var uriList = args.Data.Uris;
                    foreach (var uri in uriList)
                    {
                        if (uri.IsFile && File.Exists(uri.LocalPath))
                        {
                            await RequestPluginInstall?.Invoke(uri.LocalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }
    }
}