using System.Collections.ObjectModel;
using System.Globalization;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    public class Sidebar : Panel
    {
        private int _suspended;
        private bool _isCreated;
        private SidebarItem? _selectedItem;

        public Sidebar()
        {
        }

        public Sidebar(params SidebarItem[] items)
        {
            var itemCollection = new SidebarItemCollection(this);
            foreach (var item in items)
                itemCollection.Add(item);

            Items = itemCollection;
        }

        public SidebarItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public EventHandler? SelectedItemChanged;

        public Collection<SidebarItem> Items { get; } = new Collection<SidebarItem>();

		protected override void OnPreLoad(EventArgs e)
		{
			if (!_isCreated && _suspended <= 0)
				Create();
			base.OnPreLoad(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!_isCreated && _suspended <= 0)
				Create();
			base.OnLoad(e);
		}

		public override void SuspendLayout()
		{
			base.SuspendLayout();
			_suspended++;
		}

		public override void ResumeLayout()
		{
			if (_suspended == 0)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Must balance ResumeLayout with SuspendLayout calls"));
			_suspended--;
			base.ResumeLayout();
			CreateIfNeeded();
		}

        private void CreateIfNeeded(bool force = false)
        {
			if (_suspended > 0 || !Loaded)
			{
				if (force)
					_isCreated = false;
				return;
			}
			if (!_isCreated || force)
				Create();
        }

        private void Create()
        {
            _isCreated = true;

            var items = new StackLayoutItem[Items.Count];
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                items[i] = new StackLayoutItem(item, HorizontalAlignment.Left);
            }

            Content = new StackLayout(items);
        }

        private class SidebarItemCollection : Collection<SidebarItem>
        {
            public Sidebar Parent { get; }

            public SidebarItemCollection(Sidebar sidebar)
            {
                Parent = sidebar;
            }

			protected override void InsertItem(int index, SidebarItem item)
			{
				base.InsertItem(index, item);
				Parent.CreateIfNeeded(true);
			}

			protected override void RemoveItem(int index)
			{
				base.RemoveItem(index);
				Parent.CreateIfNeeded(true);
			}

			protected override void ClearItems()
			{
				base.ClearItems();
				Parent.CreateIfNeeded(true);
			}

			protected override void SetItem(int index, SidebarItem item)
			{
				base.SetItem(index, item);
				Parent.CreateIfNeeded(true);
			}
        }
    }

    public class SidebarItem : Panel
    {
        public SidebarItem(Control control)
        {
            Content = control;
        }

        public SidebarItem()
        {
        }
    }
}
