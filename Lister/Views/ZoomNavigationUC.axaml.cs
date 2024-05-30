using Avalonia.Controls;
using Lister.Views;

namespace Lister.Views
{
    public partial class ZoomNavigationUserControl : UserControl
    {
        


        public ZoomNavigationUserControl ()
        {
            InitializeComponent ();
        }


        internal void DisableButtons () 
        {
            zoomOn.IsEnabled = false;
            zoomOut.IsEnabled = false;
            firstPage.IsEnabled = false;
            previousPage.IsEnabled = false;
            nextPage.IsEnabled = false;
            lastPage.IsEnabled = false;
        }


        internal void EnableZoom ()
        {
            zoomOn.IsEnabled = true;
            zoomOut.IsEnabled = true;
        }


        internal void SetEnablePageNavigation ()
        {
            int pageCount = viewModel.GetPageCount ();

            if ( pageCount > 1 )
            {
                if ( ( viewModel.VisiblePageNumber > 1 ) && ( viewModel.VisiblePageNumber == pageCount ) )
                {
                    firstPage.IsEnabled = true;
                    previousPage.IsEnabled = true;
                    nextPage.IsEnabled = false;
                    lastPage.IsEnabled = false;
                }
                else if ( ( viewModel.VisiblePageNumber > 1 ) && ( viewModel.VisiblePageNumber < pageCount ) )
                {
                    firstPage.IsEnabled = true;
                    previousPage.IsEnabled = true;
                    nextPage.IsEnabled = true;
                    lastPage.IsEnabled = true;
                }
                else if ( ( viewModel.VisiblePageNumber == 1 ) && ( pageCount == 1 ) )
                {
                    firstPage.IsEnabled = false;
                    previousPage.IsEnabled = false;
                    nextPage.IsEnabled = false;
                    lastPage.IsEnabled = false;
                }
                else if ( ( viewModel.VisiblePageNumber == 1 ) && ( pageCount > 1 ) )
                {
                    firstPage.IsEnabled = false;
                    previousPage.IsEnabled = false;
                    nextPage.IsEnabled = true;
                    lastPage.IsEnabled = true;
                }
            }
        }

    }
}


//internal void PassNeighbours ( PersonSourceUserControl personSource, PersonChoosingUserControl personChoosing
//                             , TemplateChoosingUserControl templateChoosing, SceneUserControl scene ) 
//{
//    _personChoosing = personChoosing;
//    _personSource = personSource;
//    _scene = scene;
//    _templateChoosing = templateChoosing;
//}

//private PersonSourceUserControl _personSource;
//private TemplateChoosingUserControl _templateChoosing;
//private SceneUserControl _scene;
//private PersonChoosingUserControl _personChoosing;