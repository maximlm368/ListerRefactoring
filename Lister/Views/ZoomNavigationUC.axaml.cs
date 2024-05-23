using Avalonia.Controls;

namespace Lister.Views
{
    public partial class ZoomNavigationUserControl : UserControl
    {
        private PersonSourceUserControl _personSourceUserControl;
        private TemplateChoosingUserControl _templateChoosingUserControl;
        private SceneUserControl _sceneUserControl;
        private PersonChoosingUserControl _personChoosingUserControl;


        public ZoomNavigationUserControl ()
        {
            InitializeComponent ();
        }
    }
}
