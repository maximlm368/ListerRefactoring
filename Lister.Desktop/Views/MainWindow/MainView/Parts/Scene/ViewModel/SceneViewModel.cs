using Avalonia.Threading;
using Lister.Core.DocumentProcessor;
using Lister.Core.Models;
using Lister.Desktop.CoreModelReflections;
using Lister.Desktop.CoreModelReflections.BadgeVM;
using ReactiveUI;
using Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;

public partial class SceneViewModel : ReactiveObject
{
    public static bool EntireListBuildingIsChosen { get; private set; }

    private readonly int _badgeCountLimit;
    private readonly double _scalabilityCoefficient = 1.25;

    private DocumentProcessor _model;
    private double _documentScale;
    private PageViewModel _lastPage;
    private string _templateForBuilding;
    private Person _chosenPerson;
    private bool _buildingIsLocked;
    private BadgesBuildingViewModel _templateChoosingVM;
    private bool _isEntireBuilding;
    private bool _isEntireBuildingStarted;

    internal List<PageViewModel> AllPages { get; private set; }

    private PageViewModel _visiblePage;
    internal PageViewModel VisiblePage
    {
        get { return _visiblePage; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _visiblePage, value, nameof (VisiblePage));
        }
    }

    private int _pageCount;
    internal int PageCount
    {
        get { return _pageCount; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _pageCount, value, nameof (PageCount));
        }
    }

    private int _visiblePageNumber;
    internal int VisiblePageNumber
    {
        get { return _visiblePageNumber; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _visiblePageNumber, value, nameof (VisiblePageNumber));
        }
    }

    private int _badgeCount;
    internal int BadgeCount
    {
        get { return _badgeCount; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _badgeCount, value, nameof (BadgeCount));
        }
    }

    private int _incorrectBadgeCount;
    internal int IncorrectBadgeCount
    {
        get { return _incorrectBadgeCount; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _incorrectBadgeCount, value, nameof (IncorrectBadgeCount));
        }
    }

    internal List <BadgeViewModel> ProcessableBadges { get; private set; }

    private string _zoomDegreeInView;
    internal string ZoomDegreeInView
    {
        get { return _zoomDegreeInView; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _zoomDegreeInView, value, nameof (ZoomDegreeInView));
        }
    }

    private Avalonia.Thickness _borderMargin;
    internal Avalonia.Thickness BorderMargin
    {
        get { return _borderMargin; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _borderMargin, value, nameof (BorderMargin));
        }
    }

    private double _canvasTop;
    internal double CanvasTop
    {
        get { return _canvasTop; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _canvasTop, value, nameof (CanvasTop));
        }
    }

    private bool _editionMustEnable;
    internal bool EditionMustEnable
    {
        get { return _editionMustEnable; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _editionMustEnable, value, nameof (EditionMustEnable));
        }
    }

    private bool _clearIsEnable;
    internal bool ClearIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _clearIsEnable, value, nameof (ClearIsEnable));
        }
        get
        {
            return _clearIsEnable;
        }
    }

    private bool _saveIsEnable;
    internal bool SaveIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _saveIsEnable, value, nameof (SaveIsEnable));
        }
        get
        {
            return _saveIsEnable;
        }
    }

    private bool _printIsEnable;
    internal bool PrintIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _printIsEnable, value, nameof (PrintIsEnable));
        }
        get
        {
            return _printIsEnable;
        }
    }

    private bool _editIncorrectsIsSelected;
    internal bool EditIncorrectsIsSelected
    {
        private set
        {
            if ( _editIncorrectsIsSelected == value ) 
            {
                _editIncorrectsIsSelected = !_editIncorrectsIsSelected;
            }

            this.RaiseAndSetIfChanged (ref _editIncorrectsIsSelected, value, nameof (EditIncorrectsIsSelected));
        }
        get
        {
            return _editIncorrectsIsSelected;
        }
    }

    private bool _buildingIsOccured;
    internal bool BuildingIsOccured
    {
        get
        {
            return _buildingIsOccured;
        }

        set
        {
            if ( _buildingIsOccured == value )
            {
                _buildingIsOccured = !_buildingIsOccured;
            }

            this.RaiseAndSetIfChanged (ref _buildingIsOccured, value, nameof (BuildingIsOccured));
        }
    }

    private bool _badgesAreCleared;
    internal bool BadgesAreCleared
    {
        set
        {
            if ( value )
            {
                this.RaiseAndSetIfChanged (ref _badgesAreCleared, value, nameof (BadgesAreCleared));
            }
            else 
            {
                _badgesAreCleared = false;
            }
        }
        get
        {
            return _badgesAreCleared;
        }
    }


    public SceneViewModel ( int badgeCountLimit, string extentionToolTip, string shrinkingToolTip
                          , string fileIsOpenMessage, DocumentProcessor docBuilder ) 
    {
        _badgeCountLimit = badgeCountLimit;
        _extentionToolTip = extentionToolTip;
        _shrinkingToolTip = shrinkingToolTip;
        _fileIsOpenMessage = fileIsOpenMessage;

        SetButtonBlock ();

        _model = docBuilder;
        _model.ComplatedPage += HandlePageCompleting;

        _documentScale = 1;
        AllPages = new List <PageViewModel> ();
        //PageViewModel firstPage = new PageViewModel (_documentScale);
        //VisiblePage = firstPage;

        VisiblePage = new PageViewModel ( new Page (), _documentScale );
        _lastPage = VisiblePage;
        AllPages.Add (VisiblePage);

        VisiblePageNumber = 1;
        ProcessableBadges = new List <BadgeViewModel> ();
        EditionMustEnable = false;
        PageCount = 0;
    }


    internal void HandlePageCompleting ( Page complated, bool lastIsReplacable )
    {
        if ( !_isEntireBuilding ) return;

        if ( ( AllPages.Count > 0 )   &&   _isEntireBuildingStarted )
        {
            PageViewModel last = AllPages.Last ();
            List<BadgeViewModel> lastBadges = last.GetBadges ();
            ProcessableBadges.RemoveRange ( ProcessableBadges.Count - lastBadges.Count, lastBadges.Count );
            AllPages.Remove ( last );
            _isEntireBuildingStarted = false;
        }

        Dispatcher.UIThread.Invoke
        ( () =>
        {
            PageViewModel newPage = new PageViewModel ( complated, _documentScale );
            AllPages.Add ( newPage );
            ProcessableBadges.AddRange ( newPage.GetBadges () );
            BadgeCount += complated.BadgeCount;
            IncorrectBadgeCount = _model.IncorrectBadgeCount;
        } );
    }


    internal int GetLimit ( )
    {
        return _badgeCountLimit;
    }


    internal void PassTemplateChoosing ( BadgesBuildingViewModel templateChoosingViewModel )
    {
        _templateChoosingVM = templateChoosingViewModel;
    }


    internal void Build ( string templateName, Person ? person )
    {
        if ( string.IsNullOrWhiteSpace(templateName) ) 
        {
            return;
        }

        _chosenPerson = person;
        _templateForBuilding = templateName;

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

        Task task = new Task
        (
            () =>
            {
                bool buildingIsCompleted = BuildAllBadges (_templateForBuilding);

                _buildingIsLocked = false;
                EntireListBuildingIsChosen = false;

                if ( buildingIsCompleted )
                {
                    Dispatcher.UIThread.Invoke
                    (() =>
                    {
                        BuildingIsOccured = true;
                    });
                }
                else
                {
                    Dispatcher.UIThread.Invoke
                    ( () =>
                    {
                        EntireListBuildingIsChosen = false;
                        BuildingIsOccured = false;
                    } );
                }
            }
        );

        task.Start ();
    }


    private void BuildSingleBadge ()
    {
        _buildingIsLocked = true;
        BuildingIsOccured = BuildSingleBadge ( _templateForBuilding );
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

        Dispatcher.UIThread.Invoke
        ( () =>
        {
            VisiblePageNumber = futureVisiblePageNumber;
            BadgeCount = ProcessableBadges.Count;
            VisiblePage = AllPages [VisiblePageNumber - 1];
            PageCount = AllPages.Count;
            EnableButtons ();
            VisiblePage.Show ();
        } );

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
            AllPages [AllPages.Count - 1] = new PageViewModel ( resultPages.Last (), _documentScale );
        }

        BadgeViewModel added = AllPages [AllPages.Count - 1].GetBadges ().Last ();
        ProcessableBadges.Add ( added );
        _lastPage = AllPages [AllPages.Count - 1];

        VisiblePage.Hide ();
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

        AllPages = new List <PageViewModel> ();
        //VisiblePage = new PageViewModel (_documentScale);
        VisiblePage = new PageViewModel ( new Page(), _documentScale );
        _lastPage = VisiblePage;
        AllPages.Add (_lastPage);

        VisiblePageNumber = 1;
        PageCount = 0;
        BadgeCount = 0;
        IncorrectBadgeCount = 0;

        EditionMustEnable = false;
        ProcessableBadges = new List <BadgeViewModel> ();

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
            VisiblePage.Hide ();
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
        EditionMustEnable = false;
        ClearIsEnable = false;
        SaveIsEnable = false;
        PrintIsEnable = false;
    }


    internal void EnableButtons ()
    {
        EditionMustEnable = ( ProcessableBadges.Count > 0 );
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
