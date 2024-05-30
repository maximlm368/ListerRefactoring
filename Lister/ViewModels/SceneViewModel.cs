using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using QuestPDF.Helpers;
using ReactiveUI;

namespace Lister.ViewModels
{
    public class SceneViewModel : ViewModelBase
    {
        private IUniformDocumentAssembler _docAssembler;
        
        private double _documentScale;
        private double _scalabilityCoefficient;
        private List <PageViewModel> _allPages;
        private PageViewModel _lastPage;
        private Person _chosenPerson;

        private PageViewModel vPage;
        internal PageViewModel VisiblePage
        {
            get { return vPage; }
            set
            {
                this.RaiseAndSetIfChanged (ref vPage, value, nameof (VisiblePage));
            }
        }

        private int vpN;
        internal int VisiblePageNumber
        {
            get { return vpN; }
            set
            {
                this.RaiseAndSetIfChanged (ref vpN, value, nameof (VisiblePageNumber));
            }
        }

        internal List<BadgeViewModel> IncorrectBadges { get; private set; }



        public SceneViewModel ( IUniformDocumentAssembler docAssembler, ContentAssembler.Size pageSize ) 
        {
            _docAssembler = docAssembler;
            _documentScale = 1;
            _scalabilityCoefficient = 1.25;
        }


        internal void BuildBadges ( string fileName )
        {
            string pathInAvalonia = "avares://Lister/Assets";

            string badgeModelName = pathInAvalonia + "/" + fileName;
            List<Badge> requiredBadges = _docAssembler.CreateBadgesByModel (badgeModelName);

            if ( requiredBadges.Count > 0 )
            {
                List<BadgeViewModel> allBadges = new ();

                for ( int badgeCounter = 0; badgeCounter < requiredBadges.Count; badgeCounter++ )
                {
                    BadgeViewModel beingProcessedBadgeVM = new BadgeViewModel (requiredBadges [badgeCounter]);
                    allBadges.Add (beingProcessedBadgeVM);

                    if ( !beingProcessedBadgeVM.IsCorrect )
                    {
                        IncorrectBadges.Add (beingProcessedBadgeVM);
                    }
                }

                List<PageViewModel> newPages = PageViewModel.PlaceIntoPages (allBadges, pageSize, _documentScale, _lastPage);
                bool placingStartedOnLastPage = ( _lastPage != null ) && _lastPage.Equals (newPages [0]);

                if ( placingStartedOnLastPage )
                {
                    VisiblePageNumber = _allPages.Count;

                    // page number 0 corresponds last page of previous building,  VMPage.PlaceIntoPages() method
                    // added badges on it
                    newPages.RemoveAt (0);
                }

                _allPages.AddRange (newPages);
                _lastPage = _allPages.Last ();
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }
        }


        internal void BuildSingleBadge ( string fileName )
        {
            string pathInAvalonia = "avares://Lister/Assets";
            string badgeModelName = pathInAvalonia + "/" + fileName;
            Person goalPerson = ChosenPerson;
            Badge requiredBadge = _docAssembler.CreateSingleBadgeByModel (badgeModelName, goalPerson);
            BadgeViewModel goalVMBadge = new BadgeViewModel (requiredBadge);

            if ( !goalVMBadge.IsCorrect )
            {
                IncorrectBadges.Add (goalVMBadge);
            }

            bool itIsFirstBadgeBuildingInCurrentAppRun = ( VisiblePage == null );

            if ( itIsFirstBadgeBuildingInCurrentAppRun )
            {
                BadgeViewModel badgeExample = goalVMBadge.Clone ();
                VisiblePage = new PageViewModel (pageSize, badgeExample, _documentScale);
                _lastPage = VisiblePage;
                _allPages.Add (VisiblePage);
            }

            bool placingStartedAfterEntireListAddition = !_lastPage.Equals (VisiblePage);

            if ( placingStartedAfterEntireListAddition )
            {
                VisiblePage.Hide ();
                VisiblePage = _lastPage;
                VisiblePage.Show ();
            }

            PageViewModel possibleNewVisiblePage = _lastPage.AddBadge (goalVMBadge, true);
            bool timeToIncrementVisiblePageNumber = !possibleNewVisiblePage.Equals (_lastPage);

            if ( timeToIncrementVisiblePageNumber )
            {
                VisiblePage.Hide ();
                VisiblePage = possibleNewVisiblePage;
                _lastPage = VisiblePage;
                _allPages.Add (possibleNewVisiblePage);
                VisiblePage.Show ();
            }

            VisiblePageNumber = _allPages.Count;
            goalVMBadge.Show ();
        }


        internal void ClearAllPages ()
        {
            if ( _allPages.Count > 0 )
            {
                for ( int pageCounter = 0; pageCounter < _allPages.Count; pageCounter++ )
                {
                    _allPages [pageCounter].Clear ();
                }

                VisiblePage = _allPages [0];
                _allPages = new List <PageViewModel> ();
                _lastPage = VisiblePage;
                _allPages.Add (_lastPage);
                VisiblePageNumber = 1;
            }
        }


        internal void ZoomOnDocument ( short step )
        {
            _documentScale *= _scalabilityCoefficient;

            if ( VisiblePage != null )
            {
                for ( int pageCounter = 0; pageCounter < _allPages.Count; pageCounter++ )
                {
                    _allPages [pageCounter].ZoomOn (_scalabilityCoefficient);
                }

                VisiblePage.ZoomOnExampleBadge (_scalabilityCoefficient);
                _zoomDegree *= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
            }
        }


        internal void ZoomOutDocument ( short step )
        {
            _documentScale /= _scalabilityCoefficient;

            if ( VisiblePage != null )
            {
                for ( int pageCounter = 0; pageCounter < _allPages.Count; pageCounter++ )
                {
                    _allPages [pageCounter].ZoomOut (_scalabilityCoefficient);
                }

                VisiblePage.ZoomOutExampleBadge (_scalabilityCoefficient);
                _zoomDegree /= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
            }
        }


        internal void VisualiseNextPage ()
        {
            if ( VisiblePageNumber < _allPages.Count )
            {
                VisiblePage.Hide ();
                VisiblePageNumber++;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }
        }


        internal void VisualisePreviousPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePage.Hide ();
                VisiblePageNumber--;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }
        }


        internal void VisualiseLastPage ()
        {
            if ( VisiblePageNumber < _allPages.Count )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = _allPages.Count;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }
        }


        internal void VisualiseFirstPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = 1;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }
        }


        internal int VisualisePageWithNumber ( int pageNumber )
        {
            int result = VisiblePageNumber;
            bool notTheSamePage = VisiblePageNumber != pageNumber;
            bool inRange = pageNumber <= _allPages.Count;

            if ( notTheSamePage && inRange )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = pageNumber;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
                result = pageNumber;
            }

            return result;
        }


        internal List <BadgeViewModel> GetAllBadges ()
        {
            List<BadgeViewModel> allBadges = new ();

            for ( int pageCounter = 0;   pageCounter < _allPages.Count;   pageCounter++ )
            {
                int badgePairCounter = 0;

                while ( true )
                {
                    try
                    {
                        allBadges.Add (_allPages [pageCounter].EvenBadges [badgePairCounter]);
                        allBadges.Add (_allPages [pageCounter].OddBadges [badgePairCounter]);
                        badgePairCounter++;
                    }
                    catch ( ArgumentOutOfRangeException e )
                    {
                        break;
                    }
                }
            }

            return allBadges;
        }

    }
}
