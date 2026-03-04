using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using Lister.Core.Entities;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;

public partial class SceneViewModel : ObservableObject
{
    internal static bool EntireListBuildingIsChosen { get; private set; }

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

    [ObservableProperty]
    private double _canvasTop;

    private bool _isBuildSucceeded;
    internal bool IsBuildSucceeded
    {
        get => _isBuildSucceeded;

        private set
        {
            _isBuildSucceeded = value;
            OnPropertyChanged ();
        }
    }

    private bool _isPagesNotEmpty;
    internal bool IsPagesNotEmpty
    {
        get => _isPagesNotEmpty;

        private set
        {
            _isPagesNotEmpty = value;
            OnPropertyChanged ();
        }
    }

    internal List<BadgeViewModel> ProcessableBadges { get; private set; }

    public SceneViewModel ( int badgeCountLimit, DocumentProcessor docBuilder )
    {
        _badgeCountLimit = badgeCountLimit;
        _model = docBuilder;
        _model.ComplatedPage += HandlePageCompleted;
        _documentScale = 1;

        AllPages = [];
        VisiblePage = new PageViewModel ( new Page (), _documentScale );
        _lastPage = VisiblePage;
        AllPages.Add ( VisiblePage );
        VisiblePageNumber = 1;
        ProcessableBadges = [];
        PageCount = 0;
    }

    private void HandlePageCompleted ( Page complated, bool lastIsReplacable )
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

    internal int GetLimit ()
    {
        return _badgeCountLimit;
    }

    internal void Build ( string templateName, Person? person )
    {
        if ( string.IsNullOrWhiteSpace ( templateName ) )
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

    internal void BuildAllBadges ()
    {
        if ( _buildingIsLocked )
        {
            return;
        }

        _buildingIsLocked = true;

        Task task = new (
            () =>
            {
                bool buildingIsCompleted = TryBuildAllBadges ( _badgeTemplate );

                _buildingIsLocked = false;
                EntireListBuildingIsChosen = false;

                if ( buildingIsCompleted )
                {
                    Dispatcher.UIThread.Invoke (
                        () =>
                        {
                            IsBuildSucceeded = true;
                            IsPagesNotEmpty = true;
                        }
                    );
                }
                else
                {
                    Dispatcher.UIThread.Invoke (
                        () =>
                        {
                            EntireListBuildingIsChosen = false;
                            IsBuildSucceeded = false;
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
        IsBuildSucceeded = TryBuildSingleBadge ( _badgeTemplate );
        IsPagesNotEmpty = IsBuildSucceeded;
        _buildingIsLocked = false;
    }

    private bool TryBuildAllBadges ( string templateName )
    {
        _isEntireBuilding = true;
        _isEntireBuildingStarted = true;
        int futureVisiblePageNumber = AllPages.Count;

        if ( futureVisiblePageNumber == 0 )
        {
            futureVisiblePageNumber = 1;
        }

        List<Page> builtPages = _model.BuildAllPages ( templateName, _badgeCountLimit );

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
                VisiblePage.Show ();
            }
        );

        return true;
    }

    private bool TryBuildSingleBadge ( string templateName )
    {
        _isEntireBuilding = false;

        if ( ( BadgeCount + 1 ) >= _badgeCountLimit )
        {
            return false;
        }

        List<Page> resultPages = _model.BuildBadge ( templateName, _chosenPerson );

        if ( resultPages.Count > AllPages.Count )
        {
            AllPages.Add ( new PageViewModel ( resultPages.Last (), _documentScale ) );
        }
        else
        {
            AllPages [^1] = new PageViewModel ( resultPages.Last (), _documentScale );
        }

        BadgeViewModel added = AllPages [^1].GetBadges ().Last ();
        ProcessableBadges.Add ( added );
        _lastPage = AllPages [^1];

        VisiblePage?.Hide ();
        VisiblePage = _lastPage;
        VisiblePage.Show ();

        BadgeCount++;
        VisiblePageNumber = AllPages.Count;
        PageCount = AllPages.Count;
        IncorrectBadgeCount = _model.IncorrectBadgeCount;
        added.Show ();

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
        VisiblePage = new PageViewModel ( new Page (), _documentScale );
        _lastPage = VisiblePage;
        AllPages.Add ( _lastPage );
        IsPagesNotEmpty = false;
        VisiblePageNumber = 1;
        PageCount = 0;
        BadgeCount = 0;
        IncorrectBadgeCount = 0;
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

    internal void ZoomOn ()
    {
        _documentScale *= _scalabilityCoefficient;

        if ( VisiblePage != null )
        {
            for ( int pageCounter = 0; pageCounter < AllPages.Count; pageCounter++ )
            {
                AllPages [pageCounter].ZoomOn ( _scalabilityCoefficient );
            }

            CanvasTop *= _scalabilityCoefficient;
        }
    }

    internal void ZoomOut ()
    {
        _documentScale /= _scalabilityCoefficient;

        if ( VisiblePage != null )
        {
            for ( int pageCounter = 0; pageCounter < AllPages.Count; pageCounter++ )
            {
                AllPages [pageCounter].ZoomOut ( _scalabilityCoefficient );
            }

            CanvasTop /= _scalabilityCoefficient;
        }
    }

    internal int ShowPageWithNumber ( int pageNumber )
    {
        bool visiblePageExists = VisiblePage != null;
        bool notTheSamePage = VisiblePageNumber != pageNumber;
        bool inRange = pageNumber <= AllPages.Count;

        if ( visiblePageExists && notTheSamePage && inRange )
        {
            VisiblePage?.Hide ();
            VisiblePageNumber = pageNumber;

            if ( AllPages.Count > 0 )
            {
                VisiblePage = AllPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }
        }

        return VisiblePageNumber;
    }

    internal void Refresh ()
    {
        IncorrectBadgeCount = 0;

        foreach ( BadgeViewModel badge in ProcessableBadges )
        {
            if ( !badge.IsCorrect )
            {
                IncorrectBadgeCount++;
            }
        }

        VisiblePage?.Show ();
    }
}
