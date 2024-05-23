using Avalonia.Controls;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private PersonSourceUserControl _personSourceUserControl;
        private TemplateChoosingUserControl _templateChoosingUserControl;
        private ZoomNavigationUserControl _zoomNavigationUserControl;
        private PersonChoosingUserControl _personChoosingUserControl;


        public SceneUserControl ()
        {
            InitializeComponent ();
        }
    }
}
