using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Components.Zoomer.ViewModel;

public partial class ZoomerViewModel : ObservableObject
{
    private readonly double _zoomStep = 1.25;
    private readonly string _suffix = "%";
    private readonly short _maxZoom;
    private readonly short _minZoom;
    private short _currentZoom = 0;

    [ObservableProperty]
    private bool _isZoomOnEnable;

    [ObservableProperty]
    private bool _isZoomOutEnable;

    [ObservableProperty]
    private string _zoomDegreeInView = "100";

    [ObservableProperty]
    private double _zoomDegree;

    internal event Action<double>? ZoomedOn;
    internal event Action<double>? ZoomedOut;

    internal ZoomerViewModel ( string suffix, short maxZoom, short minZoom, short startZoomStep ) 
    {
        _suffix = suffix;
        _maxZoom = maxZoom;
        _minZoom = minZoom;

        ToZeroState ();

        for ( int index = 0; index < startZoomStep; index++ )
        {
            ZoomOn ();
        }

        _currentZoom = startZoomStep;
        IsZoomOnEnable = startZoomStep < _maxZoom;
        IsZoomOutEnable = startZoomStep > _minZoom;
    }

    internal void ToZeroState ( )
    {
        ZoomDegree = 100;
        ZoomDegreeInView = ZoomDegree.ToString () + " " + _suffix;
        IsZoomOnEnable = _currentZoom < _maxZoom;
        IsZoomOutEnable = _currentZoom > _minZoom;
    }

    [RelayCommand]
    internal void ZoomOn ()
    {
        if ( _currentZoom < _maxZoom )
        {
            ZoomDegree *= _zoomStep;
            short zDegree = ( short ) ZoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + _suffix;
            _currentZoom++;
        }

        if ( _currentZoom == _maxZoom )
        {
            IsZoomOnEnable = false;
        }

        if ( !IsZoomOutEnable )
        {
            IsZoomOutEnable = true;
        }

        ZoomedOn?.Invoke ( _zoomStep );
    }

    [RelayCommand]
    internal void ZoomOut ()
    {
        if ( _currentZoom > _minZoom )
        {
            ZoomDegree /= _zoomStep;
            short zDegree = ( short ) ZoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + _suffix;
            _currentZoom--;
        }

        if ( _currentZoom == _minZoom )
        {
            IsZoomOutEnable = false;
        }

        if ( !IsZoomOnEnable )
        {
            IsZoomOnEnable = true;
        }

        ZoomedOut?.Invoke ( _zoomStep );
    }

    internal void EnableZoom ( )
    {
        IsZoomOnEnable = _currentZoom < _maxZoom;
        IsZoomOutEnable = _currentZoom > _minZoom;
    }
}
