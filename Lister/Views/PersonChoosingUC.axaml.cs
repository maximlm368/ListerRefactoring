using Avalonia.Controls;
using Avalonia.Media;

namespace Lister.Views
{
    public partial class PersonChoosingUserControl : UserControl
    {
        private PersonSourceUserControl _personSourceUserControl;
        private TemplateChoosingUserControl _templateChoosingUserControl;
        private ZoomNavigationUserControl _zoomNavigationUserControl;
        private SceneUserControl _sceneUserControl;


        public PersonChoosingUserControl ()
        {
            InitializeComponent ();
        }
    }
}
