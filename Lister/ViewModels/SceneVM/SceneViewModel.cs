using Avalonia;
using Avalonia.Threading;
using ContentAssembler;
using Lister.Extentions;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Lister.ViewModels
{
    public partial class SceneViewModel : ViewModelBase
    {
        private readonly int _badgeCountLimit;
        private readonly double _scalabilityCoefficient = 1.25;

        public static bool EntireListBuildingIsChosen { get; private set; }

        private IUniformDocumentAssembler _docAssembler;
        private double _documentScale;

        private List <PageViewModel> _allPages;
        internal List <PageViewModel> AllPages 
        {
            get { return _allPages; }
            private set { _allPages = value; }
        }

        private List <PageViewModel> _printablePages;
        
        private PageViewModel _lastPage;
        private PageViewModel _lastPrintablePage;
        private int _visiblePageNumStorage;

        private string _templateForBuilding;
        private Person _chosenPerson;

        private bool _buildingIsLocked;



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
                if ( value > 0 ) 
                {
                    _visiblePageNumStorage = value;
                }

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
        internal List <BadgeViewModel> PrintableBadges { get; private set; }

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

        private BadgesBuildingViewModel _templateChoosingVM;


        public SceneViewModel ( int badgeCountLimit, string extentionToolTip, string shrinkingToolTip
                                                                            , string fileIsOpenMessage ) 
        {
            _badgeCountLimit = badgeCountLimit;
            _extentionToolTip = extentionToolTip;
            _shrinkingToolTip = shrinkingToolTip;
            _fileIsOpenMessage = fileIsOpenMessage;

            SetButtonBlock ();

            _docAssembler = App.services.GetService<IUniformDocumentAssembler> ();
            _documentScale = 1;
            AllPages = new List <PageViewModel> ();
            _printablePages = new List <PageViewModel> ();
            PageViewModel firstPage = new PageViewModel (_documentScale);
            VisiblePage = firstPage;
            _lastPage = VisiblePage;
            AllPages.Add (VisiblePage);

            _lastPrintablePage = new PageViewModel (_documentScale);
            _printablePages.Add (_lastPrintablePage);
            VisiblePageNumber = 1;
            ProcessableBadges = new List <BadgeViewModel> ();
            PrintableBadges = new List <BadgeViewModel> ();
            EditionMustEnable = false;
            PageCount = 1;
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
                EnableButtons ();
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

                    EnableButtons ();

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
                        (() =>
                        {
                            EntireListBuildingIsChosen = false;
                            BuildingIsOccured = false;
                        });
                    }
                }
            );

            task.Start ();
        }


        private bool BuildAllBadges ( string templateName )
        {
            List <Badge> requiredBadges = _docAssembler.CreateBadgesByModel (templateName);

            if ( (BadgeCount + requiredBadges.Count) >= _badgeCountLimit ) 
            {
                return false;
            }

            if ( requiredBadges.Count > 0 )
            {
                List <BadgeViewModel> allBadges = new ();
                List <BadgeViewModel> allPrintableBadges = new ();

                for ( int index = 0;   index < requiredBadges.Count;   index++ )
                {
                    Person person = requiredBadges [index].Person;

                    if ( person.IsEmpty () )
                    {
                        continue;
                    }

                    BadgeViewModel badge = new BadgeViewModel (requiredBadges [index], BadgeCount);
                    allBadges.Add (badge);

                    BadgeViewModel printableBadge = badge.Clone();
                    allPrintableBadges.Add (printableBadge);

                    ProcessableBadges.Add (badge);

                    if ( ! badge.IsCorrect )
                    {
                        IncorrectBadgeCount++;
                    }

                    BadgeCount++;
                }

                List <PageViewModel> newPages = PageViewModel.PlaceIntoPages (allBadges, _documentScale, _lastPage);
                
                List <PageViewModel> printablePages = 
                                       PageViewModel.PlaceIntoPages (allPrintableBadges, _documentScale, _lastPrintablePage);
                
                bool placingStartedOnLastPage = ( _lastPage != null )   &&   _lastPage.Equals (newPages [0]);

                if ( placingStartedOnLastPage )
                {
                    Dispatcher.UIThread.Invoke (() => { VisiblePageNumber = AllPages.Count; });
                    newPages.RemoveAt (0);
                    printablePages.RemoveAt (0);
                }

                AllPages.AddRange (newPages);
                _printablePages.AddRange (printablePages);
                _lastPage = AllPages.Last ();
                _lastPrintablePage = _printablePages.Last ();

                PrintableBadges.AddRange (allPrintableBadges);

                Dispatcher.UIThread.Invoke 
                (() => 
                {
                    VisiblePage = AllPages [VisiblePageNumber - 1];
                    PageCount = AllPages.Count;
                    VisiblePage.Show ();
                });
            }

            return true;
        }


        private void BuildSingleBadge ()
        {
            _buildingIsLocked = true;
            BuildingIsOccured = BuildSingleBadge (_templateForBuilding);
            _buildingIsLocked = false;
        }


        private bool BuildSingleBadge ( string templateName )
        {
            if ( ( BadgeCount + 1 ) >= _badgeCountLimit )
            {
                return false;
            }

            Badge requiredBadge = _docAssembler.CreateSingleBadgeByModel (templateName, _chosenPerson);
            BadgeViewModel goalVMBadge = new BadgeViewModel (requiredBadge, BadgeCount);
            BadgeViewModel printableBadge = goalVMBadge.Clone();

            ProcessableBadges.Add (goalVMBadge);

            if ( ! goalVMBadge.IsCorrect )
            {
                IncorrectBadgeCount++;
            }

            PrintableBadges.Add (printableBadge);
            bool placingStartedAfterEntireListAddition = ! _lastPage.Equals (VisiblePage);

            if ( placingStartedAfterEntireListAddition )
            {
                VisiblePage.Hide ();
                VisiblePage = _lastPage;
                VisiblePage.Show ();
            }

            PageViewModel possibleNewVisiblePage = _lastPage.AddBadge (goalVMBadge);
            PageViewModel possibleNewLastPrintablePage = _lastPrintablePage.AddBadge (printableBadge);

            bool timeToIncrementVisiblePageNumber = ! possibleNewVisiblePage.Equals (_lastPage);

            if ( timeToIncrementVisiblePageNumber )
            {
                VisiblePage.Hide ();
                VisiblePage = possibleNewVisiblePage;
                _lastPage = VisiblePage;
                _lastPrintablePage = possibleNewLastPrintablePage;
                AllPages.Add (possibleNewVisiblePage);
                _printablePages.Add (_lastPrintablePage);
                VisiblePage.Show ();
            }

            BadgeCount++;
            VisiblePageNumber = AllPages.Count;
            PageCount = AllPages.Count;
            goalVMBadge.Show ();

            return true;
        }


        internal void ClearAllPages ()
        {
            if ( AllPages.Count <= 0 )
            {
                return;
            }

            AllPages = new List <PageViewModel> ();
            VisiblePage = new PageViewModel (_documentScale);
            _lastPage = VisiblePage;
            AllPages.Add (_lastPage);

            _printablePages = new List <PageViewModel> ();
            PrintableBadges = new List <BadgeViewModel> ();
            _lastPrintablePage = new PageViewModel (_documentScale);
            _printablePages.Add (_lastPrintablePage);

            VisiblePageNumber = 1;
            PageCount = 1;
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
            return _printablePages;
        }


        internal int GetPrintablePagesCount ()
        {
            return _printablePages.Count;
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
}
