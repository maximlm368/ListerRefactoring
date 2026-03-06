using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.Entities.BadgeVM;

namespace Lister.Desktop.Views.EditionView.Parts.Filter.ViewModel;

internal partial class FilterViewModel : ObservableObject
{
    internal void ToFirst ()
    {
        if ( IsNotAccordingFilter () )
        {
            ReduceCurrentCollection ( CurrentNumber - 1 );

            if ( CurrentCollection.Count <= _maxVisibleCount )
            {
                _visibleRange = CurrentCollection.Count;
                _bottomLimit = CurrentCollection.Count - 1;
            }
        }
        else
        {
            SetProcessableInMatchCollection ();
        }

        Icons = [];
        _iconsStorage = Icons;
        CurrentNumber = 1;

        for ( int index = 0; index < _visibleRange; index++ )
        {
            BadgeCorrectnessViewModel icon = new ( CurrentCollection.ElementAt ( index ), FilterIsExtended );
            icon.CalcStringPresentation ();
            Icons.Add ( icon );
        }

        _numberAmongIcons = 1;
        ScrollOffset = 0;
        _scrollStepIndex = 0;
        _runnerWalked = 0;
        _bottomLimit = _visibleRange - 1;
        CalcRunnerHeightAndStep ( CurrentCollection.Count );
        ReplaceActiveIcon ( Icons [0] );
        EnableNavigation ();
        SetScrollerItemsCorrectWidth ( CurrentCollection.Count );
        SaveState ();

        WentToOther?.Invoke ( ActiveIcon?.BoundBadge, CurrentNumber, CurrentCollection.Count );
    }

    [RelayCommand]
    internal void ToPrevious ()
    {
        if ( CurrentNumber <= 1 )
        {
            return;
        }

        SetPreScrollingStateIfShould ();

        int possableRemovableNumber = CurrentNumber;
        int possableRemovableAmongVisibleNumber = _numberAmongIcons;
        bool sliderIsScrollable = CurrentCollection.Count > _maxVisibleCount;
        CurrentNumber--;
        _numberAmongIcons--;

        if ( IsNotAccordingFilter () )
        {
            bool endIsNotAchieved = _bottomLimit < ( CurrentCollection.Count - 1 );
            bool shiftToSideLast = endIsNotAchieved || !sliderIsScrollable;

            ReduceCurrentCollectionAndIcons
            ( possableRemovableNumber - 1, possableRemovableAmongVisibleNumber - 1, shiftToSideLast, true, sliderIsScrollable );

            CalcRunnerHeightAndStep ( CurrentCollection.Count );

            if ( !endIsNotAchieved && sliderIsScrollable )
            {
                _numberAmongIcons++;
                _bottomLimit--;
                _scrollStepIndex--;

                _runnerWalked = _scrollStepIndex * _runnerStep;
                ScrollOffset = _runnerWalked;
            }
        }
        else
        {
            SetProcessableInMatchCollection ();
        }

        if ( CurrentNumber < ( _bottomLimit - _visibleRange + 2 ) )
        {
            ScrollUpAtOneStep ();
            SaveState ();
            _numberAmongIcons = 1;
            _bottomLimit--;
        }

        BadgeCorrectnessViewModel newActiveIcon = Icons [_numberAmongIcons - 1];
        ReplaceActiveIcon ( newActiveIcon );
        EnableNavigation ();
        SetScrollerItemsCorrectWidth ( CurrentCollection.Count );

        WentToOther?.Invoke ( ActiveIcon?.BoundBadge, CurrentNumber, CurrentCollection.Count );
    }

    [RelayCommand]
    internal void ToNext ()
    {
        if ( CurrentNumber >= ProcessableCount )
        {
            return;
        }

        SetPreScrollingStateIfShould ();

        bool sliderIsScrollable = CurrentCollection.Count > _maxVisibleCount;
        CurrentNumber++;
        _numberAmongIcons++;

        if ( IsNotAccordingFilter () )
        {
            CurrentNumber--;
            _numberAmongIcons--;

            bool bottomIsNotAchieved = _bottomLimit < ( CurrentCollection.Count - 1 );
            bool shiftToSideLast = bottomIsNotAchieved || !sliderIsScrollable;

            ReduceCurrentCollectionAndIcons
            ( CurrentNumber - 1, _numberAmongIcons - 1, shiftToSideLast, true, sliderIsScrollable );

            CalcRunnerHeightAndStep ( CurrentCollection.Count );

            if ( !bottomIsNotAchieved && sliderIsScrollable )
            {
                _numberAmongIcons++;
                _bottomLimit--;
                _scrollStepIndex--;
                _runnerWalked = _scrollStepIndex * _runnerStep;
                ScrollOffset = _runnerWalked;
            }
        }
        else
        {
            SetProcessableInMatchCollection ();
        }

        if ( CurrentNumber > ( _bottomLimit + 1 ) )
        {
            ScrollDownAtOneStep ();
            SaveState ();
            _numberAmongIcons = Icons.Count;
            _bottomLimit++;
        }

        BadgeCorrectnessViewModel newActiveIcon = Icons [_numberAmongIcons - 1];
        ReplaceActiveIcon ( newActiveIcon );
        EnableNavigation ();
        SetScrollerItemsCorrectWidth ( CurrentCollection.Count );

        WentToOther?.Invoke ( ActiveIcon?.BoundBadge, CurrentNumber, CurrentCollection.Count );
    }

    internal void ToLast ()
    {
        Icons = [];
        _iconsStorage = Icons;

        if ( IsNotAccordingFilter () )
        {
            ReduceCurrentCollection ( CurrentNumber - 1 );

            if ( CurrentCollection.Count <= _maxVisibleCount )
            {
                _visibleRange = CurrentCollection.Count;
                _bottomLimit = CurrentCollection.Count - 1;
            }
        }
        else
        {
            SetProcessableInMatchCollection ();
        }

        int visibleCountBeforeEnd = CurrentCollection.Count - _visibleRange;

        for ( int index = visibleCountBeforeEnd; index < CurrentCollection.Count; index++ )
        {
            BadgeCorrectnessViewModel icon = new ( CurrentCollection.ElementAt ( index ), FilterIsExtended );
            icon.CalcStringPresentation ();
            Icons.Add ( icon );
        }

        _numberAmongIcons = Icons.Count;
        ReplaceActiveIcon ( Icons [_numberAmongIcons - 1] );
        CurrentNumber = CurrentCollection.Count;
        bool sliderIsScrollable = ( CurrentCollection.Count > _maxVisibleCount );
        _bottomLimit = CurrentCollection.Count - 1;
        CalcRunnerHeightAndStep ( CurrentCollection.Count );

        if ( !sliderIsScrollable )
        {
            _visibleRange = CurrentCollection.Count;
            _scrollStepIndex = 0;
            _runnerWalked = 0;
        }
        else
        {
            _scrollStepIndex = CurrentCollection.Count - _visibleRange;
            _runnerWalked = _scrollStepIndex * _runnerStep;
            ScrollOffset = _runnerWalked;
        }

        EnableNavigation ();
        SetScrollerItemsCorrectWidth ( CurrentCollection.Count );
        SaveState ();

        WentToOther?.Invoke ( ActiveIcon?.BoundBadge, CurrentNumber, CurrentCollection.Count );
    }

    internal void ToParticularBadge ( BadgeCorrectnessViewModel destinationIcon )
    {
        BadgeViewModel destinationBadge = destinationIcon.BoundBadge;
        string goalNumber = string.Empty;

        for ( int index = 0; index < CurrentCollection.Count; index++ )
        {
            if ( destinationBadge.Equals ( CurrentCollection.ElementAt ( index ) ) )
            {
                goalNumber = ( index + 1 ).ToString ();

                break;
            }
        }

        ToParticularBadge ( goalNumber, destinationIcon );
    }

    private void ToParticularBadge ( string goalNumber, BadgeCorrectnessViewModel newActiveIcon )
    {
        int currentNumber = CurrentNumber;

        try
        {
            bool isInt = int.TryParse ( goalNumber, out int destinationNumber );

            if ( !isInt )
            {
                return;
            }

            int oldNumber = CurrentNumber;
            int oldNumberAmongVisibleIcons = oldNumber - _scrollStepIndex;
            CurrentNumber = destinationNumber;
            int amongVisibleIconsDestinationNum = destinationNumber - _scrollStepIndex;
            _numberAmongIcons = CurrentNumber - _scrollStepIndex;
            bool sliderIsScrollable = ( CurrentCollection.Count > _visibleRange );
            _bottomLimit = _scrollStepIndex + _visibleRange - 1;

            if ( IsNotAccordingFilter () )
            {
                bool endIsNotAchieved = _bottomLimit < ( CurrentCollection.Count - 1 );
                bool exProcessableIsNotInVisibleRange = ( oldNumber > ( _bottomLimit + 1 ) ) ||
                    ( oldNumber < ( _bottomLimit - _visibleRange + 2 ) );

                if ( endIsNotAchieved )
                {
                    ReduceCurrentCollectionAndIcons ( oldNumber - 1, oldNumberAmongVisibleIcons - 1, true,
                        !exProcessableIsNotInVisibleRange, sliderIsScrollable );

                    newActiveIcon = Icons [amongVisibleIconsDestinationNum - 1];

                    if ( oldNumber < CurrentNumber )
                    {
                        CurrentNumber--;

                        if ( exProcessableIsNotInVisibleRange )
                        {
                            newActiveIcon = Icons [amongVisibleIconsDestinationNum - 1];
                            _bottomLimit--;
                            _scrollStepIndex--;
                        }
                        else
                        {
                            _numberAmongIcons--;
                            newActiveIcon = Icons [amongVisibleIconsDestinationNum - 2];
                        }
                    }
                    else
                    {
                        newActiveIcon = Icons [amongVisibleIconsDestinationNum - 1];
                    }
                }
                else
                {
                    if ( CurrentCollection.Count <= _maxVisibleCount )
                    {
                        ReduceCurrentCollectionAndIcons ( oldNumber - 1, oldNumberAmongVisibleIcons - 1, true, true, sliderIsScrollable );

                        if ( oldNumber < CurrentNumber )
                        {
                            amongVisibleIconsDestinationNum--;
                            _numberAmongIcons--;
                            CurrentNumber--;
                        }

                        newActiveIcon = Icons [amongVisibleIconsDestinationNum - 1];
                    }
                    else
                    {
                        ReduceCurrentCollectionAndIcons ( oldNumber - 1, oldNumberAmongVisibleIcons - 1, false, true, sliderIsScrollable );

                        if ( oldNumber < CurrentNumber )
                        {
                            newActiveIcon = Icons [amongVisibleIconsDestinationNum - 1];
                            CurrentNumber--;
                        }
                        else if ( oldNumber > CurrentNumber )
                        {
                            newActiveIcon = Icons [amongVisibleIconsDestinationNum];
                            _numberAmongIcons++;
                            CurrentNumber++;
                            CurrentNumber--;
                        }

                        _scrollStepIndex--;
                        _bottomLimit--;
                    }
                }

                CalcRunnerHeightAndStep ( CurrentCollection.Count );

                if ( !endIsNotAchieved && sliderIsScrollable )
                {
                    _runnerWalked = _scrollStepIndex * _runnerStep;
                    ScrollOffset = _runnerWalked;
                }
            }
            else
            {
                SetProcessableInMatchCollection ();
            }

            ReplaceActiveIcon ( newActiveIcon );
            SaveState ();
            EnableNavigation ();
            SetScrollerItemsCorrectWidth ( CurrentCollection.Count );
            SaveState ();

            WentToOther?.Invoke ( ActiveIcon?.BoundBadge, CurrentNumber, CurrentCollection.Count );
        }
        catch ( Exception )
        {
            CurrentNumber = currentNumber;
        }
    }

    private bool IsNotAccordingFilter ()
    {
        if ( ActiveIcon == null )
        {
            return false;
        }

        bool filterOccured = false;

        if ( State == FilterState.All )
        {
            return false;
        }
        else if ( State == FilterState.Corrects )
        {
            if ( ActiveIcon.BoundBadge != null && !ActiveIcon.BoundBadge.IsCorrect )
            {
                filterOccured = true;
            }
        }
        else if ( State == FilterState.Incorrects )
        {
            if ( ActiveIcon.BoundBadge != null && ActiveIcon.BoundBadge.IsCorrect )
            {
                filterOccured = true;
            }
        }

        return filterOccured;
    }

    private void SetScrollerItemsCorrectWidth ( int itemsCount )
    {
        if ( itemsCount <= _scrollerHeight / _itemHeight )
        {
            ScrollerHided?.Invoke ();
        }
        else
        {
            ScrollerShowed?.Invoke ();
        }
    }

    private void ReplaceActiveIcon ( BadgeCorrectnessViewModel newActiveIcon )
    {
        FadeIcon ( ActiveIcon );
        ActiveIcon = newActiveIcon;
        HighLightChosenIcon ( ActiveIcon );
    }

    private static void FadeIcon ( BadgeCorrectnessViewModel? icon )
    {
        if ( icon == null )
        {
            return;
        }

        icon.BoundFontWeight = FontWeight.Normal;
        icon.CalcStringPresentation ();
    }

    private void EnableNavigation ()
    {
        int badgeCount = CurrentCollection.Count;

        if ( ( CurrentNumber > 1 ) && ( CurrentNumber == badgeCount ) )
        {
            IsPreviousEnable = true;
            IsNextEnable = false;
        }
        else if ( ( CurrentNumber > 1 ) && ( CurrentNumber < badgeCount ) )
        {
            IsPreviousEnable = true;
            IsNextEnable = true;
        }
        else if ( ( CurrentNumber == 1 ) && ( badgeCount == 1 ) )
        {
            IsPreviousEnable = false;
            IsNextEnable = false;
        }
        else if ( ( CurrentNumber == 1 ) && ( badgeCount > 1 ) )
        {
            IsPreviousEnable = false;
            IsNextEnable = true;
        }
    }

    private void ReduceCurrentCollectionAndIcons ( int removableIndex, int removableIndexAmongVisibleIcons
                                                 , bool shiftToSideLast, bool shouldReduceIcons, bool shouldAddOne )
    {
        int indexInEntireList = removableIndex;
        int indexInVisibleRange = removableIndexAmongVisibleIcons;
        _visibleRange = Math.Min ( _visibleRange, Icons.Count );

        if ( shouldReduceIcons && ( CurrentCollection.Count < _maxVisibleCount ) )
        {
            _bottomLimit--;
            _visibleRange--;
        }

        if ( shouldReduceIcons && shiftToSideLast )
        {
            if ( !shouldAddOne )
            {
                Icons.RemoveAt ( Icons.Count - 1 );
            }

            for ( ; indexInVisibleRange < _visibleRange; indexInVisibleRange++, indexInEntireList++ )
            {
                BadgeCorrectnessViewModel? correspondingIcon = GetCorrespondingIcon ( indexInEntireList + 1 );

                if ( correspondingIcon == null )
                {
                    break;
                }

                Icons [indexInVisibleRange] = correspondingIcon;
            }
        }
        else if ( shouldReduceIcons && !shiftToSideLast )
        {
            for ( ; indexInVisibleRange >= 0; indexInVisibleRange--, indexInEntireList-- )
            {
                BadgeCorrectnessViewModel? correspondingIcon = GetCorrespondingIcon ( indexInEntireList - 1 );
                bool zeroIsAchieved = correspondingIcon == null;

                if ( zeroIsAchieved || correspondingIcon == null )
                {
                    break;
                }

                Icons [indexInVisibleRange] = correspondingIcon;
            }
        }

        ReduceCurrentCollection ( removableIndex );
    }

    private void ReduceCurrentCollection ( int removableIndex )
    {
        if ( State == FilterState.Corrects )
        {
            BadgeViewModel badge = Corrects.ElementAt ( removableIndex );
            Incorrects.Add ( badge );
            Corrects.Remove ( badge );
            Incorrects.Sort ( _comparer );
        }
        else if ( State == FilterState.Incorrects )
        {
            BadgeViewModel badge = Incorrects.ElementAt ( removableIndex );
            Corrects.Add ( badge );
            Incorrects.Remove ( badge );
            Corrects.Sort ( _comparer );
        }

        if ( State != FilterState.All )
        {
            ProcessableCount--;
        }
    }
}
