using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Lister.Core.BadgesCreator;
using Lister.Core.Models;
using Lister.Core.Models.Badge;
using Lister.Desktop.ExecutersForCoreAbstractions.BadgeCreator;
using Lister.Desktop.Extentions;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;

internal partial class PersonChoosingViewModel : ObservableObject
{
    private readonly int _inputLimit = 50;
    private readonly BadgeCreator? _badgeCreator;

    [ObservableProperty]
    private bool _fileNotFound;

    public PersonChoosingViewModel ()
    {

    }

    public PersonChoosingViewModel ( string placeHolder, int inputLimit, SolidColorBrush incorrectTemplateColor,
        List<SolidColorBrush> defaultColors, List<SolidColorBrush> focusedColors, List<SolidColorBrush> selectedColors,
        BadgeCreator badgesCreator
    )
    {
        _inputLimit = inputLimit;
        _placeHolder = placeHolder;
        _incorrectTemplateForeground = incorrectTemplateColor;
        _defaultBackgroundColor = defaultColors [0];
        _defaultBorderColor = defaultColors [1];
        _defaultForegroundColor = defaultColors [2];
        _selectedBackgroundColor = selectedColors [0];
        _selectedBorderColor = selectedColors [1];
        _selectedForegroundColor = selectedColors [2];
        _focusedBackgroundColor = focusedColors [0];
        _focusedBorderColor = focusedColors [1];
        VisiblePeople = [];
        ScrollerCanvasLeft = _withScroll;
        PersonsScrollValue = _oneHeight;
        ChoiceIsDisabled = true;
        _focusedEdge = _edge;
        _badgeCreator = badgesCreator;
    }

    internal void ResetPersons ()
    {
        try
        {
            List<Person>? people = _badgeCreator?.People;
            UsePeople ( people, true );
        }
        catch ( Exception )
        {
            FileNotFound = true;
            UsePeople ( null, false );
        }
    }

    private void UsePeople ( List<Person>? people, bool peopleExist )
    {
        if ( people == null )
        {
            ChoiceIsDisabled = true;

            return;
        }

        List<VisiblePerson>? visiblePeople = people?.Clone ()
             ?.Where ( person => !person.IsEmpty () )
             .Select ( person => new VisiblePerson ( person ) )
             .OrderBy ( person => person.Model.FullName )
             .ToList ();

        PeopleStorage = visiblePeople;
        InvolvedPeople = visiblePeople;
        ChosenPerson = null;
        ChoiceIsDisabled = !peopleExist;
    }
    #region DropDown

    internal void HideDropListWithChange ()
    {
        if ( _selected != null )
        {
            _selected.IsSelected = false;
            _selected = null;
        }

        if ( _focused == null )
        {
            if ( !_chosenPersonIsSetInSetter )
            {
                ChosenPerson = null;
            }
        }
        else
        {
            ChosenPerson = _focused.Model;
            PlaceHolder = ChosenPerson.FullName;
            _choiceIsAbsent = false;
            _selected = _focused;
            _selected.IsSelected = true;
            _focused.IsFocused = true;
        }

        _chosenPersonIsSetInSetter = false;
        HideDropListWithoutChange ();
        SetReadiness ();
    }


    internal void HideDropListWithoutChange ()
    {
        if ( InvolvedPeople == null || PeopleStorage == null )
        {
            return;
        }

        if ( ( InvolvedPeople.Count == 0 ) && ( PeopleStorage.Count > 0 )
             || _choiceIsAbsent
        )
        {
            RecoverVisiblePeople ();
            ShowDropDown ();
            ChosenPerson = null;
            ToStartState ();
        }

        DropDownOpacity = 0;
        VisibleHeight = 0;
        FirstItemHeight = 0;
        FirstIsVisible = false;
    }


    private void ToStartState ()
    {
        SetEntireListChosenState ();
        PlaceHolder = _placeHolder;
        EntireBackgroundColor = _selectedBackgroundColor;
        EntireForegroundColor = _selectedForegroundColor;
        EntireFontWeight = FontWeight.Bold;
        _focusedNumber = -1;
        _choiceIsAbsent = false;
    }


    internal void ShowDropDown ()
    {
        DropDownOpacity = 1;
        VisibleHeight = _visibleHeightStorage;
        FirstIsVisible = _allListMustBe;

        if ( _allListMustBe )
        {
            FirstItemHeight = _scrollingScratch;
        }
    }
    #endregion

    internal void SetChosenPerson ( string? personName )
    {
        _choiceIsAbsent = false;
        Person? person = FindPersonByName ( personName );

        if ( person == null )
        {
            return;
        }

        int seekingStart = _focusedNumber - _maxVisibleCount;

        if ( seekingStart < 0 )
        {
            seekingStart = 0;
        }

        int seekingEnd = _focusedNumber + _maxVisibleCount;

#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
        if ( seekingStart > InvolvedPeople.Count )
        {
            seekingStart = InvolvedPeople.Count;
        }
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.

        for ( int index = seekingStart; index <= seekingEnd; index++ )
        {
            VisiblePerson foundPerson = InvolvedPeople [index];

            if ( person.Equals ( foundPerson.Model ) )
            {
                if ( _focused != null )
                {
                    _focused.IsFocused = false;
                    _focused.IsSelected = false;
                }
                else
                {
                    _entireIsSelected = false;
                }

                SetSelectedToNull ();
                _focused = foundPerson;
                _focusedNumber = index;
                PlaceHolder = personName;
                EntireFontWeight = FontWeight.Normal;
                HideDropListWithChange ();
                break;
            }
        }
    }

    internal void SetEntireList ()
    {
        _choiceIsAbsent = false;

        if ( _focused != null )
        {
            _focused.IsFocused = false;
            _focused = null;
        }

        if ( _selected != null )
        {
            _selected.IsSelected = false;
            _selected = null;
        }

        EntireIsSelected = true;
        HideDropListWithChange ();
        _focusedNumber = _focusedEdge - _maxVisibleCount;
    }

    private void SetSinglePersonChosenState ()
    {
        EntireIsSelected = false;
    }

    private void SetEntireListChosenState ()
    {
        EntireIsSelected = true;
    }

    internal void ShiftScroller ( double shift )
    {
        _widthDelta += shift;
        ScrollerCanvasLeft -= shift;
        PersonListWidth -= shift;
        _withoutScroll -= shift;
        _withScroll -= shift;
    }

    private Person? FindPersonByName ( string? presentation )
    {
        if ( string.IsNullOrWhiteSpace ( presentation ) || InvolvedPeople == null )
        {
            return null;
        }

        Person? result = null;

        foreach ( VisiblePerson person in InvolvedPeople )
        {
            bool isIntresting = person.Model.IsMatchingTo ( presentation );

            if ( isIntresting )
            {
                result = person.Model;

                break;
            }
        }

        return result;
    }

    private void SetPersonList ()
    {
        if ( ( InvolvedPeople != null ) && ( InvolvedPeople.Count > 0 ) )
        {
            int count = InvolvedPeople.Count;
            PersonListHeight = _oneHeight * count;
            bool listIsWhole = ( count == PeopleStorage.Count );
            _scrollValue = 0;

            if ( listIsWhole )
            {
                SetWholeList ( count );
            }
            else
            {
                SetCutDownList ( count );
            }

            SetScrollerIfShould ();
        }
        else
        {
            FirstItemHeight = 0;
            FirstIsVisible = false;
            _allListMustBe = false;
            _visibleHeightStorage = 0;
            PersonListWidth = 0;
            ScrollerWidth = 0;
            IsPersonsScrollable = false;
            PersonsScrollValue = 0;
        }

        SetReadiness ();
    }

    private void SetWholeList ( int personCount )
    {
        _visibleHeightStorage = _oneHeight * ( Math.Min ( _maxVisibleCount, personCount ) + 1 );
        FirstIsVisible = true;
        _allListMustBe = true;
        FirstItemHeight = _scrollingScratch;
        PersonsScrollValue = _scrollingScratch;

        if ( _focused != null )
        {
            _focused.IsFocused = false;
            _focused.IsSelected = false;
            _focused = null;
        }

        _focusedEdge = _edge;
        _focusedNumber = -1;
        ChoiceIsDisabled = false;
        EntireIsSelected = true;
        SetVisiblePeopleStartingFrom ( 0 );
    }

    private void SetCutDownList ( int personCount )
    {
        FirstItemHeight = 0;
        FirstIsVisible = false;
        _allListMustBe = false;
        _visibleHeightStorage = _oneHeight * Math.Min ( _maxVisibleCount, personCount );
        EntireIsSelected = false;
        PersonsScrollValue = 0;
        _focusedNumber = 0;
        _focused = InvolvedPeople [_focusedNumber];
        _focused.IsFocused = true;
        _focusedEdge = _edge;
        SetVisiblePeopleStartingFrom ( 0 );
    }

    private void SetVisiblePeopleStartingFrom ( int scratch )
    {
        VisiblePeople?.Clear ();

        for ( int index = 0; index < Math.Min ( InvolvedPeople.Count, _maxVisibleCount ); index++ )
        {
            VisiblePeople?.Add ( InvolvedPeople [scratch + index] );
        }
    }

    private void SetInvolvedPeople ( List<VisiblePerson> involvedPeople )
    {
        SetSelectedToNull ();
        InvolvedPeople = involvedPeople;
        _scrollValue = 0;
        ShowDropDown ();
    }

    private void SetScrollerIfShould ()
    {
        if ( InvolvedPeople.Count > _maxVisibleCount )
        {
            PersonListWidth = _withScroll - _widthDelta;
            ScrollerWidth = _upperHeight;
            double scrollerWorkAreaHeight = _visibleHeightStorage - ( ScrollerWidth * 2 );
            double proportion = PersonListHeight / scrollerWorkAreaHeight;
            RunnerHeight = _visibleHeightStorage / proportion;
            RealRunnerHeight = RunnerHeight;

            if ( RunnerHeight < _minRunnerHeight )
            {
                RunnerHeight = _minRunnerHeight;
            }

            RunnerYCoordinate = _upperHeight;
            TopSpanHeight = 0;
            BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
            _runnerStep = BottomSpanHeight / ( InvolvedPeople.Count - _maxVisibleCount );
            _scrollingLength = BottomSpanHeight;
            IsPersonsScrollable = true;
        }
        else
        {
            PersonListWidth = _withoutScroll - _widthDelta;
            ScrollerWidth = 0;
            IsPersonsScrollable = false;
        }
    }

    private void SetReadiness ()
    {
        SettingsIsComplated = InvolvedPeople != null
            && InvolvedPeople.Count > 0
            && ChosenTemplate != null;
    }

    internal void RefreshPersonList ( string input )
    {
        RestrictInput ( input );

        if ( input == string.Empty )
        {
            RecoverVisiblePeople ();
            ShowDropDown ();

            return;
        }

        List<VisiblePerson> foundVisiblePeople = [];

        foreach ( VisiblePerson person in PeopleStorage )
        {
            person.IsFocused = false;
            string entireName = person.Model.FullName;

            if ( entireName.Contains ( input, StringComparison.CurrentCultureIgnoreCase ) )
            {
                foundVisiblePeople.Add ( person );
            }
        }

        SetInvolvedPeople ( foundVisiblePeople );
    }

    private void RestrictInput ( string input )
    {
        if ( input.Length > _inputLimit )
        {
            string? ph = PlaceHolder;
            PlaceHolder = "";
            PlaceHolder = ph;
        }
        else
        {
            PlaceHolder = input;
        }
    }

    internal void RecoverVisiblePeople ()
    {
        SetSelectedToNull ();
        _choiceIsAbsent = true;
        _scrollValue = _scrollingScratch;

        InvolvedPeople = PeopleStorage.Clone ();
        EntireIsSelected = false;
    }

    private void SetSelectedToNull ()
    {
        if ( _selected != null )
        {
            _selected.IsSelected = false;
            _selected = null;
        }
    }

    internal void SetUp ()
    {
        ObservableCollection<TemplateViewModel> templates = [];
        _badgeLayouts ??= BadgeLayoutProvider.GetInstance ().GetBadgeLayouts ();

        foreach ( KeyValuePair<Layout, KeyValuePair<string, List<string>>> layout in _badgeLayouts )
        {
            List<string> errors = layout.Value.Value;
            string source = layout.Value.Key;
            templates.Add (
                new TemplateViewModel ( new TemplateName ( layout.Key.TemplateName ),
                    _defaultForegroundColor,
                    _incorrectTemplateForeground,
                    errors,
                    source
                )
            );
        }

        Templates = templates;
    }
}
