using AnimatedImage.Avalonia;
using Avalonia;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;

namespace Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;

public partial class WaitingViewModel : ObservableObject
{
    private readonly double _canvasHiddenVerticalMargin = 12;

    private double _canvasTopStorage = 80;
    private double _canvasLeftStorage = 265;
    private double _canvasHeightStorage = 506;
    private double _canvasWidthStorage = 830;
    private double _canvasShownVerticalMargin = -494;
    private readonly string? _gifPath;
    private AnimatedImageSource? _gifSourceStorage;

    [ObservableProperty]
    private AnimatedImageSource? _gifSource;

    [ObservableProperty]
    private double _canvasTop;

    [ObservableProperty]
    private double _canvasLeft;

    [ObservableProperty]
    private double _canvasHeight;

    [ObservableProperty]
    private double _canvasWidth;

    [ObservableProperty]
    private double _imageHeight;

    [ObservableProperty]
    private bool _imageIsVisible;

    [ObservableProperty]
    private double _progressWidth;

    [ObservableProperty]
    private Thickness _margin;

    public WaitingViewModel ()
    {
        Margin = new Thickness ( 0, _canvasHiddenVerticalMargin );
    }

    public WaitingViewModel ( string gifPath )
    {
        _gifPath = gifPath;
        Margin = new Thickness ( 0, _canvasHiddenVerticalMargin );
    }

    internal void Darken ()
    {
        CanvasHeight = _canvasHeightStorage;
        CanvasWidth = _canvasWidthStorage;
        Margin = new Thickness ( 0, _canvasShownVerticalMargin );
        CanvasTop = _canvasTopStorage;
        CanvasLeft = _canvasLeftStorage;
    }

    internal void HandleDialogClosing ()
    {
        Margin = new Thickness ( 0, _canvasHiddenVerticalMargin );
    }

    public void Show ()
    {
        CanvasHeight = _canvasHeightStorage;
        CanvasWidth = _canvasWidthStorage;
        Margin = new Thickness ( 0, _canvasShownVerticalMargin );
        CanvasTop = _canvasTopStorage;
        CanvasLeft = _canvasLeftStorage;

        if ( _gifSourceStorage == null && _gifPath != null )
        {
            _gifSourceStorage = new AnimatedImageSourceStream ( AssetLoader.Open ( new Uri ( _gifPath ) ) );
        }

        GifSource = _gifSourceStorage;
    }

    public void Hide ()
    {
        Margin = new Thickness ( 0, _canvasHiddenVerticalMargin );
        GifSource = null;
    }

    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        CanvasWidth -= widthDiff;
        _canvasWidthStorage -= widthDiff;
        CanvasHeight -= heightDiff;
        _canvasHeightStorage -= heightDiff;
        CanvasTop -= heightDiff / 2;
        _canvasTopStorage -= heightDiff / 2;
        CanvasLeft -= widthDiff / 2;
        _canvasLeftStorage -= widthDiff / 2;
        _canvasShownVerticalMargin += heightDiff;

        if ( MainViewModel.MainViewIsWaiting )
        {
            Margin = new Thickness ( 0, _canvasShownVerticalMargin );
        }
    }
}
