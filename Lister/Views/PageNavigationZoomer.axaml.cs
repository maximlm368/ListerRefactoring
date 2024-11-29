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
    private PageNavigationZoomerViewModel _vm;
    private short _scalabilityDepth = 0;
    private short _maxDepth = 5;
    private short _minDepth = -5;
    private readonly short _scalabilityStep = 25;
    private ushort _maxScalability;
    private ushort _minScalability;
    private SceneViewModel _sceneVM;


    public PageNavigationZoomer()
    {
        InitializeComponent();
        DataContext = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();
        _vm = ( PageNavigationZoomerViewModel ) DataContext;

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


    private void PageCounterLostFocus ( object sender, RoutedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
        string text = textBox.Text;

        if ( text == "" ) 
        {
            _vm.RecoverPageCounterIfEmpty ();
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
                visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
                return;
            }

            _vm.ShowPageWithNumber (pageNumber);
            visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
        }
        catch ( System.FormatException exp )
        {
            if ( ! string.IsNullOrWhiteSpace (text) )
            {
                visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
            }
        }
        catch ( System.OverflowException exp )
        {
            visiblePageNumber.Text = _vm.VisiblePageNumber.ToString ();
        }
    }
}