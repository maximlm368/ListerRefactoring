using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using Lister.Desktop.App;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;

namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

public partial class PrintDialog : BaseWindow
{
    private static int _defaultSelectedIndex = -1;
    private PrintDialogViewModel _viewModel;


    public static PrintDialog GetPreparedDialog ( int pageAmount, PrintAdjustingData printAdjusting )
    {
        PrintDialog result = new PrintDialog (pageAmount, printAdjusting);

        result._viewModel.Prepare ();
        result._viewModel.TakeAmmountAndAdjusting (pageAmount, printAdjusting);
        result.printerChoosing.SelectedIndex = result._viewModel.SelectedIndex;

        if ( _defaultSelectedIndex < 0 )
        {
            _defaultSelectedIndex = result.printerChoosing.SelectedIndex;
        }

        if ( result.printerChoosing.SelectedIndex < 0 ) 
        {
            result.printerChoosing.SelectedIndex = _defaultSelectedIndex;
        }

        return result;
    }


    public PrintDialog ()
    {
        InitializeComponent ();
    }


    public PrintDialog ( int pageAmount, PrintAdjustingData printAdjusting ) : this()
    {
        DataContext = ListerApp.Services.GetRequiredService<PrintDialogViewModel> ();
        _viewModel = (PrintDialogViewModel) DataContext;
        allPages.IsChecked = true;
        amountText.Text = "1";
        pagesText.AcceptsReturn = true;
        CanResize = false;

        Activated += delegate { cancel.Focus (NavigationMethod.Tab, KeyModifiers.None); };

        printerSettings.FocusAdorner = null;
        print.FocusAdorner = null;
        cancel.FocusAdorner = null;
    }


    public void PagesLostFocus ( object sender, RoutedEventArgs args )
    {
        _viewModel.PagesLostFocus ();
    }


    public void PagesGotFocus ( object sender, GotFocusEventArgs args )
    {
        _viewModel.PagesGotFocus ();
    }


    public void CopiesLostFocus ( object sender, RoutedEventArgs args )
    {
        _viewModel.CopiesLostFocus ();
    }


    public void CopiesGotFocus ( object sender, GotFocusEventArgs args )
    {
        _viewModel.CopiesGotFocus ();
    }


    public void PagesSetChanged ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = sender as TextBox;

        if ( _viewModel.Pages == textBox.Text ) 
        {
            return;
        }

        _viewModel.Pages = textBox.Text;
        textBox.Text = _viewModel.Pages;
    }


    public void CopiesChanged ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = sender as TextBox;
        _viewModel.Copies = textBox.Text;
        textBox.Text = _viewModel.Copies;
    }
}