using Avalonia.Controls;
using Avalonia.LogicalTree;
using Lister.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

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
            DataContext = App.services.GetRequiredService<SceneViewModel> ();
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