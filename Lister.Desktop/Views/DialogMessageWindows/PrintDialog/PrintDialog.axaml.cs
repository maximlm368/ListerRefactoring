using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Lister.Desktop.App;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

public sealed partial class PrintDialog : Window
{
    private static int _defaultSelectedIndex = -1;
    private readonly PrintDialogViewModel _viewModel;

    public PrintDialog ()
    {
        InitializeComponent ();

        DataContext = ListerApp.Services.GetRequiredService<PrintDialogViewModel> ();
        _viewModel = ( PrintDialogViewModel ) DataContext;

        Activated += ( s, a ) => Cancel.Focus ( NavigationMethod.Tab, KeyModifiers.None );
    }

    public PrintDialog ( int pageAmount, PrintAdjustingData printAdjusting ) : this()
    {
        AllPages.IsChecked = true;
        AmountText.Text = "1";
        PagesText.AcceptsReturn = true;
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
    }

    public void PagesLostFocus ( object sender, RoutedEventArgs args )
    {
        PagesError.IsVisible = true;
    }

    public void PagesGotFocus ( object sender, GotFocusEventArgs args )
    {
        PagesError.IsVisible = false;
    }

    public void CopiesLostFocus ( object sender, RoutedEventArgs args )
    {
        CopiesError.IsVisible = true;
    }

    public void CopiesGotFocus ( object sender, GotFocusEventArgs args )
    {
        CopiesError.IsVisible = false;
    }

    public void PagesSetChanged ( object sender, TextChangedEventArgs args )
    {
        if ( sender is TextBox textBox && _viewModel.PagesInString != textBox.Text )
        {
            _viewModel.PagesInString = textBox.Text;
            textBox.Text = _viewModel.PagesInString;
        }
    }

    public void CopiesChanged ( object sender, TextChangedEventArgs args )
    {
        if ( sender is TextBox textBox )
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
