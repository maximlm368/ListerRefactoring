using Avalonia.Controls;
using Avalonia.Input;
using Lister.ViewModels;
using Lister.Views;

namespace Lister.Views
{
    public partial class ZoomNavigationUserControl : UserControl
    {
        private ZoomNavigationViewModel _viewModel;
        private short _scalabilityDepth = 0;
        private short _maxDepth = 5;
        private short _minDepth = -5;
        private readonly short _scalabilityStep = 25;
        private ushort _maxScalability;
        private ushort _minScalability;

        public ZoomNavigationUserControl ()
        {
            InitializeComponent ();
            _viewModel = ( ZoomNavigationViewModel ) DataContext;
        }


        internal void ToNextPage ( object sender, TappedEventArgs args )
        {
            _viewModel.VisualiseNextPage ();
            SetEnablePageNavigation ();
        }


        internal void ToPreviousPage ( object sender, TappedEventArgs args )
        {
            _viewModel.VisualisePreviousPage ();
            SetEnablePageNavigation ();
        }


        internal void ToLastPage ( object sender, TappedEventArgs args )
        {
            _viewModel.VisualiseLastPage ();
            SetEnablePageNavigation ();
        }


        internal void ToFirstPage ( object sender, TappedEventArgs args )
        {
            _viewModel.VisualiseFirstPage ();
            SetEnablePageNavigation ();
        }


        internal void StepOnPage ( object sender, TextChangedEventArgs args )
        {
            TextBox textBox = ( TextBox ) sender;

            try
            {
                int pageNumber = ( int ) UInt32.Parse (textBox.Text);
                _viewModel.VisualisePageWithNumber (pageNumber);
                visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
                SetEnablePageNavigation ();
            }
            catch ( System.FormatException e )
            {
                visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
            }
        }


        internal void ZoomOn ( object sender, TappedEventArgs args )
        {
            if ( _scalabilityDepth < _maxDepth )
            {

                _viewModel.ZoomOnDocument (_scalabilityStep);
                _scalabilityDepth++;
            }

            if ( _scalabilityDepth == _maxDepth )
            {
                zoomOn.IsEnabled = false;
            }

            if ( ! zoomOut.IsEnabled )
            {
                zoomOut.IsEnabled = true;
            }
        }


        internal void ZoomOut ( object sender, TappedEventArgs args )
        {
            if ( _scalabilityDepth > _minDepth )
            {
                _viewModel.ZoomOutDocument (_scalabilityStep);
                _scalabilityDepth--;
            }

            if ( _scalabilityDepth == _minDepth )
            {
                zoomOut.IsEnabled = false;
            }

            if ( ! zoomOn.IsEnabled )
            {
                zoomOn.IsEnabled = true;
            }
        }


        internal void EditIncorrectBadges ( object sender, TappedEventArgs args )
        {
            if ( incorrectBadges.Count > 0 )
            {
                BadgeEditorView badgeEditor = new BadgeEditorView ();
                badgeEditor.SetIncorrectBadges (incorrectBadges);


                owner.Content = badgeEditor;
            }
        }


        internal void SetNewScale ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            string scale = scalabilityGrade.Text;
            bool scaleIsCorrect = ( scale != null ) && ( IsScaleStringCorrect (scale) );

            if ( scaleIsCorrect )
            {
                if ( IsKeyCorrect (key) )
                {

                }


            }

            TextBox textBox = ( TextBox ) sender;
            string text = textBox.Text;
            bool textExists = ( text != null ) && ( text != string.Empty );

            if ( textExists )
            {
                if ( text.Contains ('%') )
                {
                    text = text.Remove (text.Length - 1);
                }

                //try
                //{
                //    int scale = ( int ) UInt32.Parse (text);
                //    string procent = "%";

                //    if ( text.Contains (' ') )
                //    {
                //        int spaceIndex = text.IndexOf (' ');
                //    }
                //}
                //catch ( FormatException ex )
                //{ }
            }

        }


        private bool IsScaleStringCorrect ( string beingProcessed )
        {
            bool scaleIsCorrect = beingProcessed.Length > 2;
            scaleIsCorrect = scaleIsCorrect && beingProcessed [beingProcessed.Length - 1] == '%';
            scaleIsCorrect = scaleIsCorrect && ( beingProcessed [beingProcessed.Length - 2] == ' ' );
            scaleIsCorrect = scaleIsCorrect && ( beingProcessed [beingProcessed.Length - 3] != ' ' );

            try
            {
                int lastIntegerIndex = beingProcessed.Length - 3;
                string integerPart = beingProcessed.Substring (0, lastIntegerIndex);
                ushort scale = UInt16.Parse (integerPart);
                scaleIsCorrect = scaleIsCorrect && ( scale > _minScalability ) && ( scale < _maxScalability );

                if ( scaleIsCorrect )
                {

                }
            }
            catch ( Exception ex )
            {

            }

            return scaleIsCorrect;
        }


        private bool IsKeyCorrect ( string key )
        {
            bool keyIsCorrect = ( key.Length == 2 );
            keyIsCorrect = keyIsCorrect && ( key [0] == 'D' );
            keyIsCorrect = keyIsCorrect || ( key == "Back" );
            return keyIsCorrect;
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
            ZoomNavigationViewModel vm = ( ZoomNavigationViewModel ) DataContext;

            int pageCount = vm.GetPageCount ();

            if ( pageCount > 1 )
            {
                if ( ( vm.VisiblePageNumber > 1 )   &&   ( vm.VisiblePageNumber == pageCount ) )
                {
                    firstPage.IsEnabled = true;
                    previousPage.IsEnabled = true;
                    nextPage.IsEnabled = false;
                    lastPage.IsEnabled = false;
                }
                else if ( ( vm.VisiblePageNumber > 1 )   &&   ( vm.VisiblePageNumber < pageCount ) )
                {
                    firstPage.IsEnabled = true;
                    previousPage.IsEnabled = true;
                    nextPage.IsEnabled = true;
                    lastPage.IsEnabled = true;
                }
                else if ( ( vm.VisiblePageNumber == 1 )   &&   ( pageCount == 1 ) )
                {
                    firstPage.IsEnabled = false;
                    previousPage.IsEnabled = false;
                    nextPage.IsEnabled = false;
                    lastPage.IsEnabled = false;
                }
                else if ( ( vm.VisiblePageNumber == 1 )   &&   ( pageCount > 1 ) )
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