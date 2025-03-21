using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using View.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using View.MainWindow.MainView.Parts.Scene.ViewModel;
using View.App;
using Microsoft.Extensions.DependencyInjection;


namespace View.MainWindow.MainView.Parts.NavigationZoom;

public partial class NavigationZoomUserControl : UserControl
{
    private NavigationZoomViewModel _viewModel;
    private SceneViewModel _sceneVM;
    private char [] _pageNumberAcceptables = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };


    public NavigationZoomUserControl()
    {
        InitializeComponent();
        DataContext = ListerApp.Services.GetRequiredService<NavigationZoomViewModel> ();
        _viewModel = ( NavigationZoomViewModel ) DataContext;

        firstPage.FocusAdorner = null;
        previousPage.FocusAdorner = null;
        nextPage.FocusAdorner = null;
        lastPage.FocusAdorner = null;
        zoomOn.FocusAdorner = null;
        zoomOut.FocusAdorner = null;

        visiblePageNumber.AddHandler (TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel);
    }


    private void PreventPasting ( object sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
    }


    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        MainView.SomeControlPressed = true;
    }


    private void PageCounterLostFocus ( object sender, RoutedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
        string text = textBox.Text;

        if ( text == "" ) 
        {
            _viewModel.RecoverPageCounterIfEmpty ();
            visiblePageNumber.Text = "1";
        }
    }


    internal void StepOnPage ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
        string text = textBox.Text;

        if ( string.IsNullOrWhiteSpace (text) ) return; 

        for ( int index = 0;   index < text.Length;   index++ ) 
        {
            if ( !_pageNumberAcceptables.Contains (text [index]) ) 
            {
                visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
                return;
            }
        }

        int pageNumber = ( int ) UInt32.Parse (text);

        if ( pageNumber == 0 )
        {
            pageNumber = 1;
            visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
            return;
        }

        _viewModel.ShowPageWithNumber (pageNumber);
        visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
    }
}