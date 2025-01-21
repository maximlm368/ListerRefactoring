using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Views;

public partial class PageNavigationZoomer : UserControl
{
    private PageNavigationZoomerViewModel _viewModel;
    private SceneViewModel _sceneVM;


    public PageNavigationZoomer()
    {
        InitializeComponent();
        DataContext = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();
        _viewModel = ( PageNavigationZoomerViewModel ) DataContext;

        firstPage.FocusAdorner = null;
        //firstPage.Background = null;
        
        previousPage.FocusAdorner = null;
        //previousPage.Background = null;
        
        nextPage.FocusAdorner = null;
        //nextPage.Background = null;
        
        lastPage.FocusAdorner = null;
        //lastPage.Background = null;
        
        zoomOn.FocusAdorner = null;
        //zoomOn.Background = null;

        zoomOut.FocusAdorner = null;
        //zoomOut.Background = null;

        visiblePageNumber.AddHandler (TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel);

        
        //Border border = new Border();
        //border.BoxShadow = new Avalonia.Media.BoxShadows ();

        
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

        try
        {
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
        catch ( System.FormatException exp )
        {
            if ( ! string.IsNullOrWhiteSpace (text) )
            {
                visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
            }
        }
        catch ( System.OverflowException exp )
        {
            visiblePageNumber.Text = _viewModel.VisiblePageNumber.ToString ();
        }
    }
}