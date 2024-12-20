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
        private readonly double _minRunnerHeight = 10;
        private readonly double _upDownButtonHeightWigth = 15;
        private readonly double _scrollingScratch = 25;

        //private readonly double _collectionFilterWidth = 250;
        private readonly double _sliderWidth = 50;
        private readonly double _namesFilterWidth = 200;
        private readonly double _collectionFilterMarginLeft = 220;

        private readonly double _normalOpacity = 0.4;
        private readonly double _chosenOpacity = 1;

        private int _maxVisibleCount;
        private double _scrollValue;
        private double _runnerStep;
        private double _runnerHasWalked;
        private double _runnerHasWalkedStorage;
        private double _itemHeight = 28;
        private double _entireBlockHeight = 380;
        private double _scrollHeight = 252;
        private double _iconWidth = 220;
        private double _iconWidthIncreasing = 20;
        private bool _filterIsOpen;
        private double _doubleRest;
        private int _visibleRange;
        private int _scrollStepNumberStorage;
        private int _scrollStepIndex;

        private SolidColorBrush _switcherBackground;
        internal SolidColorBrush SwitcherBackground
        {
            get { return _switcherBackground; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _switcherBackground, value, nameof (SwitcherBackground));
            }
        }

        private ObservableCollection <string> _filterNames;
        internal ObservableCollection <string> FilterNames
        {
            get { return _filterNames; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _filterNames, value, nameof (FilterNames));
            }
        }

        private Thickness _collectionFilterMargin;
        internal Thickness CollectionFilterMargin
        {
            get { return _collectionFilterMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _collectionFilterMargin, value, nameof (CollectionFilterMargin));
            }
        }

        private double _collectionFilterWidth;
        internal double CollectionFilterWidth
        {
            get { return _collectionFilterWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _collectionFilterWidth, value, nameof (CollectionFilterWidth));
            }
        }

        private double _namesFilterWidt;
        internal double NamesFilterWidth
        {
            get { return _namesFilterWidt; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _namesFilterWidt, value, nameof (NamesFilterWidth));
            }
        }

        private double _correctnessOpacity;
        internal double CorrectnessOpacity
        {
            get { return _correctnessOpacity; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _correctnessOpacity, value, nameof (CorrectnessOpacity));
            }
        }

        private double _incorrectnessOpacity;
        internal double IncorrectnessOpacity
        {
            get { return _incorrectnessOpacity; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _incorrectnessOpacity, value, nameof (IncorrectnessOpacity));
            }
        }

        private Bitmap _filterStatePicture;
        internal Bitmap FilterState
        {
            get { return _filterStatePicture; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _filterStatePicture, value, nameof (FilterState));
            }
        }

        private Bitmap _incorrectnessIconBmp;
        internal Bitmap IncorrectnessIcon
        {
            get { return _incorrectnessIconBmp; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _incorrectnessIconBmp, value, nameof (IncorrectnessIcon));
            }
        }

        private ObservableCollection <BadgeCorrectnessViewModel> _visibleIconsStorage;
        private ObservableCollection <BadgeCorrectnessViewModel> _visibleIcons;
        internal ObservableCollection <BadgeCorrectnessViewModel> VisibleIcons
        {
            get { return _visibleIcons; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _visibleIcons, value, nameof (VisibleIcons));
            }
        }

        private BadgeCorrectnessViewModel _activeIcon;
        internal BadgeCorrectnessViewModel ActiveIcon
        {
            get { return _activeIcon; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _activeIcon, value, nameof (ActiveIcon));
            }
        }

        private double _blockHeight;
        internal double EntireBlockHeight
        {
            get { return _blockHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _blockHeight, value, nameof (EntireBlockHeight));
            }
        }

        private double _scrollHeigt;
        internal double ScrollHeight
        {
            get { return _scrollHeigt; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _scrollHeigt, value, nameof (ScrollHeight));
            }
        }

        private double _scrollWidth;
        internal double ScrollWidth
        {
            get { return _scrollWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _scrollWidth, value, nameof (ScrollWidth));
            }
        }

        private double _iconWidt;
        internal double IconWidth
        {
            get { return _iconWidt; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _iconWidt, value, nameof (IconWidth));
            }
        }

        private double _runnerBruttoWalkSpace;
        internal double RunnerBruttoWalkSpace
        {
            get { return _runnerBruttoWalkSpace; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _runnerBruttoWalkSpace, value, nameof (RunnerBruttoWalkSpace));
            }
        }

        private double _scrollOffsetStorage;
        private double _scrollOffset;
        internal double ScrollOffset
        {
            get { return _scrollOffset; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _scrollOffset, value, nameof (ScrollOffset));
            }
        }

        internal double RealRunnerHeight { get; private set; }
        private double _runnerHeight;
        internal double RunnerHeight
        {
            get { return _runnerHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _runnerHeight, value, nameof (RunnerHeight));
            }
        }

        private double _runnerWalkSpace;
        internal double RunnerWalkSpace
        {
            get { return _runnerWalkSpace; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _runnerWalkSpace, value, nameof (RunnerWalkSpace));
            }
        }

        private double _upDownWidth = 20;
        private double _upDownSpace;
        internal double UpDownWidth
        {
            get { return _upDownSpace; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _upDownSpace, value, nameof (UpDownWidth));
            }
        }

        private bool _upDownIsFocusable;
        internal bool UpDownIsFocusable
        {
            get { return _upDownIsFocusable; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _upDownIsFocusable, value, nameof (UpDownIsFocusable));
            }
        }


        private void SetUpSliderBlock ( int incorrectBadgesAmmount )
        {
            SwitcherBackground = new SolidColorBrush (new Color (255, 0, 0, 200));
            SwitcherTip = _allTip;

            FilterNames = new ObservableCollection <string> () { _allLabel, _correctLabel, _incorrectLabel };

            CollectionFilterWidth = _sliderWidth;
            CollectionFilterMargin = new Thickness (_collectionFilterMarginLeft, 0);

            SwitcherWidth = _switcherWidth;

            _scrollHeight -= MainWindow.HeightfDifference;
            _entireBlockHeight -= MainWindow.HeightfDifference;

            ScrollHeight = _scrollHeight;
            EntireBlockHeight = _entireBlockHeight;

            CalcVisibleRange (incorrectBadgesAmmount);

            NamesFilterWidth = 0;

            ExtentionTip = _extentionToolTip;

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
                IconWidth = _iconWidth + _iconWidthIncreasing;
                UpDownIsFocusable = false;
                return;
            }

            if ( badgesAmount < _scrollHeight/_itemHeight ) 
            {
                ScrollWidth = 0;
                IconWidth = _iconWidth + _iconWidthIncreasing;
                UpDownWidth = _upDownWidth;
                return;
            }

            UpDownIsFocusable = true;
            UpDownWidth = _upDownWidth;
            IconWidth = _iconWidth;

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
            int currentAmount = CurrentVisibleCollection.Count;

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


        internal void MoveRunner ( double runnerStep )
        {
            double usefullWay = (_itemHeight * CurrentVisibleCollection. Count) - RunnerBruttoWalkSpace;
            double proportion = usefullWay / ( RunnerBruttoWalkSpace - RealRunnerHeight );
            double step = runnerStep * proportion;
            int steps = (int) (Math.Round (step / _itemHeight));

            if ( step > 0 )
            {
                for ( int index = 0;   index < steps;   index++ ) 
                {
                    ScrollUp ();
                }
            }
            else if ( step < 0 )
            {
                for ( int index = 0;   index > steps;   index-- )
                {
                    ScrollDown ();
                }
            }
        }


        private void ReduceCurrentCollectionAndIcons ( int removableIndex, int removableIndexAmongVisibleIcons
                                                  , bool shiftToSideLast, bool shouldReduceIcons, bool shouldAddOne )
        {
            int indexInEntireList = removableIndex;
            int indexInVisibleRange = removableIndexAmongVisibleIcons;

            _visibleRange = Math.Min (_visibleRange, VisibleIcons. Count);

            if ( shouldReduceIcons   &&   ( CurrentVisibleCollection. Count < _maxVisibleCount ) )
            {
                _visibleRangeEnd--;
                _visibleRange--;
            }

            if ( shouldReduceIcons   &&   shiftToSideLast )
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
            }
            else if ( shouldReduceIcons   &&   ! shiftToSideLast )
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

            //if ( _filterState == FilterChoosing.All )
            //{
            //    goalBadge = AllNumbered [badgeIndex];
            //    isCorrect = goalBadge.IsCorrect;
            //}
            //else 
            //{
            //    bool indexIsWithinCollection = ((_currentVisibleCollection.Count ) > badgeIndex)   &&   (badgeIndex >= 0);

            //    if ( indexIsWithinCollection ) 
            //    {
            //        goalBadge = _currentVisibleCollection.ElementAt (badgeIndex).Value;
            //        isCorrect = goalBadge.IsCorrect;
            //    }
            //}

            bool indexIsWithinCollection = ((CurrentVisibleCollection. Count) > badgeIndex)   &&   (badgeIndex >= 0);

            if ( indexIsWithinCollection )
            {
                goalBadge = CurrentVisibleCollection.ElementAt (badgeIndex).Value;
                isCorrect = goalBadge.IsCorrect;
            }

            if ( goalBadge != null ) 
            {
                result = new BadgeCorrectnessViewModel (isCorrect, goalBadge, _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
            }

            return result;
        }


        internal void ShiftRunner ( double dastinationPointer )
        {
            int currentAmount = CurrentVisibleCollection.Count;

            bool dastinationIsOnRunner = (dastinationPointer >= _runnerHasWalked)
                                      && (dastinationPointer <= (_runnerHasWalked + RunnerHeight));

            if (dastinationIsOnRunner) 
            {
                return;
            }

            _scrollStepIndex = (int)(dastinationPointer/_runnerStep);
            double wayMustWalk = _scrollStepIndex * _runnerStep;

            if (_scrollStepIndex >= (currentAmount - _visibleRange)) 
            {
                wayMustWalk = (RunnerBruttoWalkSpace - RunnerHeight);
                _scrollStepIndex = (currentAmount - _visibleRange);
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
            icon.BoundFontWeight = FontWeight.Bold;
            icon.CalcStringPresentation ( _correctnessWidthLimit
                                        , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
            icon.IconOpacity = _chosenOpacity;
        }


        private void FadeIcon ( BadgeCorrectnessViewModel icon )
        {
            icon.BoundFontWeight = FontWeight.Normal;
            icon.CalcStringPresentation ( _correctnessWidthLimit
                                        , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
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

            if ( ( CurrentVisibleCollection. Count > 0 )   &&   ( _visibleRange > 0 ) )
            {
                for ( int index = 0;   index < _visibleRange;   index++ )
                {
                    BadgeViewModel boundBadge = CurrentVisibleCollection.ElementAt (index).Value;
                    BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel (false, boundBadge
                                                                                   , _correctnessWidthLimit
                                                  , new int [2] { _minCorrectnessTextLength, _maxCorrectnessTextLength });
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