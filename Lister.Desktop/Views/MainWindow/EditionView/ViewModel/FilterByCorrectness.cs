using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Reactive.Linq;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

internal partial class BadgeEditorViewModel : ObservableObject
{
    private readonly double _switcherWidthh = 32;
    private readonly double _filterLabelWidthh = 70;

    private readonly SolidColorBrush _switcherAllForeground = new ( new Avalonia.Media.Color ( 255, 250, 250, 250 ) );
    private readonly SolidColorBrush _switcherCorrectForeground = new ( new Avalonia.Media.Color ( 255, 97, 184, 97 ) );
    private readonly SolidColorBrush _switcherIncorrectForeground = new ( new Avalonia.Media.Color ( 255, 210, 54, 80 ) );

    private readonly string _allFilter;
    private readonly string _incorrectFilter;
    private readonly string _correctFilter;
    private readonly string _allTip;
    private readonly string _correctTip;
    private readonly string _incorrectTip;
    private readonly double _narrowCorrectnessWidthLimit = 155;
    private readonly int _narrowMinCorrectnessTextLength = 14;
    private readonly int _narrowMaxCorrectnessTextLength = 20;
    private readonly double _wideCorrectnessWidthLimit = 160;
    private readonly int _wideMinCorrectnessTextLength = 15;
    private readonly int _wideMaxCorrectnessTextLength = 21;

    private FilterChoosing _filterState = FilterChoosing.All;
    private double _correctnessWidthLimit;

    [ObservableProperty]
    private bool _isDropDownOpen;

    [ObservableProperty]
    private bool _isComboboxEnabled;

    [ObservableProperty]
    private double _switcherWidth;

    [ObservableProperty]
    private double _filterLabelWidth;

    [ObservableProperty]
    private int _filterSelectedIndex;

    [ObservableProperty]
    private string? _switcherTip;

    [ObservableProperty]
    private SolidColorBrush? _switcherForeground;

    private bool IsProcessableChangedInAppropriateFilter ( int filterableNumber )
    {
        bool filterOccured = false;

        if ( _filterState == FilterChoosing.All )
        {
            return false;
        }
        else if ( _filterState == FilterChoosing.Corrects )
        {
            if ( ProcessableBadge != null && !ProcessableBadge.IsCorrect )
            {
                filterOccured = true;
            }
        }
        else if ( _filterState == FilterChoosing.Incorrects )
        {
            if ( ProcessableBadge != null && ProcessableBadge.IsCorrect )
            {
                filterOccured = true;
            }
        }

        return filterOccured;
    }

    [RelayCommand]
    internal void Filter ()
    {
        _runnerHasWalked = 0;

        if ( _filterState == FilterChoosing.All )
        {
            _filterState = FilterChoosing.Corrects;
            SwitcherForeground = _switcherCorrectForeground;
            SwitcherTip = _correctTip;
            FilterSelectedIndex = 1;
            SetProcessableInMatchFilter ();
            ScrollWidth = 0;
            CorrectNumbered.Sort ( _comparer );
            CurrentVisibleCollection = CorrectNumbered;
            IncorrectBadgesCount = 0;
        }
        else if ( _filterState == FilterChoosing.Corrects )
        {
            _filterState = FilterChoosing.Incorrects;
            SwitcherForeground = _switcherIncorrectForeground;
            SwitcherTip = _incorrectTip;
            FilterSelectedIndex = 2;
            SetProcessableInMatchFilter ();
            IncorrectNumbered.Sort ( _comparer );
            CurrentVisibleCollection = IncorrectNumbered;
            IncorrectBadgesCount = CurrentVisibleCollection.Count;
        }
        else if ( _filterState == FilterChoosing.Incorrects )
        {
            _filterState = FilterChoosing.All;
            CurrentVisibleCollection = AllNumbered;
            SwitcherForeground = _switcherAllForeground;
            SwitcherTip = _allTip;
            FilterSelectedIndex = 0;
            SetProcessableInMatchFilter ();
            IncorrectBadgesCount = IncorrectNumbered.Count;
        }

        ProcessableCount = CurrentVisibleCollection != null ? CurrentVisibleCollection.Count : 0;
        SetSliderWideness ();
        CalcVisibleRange ( CurrentVisibleCollection != null ? CurrentVisibleCollection.Count : 0 );
        SetScroller ( CurrentVisibleCollection != null ? CurrentVisibleCollection.Count : 0 );
        SetAccordingIcons ();
        EnableNavigationIfShould ();
    }

    internal void Filter ( string? filterName )
    {
        ReleaseCaptured ();
        _runnerHasWalked = 0;

        bool appIsLoadingYet = ( AllNumbered.Count < 1 )
            || ( CorrectNumbered.Count < 1 )
            || ( IncorrectNumbered.Count < 1 );

        if ( appIsLoadingYet )
        {
            return;
        }

        if ( filterName == _allFilter )
        {
            _filterState = FilterChoosing.All;
            CurrentVisibleCollection = AllNumbered;
            SwitcherForeground = _switcherAllForeground;
            SwitcherTip = _allTip;
            SetProcessableInMatchFilter ();
            IncorrectBadgesCount = IncorrectNumbered.Count;
        }
        else if ( filterName == _correctFilter )
        {
            _filterState = FilterChoosing.Corrects;

            SwitcherForeground = _switcherCorrectForeground;
            SwitcherTip = _correctTip;
            SetProcessableInMatchFilter ();
            CorrectNumbered.Sort ( _comparer );
            CurrentVisibleCollection = CorrectNumbered;
            IncorrectBadgesCount = 0;
        }
        else if ( filterName == _incorrectFilter )
        {
            _filterState = FilterChoosing.Incorrects;

            SwitcherForeground = _switcherIncorrectForeground;
            SwitcherTip = _incorrectTip;
            SetProcessableInMatchFilter ();
            IncorrectNumbered.Sort ( _comparer );
            CurrentVisibleCollection = IncorrectNumbered;
            IncorrectBadgesCount = CurrentVisibleCollection.Count;
        }

        ProcessableCount = CurrentVisibleCollection.Count;
        SetSliderWideness ();
        CalcVisibleRange ( CurrentVisibleCollection.Count );
        SetScroller ( CurrentVisibleCollection.Count );
        SetAccordingIcons ();
        EnableNavigationIfShould ();
        ExtendOrShrinkSliderItems ();
    }

    private void SetAccordingIcons ()
    {
        ProcessableBadge = null;
        VisibleIcons = [];
        NextOnSliderIsEnable = true;
        NextIsEnable = true;
        LastIsEnable = true;

        if ( _filterState == FilterChoosing.All )
        {
            SetMixedIcons ();
        }
        else if ( _filterState == FilterChoosing.Corrects )
        {
            SetIconsForCorrectFilter ();
        }
        else if ( _filterState == FilterChoosing.Incorrects )
        {
            SetIconsForIncorrectFilter ();
        }

        _numberAmongVisibleIcons = 1;
        _scrollStepIndex = 0;

        if ( ProcessableBadge != null )
        {
            SetToCorrectScale ( ProcessableBadge );
            BeingProcessedNumber = 1;
            ProcessableBadge.Show ();
            ZeroScrollerState ( VisibleIcons );
        }
        else
        {
            BeingProcessedNumber = 0;
        }

        if ( VisibleIcons.Count == 0 )
        {
            UpDownWidth = 0;
            UpDownIsFocusable = false;
            FirstIsEnable = false;
            PreviousIsEnable = false;
            NextIsEnable = false;
            LastIsEnable = false;
        }
        else
        {
            UpDownWidth = _upDownWidthh;
            UpDownIsFocusable = true;
        }
    }

    private void SetMixedIcons ()
    {
        int counter = 0;

        foreach ( BadgeViewModel badge in AllNumbered )
        {
            if ( counter == _visibleRange )
            {
                break;
            }

            VisibleIcons.Add ( new BadgeCorrectnessViewModel ( badge, _extendedScrollableIconWidth, _shrinkedIconWidth, _correctnessWidthLimit,
                                    FilterIsExtended
                               )
            );

            counter++;
        }

        ProcessableBadge = AllNumbered.ElementAt ( 0 );
        VisibleIcons [0].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
        VisibleIcons [0].CalcStringPresentation ( _correctnessWidthLimit );
        ActiveIcon = VisibleIcons [0];
    }

    private void SetIconsForCorrectFilter ()
    {
        int existingCounter = 0;
        int firstExistingCommonNumber = -1;

        foreach ( BadgeViewModel badge in CorrectNumbered )
        {
            if ( existingCounter == _visibleRange )
            {
                break;
            }

            VisibleIcons.Add ( new BadgeCorrectnessViewModel ( badge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                              , _correctnessWidthLimit, FilterIsExtended ) );

            if ( existingCounter == 0 )
            {
                VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                VisibleIcons [existingCounter].CalcStringPresentation ( _correctnessWidthLimit );
                firstExistingCommonNumber = badge.Id;
            }

            existingCounter++;
        }

        if ( firstExistingCommonNumber > -1 )
        {
            ActiveIcon = VisibleIcons [0];
            ProcessableBadge = CorrectNumbered.ElementAt ( 0 );
        }
    }

    private void SetIconsForIncorrectFilter ()
    {
        int existingCounter = 0;
        int firstExistingCommonNumber = -1;

        foreach ( BadgeViewModel badge in IncorrectNumbered )
        {
            if ( existingCounter == _visibleRange )
            {
                break;
            }

            VisibleIcons.Add ( new BadgeCorrectnessViewModel ( badge, _extendedScrollableIconWidth, _shrinkedIconWidth
                                                                               , _correctnessWidthLimit, FilterIsExtended ) );
            if ( existingCounter == 0 )
            {
                VisibleIcons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                VisibleIcons [existingCounter].CalcStringPresentation ( _correctnessWidthLimit );
                firstExistingCommonNumber = badge.Id;
            }

            existingCounter++;
        }

        if ( firstExistingCommonNumber > -1 )
        {
            ActiveIcon = VisibleIcons [0];
            ProcessableBadge = IncorrectNumbered.ElementAt ( 0 );
        }
    }

    private void SetProcessableInMatchFilter ()
    {
        if ( ProcessableBadge == null )
        {
            return;
        }

        if ( ProcessableBadge.IsCorrect )
        {
            if ( !CorrectNumbered.Contains ( ProcessableBadge ) )
            {
                CorrectNumbered.Add ( ProcessableBadge );

                if ( IncorrectNumbered.Contains ( ProcessableBadge ) )
                {
                    IncorrectNumbered.Remove ( ProcessableBadge );
                }
            }
        }
        else if ( !ProcessableBadge.IsCorrect )
        {
            if ( !IncorrectNumbered.Contains ( ProcessableBadge ) )
            {
                IncorrectNumbered.Add ( ProcessableBadge );

                if ( CorrectNumbered.Contains ( ProcessableBadge ) )
                {
                    CorrectNumbered.Remove ( ProcessableBadge );
                }
            }
        }
    }

    private void SetSliderWideness ()
    {
        if ( CurrentVisibleCollection.Count > _visibleRange )
        {
            _correctnessWidthLimit = _narrowCorrectnessWidthLimit;
        }
        else
        {
            _correctnessWidthLimit = _wideCorrectnessWidthLimit;
        }
    }

    internal void ExtendOrShrinkCollectionManagement ()
    {
        if ( FilterIsExtended )
        {
            FilterBlockMargin = new Thickness ( _collectionFilterMarginLeft, 0 );
            FilterIsExtended = false;
            ExtenderContent = "\uF060";
            SwitcherWidth = _switcherWidthh;
            FilterLabelWidth = 0;
            IsComboboxEnabled = false;
            ExtentionTip = _extentionToolTip;
            TryEnableScroller ( VisibleIcons.Count );
            ExtendOrShrinkSliderItems ();
        }
        else
        {
            FilterBlockMargin = new Thickness ( 0, 0 );
            FilterIsExtended = true;
            ExtenderContent = "\uF061";
            SwitcherWidth = 0;
            FilterLabelWidth = _filterLabelWidthh;
            IsComboboxEnabled = true;
            ExtentionTip = _shrinkingToolTip;
            TryEnableScroller ( 0 );
            ExtendOrShrinkSliderItems ();
        }
    }

    internal void ExtendOrShrinkSliderItems ()
    {
        double width;

        if ( FilterIsExtended )
        {
            double scrollerItemsCount = _scrollerHeight / _itemHeightWithMargin;

            if ( CurrentVisibleCollection.Count > scrollerItemsCount )
            {
                width = _extendedScrollableIconWidth;
            }
            else
            {
                width = _mostExtendedIconWidth;
            }
        }
        else
        {
            width = _shrinkedIconWidth;
        }

        foreach ( BadgeCorrectnessViewModel item in VisibleIcons )
        {
            item.Width = width;
        }
    }
}
