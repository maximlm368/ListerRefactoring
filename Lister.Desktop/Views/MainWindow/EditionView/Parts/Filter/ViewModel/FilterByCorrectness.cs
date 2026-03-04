using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.EditionView.Parts.Filter.ViewModel;

internal partial class FilterViewModel : ObservableObject
{
    [RelayCommand]
    internal void Filter ()
    {
        _runnerWalked = 0;

        if ( State == FilterState.All )
        {
            ToCorrect ();
        }
        else if ( State == FilterState.Corrects )
        {
            ToIncorrect ();
        }
        else if ( State == FilterState.Incorrects )
        {
            ToAll ();
        }

        Handle ();
    }

    internal void Filter ( string? filterName )
    {
        _runnerWalked = 0;

        if ( filterName == _allFilter )
        {
            ToAll ();
        }
        else if ( filterName == _correctFilter )
        {
            ToCorrect ();
        }
        else if ( filterName == _incorrectFilter )
        {
            ToIncorrect ();
        }

        Handle ();
    }

    private void ToAll () 
    {
        State = FilterState.All;
        SelectedFilterIndex = 0;
        CurrentCollection = All;
        SwitcherForeground = _white;
        SwitcherTip = _allTip;
        SetProcessableInMatchCollection ();
    }

    private void ToCorrect () 
    {
        State = FilterState.Corrects;
        SelectedFilterIndex = 1;
        SwitcherForeground = _green;
        SwitcherTip = _correctTip;
        SetProcessableInMatchCollection ();
        Corrects.Sort ( _comparer );
        CurrentCollection = Corrects;
    }

    private void ToIncorrect () 
    {
        State = FilterState.Incorrects;
        SelectedFilterIndex = 2;
        SwitcherForeground = _red;
        SwitcherTip = _incorrectTip;
        SetProcessableInMatchCollection ();
        Incorrects.Sort ( _comparer );
        CurrentCollection = Incorrects;
    }

    private void Handle () 
    {
        ProcessableCount = CurrentCollection.Count;
        CalcVisibleRange ( CurrentCollection != null ? CurrentCollection.Count : 0 );
        SetScroller ( CurrentCollection != null ? CurrentCollection.Count : 0 );
        SetAccordingIcons ();
        EnableNavigation ();
        SetScrollerItemsCorrectWidth ( CurrentCollection == null ? 0 : CurrentCollection.Count );
        CurrentNumber = 1;

        FilterChanged?.Invoke ( State );
    }

    private void SetAccordingIcons ()
    {
        IsNextEnable = true;

        if ( State == FilterState.All )
        {
            SetMixedIcons ();
        }
        else if ( State == FilterState.Corrects )
        {
            SetIconsForCorrectFilter ();
        }
        else if ( State == FilterState.Incorrects )
        {
            SetIconsForIncorrectFilter ();
        }

        _iconsStorage = Icons;
        _numberAmongIcons = 1;
        _scrollStepIndex = 0;

        if ( Icons.Count == 0 )
        {
            UpDownIsFocusable = false;
            IsPreviousEnable = false;
            IsNextEnable = false;
        }
        else
        {
            UpDownIsFocusable = true;
        }
    }

    private void SetMixedIcons ()
    {
        Icons = [];
        int counter = 0;

        foreach ( BadgeViewModel badge in All )
        {
            if ( counter == _visibleRange )
            {
                break;
            }

            BadgeCorrectnessViewModel icon = new ( badge, FilterIsExtended );
            icon.CalcStringPresentation ();
            Icons.Add ( icon );
            counter++;
        }
        
        Icons [0].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
        Icons [0].CalcStringPresentation ( );
        ActiveIcon = Icons [0];
    }

    private void SetIconsForCorrectFilter ()
    {
        Icons = [];
        int existingCounter = 0;
        int firstExistingCommonNumber = -1;

        foreach ( BadgeViewModel badge in Corrects )
        {
            if ( existingCounter == _visibleRange )
            {
                break;
            }

            BadgeCorrectnessViewModel icon = new ( badge, FilterIsExtended );
            icon.CalcStringPresentation ();
            Icons.Add ( icon );

            if ( existingCounter == 0 )
            {
                Icons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                Icons [existingCounter].CalcStringPresentation ( );
                firstExistingCommonNumber = badge.Id;
            }

            existingCounter++;
        }

        if ( firstExistingCommonNumber > -1 )
        {
            ActiveIcon = Icons [0];
        }
    }

    private void SetIconsForIncorrectFilter ()
    {
        Icons = [];
        int existingCounter = 0;
        int firstExistingCommonNumber = -1;

        foreach ( BadgeViewModel badge in Incorrects )
        {
            if ( existingCounter == _visibleRange )
            {
                break;
            }

            BadgeCorrectnessViewModel icon = new ( badge, FilterIsExtended );
            icon.CalcStringPresentation ();
            Icons.Add ( icon );

            if ( existingCounter == 0 )
            {
                Icons [existingCounter].BoundFontWeight = Avalonia.Media.FontWeight.Bold;
                Icons [existingCounter].CalcStringPresentation ( );
                firstExistingCommonNumber = badge.Id;
            }

            existingCounter++;
        }

        if ( firstExistingCommonNumber > -1 )
        {
            ActiveIcon = Icons [0];
        }
    }

    private void SetProcessableInMatchCollection ()
    {
        if ( ActiveIcon == null )
        {
            return;
        }

        if ( ActiveIcon.BoundBadge.IsCorrect )
        {
            if ( !Corrects.Contains ( ActiveIcon.BoundBadge ) )
            {
                Corrects.Add (ActiveIcon.BoundBadge);

                if ( Incorrects.Contains ( ActiveIcon.BoundBadge ) )
                {
                    Incorrects.Remove ( ActiveIcon.BoundBadge );
                }
            }
        }
        else if ( !ActiveIcon.BoundBadge.IsCorrect )
        {
            if ( !Incorrects.Contains (ActiveIcon.BoundBadge) )
            {
                Incorrects.Add (ActiveIcon.BoundBadge);

                if ( Corrects.Contains (ActiveIcon.BoundBadge) )
                {
                    Corrects.Remove (ActiveIcon.BoundBadge);
                }
            }
        }
    }
}
