using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ContentAssembler;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Helpers;
using ReactiveUI;
using System.DirectoryServices.ActiveDirectory;

namespace Lister.ViewModels
{
    public partial class SceneViewModel : ViewModelBase
    {
        public readonly double _scalabilityCoefficient = 1.25;
        private ConverterToPdf _converter;
        private IUniformDocumentAssembler _docAssembler;
        private double _documentScale;
        private double _zoomDegree = 100;
        private List <PageViewModel> _allPages;
        private List <PageViewModel> _printablePages;
        
        private PageViewModel _lastPage;
        private PageViewModel _printableLastPage;
        private SceneUserControl _view;
        private Person _chosenPerson;
        private string procentSymbol = "%";

        private PageViewModel vPage;
        internal PageViewModel VisiblePage
        {
            get { return vPage; }
            private set
            {
                this.RaiseAndSetIfChanged (ref vPage, value, nameof (VisiblePage));
            }
        }

        private int pC;
        internal int PageCount
        {
            get { return pC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pC, value, nameof (PageCount));
            }
        }

        private int vpN;
        internal int VisiblePageNumber
        {
            get { return vpN; }
            private set
            {
                this.RaiseAndSetIfChanged (ref vpN, value, nameof (VisiblePageNumber));
            }
        }

        private int bC;
        internal int BadgeCount
        {
            get { return bC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bC, value, nameof (BadgeCount));
            }
        }

        private int ibC;
        internal int IncorrectBadgeCount
        {
            get { return ibC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ibC, value, nameof (IncorrectBadgeCount));
            }
        }

        internal List <BadgeViewModel> IncorrectBadges { get; private set; }

        private string zoomDV;
        internal string ZoomDegreeInView
        {
            get { return zoomDV; }
            private set
            {
                this.RaiseAndSetIfChanged (ref zoomDV, value, nameof (ZoomDegreeInView));
            }
        }

        private Thickness bM;
        internal Thickness BorderMargin
        {
            get { return bM; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bM, value, nameof (BorderMargin));
            }
        }

        private double cT;
        internal double CanvasTop
        {
            get { return cT; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (CanvasTop));
            }
        }

        private bool eE;
        internal bool EditionMustEnable
        {
            get { return eE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eE, value, nameof (EditionMustEnable));
            }
        }



        private bool cE;
        internal bool ClearIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref cE, value, nameof (ClearIsEnable));
            }
            get
            {
                return cE;
            }
        }

        private bool sE;
        internal bool SaveIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref sE, value, nameof (SaveIsEnable));
            }
            get
            {
                return sE;
            }
        }

        private bool pE;
        internal bool PrintIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PrintIsEnable));
            }
            get
            {
                return pE;
            }
        }

        private PersonChoosingViewModel _personChoosingVM;
        private TemplateChoosingViewModel _templateChoosingVM;


        public SceneViewModel ( IUniformDocumentAssembler docAssembler, PersonChoosingViewModel personChoosingVM ) 
        {
            SetButtonBlock ();
            _converter = new ConverterToPdf ();
            _docAssembler = docAssembler;
            _personChoosingVM = personChoosingVM;
            _documentScale = 1;
            _allPages = new List <PageViewModel> ();
            _printablePages = new List <PageViewModel> ();
            PageViewModel firstPage = new PageViewModel (_documentScale);
            VisiblePage = firstPage;
            _lastPage = VisiblePage;
            _allPages.Add (VisiblePage);
            _printableLastPage = firstPage.GetDimendionalOriginalClone ();
            _printablePages.Add (_printableLastPage);
            VisiblePageNumber = 1;
            ZoomDegreeInView = _zoomDegree.ToString () + " " + procentSymbol;
            IncorrectBadges = new List<BadgeViewModel> ();
            EditionMustEnable = false;
            PageCount = 1; 
        }


        internal void PassTemplateChoosing ( TemplateChoosingViewModel templateChoosingViewModel )
        {
            _templateChoosingVM = templateChoosingViewModel;
            //_templateChoosingVM.WaitingIsSet += Handle;
        }


        internal void Handle ( )
        {
            BuildBadges (_personChoosingVM.ChosenTemplate.Name);
        }


        internal void PassView ( SceneUserControl view )
        {
            _view = view;
        }


        internal void BuildBadges ( string templateName )
        {
            List <Badge> requiredBadges = _docAssembler.CreateBadgesByModel (templateName);

            if ( requiredBadges.Count > 0 )
            {
                List <BadgeViewModel> allBadges = new ();

                for ( int index = 0;   index < requiredBadges.Count;   index++ )
                {
                    Person person = requiredBadges [index].Person;

                    if ( _personChoosingVM.IsEmpty (person) )
                    {
                        continue;
                    }

                    BadgeViewModel beingProcessedBadgeVM = new BadgeViewModel (requiredBadges [index], BadgeCount);
                    allBadges.Add (beingProcessedBadgeVM);

                    if ( ! beingProcessedBadgeVM.IsCorrect )
                    {
                        IncorrectBadges.Add (beingProcessedBadgeVM);
                        IncorrectBadgeCount++;
                    }

                    BadgeCount++;
                }

                List <PageViewModel> newPages = PageViewModel.PlaceIntoPages (allBadges, _documentScale, _lastPage);
                bool placingStartedOnLastPage = ( _lastPage != null )   &&   _lastPage.Equals (newPages [0]);

                if ( placingStartedOnLastPage )
                {
                    VisiblePageNumber = _allPages.Count;

                    // page number 0 corresponds last page of previous building,  PageViewModel.PlaceIntoPages() method
                    // added badges on it

                    PageViewModel first = newPages [0];

                    Dispatcher.UIThread.InvokeAsync
                    (() =>
                    {
                        _printablePages [_printablePages.Count - 1] = first.GetDimendionalOriginalClone ();
                    });

                    newPages.RemoveAt (0);
                }

                _allPages.AddRange (newPages);

                foreach ( PageViewModel page   in   newPages )
                {
                    Dispatcher.UIThread.InvokeAsync
                    (() =>
                    {
                        _printablePages.Add (page.GetDimendionalOriginalClone ());
                    });
                }

                _lastPage = _allPages.Last ();
                _printableLastPage = _printablePages.Last ();

                VisiblePage = _allPages [VisiblePageNumber - 1];
                PageCount = _allPages.Count;
                VisiblePage.Show ();
            }
        }


        internal void BuildSingleBadge ( string templateName )
        {
            Person goalPerson = _personChoosingVM.ChosenPerson;
            Badge requiredBadge = _docAssembler.CreateSingleBadgeByModel (templateName, goalPerson);
            BadgeViewModel goalVMBadge = new BadgeViewModel (requiredBadge, BadgeCount);

            if ( ! goalVMBadge.IsCorrect )
            {
                IncorrectBadges.Add (goalVMBadge);
                IncorrectBadgeCount++;
            }

            bool placingStartedAfterEntireListAddition = ! _lastPage.Equals (VisiblePage);

            if ( placingStartedAfterEntireListAddition )
            {
                //VisiblePage.Hide ();
                VisiblePage = _lastPage;
                VisiblePage.Show ();
            }

            PageViewModel possibleNewVisiblePage = _lastPage.AddBadge (goalVMBadge);
            bool timeToIncrementVisiblePageNumber = ! possibleNewVisiblePage.Equals (_lastPage);

            if ( timeToIncrementVisiblePageNumber )
            {
                //VisiblePage.Hide ();
                VisiblePage = possibleNewVisiblePage;
                _lastPage = VisiblePage;
                _printableLastPage = possibleNewVisiblePage.GetDimendionalOriginalClone ();
                _allPages.Add (possibleNewVisiblePage);
                _printablePages.Add (_printableLastPage);
                VisiblePage.Show ();
            }
            else 
            {
                _printablePages.RemoveAt (_printablePages.Count - 1);
                _printableLastPage = _allPages.Last ().GetDimendionalOriginalClone ();
                _printablePages.Add (_printableLastPage);
            }

            BadgeCount++;
            VisiblePageNumber = _allPages.Count;
            PageCount = _allPages.Count;
            goalVMBadge.Show ();
        }


        internal void ClearAllPages ()
        {
            if ( _allPages.Count <= 0 )
            {
                return;
            }

            _allPages.Clear ();
            _printablePages = new List <PageViewModel> ();
            VisiblePage = new PageViewModel (_documentScale);
            _lastPage = VisiblePage;
            _allPages.Add (_lastPage);

            Dispatcher.UIThread.InvokeAsync
            (() =>
            {
                _printablePages.Add (_lastPage.GetDimendionalOriginalClone());
            });

            VisiblePageNumber = 1;
            PageCount = 1;
            BadgeCount = 0;
            IncorrectBadgeCount = 0;

            EditionMustEnable = false;
            IncorrectBadges = new List <BadgeViewModel> ();

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


        internal void ZoomOn ( )
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


        internal void ZoomOut ( )
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
                //VisiblePage.Hide ();
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
                //VisiblePage.Hide ();
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
                //VisiblePage.Hide ();
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
                //VisiblePage.Hide ();
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
                //VisiblePage.Hide ();
                VisiblePageNumber = pageNumber;
                VisiblePage = _allPages [VisiblePageNumber - 1];
                VisiblePage.Show ();
            }

            return VisiblePageNumber;
        }


        internal List <PageViewModel> GetAllPages ()
        {
            return _printablePages;
            //return _allPages;
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

            int counter = 0;

            foreach ( BadgeViewModel badge   in   IncorrectBadges )
            {
                badge.Id = counter;
                counter++;
            }

            IncorrectBadgeCount = IncorrectBadges. Count;
        }


        internal void EditIncorrectBadges ()
        {
            _view.EditIncorrectBadges (IncorrectBadges, _allPages [0]);
        }


        //internal void SetEdition ()
        //{
        //    if ( IncorrectBadges.Count > 0 )
        //    {
        //        EditionMustEnable = true;
        //    }
        //    else
        //    {
        //        EditionMustEnable = false;
        //    }
        //}


        internal void ClearBadges ()
        {
            ClearAllPages ();
            DisableButtons ();

            PageNavigationZoomerViewModel zoomNavigationVM = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();

            zoomNavigationVM.DisableButtons ();
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
            EditionMustEnable = true;
            ClearIsEnable = true;
            SaveIsEnable = true;
            PrintIsEnable = true;
        }
    }
}
