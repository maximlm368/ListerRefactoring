using AnimatedImage.Avalonia;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lister.Desktop.Views.WaitingView.ViewModel;

public partial class WaitingViewModel : ObservableObject
{
    private readonly string? _gifPath;
    private AnimatedImageSource? _gifSourceStorage;

    [ObservableProperty]
    private AnimatedImageSource? _gifSource;

    [ObservableProperty]
    private bool _curtainIsVisible = false;

    public WaitingViewModel ()
    {

    }

    public WaitingViewModel ( string gifPath )
    {
        _gifPath = gifPath;

        if ( _gifSourceStorage == null && _gifPath != null )
        {
            _gifSourceStorage = new AnimatedImageSourceStream ( AssetLoader.Open ( new Uri ( _gifPath ) ) );
        }
    }

    public void Show ()
    {
        GifSource = null;
        CurtainIsVisible = true;
    }

    public void ShowWithGif ()
    {
        GifSource = _gifSourceStorage;
        CurtainIsVisible = true;
    }

    public void Hide ()
    {
        GifSource = null;
        CurtainIsVisible = false;
    }
}
