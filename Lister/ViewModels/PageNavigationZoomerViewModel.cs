using ContentAssembler;
using DocumentFormat.OpenXml.Math;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class PageNavigationZoomerViewModel : ViewModelBase
    {
        private readonly double _scalabilityCoefficient = 1.25;
        private readonly string _procentSymbol;
        private readonly short _maxDepth;
        private readonly short _minDepth;

        private short _scalabilityDepth = 0;

        private int _visiblePageNumber;
        internal int VisiblePageNumber
        {
            get { return _visiblePageNumber; }
            set
            {
                this.RaiseAndSetIfChanged (ref _visiblePageNumber, value, nameof (VisiblePageNumber));
            }
        }

        private bool fE;
        internal bool FirstIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref fE, value, nameof (FirstIsEnable));
            }
            get
            {
                return fE;
            }
        }

        private bool pE;
        internal bool PreviousIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PreviousIsEnable));
            }
            get
            {
                return pE;
            }
        }

        private bool nE;
        internal bool NextIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref nE, value, nameof (NextIsEnable));
            }
            get
            {
                return nE;
            }
        }

        private bool lE;
        internal bool LastIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref lE, value, nameof (LastIsEnable));
            }
            get
            {
                return lE;
            }
        }

        private bool zonE;
        internal bool ZoomOnIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref zonE, value, nameof (ZoomOnIsEnable));
            }
            get
            {
                return zonE;
            }
        }

        private bool zoutE;
        internal bool ZoomOutIsEnable
        {
            set
            {
                this.RaiseAndSetIfChanged (ref zoutE, value, nameof (ZoomOutIsEnable));
            }
            get
            {
                return zoutE;
            }
        }

        private string _zoomDegreeInView;
        internal string ZoomDegreeInView
        {
            get { return _zoomDegreeInView; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _zoomDegreeInView, value, nameof (ZoomDegreeInView));
            }
        }

        private double _zoomDegree;
        internal double ZoomDegree
        {
            get { return _zoomDegree; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _zoomDegree, value, nameof (ZoomDegree));
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


        public PageNavigationZoomerViewModel ( string procentSymbol, short maxDepth, short minDepth )
        {
            _procentSymbol = procentSymbol;
            _maxDepth = maxDepth;
            _minDepth = minDepth;

            ToZeroState ();
        }


        internal void ShowNextPage ()
        {
            if ( VisiblePageNumber < PageCount )
            {
                VisiblePageNumber++;
            }

            SetEnablePageNavigation ();
        }


        internal void ShowPreviousPage ()
        {
            if ( VisiblePageNumber > 1 )
            {
                VisiblePageNumber--;
            }

            SetEnablePageNavigation ();
        }


        internal void ShowLastPage ()
        {
            if ( VisiblePageNumber < PageCount )
            {
                VisiblePageNumber = PageCount;
            }

            SetEnablePageNavigation ();
        }


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

            if ( (pageNumber < 1)   ||   (pageNumber > PageCount) ) 
            {
                VisiblePageNumber = num;
                return;
            }

            VisiblePageNumber = pageNumber;
            SetEnablePageNavigation ();
        }


        internal void ZoomOn ()
        {
            if ( _scalabilityDepth < _maxDepth )
            {
                ZoomDegree *= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + _procentSymbol;
                _scalabilityDepth++;
            }

            if ( _scalabilityDepth == _maxDepth )
            {
                ZoomOnIsEnable = false;
            }

            if ( ! ZoomOutIsEnable )
            {
                ZoomOutIsEnable = true;
            }
        }


        internal void ZoomOut ()
        {
            if ( _scalabilityDepth > _minDepth )
            {
                ZoomDegree /= _scalabilityCoefficient;
                short zDegree = ( short ) _zoomDegree;
                ZoomDegreeInView = zDegree.ToString () + " " + _procentSymbol;
                _scalabilityDepth--;
            }

            if ( _scalabilityDepth == _minDepth )
            {
                ZoomOutIsEnable = false;
            }

            if ( ! ZoomOnIsEnable )
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
            if ( ( VisiblePageNumber > 1 ) && ( VisiblePageNumber == PageCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( VisiblePageNumber > 1 ) && ( VisiblePageNumber < PageCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = true;
                LastIsEnable = true;
            }
            else if ( ( VisiblePageNumber == 1 ) && ( PageCount == 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( VisiblePageNumber == 1 ) && ( PageCount > 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = true;
                LastIsEnable = true;
            }
        }


        internal void ToZeroState ()
        {
            DisableButtons ();

            _scalabilityDepth = 0;
            ZoomDegree = 100;
            ZoomDegreeInView = ZoomDegree.ToString () + " " + _procentSymbol;
            VisiblePageNumber = 1;
            PageCount = 0;
        }


        private void DisableButtons ()
        {
            ZoomOnIsEnable = false;
            ZoomOutIsEnable = false;
            FirstIsEnable = false;
            PreviousIsEnable = false;
            NextIsEnable = false;
            LastIsEnable = false;
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
}
