using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

internal partial class EditorViewModel : ObservableObject
{
    private readonly double _minRunnerHeight = 10;
    private readonly double _upDownButtonHeightWigth = 15;
    private readonly double _scrollingScratch = 25;
    private readonly double _sliderWidth = 50;
    private readonly double _namesFilterWidthh = 200;
    private readonly double _collectionFilterMarginLeft = 232;
    private int _maxVisibleCount;
    private double _scrollValue;
    private double _runnerStep;
    private double _runnerHasWalked;
    private double _runnerHasWalkedStorage;
    private double _itemHeightWithMargin = 28;
    private double _entireBlockHeightt = 380;
    private double _scrollerHeight = 224;
    private double _extendedScrollableIconWidth = 219;
    private double _mostExtendedIconWidth = 224;
    private double _shrinkedIconWidth = 24;
    private double _iconWidthIncreasing = 20;
    private double _doubleRest;
    private int _visibleRange;
    private int _scrollStepNumberStorage;
    private int _scrollStepIndex;
    private ObservableCollection<BadgeCorrectnessViewModel>? _visibleIconsStorage = [];
    private double _scrollOffsetStorage;
    private double _upDownWidthh = 20;

    [ObservableProperty]
    private ObservableCollection<string>? _filterNames;

    [ObservableProperty]
    private Thickness _filterBlockMargin;

    [ObservableProperty]
    private double _collectionFilterWidth;

    [ObservableProperty]
    private double _namesFilterWidth;

    [ObservableProperty]
    private double _correctnessOpacity;

    [ObservableProperty]
    private double _incorrectnessOpacity;

    [ObservableProperty]
    private BadgeCorrectnessViewModel? _activeIcon;

    [ObservableProperty]
    private double _entireBlockHeight;

    [ObservableProperty]
    private double _scrollHeight = 234;

    [ObservableProperty]
    private double _scrollWidth;

    [ObservableProperty]
    private double _sliderCollectionWidth;

    [ObservableProperty]
    private double _runnerBruttoWalkSpace;

    [ObservableProperty]
    private double _scrollOffset;

    internal double RealRunnerHeight { get; private set; }

    [ObservableProperty]
    private double _runnerHeight;

    [ObservableProperty]
    private double _runnerWalkSpace;

    [ObservableProperty]
    private double _upDownWidth;

    [ObservableProperty]
    private bool _upDownIsFocusable;

    [ObservableProperty]
    private bool _upDownIsVisible;

    [ObservableProperty]
    private Thickness _sliderMargin;

    private bool _previousOnSliderIsEnable;
    internal bool PreviousOnSliderIsEnable
    {
        get 
        {
            return _previousOnSliderIsEnable; 
        }

        private set
        {
            if ( UpDownIsVisible )
            {
                _previousOnSliderIsEnable = value;
                OnPropertyChanged ();
            }
        }
    }

    private bool _nextOnSliderIsEnable;
    internal bool NextOnSliderIsEnable
    {
        get 
        { 
            return _nextOnSliderIsEnable; 
        }

        private set
        {
            if ( UpDownIsVisible )
            {
                _nextOnSliderIsEnable = value;
                OnPropertyChanged ();
            }
        }
    }

    private bool _filterIsExtended;
    internal bool FilterIsExtended
    {
        get 
        {
            return _filterIsExtended; 
        }

        private set
        {
            _filterIsExtended = value;
            OnPropertyChanged ();
            FilterIsShrinked = !value;
            UpDownIsFocusable = !value;
        }
    }

    [ObservableProperty]
    private bool _filterIsShrinked;

    private void SetUpScrollBlock ( int incorrectBadgesAmmount )
    {
        SwitcherForeground = _white;
        SwitcherTip = _allTip;
        FilterNames = [_allFilter, _correctFilter, _incorrectFilter];
        CollectionFilterWidth = _sliderWidth;
        FilterBlockMargin = new Thickness ( _collectionFilterMarginLeft, 0 );
        SwitcherWidth = _switcherWidthh;
        //ScrollHeight = _scrollHeight;
        EntireBlockHeight = _entireBlockHeightt;
        CalcVisibleRange ( incorrectBadgesAmmount );
        NamesFilterWidth = 0;
        ExtentionTip = _extentionToolTip;
        SetScroller ( incorrectBadgesAmmount );
    }

    private void CalcVisibleRange ( int countInCollection )
    {
        _visibleRange = ( int ) ( ScrollHeight / _itemHeightWithMargin );
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

        if ( badgesAmount < ScrollHeight / _itemHeightWithMargin )
        {
            ScrollWidth = 0;
            SliderCollectionWidth = _extendedScrollableIconWidth + _iconWidthIncreasing;
            UpDownWidth = _upDownWidthh;
            return;
        }

        UpDownIsFocusable = true;
        UpDownWidth = _upDownWidthh;
        SliderCollectionWidth = _extendedScrollableIconWidth;
        CalcRunner ( badgesAmount );
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
                CalcRunnerHeightAndStep ( badgesAmount );
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
        RunnerHeight = ScrollHeight / proportion;
        RealRunnerHeight = RunnerHeight;

        if ( RunnerHeight < _minRunnerHeight )
        {
            RunnerHeight = _minRunnerHeight;
        }

        RunnerWalkSpace = RunnerBruttoWalkSpace - RunnerHeight;
        _runnerStep = RunnerWalkSpace / ( badgesAmount - _maxVisibleCount );
    }

    internal void ScrollUp ()
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
        ObservableCollection<BadgeCorrectnessViewModel> iconsCopy = [];
        int index = 0;
        int badgeNumberInCurrentList = _scrollStepIndex + index;
        BadgeCorrectnessViewModel? firstIcon = GetCorrespondingIcon ( badgeNumberInCurrentList );
        iconsCopy.Add ( firstIcon );

        for ( ; index < ( _visibleRange - 1 ); index++ )
        {
            iconsCopy.Add ( VisibleIcons [index] );
        }

        VisibleIcons = iconsCopy;
        _runnerHasWalked = _scrollStepIndex * _runnerStep;
        ScrollOffset = Math.Round ( _runnerHasWalked );
    }

    internal void ScrollDown ()
    {
        ScrollDownAtOneStep ();
        ShowActiveIconIfInRange ();
    }

    private void ScrollDownAtOneStep ()
    {
        int currentAmount = CurrentVisibleCollection.Count;

        if ( _scrollStepIndex == currentAmount - _visibleRange )
        {
            return;
        }

        _scrollStepIndex++;
        ObservableCollection<BadgeCorrectnessViewModel> iconsCopy = new ();
        int index = 1;

        for ( ; index < _visibleRange; index++ )
        {
            iconsCopy.Add ( VisibleIcons [index] );
        }

        int badgeNumberInCurrentList = ( _scrollStepIndex + index - 1 );
        BadgeCorrectnessViewModel lastIcon = GetCorrespondingIcon ( badgeNumberInCurrentList );
        iconsCopy.Add ( lastIcon );
        VisibleIcons = iconsCopy;
        _runnerHasWalked = _scrollStepIndex * _runnerStep;
        ScrollOffset = _runnerHasWalked;
    }

    internal void MoveRunner ( double runnerStep )
    {
        double usefullWay = ( _itemHeightWithMargin * CurrentVisibleCollection.Count ) - RunnerBruttoWalkSpace;
        double proportion = usefullWay / ( RunnerBruttoWalkSpace - RealRunnerHeight );
        double step = runnerStep * proportion;
        int steps = ( int ) ( Math.Round ( step / _itemHeightWithMargin ) );

        if ( step > 0 )
        {
            for ( int index = 0; index < steps; index++ )
            {
                ScrollUp ();
            }
        }
        else if ( step < 0 )
        {
            for ( int index = 0; index > steps; index-- )
            {
                ScrollDown ();
            }
        }
    }

    private BadgeCorrectnessViewModel? GetCorrespondingIcon ( int badgeIndex )
    {
        if ( CurrentVisibleCollection == null || CurrentVisibleCollection.Count == 0 )
        {
            return null;
        }

        BadgeViewModel? goalBadge = null;
        bool indexIsWithin = ( CurrentVisibleCollection.Count > badgeIndex ) && ( badgeIndex >= 0 );

        if ( indexIsWithin )
        {
            goalBadge = CurrentVisibleCollection.ElementAt ( badgeIndex );
        }

        if ( goalBadge != null )
        {
            return new BadgeCorrectnessViewModel ( goalBadge, _extendedScrollableIconWidth, _shrinkedIconWidth, _correctnessWidthLimit,
                FilterIsExtended );
        }

        return null;
    }

    internal void ShiftRunner ( double dastinationPointer )
    {
        if ( CurrentVisibleCollection == null || CurrentVisibleCollection.Count == 0 )
        {
            return;
        }

        bool dastinationIsOnRunner = ( dastinationPointer >= _runnerHasWalked ) && ( dastinationPointer <= ( _runnerHasWalked + RunnerHeight ) );

        if ( dastinationIsOnRunner )
        {
            return;
        }

        _scrollStepIndex = ( int ) ( dastinationPointer / _runnerStep );
        double wayMustWalk = _scrollStepIndex * _runnerStep;

        if ( _scrollStepIndex >= ( CurrentVisibleCollection.Count - _visibleRange ) )
        {
            wayMustWalk = RunnerBruttoWalkSpace - RunnerHeight;
            _scrollStepIndex = CurrentVisibleCollection.Count - _visibleRange;
        }

        VisibleIcons = [];

        for ( int index = 0; index < _visibleRange; index++ )
        {
            VisibleIcons.Add ( GetCorrespondingIcon ( _scrollStepIndex + index ) );
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

    private void ShowActiveIconIfInRange ()
    {
        bool activeIconIsVisible = ( BeingProcessedNumber > _scrollStepIndex )
                                   &&
                                   ( BeingProcessedNumber < ( _scrollStepIndex + _visibleRange + 1 ) );

        if ( activeIconIsVisible )
        {
            _numberAmongVisibleIcons = ( BeingProcessedNumber - _scrollStepIndex );
            _visibleRangeEnd = ( _scrollStepIndex + _visibleRange - 1 );
            SaveScrollerState ();
            ActiveIcon = VisibleIcons [BeingProcessedNumber - _scrollStepIndex - 1];
            HighLightChosenIcon ( ActiveIcon );
        }
    }

    private void SetScrollerToStateBeforeScrollingIfShould ()
    {
        if ( !_visibleIconsStorage.Equals ( VisibleIcons ) )
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

    private void ZeroScrollerState ( ObservableCollection<BadgeCorrectnessViewModel> icons )
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
