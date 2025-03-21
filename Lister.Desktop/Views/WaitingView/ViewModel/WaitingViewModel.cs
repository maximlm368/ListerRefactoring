using AnimatedImage.Avalonia;
using Avalonia;
using View.App;
using View.MainWindow.MainView.ViewModel;
using ReactiveUI;

namespace View.WaitingView.ViewModel;

public partial class WaitingViewModel : ReactiveObject
{
    private readonly double _canvasHiddenVerticalMargin = 12;

    private WaitingView? _view;
    private double _canvasTopStorage = 80;
    private double _canvasLeftStorage = 265;
    private double _canvasHeightStorage = 506;
    private double _canvasWidthStorage = 830;
    private double _canvasShownVerticalMargin = -494;
    private double _imageHeightStorage = 300;
    private string _gifName;
    private AnimatedImageSource _gifSourceStorage;

    private AnimatedImageSource? _gifSource;
    public AnimatedImageSource? GifSource
    {
        get { return _gifSource; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _gifSource, value, nameof( GifSource ) );
        }
    }

    private double _canvasTop;
    public double CanvasTop
    {
        get { return _canvasTop; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _canvasTop, value, nameof( CanvasTop ) );
        }
    }

    private double _canvasLeft;
    public double CanvasLeft
    {
        get { return _canvasLeft; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _canvasLeft, value, nameof( CanvasLeft ) );
        }
    }

    private double _canvasHeight;
    public double CanvasHeight
    {
        get { return _canvasHeight; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _canvasHeight, value, nameof( CanvasHeight ) );
        }
    }

    private double _canvasWidth;
    public double CanvasWidth
    {
        get { return _canvasWidth; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _canvasWidth, value, nameof( CanvasWidth ) );
        }
    }

    private double _imageHeight;
    public double ImageHeight
    {
        get { return _imageHeight; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _imageHeight, value, nameof( ImageHeight ) );
        }
    }

    private bool _imageIsVisible;
    public bool ImageIsVisible
    {
        get { return _imageIsVisible; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _imageIsVisible, value, nameof( ImageIsVisible ) );
        }
    }

    private double _progressWidth;
    public double ProgressWidth
    {
        get { return _progressWidth; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _progressWidth, value, nameof( ProgressWidth ) );
        }
    }

    private Thickness _margin;
    public Thickness Margin
    {
        get { return _margin; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _margin, value, nameof( Margin ) );
        }
    }


    public WaitingViewModel(string gifName)
    {
        _gifName = gifName;
        Margin = new Thickness( 0, _canvasHiddenVerticalMargin );
    }


    internal void Darken()
    {
        CanvasHeight = _canvasHeightStorage;
        CanvasWidth = _canvasWidthStorage;
        Margin = new Thickness( 0, _canvasShownVerticalMargin );
        CanvasTop = _canvasTopStorage;
        CanvasLeft = _canvasLeftStorage;
    }


    internal void HandleDialogClosing()
    {
        Margin = new Thickness( 0, _canvasHiddenVerticalMargin );
    }


    public void Show()
    {
        CanvasHeight = _canvasHeightStorage;
        CanvasWidth = _canvasWidthStorage;
        Margin = new Thickness( 0, _canvasShownVerticalMargin );
        CanvasTop = _canvasTopStorage;
        CanvasLeft = _canvasLeftStorage;

        if (_gifSourceStorage == null)
        {
            string waintingImageIriString = "avares://Lister.Desktop/Assets/" + _gifName;
            _gifSourceStorage = new AnimatedImageSourceUri( new Uri( waintingImageIriString ) );
        }

        GifSource = _gifSourceStorage;
    }


    public void Hide()
    {
        Margin = new Thickness( 0, _canvasHiddenVerticalMargin );
        GifSource = null;
    }


    public void ChangeSize(double heightDiff, double widthDiff)
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

        if (MainViewModel.MainViewIsWaiting)
        {
            Margin = new Thickness( 0, _canvasShownVerticalMargin );
        }
    }
}
