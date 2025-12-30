using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;

public partial class SceneViewModel : ObservableObject
{
    private readonly double _buttonWidthq = 32;
    private readonly double _extention = 170;
    private readonly double _buttonBlockWidthq = 64;
    private readonly double _workAreaLeftMargin = -74;
    private readonly string _extentionToolTip = string.Empty;
    private readonly string _shrinkingToolTip = string.Empty;

    private bool _blockIsExtended = false;

    [ObservableProperty]
    private double _hintWidth;

    [ObservableProperty]
    private double _buttonWidth;

    [ObservableProperty]
    private double _buttonBlockWidth;

    [ObservableProperty]
    private Thickness _workAreaMargin;

    [ObservableProperty]
    private Thickness _extenderMargin;

    [ObservableProperty]
    private string _extentionTip = string.Empty;

    [ObservableProperty]
    private string _extenderContent = string.Empty;

    private bool _pdfIsWanted;
    public bool PdfIsWanted
    {
        get { return _pdfIsWanted; }
        private set
        {
            _pdfIsWanted = value;
            OnPropertyChanged ();
        }
    }
    
    private bool _printingIsWanted;
    public bool PrintingIsWanted
    {
        get { return _printingIsWanted; }
        private set
        {
            _printingIsWanted = value;
            OnPropertyChanged ();
        }
    }

    private void SetButtonBlock ( )
    {
        ButtonWidth = _buttonWidthq;
        HintWidth = 0;
        ButtonBlockWidth = _buttonBlockWidthq;
        WorkAreaMargin = new Thickness (_workAreaLeftMargin, 0);
        ExtenderMargin = new Thickness (30, 8);
        ExtentionTip = _extentionToolTip;
        ExtenderContent = "\uF061";
    }

    [RelayCommand]
    internal void ExtendButtons ()
    {
        if ( _blockIsExtended )
        {
            HintWidth = 0;
            ButtonWidth -= _extention;
            ButtonBlockWidth -= _extention;
            WorkAreaMargin = new Thickness (( WorkAreaMargin. Left + _extention ), 0);
            ExtenderMargin = new Thickness (( ExtenderMargin.Left - _extention ), 8);
            ExtentionTip = _extentionToolTip;
            ExtenderContent = "\uF061";
            _blockIsExtended = false;
        }
        else 
        {
            HintWidth = _extention;
            ButtonWidth += _extention;
            ButtonBlockWidth += _extention;
            WorkAreaMargin = new Thickness (( WorkAreaMargin. Left - _extention ), 0);
            ExtenderMargin = new Thickness (( ExtenderMargin.Left + _extention ), 8);
            ExtentionTip = _shrinkingToolTip;
            ExtenderContent = "\uF060";
            _blockIsExtended = true;
        }
    }

    internal void Edit ()
    {
        EditIsSelected = true;
    }

    internal void ClearBadges ()
    {
        ClearAllPages ();
        DisableButtons ();
        BadgesAreCleared = true;
        BadgesAreCleared = false;
    }

    internal void GeneratePdf ()
    {
        PdfIsWanted = true;
    }

    public void Print ()
    {
        PrintingIsWanted = true;
    }
}
