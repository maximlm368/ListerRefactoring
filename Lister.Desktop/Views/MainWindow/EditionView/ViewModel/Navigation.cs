using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

internal partial class BadgeEditorViewModel : ObservableObject
{
    private int _numberAmongVisibleIcons = 1;

    [ObservableProperty]
    private bool _firstIsEnable;

    private bool _previousIsEnable;
    internal bool PreviousIsEnable
    {
        get
        {
            return _previousIsEnable;
        }

        private set
        {
            _previousIsEnable = value;
            OnPropertyChanged ();
            PreviousOnSliderIsEnable = value;
        }
    }

    private bool _nextIsEnable;
    internal bool NextIsEnable
    {
        get
        {
            return _nextIsEnable;
        }

        private set
        {
            _nextIsEnable = value;
            OnPropertyChanged ();
            NextOnSliderIsEnable = value;
        }
    }

    [ObservableProperty]
    private bool _lastIsEnable;

    private int _beingProcessedNumber;
    internal int BeingProcessedNumber
    {
        get
        {
            return _beingProcessedNumber;
        }

        private set
        {
            if ( value < 1 )
            {
                BeingProcessedNumberText = string.Empty;
            }
            else
            {
                BeingProcessedNumberText = value.ToString ();
            }

            _beingProcessedNumber = value;
            OnPropertyChanged ();
        }
    }

    [ObservableProperty]
    private string? _beingProcessedNumberText;

    internal void ToFirst ()
    {
        ResetIncorrectCountValue ();

        if ( IsProcessableChangedInAppropriateFilter ( BeingProcessedNumber ) )
        {
            ReduceCurrentCollection ( BeingProcessedNumber - 1 );

            if ( CurrentVisibleCollection.Count <= _maxVisibleCount )
            {
                _visibleRange = CurrentVisibleCollection.Count;
                _visibleRangeEnd = CurrentVisibleCollection.Count - 1;
                ScrollWidth = 0;
            }
        }

        VisibleIcons = [];
        _visibleIconsStorage = VisibleIcons;
        BeingProcessedNumber = 1;

        for ( int index = 0; index < _visibleRange; index++ )
        {
            VisibleIcons.Add (
                new BadgeCorrectnessViewModel ( CurrentVisibleCollection.ElementAt ( index ), _extendedScrollableIconWidth, _shrinkedIconWidth,
                    _correctnessWidthLimit, FilterIsExtended
                )
            );

            if ( index == 0 )
            {
                ProcessableBadge?.Hide ();
                ProcessableBadge = GetAppropriateDraft ( BeingProcessedNumber );
            }
        }

        _numberAmongVisibleIcons = 1;
        SetToCorrectScale ( ProcessableBadge );
        ProcessableBadge?.Show ();
        ScrollOffset = 0;
        _scrollStepIndex = 0;
        _runnerHasWalked = 0;
        _visibleRangeEnd = _visibleRange - 1;
        CalcRunnerHeightAndStep ( CurrentVisibleCollection.Count );
        ReplaceActiveIcon ( VisibleIcons [0] );
        ResetManagingControlsEnablings ();
        SetScrollerItemsCorrectWidth ( CurrentVisibleCollection.Count );
    }


    internal void ToPrevious ()
    {
        if ( ( BeingProcessedNumber <= 1 ) || ( IsDropDownOpen ) )
        {
            return;
        }

        ResetIncorrectCountValue ();
        int possableRemovableNumber = BeingProcessedNumber;
        int possableRemovableAmongVisibleNumber = _numberAmongVisibleIcons;
        SetScrollerToStateBeforeScrollingIfShould ();
        int absoluteNumber = BeingProcessedNumber + 1;
        bool isProcessableChangedInSpecificFilter = IsProcessableChangedInAppropriateFilter ( BeingProcessedNumber );
        bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );
        BeingProcessedNumber--;
        _numberAmongVisibleIcons--;

        if ( isProcessableChangedInSpecificFilter )
        {
            bool endIsNotAchieved = ( _visibleRangeEnd < ( CurrentVisibleCollection.Count - 1 ) );
            bool shiftToSideLast = ( endIsNotAchieved || !sliderIsScrollable );

            ReduceCurrentCollectionAndIcons
            ( possableRemovableNumber - 1, possableRemovableAmongVisibleNumber - 1, shiftToSideLast, true, sliderIsScrollable );

            CalcRunnerHeightAndStep ( CurrentVisibleCollection.Count );

            if ( !endIsNotAchieved && sliderIsScrollable )
            {
                _numberAmongVisibleIcons++;
                _visibleRangeEnd--;
                _scrollStepIndex--;

                _runnerHasWalked = _scrollStepIndex * _runnerStep;
                ScrollOffset = _runnerHasWalked;
            }
        }

        if ( BeingProcessedNumber < ( _visibleRangeEnd - _visibleRange + 2 ) )
        {
            ScrollUpAtOneStep ();
            SaveScrollerState ();
            _numberAmongVisibleIcons = 1;
            _doubleRest = 0;
            _visibleRangeEnd--;
        }

        ProcessableBadge.Hide ();
        ProcessableBadge = GetAppropriateDraft ( BeingProcessedNumber );
        SetToCorrectScale ( ProcessableBadge );
        ProcessableBadge?.Show ();
        BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongVisibleIcons - 1];
        ReplaceActiveIcon ( newActiveIcon );
        sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

        if ( !sliderIsScrollable )
        {
            ScrollWidth = 0;
        }

        ResetManagingControlsEnablings ();
        SetScrollerItemsCorrectWidth ( CurrentVisibleCollection.Count );
    }


    internal void ToNext ()
    {
        if ( ( BeingProcessedNumber >= ProcessableCount ) || ( IsDropDownOpen ) )
        {
            return;
        }

        ResetIncorrectCountValue ();
        SetScrollerToStateBeforeScrollingIfShould ();
        bool isProcessableChangedInSpecificFilter = IsProcessableChangedInAppropriateFilter ( BeingProcessedNumber );
        bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );
        BeingProcessedNumber++;
        _numberAmongVisibleIcons++;

        if ( isProcessableChangedInSpecificFilter )
        {
            BeingProcessedNumber--;
            _numberAmongVisibleIcons--;

            bool endIsNotAchieved = ( _visibleRangeEnd < ( CurrentVisibleCollection.Count - 1 ) );
            bool shiftToSideLast = ( endIsNotAchieved || !sliderIsScrollable );

            ReduceCurrentCollectionAndIcons
            ( ( BeingProcessedNumber - 1 ), ( _numberAmongVisibleIcons - 1 ), shiftToSideLast, true, sliderIsScrollable );

            CalcRunnerHeightAndStep ( CurrentVisibleCollection.Count );

            if ( !endIsNotAchieved && sliderIsScrollable )
            {
                _numberAmongVisibleIcons++;
                _visibleRangeEnd--;
                _scrollStepIndex--;
                _runnerHasWalked = _scrollStepIndex * _runnerStep;
                ScrollOffset = _runnerHasWalked;
            }
        }

        if ( BeingProcessedNumber > ( _visibleRangeEnd + 1 ) )
        {
            ScrollDownAtOneStep ();
            SaveScrollerState ();
            _numberAmongVisibleIcons = VisibleIcons.Count;
            _doubleRest = 0;
            _visibleRangeEnd++;
        }

        ProcessableBadge.Hide ();
        ProcessableBadge = GetAppropriateDraft ( BeingProcessedNumber );
        ProcessableBadge?.Show ();
        SetToCorrectScale ( ProcessableBadge );
        BadgeCorrectnessViewModel newActiveIcon = VisibleIcons [_numberAmongVisibleIcons - 1];
        ReplaceActiveIcon ( newActiveIcon );
        sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

        if ( !sliderIsScrollable )
        {
            ScrollWidth = 0;
        }

        ResetManagingControlsEnablings ();
        SetScrollerItemsCorrectWidth ( CurrentVisibleCollection.Count );
    }


    internal void ToLast ()
    {
        ResetIncorrectCountValue ();
        BadgeViewModel newProcessable = null;
        int oldNumber = BeingProcessedNumber;
        VisibleIcons = [];
        _visibleIconsStorage = VisibleIcons;
        bool isProcessableChangedInSpecificFilter = IsProcessableChangedInAppropriateFilter ( BeingProcessedNumber );

        if ( isProcessableChangedInSpecificFilter )
        {
            ReduceCurrentCollection ( BeingProcessedNumber - 1 );

            if ( CurrentVisibleCollection.Count <= _maxVisibleCount )
            {
                _visibleRange = CurrentVisibleCollection.Count;
                _visibleRangeEnd = CurrentVisibleCollection.Count - 1;
                ScrollWidth = 0;
            }
        }

        int visibleCountBeforeEnd = CurrentVisibleCollection.Count - _visibleRange;

        for ( int index = visibleCountBeforeEnd; index < CurrentVisibleCollection.Count; index++ )
        {
            BadgeViewModel boundBadge = CurrentVisibleCollection.ElementAt ( index );
            VisibleIcons.Add ( new BadgeCorrectnessViewModel ( boundBadge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                                 , _correctnessWidthLimit, FilterIsExtended ) );
        }

        _numberAmongVisibleIcons = VisibleIcons.Count;
        ReplaceActiveIcon ( VisibleIcons [_numberAmongVisibleIcons - 1] );
        BeingProcessedNumber = CurrentVisibleCollection.Count;
        BeingProcessedNumber = GetAppropriateLastNumber ();
        ProcessableBadge.Hide ();
        ProcessableBadge = GetAppropriateDraft ( BeingProcessedNumber );
        ProcessableBadge?.Show ();
        SetToCorrectScale ( ProcessableBadge );
        bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );
        _visibleRangeEnd = CurrentVisibleCollection.Count - 1;
        CalcRunnerHeightAndStep ( CurrentVisibleCollection.Count );

        if ( !sliderIsScrollable )
        {
            ScrollWidth = 0;
            _visibleRange = CurrentVisibleCollection.Count;
            _scrollStepIndex = 0;
            _runnerHasWalked = 0;
        }
        else
        {
            _scrollStepIndex = CurrentVisibleCollection.Count - _visibleRange;
            _runnerHasWalked = _scrollStepIndex * _runnerStep;
            ScrollOffset = _runnerHasWalked;
        }

        ResetManagingControlsEnablings ();
        SetScrollerItemsCorrectWidth ( CurrentVisibleCollection.Count );
    }


    internal void ToParticularBadge ( BadgeCorrectnessViewModel destinationIcon )
    {
        BadgeViewModel destinationBadge = destinationIcon.BoundBadge;

        if ( destinationBadge.Equals ( ProcessableBadge ) )
        {
            return;
        }

        string destinationNumberText = string.Empty;

        for ( int index = 0; index < CurrentVisibleCollection.Count; index++ )
        {
            if ( destinationBadge.Equals ( CurrentVisibleCollection.ElementAt ( index ) ) )
            {
                destinationNumberText = ( index + 1 ).ToString ();
                break;
            }
        }

        ToParticularBadge ( destinationNumberText, destinationIcon );
    }


    private void ToParticularBadge ( string destinationNumberAsText, BadgeCorrectnessViewModel newActiveIcon )
    {
        try
        {
            ResetIncorrectCountValue ();
            int destinationNumber = int.Parse ( destinationNumberAsText );
            bool isProcessableChangedInSpecificFilter = IsProcessableChangedInAppropriateFilter ( BeingProcessedNumber );
            int oldNumber = BeingProcessedNumber;
            int oldNumberAmongVisibleIcons = oldNumber - _scrollStepIndex;
            BeingProcessedNumber = destinationNumber;
            int amongVisibleIconsDestinationNum = destinationNumber - _scrollStepIndex;
            _numberAmongVisibleIcons = BeingProcessedNumber - _scrollStepIndex;
            ProcessableBadge.Hide ();
            ProcessableBadge = GetAppropriateDraft ( BeingProcessedNumber );
            bool sliderIsScrollable = ( CurrentVisibleCollection.Count > _visibleRange );
            _visibleRangeEnd = _scrollStepIndex + _visibleRange - 1;

            if ( isProcessableChangedInSpecificFilter )
            {
                bool endIsNotAchieved = ( _visibleRangeEnd < ( CurrentVisibleCollection.Count - 1 ) );
                bool exProcessableIsNotInVisibleRange = ( oldNumber > ( _visibleRangeEnd + 1 ) )
                                                        || ( oldNumber < ( _visibleRangeEnd - _visibleRange + 2 ) );

                if ( endIsNotAchieved )
                {
                    ReduceCurrentCollectionAndIcons ( ( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 )
                                                     , true, !exProcessableIsNotInVisibleRange, sliderIsScrollable );

                    newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];

                    if ( oldNumber < BeingProcessedNumber )
                    {
                        BeingProcessedNumber--;

                        if ( exProcessableIsNotInVisibleRange )
                        {
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                            _visibleRangeEnd--;
                            _scrollStepIndex--;
                        }
                        else
                        {
                            _numberAmongVisibleIcons--;
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 2];
                        }
                    }
                    else
                    {
                        newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                    }
                }
                else
                {
                    if ( CurrentVisibleCollection.Count <= _maxVisibleCount )
                    {
                        ReduceCurrentCollectionAndIcons
                        ( ( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ), true, true, sliderIsScrollable );

                        if ( oldNumber < BeingProcessedNumber )
                        {
                            amongVisibleIconsDestinationNum--;
                            _numberAmongVisibleIcons--;
                            BeingProcessedNumber--;
                        }

                        newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                    }
                    else
                    {
                        ReduceCurrentCollectionAndIcons
                        ( ( oldNumber - 1 ), ( oldNumberAmongVisibleIcons - 1 ), false, true, sliderIsScrollable );

                        if ( oldNumber < BeingProcessedNumber )
                        {
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum - 1];
                            BeingProcessedNumber--;
                        }
                        else if ( oldNumber > BeingProcessedNumber )
                        {
                            newActiveIcon = VisibleIcons [amongVisibleIconsDestinationNum];
                            _numberAmongVisibleIcons++;
                            BeingProcessedNumber++;
                            BeingProcessedNumber--;
                        }

                        _scrollStepIndex--;
                        _visibleRangeEnd--;
                    }
                }

                CalcRunnerHeightAndStep ( CurrentVisibleCollection.Count );

                if ( !endIsNotAchieved && sliderIsScrollable )
                {
                    _runnerHasWalked = _scrollStepIndex * _runnerStep;
                    ScrollOffset = _runnerHasWalked;
                }
            }

            ReplaceActiveIcon ( newActiveIcon );
            SaveScrollerState ();
            SetToCorrectScale ( ProcessableBadge );
            ProcessableBadge.Show ();
            sliderIsScrollable = ( CurrentVisibleCollection.Count > _maxVisibleCount );

            if ( !sliderIsScrollable )
            {
                ScrollWidth = 0;
            }

            ResetManagingControlsEnablings ();
            SetScrollerItemsCorrectWidth ( CurrentVisibleCollection.Count );
        }
        catch ( Exception ex )
        {
            BeingProcessedNumber = BeingProcessedNumber;
        }
    }


    private void ResetIncorrectCountValue ()
    {
        if ( ( _filterState == FilterChoosing.Corrects ) && ( !ProcessableBadge.IsCorrect ) )
        {
            IncorrectBadgesCount = 0;
        }
    }


    private void ResetManagingControlsEnablings ()
    {
        EnableNavigationIfShould ();
        MoversAreEnable = false;
        SplitterIsEnable = false;
        ZoommerIsEnable = false;
        ReleaseCaptured ();
        TryEnableScroller ( _visibleRange );
    }


    private void SetScrollerItemsCorrectWidth ( int enablingRange )
    {
        double scrollerItemsCount = _scrollerHeight / _itemHeightWithMargin;

        if ( enablingRange <= scrollerItemsCount )
        {
            if ( FilterIsExtended )
            {
                SliderCollectionWidth = _mostExtendedIconWidth;

                foreach ( BadgeCorrectnessViewModel item in VisibleIcons )
                {
                    item.Width = _mostExtendedIconWidth;
                }
            }
        }
        else
        {
            if ( FilterIsExtended )
            {
                SliderCollectionWidth = _extendedScrollableIconWidth;

                foreach ( BadgeCorrectnessViewModel item in VisibleIcons )
                {
                    item.Width = _extendedScrollableIconWidth;
                }
            }
        }
    }


    private void ReplaceActiveIcon ( BadgeCorrectnessViewModel newActiveIcon )
    {
        FadeIcon ( ActiveIcon );
        ActiveIcon = newActiveIcon;
        HighLightChosenIcon ( ActiveIcon );
    }


    private BadgeViewModel? GetAppropriateDraft ( int number )
    {
        BadgeViewModel? goalBadge = null;
        goalBadge = CurrentVisibleCollection?.ElementAt ( number - 1 );

        return goalBadge;
    }


    private int GetAppropriateLastNumber ()
    {
        int result = 0;
        result = CurrentVisibleCollection.Count;

        return result;
    }


    private void EnableNavigationIfShould ()
    {
        int badgeCount = CurrentVisibleCollection.Count;

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

        if ( CurrentVisibleCollection.Count < 1 ) PeopleGotEmpty?.Invoke ();
    }


    private void ReduceCurrentCollectionAndIcons ( int removableIndex, int removableIndexAmongVisibleIcons
                                                 , bool shiftToSideLast, bool shouldReduceIcons, bool shouldAddOne )
    {
        int indexInEntireList = removableIndex;
        int indexInVisibleRange = removableIndexAmongVisibleIcons;
        _visibleRange = Math.Min ( _visibleRange, VisibleIcons.Count );

        if ( shouldReduceIcons && ( CurrentVisibleCollection.Count < _maxVisibleCount ) )
        {
            _visibleRangeEnd--;
            _visibleRange--;
        }

        if ( shouldReduceIcons && shiftToSideLast )
        {
            if ( !shouldAddOne )
            {
                VisibleIcons.RemoveAt ( VisibleIcons.Count - 1 );
            }

            for ( ; indexInVisibleRange < _visibleRange; indexInVisibleRange++, indexInEntireList++ )
            {
                BadgeCorrectnessViewModel correspondingIcon = GetCorrespondingIcon ( indexInEntireList + 1 );

                if ( correspondingIcon == null )
                {
                    break;
                }

                VisibleIcons [indexInVisibleRange] = correspondingIcon;
            }
        }
        else if ( shouldReduceIcons && !shiftToSideLast )
        {
            for ( ; indexInVisibleRange >= 0; indexInVisibleRange--, indexInEntireList-- )
            {
                BadgeCorrectnessViewModel correspondingIcon = GetCorrespondingIcon ( indexInEntireList - 1 );
                bool zeroIsAchieved = ( correspondingIcon == null );

                if ( zeroIsAchieved )
                {
                    break;
                }

                VisibleIcons [indexInVisibleRange] = correspondingIcon;
            }
        }

        ReduceCurrentCollection ( removableIndex );
    }

    private void ReduceCurrentCollection ( int removableIndex )
    {
        if ( _filterState == FilterChoosing.Corrects )
        {
            BadgeViewModel badge = CorrectNumbered.ElementAt ( removableIndex );
            IncorrectNumbered.Add ( badge );
            CorrectNumbered.Remove ( badge );
            IncorrectNumbered.Sort ( _comparer );
        }
        else if ( _filterState == FilterChoosing.Incorrects )
        {
            BadgeViewModel badge = IncorrectNumbered.ElementAt ( removableIndex );
            CorrectNumbered.Add ( badge );
            IncorrectNumbered.Remove ( badge );
            CorrectNumbered.Sort ( _comparer );
        }

        if ( _filterState != FilterChoosing.All )
        {
            ProcessableCount--;
        }
    }
}