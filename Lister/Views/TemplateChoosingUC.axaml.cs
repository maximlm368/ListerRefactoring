using Avalonia.Controls;

namespace Lister.Views
{
    public partial class TemplateChoosingUserControl : UserControl
    {
        private PersonSourceUserControl _personSourceUserControl;
        private ZoomNavigationUserControl _zoomNavigationUserControl;
        private SceneUserControl _sceneUserControl;
        private PersonChoosingUserControl _personChoosingUserControl;


        public TemplateChoosingUserControl ()
        {
            InitializeComponent ();
        }
    }
}
