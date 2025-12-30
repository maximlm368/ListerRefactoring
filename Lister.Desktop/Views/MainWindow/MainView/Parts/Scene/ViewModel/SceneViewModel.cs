using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using Lister.Core.Models;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;

public partial class SceneViewModel : ObservableObject
{
    public static bool EntireListBuildingIsChosen { get; private set; }

    private readonly int _badgeCountLimit;
    private readonly double _scalabilityCoefficient = 1.25;
    private readonly DocumentProcessor _model;
    private double _documentScale;
    private PageViewModel? _lastPage;
    private string _badgeTemplate = string.Empty;
    private Person? _chosenPerson;
    private bool _buildingIsLocked;
    private bool _isEntireBuilding;
    private bool _isEntireBuildingStarted;

    #region Properties
    internal List<PageViewModel> AllPages { get; private set; }

    [ObservableProperty]
    private PageViewModel? _visiblePage;

    [ObservableProperty]
    private int _pageCount;

    [ObservableProperty]
    private int _visiblePageNumber;

    [ObservableProperty]
    private int _badgeCount;

    [ObservableProperty]
    private int _incorrectBadgeCount;

    internal List <BadgeViewModel> ProcessableBadges { get; private set; }

    [ObservableProperty]
    private string _zoomDegreeInView = "100";

    [ObservableProperty]
    private Avalonia.Thickness _borderMargin;

    [ObservableProperty]
    private double _canvasTop;

    [ObservableProperty]
    private bool _editionIsEnable;

    [ObservableProperty]
    private bool _clearIsEnable;

    [ObservableProperty]
    private bool _saveIsEnable;

    [ObservableProperty]
    private bool _printIsEnable;

    private bool _editIsSelected;
    internal bool EditIsSelected
    {
        get
        {
            return _editIsSelected;
        }

        private set
        {
            _editIsSelected = value;
            OnPropertyChanged ();
            
        }
    }

    private bool _buildingIsOccured;
    internal bool BuildingIsOccured
    {
        get
        {
            return _buildingIsOccured;
        }

        private set
        {
            _buildingIsOccured = value;
            OnPropertyChanged ();
        }
    }

    private bool _badgesAreCleared;
    internal bool BadgesAreCleared
    {
        get
        {
            return _badgesAreCleared;
        }

        private set
        {
            _badgesAreCleared = value;
            OnPropertyChanged ();
        }
    }
    #endregion

    public SceneViewModel ( int badgeCountLimit, string extentionToolTip, string shrinkingToolTip, DocumentProcessor docBuilder ) 
    {
        _badgeCountLimit = badgeCountLimit;
        _extentionToolTip = extentionToolTip;
        _shrinkingToolTip = shrinkingToolTip;

        SetButtonBlock ();

        _model = docBuilder;
        _model.ComplatedPage += HandlePageCompleting;

        _documentScale = 1;
        AllPages = [];
        VisiblePage = new PageViewModel ( new Page (), _documentScale );
        _lastPage = VisiblePage;
        AllPages.Add (VisiblePage);

        VisiblePageNumber = 1;
        ProcessableBadges = [];
        EditionIsEnable = false;
        PageCount = 0;
    }

    internal void HandlePageCompleting ( Page complated, bool lastIsReplacable )
    {
        if ( !_isEntireBuilding ) 
        {
            return;
        }

        if ( AllPages.Count > 0 && _isEntireBuildingStarted )
        {
            PageViewModel last = AllPages.Last ();
            List<BadgeViewModel> lastBadges = last.GetBadges ();
            ProcessableBadges.RemoveRange ( ProcessableBadges.Count - lastBadges.Count, lastBadges.Count );
            AllPages.Remove ( last );
            _isEntireBuildingStarted = false;
        }

        Dispatcher.UIThread.Invoke ( 
            () =>
            {
                PageViewModel newPage = new ( complated, _documentScale );
                AllPages.Add ( newPage );
                ProcessableBadges.AddRange ( newPage.GetBadges () );
                BadgeCount += complated.BadgeCount;
                IncorrectBadgeCount = _model.IncorrectBadgeCount;
            } 
        );
    }

    internal int GetLimit ( )
    {
        return _badgeCountLimit;
    }

    internal void Build ( string templateName, Person ? person )
    {
        if ( string.IsNullOrWhiteSpace(templateName) ) 
        {
            return;
        }

        _chosenPerson = person;
        _badgeTemplate = templateName;

        if ( _chosenPerson == null )
        {
            EntireListBuildingIsChosen = true;
        }
        else
        {
            BuildSingleBadge ();
        }
    }

    internal void BuildDuringWaiting ()
    {
        if ( !_buildingIsLocked )
        {
            BuildAllBadges ();
        }
    }

    private void BuildAllBadges ()
    {
        _buildingIsLocked = true;

        Task task = new (
            () =>
            {
                bool buildingIsCompleted = BuildAllBadges (_badgeTemplate);

                _buildingIsLocked = false;
                EntireListBuildingIsChosen = false;

                if ( buildingIsCompleted )
                {
                    Dispatcher.UIThread.Invoke (
                        () =>
                        {
                            BuildingIsOccured = true;
                        }
                    );
                }
                else
                {
                    Dispatcher.UIThread.Invoke ( 
                        () =>
                        {
                            EntireListBuildingIsChosen = false;
                            BuildingIsOccured = false;
                        } 
                    );
                }
            }
        );

        task.Start ();
    }

    private void BuildSingleBadge ()
    {
        _buildingIsLocked = true;
        BuildingIsOccured = BuildSingleBadge ( _badgeTemplate );
        _buildingIsLocked = false;
    }

    private bool BuildAllBadges ( string templateName )
    {
        _isEntireBuilding = true;
        _isEntireBuildingStarted = true;

        int futureVisiblePageNumber = AllPages.Count;

        if ( futureVisiblePageNumber == 0 )
        {
            futureVisiblePageNumber = 1;
        }

        List <Page> builtPages = _model.BuildAllPages (templateName, _badgeCountLimit);

        if ( builtPages.Count < 1 ) 
        {
            return false;
        }

        _lastPage = AllPages.Last ();

        Dispatcher.UIThread.Invoke ( 
            () =>
            {
                VisiblePageNumber = futureVisiblePageNumber;
                BadgeCount = ProcessableBadges.Count;
                VisiblePage = AllPages [VisiblePageNumber - 1];
                PageCount = AllPages.Count;
                EnableButtons ();
                VisiblePage.Show ();
            } 
        );

        return true;
    }

    private bool BuildSingleBadge ( string templateName )
    {
        _isEntireBuilding = false;

        if ( ( BadgeCount + 1 ) >= _badgeCountLimit )
        {
            return false;
        }

        List<Page> resultPages =_model.BuildBadge (templateName, _chosenPerson);

        if ( resultPages.Count > AllPages.Count )
        {
            AllPages.Add ( new PageViewModel ( resultPages.Last (), _documentScale ) );
        }
        else 
        {
            AllPages [^1 ] = new PageViewModel ( resultPages.Last (), _documentScale );
        }

        BadgeViewModel added = AllPages [^1 ].GetBadges ().Last ();
        ProcessableBadges.Add ( added );
        _lastPage = AllPages [^1 ];

        VisiblePage?.Hide ();
        VisiblePage = _lastPage;
        VisiblePage.Show ();

        BadgeCount++;
        VisiblePageNumber = AllPages.Count;
        PageCount = AllPages.Count;
        IncorrectBadgeCount = _model.IncorrectBadgeCount;
        added.Show ();
        EnableButtons ();

        return true;
    }

    internal void ClearAllPages ()
    {
        if ( AllPages.Count == 0 )
        {
            return;
        }

        _model.Clear ();

        AllPages = [];
        VisiblePage = new PageViewModel ( new Page(), _documentScale );
        _lastPage = VisiblePage;
        AllPages.Add (_lastPage);

        VisiblePageNumber = 1;
        PageCount = 0;
        BadgeCount = 0;
        IncorrectBadgeCount = 0;

        EditionIsEnable = false;
        ProcessableBadges = [];

        if ( _documentScale > 1 )
        {
            while ( _documentScale != 1 )
            {
                ZoomOut ();
            }
        }
        else if ( _documentScale < 1 ) 
        {
            while ( _documentScale != 1 )
            {
                ZoomOn ();
            }
        }
    }

    internal void Zoom ( double newZoomDegree )
    {
        double degree = _documentScale * 100;

        if ( newZoomDegree > degree )
        {
            ZoomOn ();
        }
        else if ( newZoomDegree < degree ) 
        {
            ZoomOut ();
        }
    }

    private void ZoomOn ( )
    {
        _documentScale *= _scalabilityCoefficient;

        if ( VisiblePage != null )
        {
            for ( int pageCounter = 0;   pageCounter < AllPages.Count;   pageCounter++ )
            {
                AllPages [pageCounter].ZoomOn (_scalabilityCoefficient);
            }

            CanvasTop *= _scalabilityCoefficient;
            double marginLeft = _scalabilityCoefficient * BorderMargin. Left;
            BorderMargin = new Avalonia.Thickness (marginLeft, 0, 0, 0);
        }
    }

    private void ZoomOut ( )
    {
        _documentScale /= _scalabilityCoefficient;

        if ( VisiblePage != null )
        {
            for ( int pageCounter = 0;   pageCounter < AllPages.Count;   pageCounter++ )
            {
                AllPages [pageCounter].ZoomOut (_scalabilityCoefficient);
            }

            CanvasTop /= _scalabilityCoefficient;
            double marginLeft = BorderMargin. Left / _scalabilityCoefficient;
            BorderMargin = new Avalonia.Thickness (marginLeft, 0, 0, 0);
        }
    }

    internal int ShowPageWithNumber ( int pageNumber )
    {
        bool visiblePageExists = ( VisiblePage != null );
        bool notTheSamePage = ( VisiblePageNumber != pageNumber );
        bool inRange = pageNumber <= AllPages.Count;

        if ( visiblePageExists   &&   notTheSamePage   &&   inRange )
        {
            VisiblePage?.Hide ();
            VisiblePageNumber = pageNumber;
            VisiblePage = AllPages [VisiblePageNumber - 1];
            VisiblePage.Show ();
        }

        return VisiblePageNumber;
    }

    internal List <PageViewModel> GetPrintablePages ()
    {
        return AllPages;
    }

    private void DisableButtons ()
    {
        EditionIsEnable = false;
        ClearIsEnable = false;
        SaveIsEnable = false;
        PrintIsEnable = false;
    }

    internal void EnableButtons ()
    {
        EditionIsEnable = ProcessableBadges.Count > 0;
        ClearIsEnable = true;
        SaveIsEnable = true;
        PrintIsEnable = true;
    }

    internal void ResetIncorrects ()
    {
        IncorrectBadgeCount = 0;

        foreach ( BadgeViewModel badge   in   ProcessableBadges )
        {
            if ( ! badge.IsCorrect )
            {
                IncorrectBadgeCount++;
            }
        }
    }
}
