using Avalonia.Controls;
using Avalonia.Input;
using Lister.ViewModels;
using Lister.Views;
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
    }



    public partial class EditorButtom : Button 
    {
        internal List<BadgeViewModel> IncorrectBadges { get; private set; }


        public EditorButtom(): base () { }
    }
}


