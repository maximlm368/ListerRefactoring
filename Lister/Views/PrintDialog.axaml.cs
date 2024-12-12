using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;
using ReactiveUI;
using Avalonia.Media;
using ExtentionsAndAuxiliary;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Interactivity;
using static QuestPDF.Helpers.Colors;

namespace Lister.Views;

public partial class PrintDialog : BaseWindow
{
    private PrintDialogViewModel _viewModel;


    public PrintDialog ()
    {
        InitializeComponent ();
    }


    public PrintDialog ( int pageAmount, PrintAdjustingData printAdjusting ) : this()
    {
        DataContext = App.services.GetRequiredService<PrintDialogViewModel> ();
        _viewModel = (PrintDialogViewModel) DataContext;

        _viewModel.Prepare ();
        _viewModel.TakeAmmountAndAdjusting (pageAmount, printAdjusting);

        printerChoosing.SelectedIndex = _viewModel.SelectedIndex;
        allPages.IsChecked = true;
        amountText.Text = "1";

        pagesText.AcceptsReturn = true;

        CanResize = false;

        //printerChoosing.SelectionChanged += PrinterChanged;

        Activated += delegate { cancel.Focus (NavigationMethod.Tab, KeyModifiers.None); };
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
        _viewModel.Pages = textBox.Text;
        textBox.Text = _viewModel.Pages;
    }


    public void CopiesChanged ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = sender as TextBox;
        _viewModel.Copies = textBox.Text;
        textBox.Text = _viewModel.Copies;
    }


    public void PrinterChanged ( object sender, SelectionChangedEventArgs args )
    {

    }
}



public class PrintAdjustingData 
{
    public string PrinterName { get; set; }
    public List<int> PageNumbers { get; set; }
    public int CopiesAmount { get; set; }
    public bool Cancelled { get; set; }
}