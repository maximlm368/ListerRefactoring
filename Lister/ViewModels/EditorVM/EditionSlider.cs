using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using System.Diagnostics;

namespace Lister.ViewModels
{
    public partial class BadgeEditorViewModel : ViewModelBase
    {
        private static readonly double _minRunnerHeight = 10;
        private static readonly double _upDownButtonHeightWigth = 15;
        private static readonly double _scrollingScratch = 25;
        private int _maxVisibleCount;
        
        //private Stopwatch _scrollTimer;
        //private int _timesCommandCalled;
        
        private double _scrollValue;
        private double _runnerStep;
        private double _runnerHasWalked;
        private double _runnerHasWalkedStorage;
        private double _scrollingLength;

        //private double _scrollHeight = 204;
        private double _itemHeight = 28;

        private readonly double _collectionFilterWidth = 250;
        private readonly double _sliderWidth = 50;
        private readonly double _namesFilterWidth = 200;
        private double _entireBlockHeight = 380;
        private double _scrollHeight = 252;
        
        private readonly double _normalOpacity = 0.4;
        private readonly double _chosenOpacity = 1;
        private bool _filterIsOpen;

        private double _doubleRest;
        private int _visibleRange;
        private int _scrollStepNumberStorage;
        private int _scrollStepIndex;


        private SolidColorBrush sB;
        internal SolidColorBrush SwitcherBackground
        {
            get { return sB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sB, value, nameof (SwitcherBackground));
            }
        }

        private ObservableCollection <string> fN;
        internal ObservableCollection <string> FilterNames
        {
            get { return fN; }
            private set
            {
                this.RaiseAndSetIfChanged (ref fN, value, nameof (FilterNames));
            }
        }

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
        internal Bitmap FilterState
        {
            get { return cR; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cR, value, nameof (FilterState));
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

        private ObservableCollection <BadgeCorrectnessViewModel> _visibleIconsStorage;
        private ObservableCollection <BadgeCorrectnessViewModel> vI;
        internal ObservableCollection <BadgeCorrectnessViewModel> VisibleIcons
        {
            get { return vI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref vI, value, nameof (VisibleIcons));
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

        private double sW;
        internal double ScrollWidth
        {
            get { return sW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sW, value, nameof (ScrollWidth));
            }
        }

        private double rBWS;
        internal double RunnerBruttoWalkSpace
        {
            get { return rBWS; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rBWS, value, nameof (RunnerBruttoWalkSpace));
            }
        }

        private double _scrollOffsetStorage;
        private double sO;
        internal double ScrollOffset
        {
            get { return sO; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sO, value, nameof (ScrollOffset));
            }
        }

        internal double RealRunnerHeight { get; private set; }
        private double rH;
        internal double RunnerHeight
        {
            get { return rH; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rH, value, nameof (RunnerHeight));
            }
        }

        private double rWS;
        internal double RunnerWalkSpace
        {
            get { return rWS; }
            private set
            {
                this.RaiseAndSetIfChanged (ref rWS, value, nameof (RunnerWalkSpace));
            }
        }

        private double _upDownWidth = 20;
        private double uDW;
        internal double UpDownWidth
        {
            get { return uDW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref uDW, value, nameof (UpDownWidth));
            }
        }

        private bool uDF;
        internal bool UpDownIsFocusable
        {
            get { return uDF; }
            private set
            {
                this.RaiseAndSetIfChanged (ref uDF, value, nameof (UpDownIsFocusable));
            }
        }


        private void SetUpSliderBlock ( int incorrectBadgesAmmount )
        {
            SwitcherBackground = new SolidColorBrush (new Color (255, 0, 0, 200));

            FilterNames = new ObservableCollection <string> () { "Все", "Без ошибок", "С ошибками" };

            CollectionFilterWidth = _sliderWidth;
            CollectionFilterMargin = new Thickness (_namesFilterWidth, 0);

            SwitcherWidth = _switcherWidth;

            _scrollHeight -= MainWindow.HeightfDifference;
            _entireBlockHeight -= MainWindow.HeightfDifference;

            ScrollHeight = _scrollHeight;
            EntireBlockHeight = _entireBlockHeight;

            CalcVisibleRange (incorrectBadgesAmmount);

            NamesFilterWidth = 0;

            SetScroller (incorrectBadgesAmmount);
        }


        private void CalcVisibleRange ( int countInCollection )
        {
            _visibleRange = ( int ) ( _scrollHeight / _itemHeight );
            _maxVisibleCount = _visibleRange;
            _visibleRange = Math.Min ( _visibleRange, countInCollection );
            _visibleRangeEnd = _visibleRange - 1;
        }


        private void SetScroller ( int badgesAmount )
        {
            if ( badgesAmount == 0 )
            {
                ScrollWidth = 0;
                UpDownWidth = 0;
                UpDownIsFocusable = false;
                return;
            }

            if ( badgesAmount < _scrollHeight/_itemHeight ) 
            {
                ScrollWidth = 0;
                UpDownWidth = _upDownWidth;
                return;
            }

            UpDownIsFocusable = true;
            UpDownWidth = _upDownWidth;

            CalcRunner (badgesAmount);
            FilterState = null;
        }


        private void CalcRunner ( int badgesAmount )
        {
            if ( badgesAmount > 0 )
            {
                if ( badgesAmount <= _maxVisibleCount )
                {
                    ScrollWidth = 0;
                }
                else 
                {
                    CalcRunnerHeightAndStep (badgesAmount);
                    ScrollWidth = _upDownButtonHeightWigth;
                    ScrollOffset = 0;
                }
            }
            else 
            {
                ScrollWidth = 0;
            }
        }


        private void CalcRunnerHeightAndStep ( int badgesAmount )
        {
            RunnerBruttoWalkSpace = _scrollHeight - ( _upDownButtonHeightWigth * 2 );
            double proportion = ( _itemHeight * badgesAmount ) / RunnerBruttoWalkSpace;
            RunnerHeight = _scrollHeight / proportion;

            RealRunnerHeight = RunnerHeight;

            if ( RunnerHeight < _minRunnerHeight )
            {
                RunnerHeight = _minRunnerHeight;
            }

            RunnerWalkSpace = RunnerBruttoWalkSpace - RunnerHeight;
            _runnerStep = RunnerWalkSpace / ( badgesAmount - _maxVisibleCount );
        }


        internal void ChangeScrollHeight ( double delta )
        {
            try
            {
                //_scrollHeight -= delta;
                //_entireBlockHeight -= delta;
                //EntireBlockHeight -= delta;
                //ScrollHeight -= delta;
                //int newVisibleRange = ( int ) ( _scrollHeight / _itemHeight );
                //int diff = newVisibleRange - _visibleRange;
                //_visibleRange = newVisibleRange;
                //_visibleRangeEnd += diff;
                //RunnerBruttoWalkSpace -= delta;

                //VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

                //for ( int index = 0;   index < _visibleRange;   index++ )
                //{
                //    VisibleIcons.Add (new BadgeCorrectnessViewModel (false, _allReadonlyBadges [_scrollStepNumber + index]));
                //}

                //ActiveIcon = VisibleIcons [BeingProcessedNumber - _scrollStepNumber - 1];
                //HighLightChosenIcon (ActiveIcon);
            }
            catch ( Exception ex ) {}
        }


        internal void ScrollUp ( )
        {
            ScrollUpAtOneStep ();
            ShowActiveIconIfInRange ();
        }


        internal void ScrollUpAtOneStep ()
        {
            if ( _scrollStepIndex == 0 )
            {
                return;
            }

            _scrollStepIndex--;

            ObservableCollection <BadgeCorrectnessViewModel> iconsCopy = new ();
            int index = 0;

            int badgeNumberInCurrentList = ( _scrollStepIndex + index );

            BadgeCorrectnessViewModel firstIcon = GetCorrespondingIcon (badgeNumberInCurrentList);

            iconsCopy.Add (firstIcon);

            for ( ; index < ( _visibleRange - 1 );   index++ )
            {
                iconsCopy.Add (VisibleIcons [index]);
            }

            VisibleIcons = iconsCopy;
            _runnerHasWalked = _scrollStepIndex * _runnerStep;
            ScrollOffset = Math.Round (_runnerHasWalked);
        }


        internal void ScrollDown ( )
        {
            ScrollDownAtOneStep ();
            ShowActiveIconIfInRange ();
        }


        private void ScrollDownAtOneStep ( )
        {
            int currentAmount = _currentVisibleCollection.Count;

            if ( _scrollStepIndex == currentAmount - _visibleRange )
            {
                return;
            }

            _scrollStepIndex++;

            ObservableCollection <BadgeCorrectnessViewModel> iconsCopy = new ();
            
            int index = 1;

            for ( ;   index < _visibleRange;   index++ )
            {
                iconsCopy.Add (VisibleIcons [index]);
            }

            int badgeNumberInCurrentList = ( _scrollStepIndex + index - 1 );

            BadgeCorrectnessViewModel lastIcon = GetCorrespondingIcon (badgeNumberInCurrentList);

            iconsCopy.Add (lastIcon);
            VisibleIcons = iconsCopy;
            _runnerHasWalked = _scrollStepIndex * _runnerStep;
            ScrollOffset = _runnerHasWalked;
        }


        private void ReduceCurrentCollectionAndIcon ( int removableIndex, int removableIndexAmongVisibleIcons
                                                  , bool shiftToSideLast, bool shouldShrinkIcons, bool shouldAddOne )
        {
            int indexInEntireList = removableIndex;
            int indexInVisibleRange = removableIndexAmongVisibleIcons;

            _visibleRange = Math.Min (_visibleRange, VisibleIcons. Count);

            if ( shouldShrinkIcons && ( _currentVisibleCollection.Count < _maxVisibleCount ) )
            {
                _visibleRangeEnd--;
                _visibleRange--;
            }

            if ( shouldShrinkIcons   &&   shiftToSideLast )
            {
                if ( ! shouldAddOne ) 
                {
                    VisibleIcons.RemoveAt (VisibleIcons. Count - 1);
                }

                for ( ; indexInVisibleRange < _visibleRange;   indexInVisibleRange++, indexInEntireList++ )
                {
                    BadgeCorrectnessViewModel correspondingIcon = GetCorrespondingIcon (indexInEntireList + 1);

                    if ( correspondingIcon == null )
                    {
                        break;
                    }

                    VisibleIcons [indexInVisibleRange] = correspondingIcon;
                }

                BadgeCorrectnessViewModel lastIcon = GetCorrespondingIcon (indexInEntireList + 1);

                if ( lastIcon != null ) 
                {
                    VisibleIcons.Add (lastIcon);
                }
            }
            else if ( shouldShrinkIcons   &&   ! shiftToSideLast )
            {
                for ( ; indexInVisibleRange >= 0;   indexInVisibleRange--, indexInEntireList-- )
                {
                    BadgeCorrectnessViewModel correspondingIcon = GetCorrespondingIcon (indexInEntireList - 1);

                    bool zeroIsAchieved = ( correspondingIcon == null );

                    if ( zeroIsAchieved )
                    {
                        break;
                    }

                    VisibleIcons [indexInVisibleRange] = correspondingIcon;
                }
            }

            ReduceCurrentCollection (removableIndex);
        }


        private void ReduceCurrentCollection ( int removableIndex ) 
        {
            if ( _filterState == FilterChoosing.Corrects )
            {
                BadgeViewModel badge = CorrectNumbered.ElementAt (removableIndex).Value;
                IncorrectNumbered.Add (badge.Id, badge);
                CorrectNumbered.Remove (badge.Id);
            }
            else if ( _filterState == FilterChoosing.Incorrects )
            {
                BadgeViewModel badge = IncorrectNumbered.ElementAt (removableIndex).Value;
                CorrectNumbered.Add (badge.Id, badge);
                IncorrectNumbered.Remove (badge.Id);
            }

            if ( _filterState != FilterChoosing.All )
            {
                ProcessableCount--;
            }
        }


        private BadgeCorrectnessViewModel ? GetCorrespondingIcon ( int badgeIndex )
        {
            BadgeCorrectnessViewModel result = null;
            bool isCorrect = false;
            BadgeViewModel goalBadge = null;

            if ( _filterState == FilterChoosing.All )
            {
                goalBadge = AllNumbered [badgeIndex];
                isCorrect = goalBadge.IsCorrect;
            }
            else 
            {
                bool indexIsWithinCollection = ((_currentVisibleCollection.Count ) > badgeIndex)   &&   (badgeIndex >= 0);

                if ( indexIsWithinCollection ) 
                {
                    goalBadge = _currentVisibleCollection.ElementAt (badgeIndex).Value;
                    isCorrect = goalBadge.IsCorrect;
                }
            }

            if ( goalBadge != null ) 
            {
                result = new BadgeCorrectnessViewModel (isCorrect, goalBadge);
            }

            return result;
        }


        internal void ShiftRunner ( double dastinationPointer )
        {
            int currentAmount = _currentVisibleCollection.Count;

            bool dastinationIsOnRunner = ( dastinationPointer >= _runnerHasWalked )
                                      && ( dastinationPointer <= (_runnerHasWalked + RunnerHeight) );

            if (dastinationIsOnRunner) 
            {
                return;
            }

            _scrollStepIndex = (int)(dastinationPointer/_runnerStep);
            double wayMustWalk = _scrollStepIndex * _runnerStep;

            if (_scrollStepIndex >= (currentAmount - _visibleRange)) 
            {
                wayMustWalk = ( RunnerBruttoWalkSpace - RunnerHeight );
                _scrollStepIndex = ( currentAmount - _visibleRange );
            }

            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();

            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                VisibleIcons.Add (GetCorrespondingIcon (_scrollStepIndex + index));
            }
            
            _runnerHasWalked = wayMustWalk;
            ScrollOffset = wayMustWalk;
            ShowActiveIconIfInRange ();
        }


        internal void ScrollByWheel ( bool isDirectionUp )
        {
            if ( ScrollWidth == 0 ) 
            {
                return;
            }

            if ( isDirectionUp )
            {
                ScrollUp ();
            }
            else 
            {
                ScrollDown ();
            }
        }


        private void ShowActiveIconIfInRange ( )
        {
            bool activeIconIsVisible = ( BeingProcessedNumber > _scrollStepIndex )
                           && ( BeingProcessedNumber < ( _scrollStepIndex + _visibleRange + 1 ) );

            if ( activeIconIsVisible )
            {
                _numberAmongVisibleIcons = (BeingProcessedNumber - _scrollStepIndex);
                _visibleRangeEnd = (_scrollStepIndex + _visibleRange - 1);
                SaveSliderState ();
                ActiveIcon = VisibleIcons [BeingProcessedNumber - _scrollStepIndex - 1];
                HighLightChosenIcon (ActiveIcon);
            }
        }


        private void SetSliderToStationBeforeScrollingIfShould ( )
        {
            if ( ! _visibleIconsStorage.Equals (VisibleIcons) )
            {
                VisibleIcons = _visibleIconsStorage;
                _scrollStepIndex = _scrollStepNumberStorage;
                _runnerHasWalked = _runnerHasWalkedStorage;
                ScrollOffset = _scrollOffsetStorage;
            }
        }


        private void SaveSliderState ()
        {
            _scrollStepNumberStorage = _scrollStepIndex;
            _runnerHasWalkedStorage = _runnerHasWalked;
            _scrollOffsetStorage = ScrollOffset;
            _visibleIconsStorage = VisibleIcons;
        }


        private void ZeroSliderStation ( ObservableCollection <BadgeCorrectnessViewModel> icons )
        {
            _scrollStepNumberStorage = 0;
            _runnerHasWalkedStorage = 0;
            _scrollOffsetStorage = 0;
            _visibleIconsStorage = icons;
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


        private void ResetActiveIcon ()
        {
            if ( BeingProcessedBadge. IsCorrect )
            {
                if ( ! ActiveIcon. Correctness )
                {
                    ActiveIcon.SwitchCorrectness ();

                    if ( _filterState == FilterChoosing.All )
                    {
                        try
                        {
                            CorrectNumbered.Add (BeingProcessedBadge. Id, BeingProcessedBadge);
                            IncorrectNumbered.Remove (BeingProcessedBadge. Id);
                        }
                        catch ( Exception ex ) { }
                    }

                    IncorrectBadgesCount--;
                }
            }
            else if ( ! BeingProcessedBadge. IsCorrect )
            {
                if ( ActiveIcon. Correctness )
                {
                    ActiveIcon.SwitchCorrectness ();

                    if ( _filterState == FilterChoosing.All )
                    {
                        try
                        {
                            IncorrectNumbered.Add (BeingProcessedBadge. Id, BeingProcessedBadge);
                            CorrectNumbered.Remove (BeingProcessedBadge. Id);
                        }
                        catch ( Exception ex ) { }
                    }

                    IncorrectBadgesCount++;
                }
            }
        }


        private void SetVisibleIcons ()
        {
            VisibleIcons = new ObservableCollection <BadgeCorrectnessViewModel> ();
            _visibleIconsStorage = new ObservableCollection <BadgeCorrectnessViewModel> ();

            if ( ( _allReadonlyBadges != null )   &&   ( _allReadonlyBadges. Count > 0 )   &&   ( _visibleRange > 0 ) )
            {
                for ( int index = 0;   index < _visibleRange;   index++ )
                {
                    BadgeViewModel boundBadge = _currentVisibleCollection.ElementAt (index).Value;
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, boundBadge);
                    VisibleIcons.Add (icon);
                    _visibleIconsStorage.Add (icon);
                    FadeIcon (icon);

                    if ( index == 0 ) 
                    {
                        ActiveIcon = icon;
                        HighLightChosenIcon (icon);
                    }
                } 
            }
        }
    }
}