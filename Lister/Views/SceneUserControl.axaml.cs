using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            this.Margin = new Avalonia.Thickness (5);
        }

        
    }
}



//internal void PassNeighbours ( PersonSourceUserControl personSource, PersonChoosingUserControl personChoosing
//                             , ZoomNavigationUserControl zoomNavigation, TemplateChoosingUserControl templateChoosing )
//{
//    _personChoosing = personChoosing;
//    _personSource = personSource;
//    _zoomNavigation = zoomNavigation;
//    _templateChoosing = templateChoosing;


//}