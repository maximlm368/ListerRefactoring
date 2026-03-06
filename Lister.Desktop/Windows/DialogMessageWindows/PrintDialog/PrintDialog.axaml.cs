using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Windows.DialogMessageWindows.PrintDialog;
using System.ComponentModel;

namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

public sealed partial class PrintDialogWindow : Window
{
    private static int _defaultSelectedIndex = -1;
    private readonly PrintDialogViewModel? _viewModel;

    public PrintDialogWindow ()
    {
        InitializeComponent ();
    }

    public PrintDialogWindow ( int pageAmount, PrintAdjustingData printAdjusting, PrintDialogViewModel dataContext ) : this ()
    {
        DataContext = dataContext;
        _viewModel = ( PrintDialogViewModel ) DataContext;
        Activated += ( s, a ) => Cancel.Focus ( NavigationMethod.Tab, KeyModifiers.None );

        AllPages.IsChecked = true;
        Pages.AcceptsReturn = true;
        PrinterSettings.FocusAdorner = null;
        Print.FocusAdorner = null;
        Cancel.FocusAdorner = null;

        _viewModel.PropertyChanged += ViewModelChanged;

        _viewModel.AdjustPrinting ( pageAmount, printAdjusting );
        PrinterChoosing.SelectedIndex = _viewModel.SelectedIndex;

        if ( _defaultSelectedIndex < 0 )
        {
            _defaultSelectedIndex = PrinterChoosing.SelectedIndex;
        }

        if ( PrinterChoosing.SelectedIndex < 0 )
        {
            PrinterChoosing.SelectedIndex = _defaultSelectedIndex;
        }

        AllPages.IsCheckedChanged += ( sender, args ) =>
        {
            if ( AllPages.IsChecked == true )
            {
                PagesError.IsVisible = IncorrectPages.IsVisible = false;
                Pages.IsVisible = true;
            }
        };
    }

    private void PagesGotFocus ( object sender, GotFocusEventArgs args )
    {
        if ( sender is TextBox textBox && string.IsNullOrEmpty ( textBox.Text ) )
        {
            _viewModel.Pages = string.Empty;
        }

        PagesError.IsVisible = IncorrectPages.IsVisible = false;
        Pages.IsVisible = true;
        Pages.Focus ();
    }

    private void PagesLostFocus ( object sender, RoutedEventArgs args )
    {
        PagesError.IsVisible = IncorrectPages.IsVisible = _viewModel.HasPagesError;
    }

    private void CopiesGotFocus ( object sender, GotFocusEventArgs args )
    {
        CopiesError.IsVisible = IncorrectCopies.IsVisible = false;
        Copies.IsVisible = true;
        Copies.Focus ();
    }

    private void CopiesLostFocus ( object sender, RoutedEventArgs args )
    {
        CopiesError.IsVisible = IncorrectCopies.IsVisible = _viewModel.HasCopiesError;
    }

    private void PagesChanged ( object sender, TextChangedEventArgs args )
    {
        if ( sender is TextBox textBox && _viewModel.Pages != textBox.Text )
        {
            _viewModel.Pages = textBox.Text;
            textBox.Text = _viewModel.Pages;
        }
    }

    private void CopiesChanged ( object sender, TextChangedEventArgs args )
    {
        if ( sender is TextBox textBox && _viewModel.Copies != textBox.Text )
        {
            _viewModel.Copies = textBox.Text;
            textBox.Text = _viewModel.Copies;
        }
    }

    private void ViewModelChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "IsClosing" )
        {
            Close ();
        }
    }
}
