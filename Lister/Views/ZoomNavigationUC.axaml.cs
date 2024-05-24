using Avalonia.Controls;

namespace Lister.Views
{
    public partial class ZoomNavigationUserControl : UserControl
    {
        private PersonSourceUserControl _personSource;
        private TemplateChoosingUserControl _templateChoosing;
        private SceneUserControl _scene;
        private PersonChoosingUserControl _personChoosing;


        public ZoomNavigationUserControl ()
        {
            InitializeComponent ();
        }


        internal void PassNeighbours ( PersonSourceUserControl personSource, PersonChoosingUserControl personChoosing
                                     , TemplateChoosingUserControl templateChoosing, SceneUserControl scene ) 
        {
            _personChoosing = personChoosing;
            _personSource = personSource;
            _scene = scene;
            _templateChoosing = templateChoosing;
        }
    }
}
