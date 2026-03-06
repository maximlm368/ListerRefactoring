using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Desktop.Entities.BadgeVM;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.EditionView.Parts.Filter.ViewModel;

internal partial class FilterViewModel : ObservableObject
{
    private readonly double _minRunnerHeight = 10;
    private readonly double _upDownButtonMeasure = 15;
    private readonly double _scrollHeight = 234;
    private readonly double _itemHeight = 28;
    private readonly double _scrollerHeight = 224;
    private int _maxVisibleCount;
    private double _runnerStep;
    private double _runnerWalked;
    private double _runnerWalkedStorage;
    private int _visibleRange = 8;
    private int _scrollStepNumberStorage;
    private int _scrollStepIndex;
    private ObservableCollection<BadgeCorrectnessViewModel>? _iconsStorage = [];
    private double _scrollOffsetStorage;
    
    [ObservableProperty]
    private ObservableCollection<string>? _filterNames;

    [ObservableProperty]
    private double _entireBlockHeight;

    [ObservableProperty]
    private double _runnerBruttoWalkSpace;

    [ObservableProperty]
    private double _scrollOffset;

    [ObservableProperty]
    private double _runnerHeight;

    [ObservableProperty]
    private double _runnerWalkSpace;

    [ObservableProperty]
    private bool _upDownIsFocusable;

    [ObservableProperty]
    private bool _filterIsShrinked;

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

    internal double RealRunnerHeight { get; private set; }

    private void CalcVisibleRange ( int countInCollection )
    {
        _visibleRange = ( int ) ( _scrollHeight / _itemHeight );
        _maxVisibleCount = _visibleRange;
        _visibleRange = Math.Min ( _visibleRange, countInCollection );
        _bottomLimit = _visibleRange - 1;
    }

    private void SetScroller ( int badgesAmount )
    {
        if ( badgesAmount == 0 )
        {
            UpDownIsFocusable = false;

            return;
        }

        if ( badgesAmount < _scrollHeight / _itemHeight )
        {
            return;
        }

        UpDownIsFocusable = true;
        CalcRunner ( badgesAmount );
    }

    private void CalcRunner ( int badgesAmount )
    {
        if ( badgesAmount > 0 && badgesAmount > _maxVisibleCount )
        {
            CalcRunnerHeightAndStep ( badgesAmount );
            ScrollOffset = 0;
        }
    }

    private void CalcRunnerHeightAndStep ( int badgesAmount )
    {
        RunnerBruttoWalkSpace = _scrollerHeight - ( _upDownButtonMeasure * 2 );
        double proportion = _itemHeight * badgesAmount / RunnerBruttoWalkSpace;
        RunnerHeight = _scrollHeight / proportion;
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

        if ( firstIcon != null ) 
        {
            iconsCopy.Add ( firstIcon );
        }
        
        for ( ; index < ( _visibleRange - 1 ); index++ )
        {
            iconsCopy.Add ( Icons [index] );
        }

        Icons = iconsCopy;
        _runnerWalked = _scrollStepIndex * _runnerStep;
        ScrollOffset = Math.Round ( _runnerWalked );
    }

    internal void ScrollDown ()
    {
        ScrollDownAtOneStep ();
        ShowActiveIconIfInRange ();
    }

    private void ScrollDownAtOneStep ()
    {
        int currentAmount = CurrentCollection.Count;

        if ( _scrollStepIndex == currentAmount - _visibleRange )
        {
            return;
        }

        _scrollStepIndex++;
        ObservableCollection<BadgeCorrectnessViewModel> iconsCopy = [];
        int index = 1;

        for ( ; index < _visibleRange; index++ )
        {
            iconsCopy.Add ( Icons [index] );
        }

        int badgeNumberInCurrentList = _scrollStepIndex + index - 1;
        BadgeCorrectnessViewModel? lastIcon = GetCorrespondingIcon ( badgeNumberInCurrentList );

        if ( lastIcon != null ) 
        {
            iconsCopy.Add ( lastIcon );
        }

        Icons = iconsCopy;
        _runnerWalked = _scrollStepIndex * _runnerStep;
        ScrollOffset = _runnerWalked;
    }

    internal void MoveRunner ( double runnerStep )
    {
        double usefullWay = ( _itemHeight * CurrentCollection.Count ) - RunnerBruttoWalkSpace;
        double proportion = usefullWay / ( RunnerBruttoWalkSpace - RealRunnerHeight );
        double step = runnerStep * proportion;
        int steps = ( int ) ( Math.Round ( step / _itemHeight ) );

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
        if ( CurrentCollection == null || CurrentCollection.Count == 0 )
        {
            return null;
        }

        BadgeViewModel? goalBadge = null;
        bool indexIsWithin = ( CurrentCollection.Count > badgeIndex ) && ( badgeIndex >= 0 );

        if ( indexIsWithin )
        {
            goalBadge = CurrentCollection.ElementAt ( badgeIndex );
        }

        if ( goalBadge != null )
        {
            return new BadgeCorrectnessViewModel ( goalBadge, FilterIsExtended );
        }

        return null;
    }

    internal void ShiftRunner ( double dastinationPointer )
    {
        if ( CurrentCollection == null || CurrentCollection.Count == 0 )
        {
            return;
        }

        bool dastinationIsOnRunner = ( dastinationPointer >= _runnerWalked ) && ( dastinationPointer <= ( _runnerWalked + RunnerHeight ) );

        if ( dastinationIsOnRunner )
        {
            return;
        }

        _scrollStepIndex = ( int ) ( dastinationPointer / _runnerStep );
        double wayMustWalk = _scrollStepIndex * _runnerStep;

        if ( _scrollStepIndex >= ( CurrentCollection.Count - _visibleRange ) )
        {
            wayMustWalk = RunnerBruttoWalkSpace - RunnerHeight;
            _scrollStepIndex = CurrentCollection.Count - _visibleRange;
        }

        Icons = [];

        for ( int index = 0; index < _visibleRange; index++ )
        {
            BadgeCorrectnessViewModel? icon = GetCorrespondingIcon ( _scrollStepIndex + index );

            if ( icon != null ) 
            {
                Icons.Add ( icon );
            }
        }

        _runnerWalked = wayMustWalk;
        ScrollOffset = wayMustWalk;
        ShowActiveIconIfInRange ();
    }

    internal void ScrollByWheel ( bool isDirectionUp )
    {
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
        bool activeIconIsVisible = ( CurrentNumber > _scrollStepIndex ) && ( CurrentNumber < ( _scrollStepIndex + _visibleRange + 1 ) );

        if ( activeIconIsVisible )
        {
            _numberAmongIcons = CurrentNumber - _scrollStepIndex;
            _bottomLimit = _scrollStepIndex + _visibleRange - 1;
            SaveState ();
            ActiveIcon = Icons [CurrentNumber - _scrollStepIndex - 1];
            HighLightChosenIcon ( ActiveIcon );
        }
    }

    private void SetPreScrollingStateIfShould ()
    {
        if ( _iconsStorage != null && !_iconsStorage.Equals ( Icons ) )
        {
            Icons = _iconsStorage;
            _scrollStepIndex = _scrollStepNumberStorage;
            _runnerWalked = _runnerWalkedStorage;
            ScrollOffset = _scrollOffsetStorage;
        }
    }

    private void SaveState ()
    {
        _scrollStepNumberStorage = _scrollStepIndex;
        _runnerWalkedStorage = _runnerWalked;
        _scrollOffsetStorage = ScrollOffset;
        _iconsStorage = Icons;
    }

    private static void HighLightChosenIcon ( BadgeCorrectnessViewModel icon )
    {
        icon.BoundFontWeight = FontWeight.Bold;
        icon.CalcStringPresentation ();
    }
}
