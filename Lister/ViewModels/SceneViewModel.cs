using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using ContentAssembler;
using Lister.Views;
using QuestPDF.Helpers;
using ReactiveUI;

namespace Lister.ViewModels
{
    public class SceneViewModel : ViewModelBase
    {
        private IUniformDocumentAssembler _docAssembler;
        
        private double _documentScale;
        private double _scalabilityCoefficient;
        private double _zoomDegree;
        private List <PageViewModel> _allPages;
        private PageViewModel _lastPage;
        private Person _chosenPerson;
        private string procentSymbol;

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

        internal List <BadgeViewModel> IncorrectBadges { get; private set; }

        private string zoomDV;
        internal string ZoomDegreeInView
        {
            get { return zoomDV; }
            set
            {
                this.RaiseAndSetIfChanged (ref zoomDV, value, nameof (ZoomDegreeInView));
            }
        }

        private Thickness bM;
        internal Thickness BorderMargin
        {
            get { return bM; }
            set
            {
                this.RaiseAndSetIfChanged (ref bM, value, nameof (BorderMargin));
            }
        }

        private double cT;
        internal double CanvasTop
        {
            get { return cT; }
            set
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (CanvasTop));
            }
        }

        private bool eE;
        internal bool EditionMustEnable
        {
            get { return eE; }
            set
            {
                this.RaiseAndSetIfChanged (ref eE, value, nameof (EditionMustEnable));
            }
        }

        private PersonChoosingViewModel _personChoosingVM;


        public SceneViewModel ( IUniformDocumentAssembler docAssembler, PersonChoosingViewModel personChoosingVM ) 
        {
            _docAssembler = docAssembler;
            _personChoosingVM = personChoosingVM;
            _documentScale = 1;
            _allPages = new List <PageViewModel> ();
            VisiblePage = new PageViewModel (_documentScale);
            _lastPage = VisiblePage;
            _allPages.Add (VisiblePage);
            _scalabilityCoefficient = 1.25;
            VisiblePageNumber = 1;
            procentSymbol = "%";
            _zoomDegree = 100;
            ZoomDegreeInView = _zoomDegree.ToString () + " " + procentSymbol;
            IncorrectBadges = new List<BadgeViewModel> ();
            EditionMustEnable = false;
        }


        internal void BuildBadges ( string templateName )
        {
            List<Badge> requiredBadges = _docAssembler.CreateBadgesByModel (templateName);

            if ( requiredBadges.Count > 0 )
            {
                List<BadgeViewModel> allBadges = new ();

                for ( int index = 0;   index < requiredBadges.Count;   index++ )
                {
                    BadgeViewModel beingProcessedBadgeVM = new BadgeViewModel (requiredBadges [index]);
                    allBadges.Add (beingProcessedBadgeVM);

                    if ( ! beingProcessedBadgeVM.IsCorrect )
                    {
                        IncorrectBadges.Add (beingProcessedBadgeVM);
                        EditionMustEnable = true;
                    }
                }

                List <PageViewModel> newPages = PageViewModel.PlaceIntoPages (allBadges, _documentScale, _lastPage);
                bool placingStartedOnLastPage = ( _lastPage != null )   &&   _lastPage.Equals (newPages [0]);

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


        internal void BuildSingleBadge ( string templateName )
        {
            Person goalPerson = _personChoosingVM.ChosenPerson;
            Badge requiredBadge = _docAssembler.CreateSingleBadgeByModel (templateName, goalPerson);
            BadgeViewModel goalVMBadge = new BadgeViewModel (requiredBadge);

            if ( ! goalVMBadge.IsCorrect )
            {
                IncorrectBadges.Add (goalVMBadge);
                EditionMustEnable = true;
            }

            bool placingStartedAfterEntireListAddition = !_lastPage.Equals (VisiblePage);

            if ( placingStartedAfterEntireListAddition )
            {
                VisiblePage.Hide ();
                VisiblePage = _lastPage;
                VisiblePage.Show ();
            }

            PageViewModel possibleNewVisiblePage = _lastPage.AddBadge (goalVMBadge, true);
            bool timeToIncrementVisiblePageNumber = ! possibleNewVisiblePage.Equals (_lastPage);

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
                //for ( int pageCounter = 0;   pageCounter < _allPages.Count;   pageCounter++ )
                //{
                //    _allPages [pageCounter].Clear ();
                //}

                _allPages.Clear ();
                VisiblePage = new PageViewModel (_documentScale);
                _lastPage = VisiblePage;
                _allPages.Add (_lastPage);
                VisiblePageNumber = 1;
            }
        }


        internal void ZoomOn ( short step )
        {
            _documentScale *= _scalabilityCoefficient;

            if ( VisiblePage != null )
            {
                for ( int pageCounter = 0;   pageCounter < _allPages.Count;   pageCounter++ )
                {
                    _allPages [pageCounter].ZoomOn (_scalabilityCoefficient);
                }

                _zoomDegree *= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;

                CanvasTop *= _scalabilityCoefficient;
                double marginLeft = _scalabilityCoefficient * BorderMargin. Left;
                BorderMargin = new Thickness (marginLeft, 0, 0, 0);
            }
        }


        internal void ZoomOut ( short step )
        {
            _documentScale /= _scalabilityCoefficient;

            if ( VisiblePage != null )
            {
                for ( int pageCounter = 0;   pageCounter < _allPages.Count;   pageCounter++ )
                {
                    _allPages [pageCounter].ZoomOut (_scalabilityCoefficient);
                }

                _zoomDegree /= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + procentSymbol;

                CanvasTop /= _scalabilityCoefficient;
                double marginLeft = BorderMargin. Left / _scalabilityCoefficient;
                BorderMargin = new Thickness (marginLeft, 0, 0, 0);
            }
        }


        internal int VisualiseNextPage ()
        {
            if ( VisiblePageNumber < _allPages.Count )
            {
                VisiblePage.Hide ();
                VisiblePageNumber++;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualisePreviousPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePage.Hide ();
                VisiblePageNumber--;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualiseLastPage ()
        {
            if ( VisiblePageNumber < _allPages.Count )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = _allPages.Count;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualiseFirstPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = 1;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal int VisualisePageWithNumber ( int pageNumber )
        {
            bool visiblePageExists = ( VisiblePage != null );
            bool notTheSamePage = ( VisiblePageNumber != pageNumber );
            bool inRange = pageNumber <= _allPages.Count;

            if ( visiblePageExists   &&   notTheSamePage   &&   inRange )
            {
                VisiblePage.Hide ();
                VisiblePageNumber = pageNumber;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal List <PageViewModel> GetAllPages ()
        {
            return _allPages;
        }


        internal int GetPageCount ()
        {
            return _allPages.Count;
        }


        internal void ResetIncorrects ()
        {
            List <BadgeViewModel> corrects = new List <BadgeViewModel> ();

            foreach ( BadgeViewModel badge   in   IncorrectBadges ) 
            {
                if ( badge.IsCorrect )
                {
                    corrects.Add (badge);
                }
            }

            foreach ( BadgeViewModel correctBadge   in   corrects )
            {
                IncorrectBadges.Remove (correctBadge);
            }
        }

    }
}
