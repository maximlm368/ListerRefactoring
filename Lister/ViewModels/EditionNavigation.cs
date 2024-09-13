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
        private bool fE;
        internal bool FirstIsEnable
        {
            get { return fE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fE, value, nameof (FirstIsEnable));
            }
        }

        private bool pE;
        internal bool PreviousIsEnable
        {
            get { return pE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pE, value, nameof (PreviousIsEnable));
            }
        }

        private bool nE;
        internal bool NextIsEnable
        {
            get { return nE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref nE, value, nameof (NextIsEnable));
            }
        }

        private bool lE;
        internal bool LastIsEnable
        {
            get { return lE; }
            private set
            {
                this.RaiseAndSetIfChanged (ref lE, value, nameof (LastIsEnable));
            }
        }

        private int bpN;
        internal int BeingProcessedNumber
        {
            get { return bpN; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bpN, value, nameof (BeingProcessedNumber));
            }
        }


        internal void ToFirst ()
        {
            BeingProcessedBadge.Hide ();
            FilterProcessableBadge (BeingProcessedNumber);
            BeingProcessedBadge = VisibleBadges [0];
            FadeIcon (ActiveIcon);
            ActiveIcon = VisibleIcons [0];
            HighLightChosenIcon (ActiveIcon);
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = 1;
            SetToCorrectScale (BeingProcessedBadge);
            EnableNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            SliderOffset = new Vector (0, 0);

            //Crutch instead SliderOffset
            _view.sliderScroller.ScrollToHome ();
            _visibleRangeEnd = _visibleRange - 1;
        }


        internal void ToPrevious ()
        {
            if ( BeingProcessedNumber <= 1 )
            {
                return;
            }

            EnableNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            BeingProcessedBadge.Hide ();
            FilterProcessableBadge (BeingProcessedNumber);
            BeingProcessedNumber--;
            BeingProcessedBadge = VisibleBadges [BeingProcessedNumber - 1];
            SetToCorrectScale (BeingProcessedBadge);

            ReleaseCaptured ();

            if ( BeingProcessedNumber < ( _visibleRangeEnd - _visibleRange + 2 ) )
            {
                SliderOffset = new Vector (0, SliderOffset.Y - _itemHeight - _doubleRest);
                _doubleRest = 0;
                _visibleRangeEnd--;
            }

            



            FadeIcon (ActiveIcon);
            ActiveIcon = VisibleIcons [BeingProcessedNumber - 1];
            HighLightChosenIcon (ActiveIcon);
            BeingProcessedBadge.Show ();
        }


        internal void ToNext ()
        {
            if ( BeingProcessedNumber >= ProcessableCount )
            {
                return;
            }

            BeingProcessedBadge.Hide ();

            int number = BeingProcessedNumber + 1;
            bool filterOccured = FilterProcessableBadge (BeingProcessedNumber);

            if ( filterOccured )
            {
                number--;
            }

            BeingProcessedNumber = number;

            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            //Crutch instead SliderOffset
            //_view. sliderScroller.Offset = new Vector (0, SliderOffset.Y);

            //SaveProcessableIconVisible ();

            if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
            {
                _containingPair = null;

                foreach ( int [] pair   in   _loadedStartEndPairs )
                {
                    if ( ( ( BeingProcessedNumber - 2 ) >= pair [0] ) && ( ( BeingProcessedNumber - 2 ) <= pair [1] ) )
                    {
                        _containingPair = pair;
                        break;
                    }
                }

                if ( _containingPair == null )
                {
                    _containingPair = new int [2] { ( BeingProcessedNumber - 1 ), ( BeingProcessedNumber - 1 ) };
                    _loadedStartEndPairs.Add (_containingPair);
                }

                bool goneOutOfPair = ( _containingPair [1] < ( BeingProcessedNumber - 1 ) )
                                     ||
                                     ( _containingPair [0] == _containingPair [1] );

                if ( goneOutOfPair )
                {
                    BadgeViewModel badge = _badgeStorage [BeingProcessedNumber - 1];

                    if ( ! _drafts.ContainsKey (badge) )
                    {
                        BadgeViewModel clone = badge.Clone ();
                        _drafts.Add (badge, clone);

                        IncorrectBadges.Add (clone);
                        VisibleBadges.Add (clone);

                        BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, clone);
                        VisibleIcons.Add (icon);

                        _containingPair [1]++;
                    }
                }

                ScrollViewer scroller = _view.sliderScroller;

                SliderOffset = new Vector (0, scroller.Offset.Y + _itemHeight - _doubleRest);
                _doubleRest = 0;
                _visibleRangeEnd++;
            }

            BeingProcessedBadge = VisibleBadges [number - 1];
            BeingProcessedBadge.Show ();

            SetToCorrectScale (BeingProcessedBadge);
            EnableNavigation ();

            FadeIcon (ActiveIcon);
            ActiveIcon = VisibleIcons [number - 1];
            HighLightChosenIcon (ActiveIcon);
        }


        internal void ToLast ()
        {
            BeingProcessedBadge.Hide ();

            _containingPair = null;
            int maxLoadedNumber = 0;

            foreach ( int [] pair   in   _loadedStartEndPairs )
            {
                int pairEnd = pair [1];

                if ( pairEnd > maxLoadedNumber )
                {
                    maxLoadedNumber = pair [1];
                }

                if ( ( ( _badgeStorage.Count - 1 ) >= pair [0] )   &&   ( ( _badgeStorage.Count - 1 ) <= pair [1] ) )
                {
                    _containingPair = pair;
                }
            }

            if ( _containingPair == null )
            {
                int restToEnd = Math.Min (_loadingBadgeCount, ( _badgeStorage.Count - maxLoadedNumber - 1 ));
                int newPairStart = _badgeStorage.Count - restToEnd - 1;
                _containingPair = new int [2] { newPairStart, ( _badgeStorage.Count - 1 ) };
                _loadedStartEndPairs.Add (_containingPair);
            }

            bool goneOutOfPair = ( _containingPair [1] < ( BeingProcessedNumber - 1 ) )
                                 ||
                                 ( _containingPair [0] == _containingPair [1] );




            if ( VisibleBadges.Count < _badgeStorage.Count )
            {
                int diff = Math.Min (_loadingBadgeCount, ( _badgeStorage.Count - maxLoadedNumber - 1 ));

                for ( int index = ( _badgeStorage.Count - diff ); index < _badgeStorage.Count; index++ )
                {
                    BadgeViewModel badge = _badgeStorage [index];

                    if ( !_drafts.ContainsKey (badge) )
                    {
                        BadgeViewModel clone = badge.Clone ();
                        _drafts.Add (badge, clone);
                        IncorrectBadges.Add (clone);
                        VisibleBadges.Add (clone);

                        BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, clone);
                        VisibleIcons.Add (icon);
                    }
                }
            }

            int number = VisibleBadges.Count;
            bool filterOccured = FilterProcessableBadge (BeingProcessedNumber);

            if ( filterOccured )
            {
                number--;
            }

            BeingProcessedBadge = VisibleBadges [number - 1];
            FadeIcon (ActiveIcon);
            ActiveIcon = VisibleIcons [number - 1];
            HighLightChosenIcon (ActiveIcon);
            BeingProcessedBadge.Show ();
            BeingProcessedNumber = _badgeStorage.Count;
            SetToCorrectScale (BeingProcessedBadge);
            EnableNavigation ();
            MoversAreEnable = false;
            SplitterIsEnable = false;
            ZoommerIsEnable = false;

            ReleaseCaptured ();

            double verticalOffset = ( VisibleBadges.Count - _visibleRange ) * _itemHeight;
            SliderOffset = new Vector (0, verticalOffset);

            //Crutch instead SliderOffset
            _view.sliderScroller.ScrollToEnd ();
            _visibleRangeEnd = VisibleBadges.Count - 1;
        }


        internal void ToParticularBadge ( BadgeViewModel goalBadge )
        {
            if ( goalBadge.Equals (BeingProcessedBadge) )
            {
                return;
            }

            string numberAsText = string.Empty;

            for ( int index = 0; index < VisibleBadges.Count; index++ )
            {
                if ( goalBadge.Equals (VisibleBadges [index]) )
                {
                    numberAsText = ( index + 1 ).ToString ();
                    break;
                }
            }

            ToParticularBadge (numberAsText);
        }


        private void ToParticularBadge ( string numberAsText )
        {
            try
            {
                int number = int.Parse (numberAsText);

                if ( number > VisibleBadges.Count || number < 1 )
                {
                    BeingProcessedNumber = BeingProcessedNumber;
                    return;
                }

                BeingProcessedBadge.Hide ();
                bool filterOccured = FilterProcessableBadge (number);

                if ( filterOccured && ( number > BeingProcessedNumber ) )
                {
                    number--;
                }

                BeingProcessedBadge = VisibleBadges [number - 1];

                if ( ActiveIcon != null )
                {
                    FadeIcon (ActiveIcon);
                }

                ActiveIcon = VisibleIcons [number - 1];
                HighLightChosenIcon (ActiveIcon);
                BeingProcessedBadge.Show ();
                BeingProcessedNumber = number;
                SetToCorrectScale (BeingProcessedBadge);
                EnableNavigation ();
                MoversAreEnable = false;
                SplitterIsEnable = false;
                ZoommerIsEnable = false;
                _view.editorTextBox.IsEnabled = false;
                _view.scalabilityGrade.IsEnabled = false;

                ReleaseCaptured ();

                //SaveProcessableIconVisible ();
            }
            catch ( Exception ex )
            {
                BeingProcessedNumber = BeingProcessedNumber;
            }
        }


        private void SaveProcessableIconVisible ()
        {
            bool numberIsOutOfRange = BeingProcessedNumber > ( _visibleRangeEnd + 1 )
                                      ||
                                      BeingProcessedNumber < ( _visibleRangeEnd + 1 - _visibleRange );

            if ( numberIsOutOfRange )
            {
                ScrollViewer scroller = _view.sliderScroller;

                int offsetedItemsCount = ( int ) ( scroller.Offset.Y / _itemHeight );
                _doubleRest = ( scroller.Offset.Y - ( offsetedItemsCount * _itemHeight ) );

                int diff = _visibleRange - ( BeingProcessedNumber - offsetedItemsCount );
                _visibleRangeEnd = BeingProcessedNumber + diff - 1;
            }
        }


        private void SaveProcessableIconVisibleCh ()
        {
            bool numberIsOutOfRange = BeingProcessedNumber > ( _visibleRangeEnd + 1 )
                                      ||
                                      BeingProcessedNumber < ( _visibleRangeEnd + 1 - _visibleRange );

            if ( numberIsOutOfRange )
            {
                _visibleRangeEnd = BeingProcessedNumber + _visibleRange / 2;
                SliderOffset = new Vector (0, _itemHeight * ( BeingProcessedNumber - _visibleRange / 2 ));
            }
        }


        private void EnableNavigation ()
        {
            int badgeCount = _badgeStorage.Count;

            if ( ( BeingProcessedNumber > 1 ) && ( BeingProcessedNumber == badgeCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( BeingProcessedNumber > 1 ) && ( BeingProcessedNumber < badgeCount ) )
            {
                FirstIsEnable = true;
                PreviousIsEnable = true;
                NextIsEnable = true;
                LastIsEnable = true;
            }
            else if ( ( BeingProcessedNumber == 1 ) && ( badgeCount == 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = false;
                LastIsEnable = false;
            }
            else if ( ( BeingProcessedNumber == 1 ) && ( badgeCount > 1 ) )
            {
                FirstIsEnable = false;
                PreviousIsEnable = false;
                NextIsEnable = true;
                LastIsEnable = true;
            }
        }
    }
}