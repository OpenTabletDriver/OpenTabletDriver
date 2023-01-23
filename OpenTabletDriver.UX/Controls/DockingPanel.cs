using Eto.Forms;

namespace OpenTabletDriver.UX.Controls
{
    /// <summary>
    /// A layout that allows controls to be anchored to the edges or the center of the panel.
    /// </summary>
    public class DockingLayout : Panel
    {
        private readonly Panel _topPanel, _rightPanel, _bottomPanel, _leftPanel, _centerPanel;

        public DockingLayout()
        {
            var dynamicLayout = new DynamicLayout();

            _topPanel = new Panel();
            _rightPanel = new Panel();
            _bottomPanel = new Panel();
            _leftPanel = new Panel();
            _centerPanel = new Panel();

            dynamicLayout.BeginVertical();
            dynamicLayout.Add(_topPanel, xscale: true);

            dynamicLayout.BeginHorizontal();
            dynamicLayout.Add(_leftPanel, yscale: true);
            dynamicLayout.Add(_centerPanel, xscale: true, yscale: true);
            dynamicLayout.Add(_rightPanel, yscale: true);
            dynamicLayout.EndHorizontal();

            dynamicLayout.Add(_bottomPanel, xscale: true);
            dynamicLayout.EndVertical();

            Content = dynamicLayout;
        }

        public Control? Top
        {
            get => _topPanel.Content;
            set => _topPanel.Content = value;
        }

        public Control? Right
        {
            get => _rightPanel.Content;
            set => _rightPanel.Content = value;
        }

        public Control? Bottom
        {
            get => _bottomPanel.Content;
            set => _bottomPanel.Content = value;
        }

        public Control? Left
        {
            get => _leftPanel.Content;
            set => _leftPanel.Content = value;
        }

        public Control? Center
        {
            get => _centerPanel.Content;
            set => _centerPanel.Content = value;
        }
    }
}
