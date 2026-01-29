using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

internal partial class EditorViewModel : ObservableObject
{
    public readonly double _scalabilityCoefficient = 1.25;
    private readonly short _maxDepth = 5;
    private readonly short _minDepth = 0;
    private double _zoomDegree = 100;
    private string procentSymbol = "%";
    private short _scalabilityDepth = 2;

    private bool _zoomerIsEnable;
    internal bool ZoommerIsEnable
    {
        get => _zoomerIsEnable;

        private set
        {
            if ( value )
            {
                FocusedFontSizeColor = _focusedFontsizeColor;
                FocusedFontSizeBorderColor = _focusedFontsizeBorderColor;
            }
            else
            {
                FocusedFontSizeColor = _releasedFontsizeColor;
                FocusedFontSizeBorderColor = _releasedFontsizeBorderColor;
            }

            _zoomerIsEnable = value;
            OnPropertyChanged ();
        }
    }

    [ObservableProperty]
    private bool _zoomOnIsEnable;

    [ObservableProperty]
    private bool _zoomOutIsEnable;

    [ObservableProperty]
    private string? _zoomDegreeInView;

    private void SetUpZoommer ()
    {
        ZoomOnIsEnable = true;
        ZoomOutIsEnable = true;
        _zoomDegree *= _scalabilityCoefficient;
        _zoomDegree *= _scalabilityCoefficient;
        ZoomDegreeInView = Math.Round ( _zoomDegree ).ToString () + " " + procentSymbol;
    }

    #region Zoom
    [RelayCommand]
    internal void ZoomOn ()
    {
        if ( ProcessableBadge == null )
        {
            return;
        }

        if ( _scalabilityDepth < _maxDepth )
        {
            ProcessableBadge.ZoomOn ( _scalabilityCoefficient );
            _scalabilityDepth++;
            _zoomDegree *= _scalabilityCoefficient;
            _scale *= _scalabilityCoefficient;
            short zDegree = ( short ) _zoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
        }

        if ( _scalabilityDepth == _maxDepth )
        {
            ZoomOnIsEnable = false;
        }

        if ( !ZoomOutIsEnable )
        {
            ZoomOutIsEnable = true;
        }
    }

    [RelayCommand]
    internal void ZoomOut ()
    {
        if ( ProcessableBadge == null )
        {
            return;
        }

        if ( _scalabilityDepth > _minDepth )
        {
            ProcessableBadge.ZoomOut ( _scalabilityCoefficient );
            _scalabilityDepth--;
            _zoomDegree /= _scalabilityCoefficient;
            _scale /= _scalabilityCoefficient;
            short zDegree = ( short ) _zoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
        }

        if ( _scalabilityDepth == _minDepth )
        {
            ZoomOutIsEnable = false;
        }

        if ( !ZoomOnIsEnable )
        {
            ZoomOnIsEnable = true;
        }
    }
    #endregion

    #region Scale
    private void SetOriginalScale ( BadgeViewModel beingPrecessed, double scale )
    {
        if ( scale != 1 )
        {
            beingPrecessed.ZoomOn ( scale );
        }
    }

    private void SetToCorrectScale ( BadgeViewModel? processable )
    {
        if ( processable == null ) 
        {
            return;
        }

        if ( processable.Scale != _scale )
        {
            if ( processable.Scale != 1 )
            {
                processable.ZoomOut ( processable.Scale );
            }

            processable.ZoomOn ( _scale );
        }
    }
    #endregion
}
