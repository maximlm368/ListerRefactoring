using Avalonia;
using Avalonia.Media;
using Lister.Desktop.ModelMappings.BadgeVM;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

public partial class BadgeEditorViewModel : ReactiveObject
{
    private readonly double _minRunnerHeight = 10;
    private readonly double _upDownButtonHeightWigth = 15;
    private readonly double _scrollingScratch = 25;

    private readonly double _sliderWidth = 50;
    private readonly double _namesFilterWidth = 200;
    private readonly double _collectionFilterMarginLeft = 232;

    private int _maxVisibleCount;
    private double _scrollValue;
    private double _runnerStep;
    private double _runnerHasWalked;
    private double _runnerHasWalkedStorage;
    private double _itemHeightWithMargin = 28;
    private double _entireBlockHeight = 380;
    private double _scrollerHeight = 224;
    private double _extendedScrollableIconWidth = 219;
    private double _mostExtendedIconWidth = 224;
    private double _shrinkedIconWidth = 24;
    private double _iconWidthIncreasing = 20;
    private double _doubleRest;
    private int _visibleRange;
    private int _scrollStepNumberStorage;
    private int _scrollStepIndex;
    private ObservableCollection<BadgeCorrectnessViewModel> _visibleIconsStorage;
    private double _scrollOffsetStorage;
    private double _upDownWidth = 20;

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
    internal Thickness FilterBlockMargin
    {
        get { return _collectionFilterMargin; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _collectionFilterMargin, value, nameof (FilterBlockMargin));
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

    private double _scrollHeight = 234;
    internal double ScrollHeight
    {
        get { return _scrollHeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _scrollHeight, value, nameof (ScrollHeight));
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

    private double _sliderCollectionWidth;
    internal double SliderCollectionWidth
    {
        get { return _sliderCollectionWidth; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _sliderCollectionWidth, value, nameof (SliderCollectionWidth));
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

    private bool _upDownIsVisible;
    internal bool UpDownIsVisible
    {
        get { return _upDownIsVisible; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _upDownIsVisible, value, nameof (UpDownIsVisible));
        }
    }

    private Thickness _sliderMargin;
    internal Thickness SliderMargin
    {
        get { return _sliderMargin; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _sliderMargin, value, nameof (SliderMargin));
        }
    }

    private bool _previousOnSliderIsEnable;
    internal bool PreviousOnSliderIsEnable
    {
        get { return _previousOnSliderIsEnable; }
        private set
        {
            if ( UpDownIsVisible ) 
            {
                _previousOnSliderIsEnable = value;
                this.RaisePropertyChanged (nameof (PreviousOnSliderIsEnable));
            }
        }
    }

    private bool _nextOnSliderIsEnable;
    internal bool NextOnSliderIsEnable
    {
        get { return _nextOnSliderIsEnable; }
        private set
        {
            if ( UpDownIsVisible )
            {
                _nextOnSliderIsEnable = value;
                this.RaisePropertyChanged (nameof (NextOnSliderIsEnable));
            }
        }
    }

    private bool _filterIsExtended;
    internal bool FilterIsExtended 
    {
        get { return _filterIsExtended; }

        private set 
        {
            this.RaiseAndSetIfChanged (ref _filterIsExtended, value, nameof (FilterIsExtended));
            FilterIsShrinked = ! value;
            UpDownIsFocusable = ! value;
        }
    }

    private bool _filterIsShrinked;
    internal bool FilterIsShrinked
    {
        get { return _filterIsShrinked; }

        private set
        {
            this.RaiseAndSetIfChanged (ref _filterIsShrinked, value, nameof (FilterIsShrinked));
        }
    }


    private void SetUpScrollBlock ( int incorrectBadgesAmmount )
    {
        SwitcherForeground = _switcherAllForeground;
        SwitcherTip = _allTip;
        FilterNames = new ObservableCollection <string> () { _allFilter, _correctFilter, _incorrectFilter };
        CollectionFilterWidth = _sliderWidth;
        FilterBlockMargin = new Thickness (_collectionFilterMarginLeft, 0);
        SwitcherWidth = _switcherWidth;
        ScrollHeight = _scrollHeight;
        EntireBlockHeight = _entireBlockHeight;
        CalcVisibleRange (incorrectBadgesAmmount);
        NamesFilterWidth = 0;
        ExtentionTip = _extentionToolTip;
        SetScroller (incorrectBadgesAmmount);
    }


    private void CalcVisibleRange ( int countInCollection )
    {
        _visibleRange = ( int ) ( _scrollHeight / _itemHeightWithMargin );
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
            SliderCollectionWidth = _extendedScrollableIconWidth + _iconWidthIncreasing;
            UpDownIsFocusable = false;
            return;
        }

        if ( badgesAmount < _scrollHeight/_itemHeightWithMargin ) 
        {
            ScrollWidth = 0;
            SliderCollectionWidth = _extendedScrollableIconWidth + _iconWidthIncreasing;
            UpDownWidth = _upDownWidth;
            return;
        }

        UpDownIsFocusable = true;
        UpDownWidth = _upDownWidth;
        SliderCollectionWidth = _extendedScrollableIconWidth;
        CalcRunner (badgesAmount);
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
        RunnerBruttoWalkSpace = _scrollerHeight - ( _upDownButtonHeightWigth * 2 );
        double proportion = ( _itemHeightWithMargin * badgesAmount ) / RunnerBruttoWalkSpace;
        RunnerHeight = _scrollHeight / proportion;
        RealRunnerHeight = RunnerHeight;

        if ( RunnerHeight < _minRunnerHeight )
        {
            RunnerHeight = _minRunnerHeight;
        }

        RunnerWalkSpace = RunnerBruttoWalkSpace - RunnerHeight;
        _runnerStep = RunnerWalkSpace / ( badgesAmount - _maxVisibleCount );
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

        for ( ;   index < ( _visibleRange - 1 );   index++ )
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
        double usefullWay = (_itemHeightWithMargin * CurrentVisibleCollection. Count) - RunnerBruttoWalkSpace;
        double proportion = usefullWay / ( RunnerBruttoWalkSpace - RealRunnerHeight );
        double step = runnerStep * proportion;
        int steps = (int) (Math.Round (step / _itemHeightWithMargin));

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


    private BadgeCorrectnessViewModel ? GetCorrespondingIcon ( int badgeIndex )
    {
        BadgeCorrectnessViewModel result = null;
        BadgeViewModel goalBadge = null;
        bool indexIsWithinCollection = ((CurrentVisibleCollection. Count) > badgeIndex)   
                                       &&
                                       (badgeIndex >= 0);

        if ( indexIsWithinCollection )
        {
            goalBadge = CurrentVisibleCollection.ElementAt (badgeIndex);
        }

        if ( goalBadge != null ) 
        {
            result = new BadgeCorrectnessViewModel (goalBadge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                          , _correctnessWidthLimit, FilterIsExtended);
        }

        return result;
    }


    internal void ShiftRunner ( double dastinationPointer )
    {
        int currentAmount = CurrentVisibleCollection.Count;
        bool dastinationIsOnRunner = (dastinationPointer >= _runnerHasWalked)
                                     && 
                                     (dastinationPointer <= (_runnerHasWalked + RunnerHeight));

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
                                   && 
                                   ( BeingProcessedNumber < ( _scrollStepIndex + _visibleRange + 1 ) );

        if ( activeIconIsVisible )
        {
            _numberAmongVisibleIcons = (BeingProcessedNumber - _scrollStepIndex);
            _visibleRangeEnd = (_scrollStepIndex + _visibleRange - 1);
            SaveScrollerState ();
            ActiveIcon = VisibleIcons [BeingProcessedNumber - _scrollStepIndex - 1];
            HighLightChosenIcon (ActiveIcon);
        }
    }


    private void SetScrollerToStateBeforeScrollingIfShould ( )
    {
        if ( ! _visibleIconsStorage.Equals (VisibleIcons) )
        {
            VisibleIcons = _visibleIconsStorage;
            ExtendOrShrinkSliderItems ();
            _scrollStepIndex = _scrollStepNumberStorage;
            _runnerHasWalked = _runnerHasWalkedStorage;
            ScrollOffset = _scrollOffsetStorage;
        }
    }


    private void SaveScrollerState ()
    {
        _scrollStepNumberStorage = _scrollStepIndex;
        _runnerHasWalkedStorage = _runnerHasWalked;
        _scrollOffsetStorage = ScrollOffset;
        _visibleIconsStorage = VisibleIcons;
    }


    private void ZeroScrollerState ( ObservableCollection <BadgeCorrectnessViewModel> icons )
    {
        _scrollStepNumberStorage = 0;
        _runnerHasWalkedStorage = 0;
        _scrollOffsetStorage = 0;
        _visibleIconsStorage = icons;
    }


    private void HighLightChosenIcon ( BadgeCorrectnessViewModel icon )
    {
        icon.BoundFontWeight = FontWeight.Bold;
        icon.CalcStringPresentation ( _correctnessWidthLimit );
    }


    private void TryEnableScroller ( int enablingRange )
    {
        double scrollerItemsCount = _scrollerHeight / _itemHeightWithMargin;

        if ( enablingRange < 2 )
        {
            UpDownIsVisible = false;
            SliderMargin = new Thickness ( 12, 40 );
        }
        else
        {
            if ( FilterIsExtended )
            {
                UpDownIsVisible = false;
                SliderMargin = new Thickness ( 12, 40 );
            }
            else
            {
                UpDownIsVisible = true;
                SliderMargin = new Thickness ( 12, 0 );
                NextOnSliderIsEnable = NextIsEnable;
                PreviousOnSliderIsEnable = PreviousIsEnable;
            }
        }
    }
}