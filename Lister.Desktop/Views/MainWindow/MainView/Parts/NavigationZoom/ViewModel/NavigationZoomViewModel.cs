using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.NavigationZoom.ViewModel;

public partial class NavigationZoomViewModel : ObservableObject
{
    private readonly double _scalabilityCoefficient = 1.25;
    private readonly string _procentSymbol;
    private readonly short _maxDepth;
    private readonly short _minDepth;
    private short _scalabilityDepth = 0;

    [ObservableProperty]
    private int _visiblePageNumber;

    [ObservableProperty]
    private bool _firstIsEnable;

    [ObservableProperty]
    private bool _previousIsEnable;

    [ObservableProperty]
    private bool _nextIsEnable;

    [ObservableProperty]
    private bool _lastIsEnable;

    [ObservableProperty]
    private bool _zoomOnIsEnable;

    [ObservableProperty]
    private bool _zoomOutIsEnable;

    [ObservableProperty]
    private string _zoomDegreeInView = "100";

    [ObservableProperty]
    private double _zoomDegree;

    [ObservableProperty]
    private int _pageCount;

    public NavigationZoomViewModel ( string procentSymbol, short maxDepth, short minDepth )
    {
        _procentSymbol = procentSymbol;
        _maxDepth = maxDepth;
        _minDepth = minDepth;

        ToZeroState ();
    }

    [RelayCommand]
    internal void ShowNextPage ()
    {
        if ( VisiblePageNumber < PageCount )
        {
            VisiblePageNumber++;
        }

        SetEnablePageNavigation ();
    }

    [RelayCommand]
    internal void ShowPreviousPage ()
    {
        if ( VisiblePageNumber > 1 )
        {
            VisiblePageNumber--;
        }

        SetEnablePageNavigation ();
    }

    [RelayCommand]
    internal void ShowLastPage ()
    {
        if ( VisiblePageNumber < PageCount )
        {
            VisiblePageNumber = PageCount;
        }

        SetEnablePageNavigation ();
    }

    [RelayCommand]
    internal void ShowFirstPage ()
    {
        if ( VisiblePageNumber > 1 )
        {
            VisiblePageNumber = 1;
        }

        SetEnablePageNavigation ();
    }

    internal void ShowPageWithNumber ( int pageNumber )
    {
        int num = VisiblePageNumber;

        if ( pageNumber < 1 || pageNumber > PageCount )
        {
            VisiblePageNumber = num;
            return;
        }

        VisiblePageNumber = pageNumber;
        SetEnablePageNavigation ();
    }

    [RelayCommand]
    internal void ZoomOn ()
    {
        if ( _scalabilityDepth < _maxDepth )
        {
            ZoomDegree *= _scalabilityCoefficient;
            short zDegree = ( short ) ZoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + _procentSymbol;
            _scalabilityDepth++;
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
        if ( _scalabilityDepth > _minDepth )
        {
            ZoomDegree /= _scalabilityCoefficient;
            short zDegree = ( short ) ZoomDegree;
            ZoomDegreeInView = zDegree.ToString () + " " + _procentSymbol;
            _scalabilityDepth--;
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

    internal void SetEnablePageNavigation ( int pageCount, int currentVisibleNum )
    {
        if ( pageCount > 0 )
        {
            PageCount = pageCount;
            VisiblePageNumber = currentVisibleNum;
            SetEnablePageNavigation ();
        }
    }

    internal void SetEnablePageNavigation ()
    {
        if ( VisiblePageNumber > 1 && VisiblePageNumber == PageCount )
        {
            FirstIsEnable = true;
            PreviousIsEnable = true;
            NextIsEnable = false;
            LastIsEnable = false;
        }
        else if ( VisiblePageNumber > 1 && VisiblePageNumber < PageCount )
        {
            FirstIsEnable = true;
            PreviousIsEnable = true;
            NextIsEnable = true;
            LastIsEnable = true;
        }
        else if ( VisiblePageNumber == 1 && PageCount == 1 )
        {
            FirstIsEnable = false;
            PreviousIsEnable = false;
            NextIsEnable = false;
            LastIsEnable = false;
        }
        else if ( VisiblePageNumber == 1 && PageCount > 1 )
        {
            FirstIsEnable = false;
            PreviousIsEnable = false;
            NextIsEnable = true;
            LastIsEnable = true;
        }
    }

    internal void ToZeroState ()
    {
        ZoomOnIsEnable = false;
        ZoomOutIsEnable = false;
        FirstIsEnable = false;
        PreviousIsEnable = false;
        NextIsEnable = false;
        LastIsEnable = false;

        _scalabilityDepth = 0;
        ZoomDegree = 100;
        ZoomDegreeInView = ZoomDegree.ToString () + " " + _procentSymbol;
        VisiblePageNumber = 1;
        PageCount = 0;
    }

    internal void EnableZoomIfPossible ( bool isPossible )
    {
        if ( isPossible )
        {
            ZoomOnIsEnable = true;
            ZoomOutIsEnable = true;
        }
    }

    internal void RecoverPageCounterIfEmpty ()
    {
        VisiblePageNumber = 1;
    }
}
