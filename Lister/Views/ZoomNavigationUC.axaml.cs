using Avalonia.Controls;
using Avalonia.Input;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lister.Views
{
    public partial class ZoomNavigationUserControl : UserControl
    {
        private ZoomNavigationViewModel _vm;
        private short _scalabilityDepth = 0;
        private short _maxDepth = 5;
        private short _minDepth = -5;
        private readonly short _scalabilityStep = 25;
        private ushort _maxScalability;
        private ushort _minScalability;
        private SceneViewModel _sceneVM;

        public ZoomNavigationUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<ZoomNavigationViewModel> ();
            _vm = ( ZoomNavigationViewModel ) DataContext;
        }


        //internal void ToNextPage ( object sender, TappedEventArgs args )
        //{
        //    _vm.VisualiseNextPage ();
        //    SetEnablePageNavigation ();
        //}


        //internal void ToPreviousPage ( object sender, TappedEventArgs args )
        //{
        //    _vm.VisualisePreviousPage ();
        //    SetEnablePageNavigation ();
        //}


        //internal void ToLastPage ( object sender, TappedEventArgs args )
        //{
        //    _vm.VisualiseLastPage ();
        //    SetEnablePageNavigation ();
        //}


        //internal void ToFirstPage ( object sender, TappedEventArgs args )
        //{
        //    _vm.VisualiseFirstPage ();
        //    SetEnablePageNavigation ();
        //}


        internal void StepOnPage ( object sender, TextChangedEventArgs args )
        {
            TextBox textBox = ( TextBox ) sender;
            string text = textBox.Text;

            try
            {
                int pageNumber = ( int ) UInt32.Parse (text);

                if ( pageNumber == 0 )
                {
                    visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
                    return;
                }

                _vm.VisualisePageWithNumber (pageNumber);
                visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
                SetEnablePageNavigation ();
            }
            catch ( System.FormatException exp )
            {
                if ( ! string.IsNullOrWhiteSpace(text) ) 
                {
                    visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
                }
            }
            catch ( System.OverflowException exp )
            {
                visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
            }
        }


        //internal void ZoomOn ( object sender, TappedEventArgs args )
        //{
        //    ModernMainView parent = this.Parent.Parent as ModernMainView;
        //    SceneUserControl scene = parent.scene;

        //    if ( _scalabilityDepth < _maxDepth )
        //    {

        //        _vm.ZoomOn (_scalabilityStep);
        //        _scalabilityDepth++;
        //    }

        //    if ( _scalabilityDepth == _maxDepth )
        //    {
        //        zoomOn.IsEnabled = false;
        //    }

        //    if ( ! zoomOut.IsEnabled )
        //    {
        //        zoomOut.IsEnabled = true;
        //    }
        //}


        //internal void ZoomOut ( object sender, TappedEventArgs args )
        //{
        //    ModernMainView parent = this.Parent.Parent as ModernMainView;
        //    SceneUserControl scene = parent.scene;

        //    if ( _scalabilityDepth > _minDepth )
        //    {
        //        _vm.ZoomOut (_scalabilityStep);
        //        _scalabilityDepth--;
        //    }

        //    if ( _scalabilityDepth == _minDepth )
        //    {
        //        zoomOut.IsEnabled = false;
        //    }

        //    if ( ! zoomOn.IsEnabled )
        //    {
        //        zoomOn.IsEnabled = true;
        //    }
        //}


        //internal void EditIncorrectBadges ( object sender, TappedEventArgs args )
        //{
        //    List <BadgeViewModel> incorrects = _vm.GetIncorrectBadges ();

        //    ModernMainView ancestorView = this.Parent.Parent as ModernMainView;
        //    MainWindow owner = ancestorView.Parent as MainWindow;

        //    if ( incorrects.Count > 0 )
        //    {
        //        BadgeEditorView badgeEditor = new BadgeEditorView ();
        //        badgeEditor.ChangeSize (owner.WidthDifference, owner.HeightDifference);
        //        owner.ResetDifference ();
        //        badgeEditor.PassIncorrectBadges (incorrects);
        //        badgeEditor.PassBackPoint (ancestorView);
        //        owner.Content = badgeEditor;
        //    }
        //}


        //internal void SetNewScale ( object sender, KeyEventArgs args )
        //{
        //    string key = args.Key.ToString ();
        //    string scale = scalabilityGrade.Text;
        //    bool scaleIsCorrect = ( scale != null ) && ( IsScaleStringCorrect (scale) );

        //    if ( scaleIsCorrect )
        //    {
        //        if ( IsKeyCorrect (key) )
        //        {

        //        }


        //    }

        //    TextBox textBox = ( TextBox ) sender;
        //    string text = textBox.Text;
        //    bool textExists = ( text != null ) && ( text != string.Empty );

        //    if ( textExists )
        //    {
        //        if ( text.Contains ('%') )
        //        {
        //            text = text.Remove (text.Length - 1);
        //        }

        //        //try
        //        //{
        //        //    int scale = ( int ) UInt32.Parse (text);
        //        //    string procent = "%";

        //        //    if ( text.Contains (' ') )
        //        //    {
        //        //        int spaceIndex = text.IndexOf (' ');
        //        //    }
        //        //}
        //        //catch ( FormatException ex )
        //        //{ }
        //    }

        //}


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
            int pageCount = _vm.GetPageCount ();

            if ( pageCount > 1 )
            {
                if ( ( _vm.VisiblePageNumber > 1 )   &&   ( _vm.VisiblePageNumber == pageCount ) )
                {
                    firstPage.IsEnabled = true;
                    previousPage.IsEnabled = true;
                    nextPage.IsEnabled = false;
                    lastPage.IsEnabled = false;
                }
                else if ( ( _vm.VisiblePageNumber > 1 )   &&   ( _vm.VisiblePageNumber < pageCount ) )
                {
                    firstPage.IsEnabled = true;
                    previousPage.IsEnabled = true;
                    nextPage.IsEnabled = true;
                    lastPage.IsEnabled = true;
                }
                else if ( ( _vm.VisiblePageNumber == 1 )   &&   ( pageCount == 1 ) )
                {
                    firstPage.IsEnabled = false;
                    previousPage.IsEnabled = false;
                    nextPage.IsEnabled = false;
                    lastPage.IsEnabled = false;
                }
                else if ( ( _vm.VisiblePageNumber == 1 )   &&   ( pageCount > 1 ) )
                {
                    firstPage.IsEnabled = false;
                    previousPage.IsEnabled = false;
                    nextPage.IsEnabled = true;
                    lastPage.IsEnabled = true;
                }
            }
        }


        //internal void EditIncorrectBadges ( object sender, TappedEventArgs args )
        //{
        //    ModernMainView mainView = Parent.Parent as ModernMainView;
        //    MainWindow window = mainView.Parent as MainWindow;
        //    //MainWindow window = MainWindow.GetMainWindow ();

        //    if ( _sceneVM == null )
        //    {
        //        _sceneVM = App.services.GetRequiredService<SceneViewModel> ();
        //    }

        //    if ( ( window != null ) && ( _sceneVM.IncorrectBadges.Count > 0 ) )
        //    {
        //        BadgeEditorView editorView = new BadgeEditorView ();
        //        editorView.ChangeSize (window.WidthDifference, window.HeightDifference);
        //        window.CancelSizeDifference ();
        //        editorView.PassIncorrectBadges (_sceneVM.IncorrectBadges);
        //        editorView.PassBackPoint (mainView);
        //        window.Content = editorView;
        //    }
        //}

    }



    public partial class EditorButtom : Button 
    {
        internal List<BadgeViewModel> IncorrectBadges { get; private set; }


        public EditorButtom(): base () { }
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

//internal void ZoomOn ( object sender, TappedEventArgs args )
//{
//    if ( _scalabilityDepth < _maxDepth )
//    {

//        _vm.ZoomOn (_scalabilityStep);
//        _scalabilityDepth++;
//    }

//    if ( _scalabilityDepth == _maxDepth )
//    {
//        zoomOn.IsEnabled = false;
//    }

//    if ( !zoomOut.IsEnabled )
//    {
//        zoomOut.IsEnabled = true;
//    }
//}


//internal void ZoomOut ( object sender, TappedEventArgs args )
//{
//    if ( _scalabilityDepth > _minDepth )
//    {
//        _vm.ZoomOut (_scalabilityStep);
//        _scalabilityDepth--;
//    }

//    if ( _scalabilityDepth == _minDepth )
//    {
//        zoomOut.IsEnabled = false;
//    }

//    if ( !zoomOn.IsEnabled )
//    {
//        zoomOn.IsEnabled = true;
//    }
//}