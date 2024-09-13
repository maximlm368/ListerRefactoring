using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Globalization;
using System.Reflection.Metadata;
using ExtentionsAndAuxiliary;
using Microsoft.VisualBasic;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using System.Linq.Expressions;
using Lister.Views;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Threading;
using System.Reflection;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        //private double _scrollHeight = 204;
        private double _itemHeight = 28;

        private readonly double _collectionFilterWidth = 250;
        private readonly double _sliderWidth = 50;
        private readonly double _namesFilterWidth = 200;
        private readonly double _entireBlockHeight = 385;
        private  double _scrollHeight = 280;
        private readonly double _workAreaWidth = 550;
        private readonly double _normalOpacity = 0.4;
        private readonly double _chosenOpacity = 1;
        private bool _filterIsOpen;

        private double _doubleRest;
        private int _visibleRange;
        private readonly int _loadingBadgeCount = 20;
        private List<int []> _loadedStartEndPairs;
        private int [] _containingPair;
        private bool _containingPairExists;


        private Thickness cFM;
        internal Thickness CollectionFilterMargin
        {
            get { return cFM; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cFM, value, nameof (CollectionFilterMargin));
            }
        }

        private double cMW;
        internal double CollectionFilterWidth
        {
            get { return cMW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cMW, value, nameof (CollectionFilterWidth));
            }
        }

        private double nFW;
        internal double NamesFilterWidth
        {
            get { return nFW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref nFW, value, nameof (NamesFilterWidth));
            }
        }

        private double cO;
        internal double CorrectnessOpacity
        {
            get { return cO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cO, value, nameof (CorrectnessOpacity));
            }
        }

        private double iO;
        internal double IncorrectnessOpacity
        {
            get { return iO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref iO, value, nameof (IncorrectnessOpacity));
            }
        }

        private Bitmap cR;
        internal Bitmap CorrectnessIcon
        {
            get { return cR; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cR, value, nameof (CorrectnessIcon));
            }
        }

        private Bitmap iC;
        internal Bitmap IncorrectnessIcon
        {
            get { return iC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref iC, value, nameof (IncorrectnessIcon));
            }
        }

        private Vector sO;
        internal Vector SliderOffset
        {
            get { return sO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sO, value, nameof (SliderOffset));
            }
        }

        private ObservableCollection<BadgeCorrectnessViewModel> cL;
        internal ObservableCollection<BadgeCorrectnessViewModel> VisibleIcons
        {
            get { return cL; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cL, value, nameof (VisibleIcons));
            }
        }

        private BadgeCorrectnessViewModel bpI;
        internal BadgeCorrectnessViewModel ActiveIcon
        {
            get { return bpI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpI, value, nameof (ActiveIcon));
            }
        }

        private double eBH;
        internal double EntireBlockHeight
        {
            get { return eBH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eBH, value, nameof (EntireBlockHeight));
            }
        }

        private double sH;
        internal double ScrollHeight
        {
            get { return sH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sH, value, nameof (ScrollHeight));
            }
        }


        private void SetUpSliderBlock ( )
        {
            _loadedStartEndPairs = new ();
            CollectionFilterWidth = _sliderWidth;
            CollectionFilterMargin = new Thickness (_namesFilterWidth, 0);
            ScrollHeight = _scrollHeight;
            EntireBlockHeight = _entireBlockHeight;
            NamesFilterWidth = 0;
            SliderOffset = new Vector (0, 0);
        }


        internal void ChangeScrollHeight ( double delta )
        {
            _scrollHeight -= delta;
            _visibleRange = ( int ) ( _scrollHeight / _itemHeight );
            _visibleRangeEnd = _visibleRange - 1;
        }


        private void HighLightChosenIcon ( BadgeCorrectnessViewModel icon )
        {
            //icon.BorderColor = new SolidColorBrush (MainWindow.black);
            icon.BoundFontWeight = FontWeight.Bold;
            icon.IconOpacity = _chosenOpacity;
        }


        private void FadeIcon ( BadgeCorrectnessViewModel icon )
        {
            //icon.BorderColor = new SolidColorBrush (MainWindow.white);
            icon.BoundFontWeight = FontWeight.Normal;
            icon.IconOpacity = _normalOpacity;
        }


        internal void ExtendOrShrinkCollectionManagement ()
        {
            if ( _filterIsOpen )
            {
                CollectionFilterMargin = new Thickness (_namesFilterWidth, 0);
                WorkAreaWidth += _namesFilterWidth;
                _filterIsOpen = false;
            }
            else
            {
                CollectionFilterMargin = new Thickness (0, 0);
                WorkAreaWidth -= _namesFilterWidth;
                _filterIsOpen = true;
            }
        }


        private void ResetActiveIcon ()
        {
            if ( BeingProcessedBadge.IsCorrect )
            {
                VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (true, BeingProcessedBadge);

                if ( !FixedBadges.Contains (BeingProcessedBadge) )
                {
                    FixedBadges.Add (BeingProcessedBadge);
                }

                if ( IncorrectBadges.Contains (BeingProcessedBadge) )
                {
                    IncorrectBadges.Remove (BeingProcessedBadge);
                }

            }
            else
            {
                if ( VisibleIcons [BeingProcessedNumber - 1].Correctness )
                {
                    VisibleIcons [BeingProcessedNumber - 1] = new BadgeCorrectnessViewModel (false, BeingProcessedBadge);
                }

                if ( !IncorrectBadges.Contains (BeingProcessedBadge) )
                {
                    IncorrectBadges.Add (BeingProcessedBadge);
                }

                if ( FixedBadges.Contains (BeingProcessedBadge) )
                {
                    FixedBadges.Remove (BeingProcessedBadge);
                }
            }

            ActiveIcon = VisibleIcons [BeingProcessedNumber - 1];
            HighLightChosenIcon (ActiveIcon);
        }


        private void SetVisibleIcons ()
        {
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            if ( ( VisibleBadges != null )   &&   ( VisibleBadges. Count > 0 ) )
            {
                for ( int index = 0;   index < VisibleBadges. Count;   index++ )
                {
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, VisibleBadges [index]);
                    VisibleIcons.Add (icon);
                    FadeIcon (icon);
                }

                ActiveIcon = VisibleIcons [0];
                HighLightChosenIcon (ActiveIcon);
            }
        }
    }
}