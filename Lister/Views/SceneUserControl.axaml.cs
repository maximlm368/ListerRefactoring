using Avalonia.Controls;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private PersonSourceUserControl _personSource;
        private TemplateChoosingUserControl _templateChoosing;
        private ZoomNavigationUserControl _zoomNavigation;
        private PersonChoosingUserControl _personChoosing;


        public SceneUserControl ()
        {
            InitializeComponent ();
        }


        internal void PassNeighbours ( PersonSourceUserControl personSource, PersonChoosingUserControl personChoosing
                                     , ZoomNavigationUserControl zoomNavigation, TemplateChoosingUserControl templateChoosing )
        {
            _personChoosing = personChoosing;
            _personSource = personSource;
            _zoomNavigation = zoomNavigation;
            _templateChoosing = templateChoosing;
        }
    }
}
