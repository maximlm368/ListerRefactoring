using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Lister.Desktop.App;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Microsoft.Extensions.DependencyInjection;

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
        allPages.IsChecked = true;
        amountText.Text = "1";
        pagesText.AcceptsReturn = true;
        printerSettings.FocusAdorner = null;
        print.FocusAdorner = null;
        cancel.FocusAdorner = null;

        Activated += ( s, a ) => { cancel.Focus ( NavigationMethod.Tab, KeyModifiers.None ); };
    }

    public PrintDialog ( int pageAmount, PrintAdjustingData printAdjusting ) : this()
    {
        _viewModel.AdjustPrinting ( pageAmount, printAdjusting );
        printerChoosing.SelectedIndex = _viewModel.SelectedIndex;

        if ( _defaultSelectedIndex < 0 )
        {
            _defaultSelectedIndex = printerChoosing.SelectedIndex;
        }

        if ( printerChoosing.SelectedIndex < 0 )
        {
            printerChoosing.SelectedIndex = _defaultSelectedIndex;
        }
    }

    public void PagesLostFocus ( object sender, RoutedEventArgs args )
    {
        pagesError.IsVisible = true;
    }

    public void PagesGotFocus ( object sender, GotFocusEventArgs args )
    {
        pagesError.IsVisible = false;
    }

    public void CopiesLostFocus ( object sender, RoutedEventArgs args )
    {
        copiesError.IsVisible = true;
    }

    public void CopiesGotFocus ( object sender, GotFocusEventArgs args )
    {
        copiesError.IsVisible = false;
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
}
