﻿//using Core.BadgesProvider;
//using Core.DataAccess.PeopleSource;
//using Core.Models;
//using Core.Models.Badge;
//using Core.DataAccess;
//using ExtentionsAndAuxiliary;
//using System.Collections.ObjectModel;

namespace Core.DocumentProcessor;

//public partial class PersonPreparer
//{
//    private Dictionary <Layout, KeyValuePair<string, List<string>>> _badgeLayouts;

//    private bool _fileNotFound;
//    public bool FileNotFound
//    {
//        get { return _fileNotFound; }
//        private set
//        {
//            _fileNotFound = value;
//        }
//    }


//    public PersonPreparer ( )
//    {
//        if ( _badgeLayouts == null )
//        {
//            _badgeLayouts = BadgeAppearence.GetBadgeLayouts ();
//        }
//    }


//    internal void SetPersonsFromFile ( string? path )
//    {
//        bool valueIsSuitable = ! string.IsNullOrWhiteSpace (path);

//        if ( valueIsSuitable )
//        {
//            BadgesGetter badgesProvider = new BadgesGetter ();

//            try
//            {
//                List<Person> persons = badgesProvider.GetPersons (path);
//                SetPersonsFromNewSource (persons);
//            }
//            catch ( Exception ex )
//            {
//                FileNotFound = true;
//                SetPersonsFromNewSource (null);
//            }
//        }
//        else
//        {
//            SetPersonsFromNewSource (null);
//        }
//    }


//    private void SetPersonsFromNewSource ( List<Person>? persons )
//    {
//        if ( persons == null )
//        {
//            return;
//        }

//        List<Person> sortablePersons = persons.Clone ();
//        IComparer<Person> comparer = new RusStringComparer<Person> ();
//        sortablePersons.Sort (comparer);

//        List<VisiblePerson> peopleStorage = new ();
//        List<VisiblePerson> involvedPeople = new ();

//        foreach ( Person person in sortablePersons )
//        {
//            if ( person.IsEmpty () )
//            {
//                continue;
//            }

//            VisiblePerson visiblePerson = new VisiblePerson (person);
//            peopleStorage.Add (visiblePerson);
//            involvedPeople.Add (visiblePerson);
//        }

//        PeopleStorage = peopleStorage;
//        InvolvedPeople = involvedPeople;
//        ChosenPerson = null;
//    }


//    internal void SetChosenPerson ( string personName )
//    {
//        _choiceIsAbsent = false;
//        Person person = FindPersonByStringPresentation (personName);
//        int seekingScratch = _focusedNumber - _maxVisibleCount;

//        if ( seekingScratch < 0 )
//        {
//            seekingScratch = 0;
//        }

//        int seekingEnd = _focusedNumber + _maxVisibleCount;

//        if ( seekingScratch > InvolvedPeople.Count )
//        {
//            seekingScratch = InvolvedPeople.Count;
//        }

//        for ( int index = seekingScratch; index <= seekingEnd; index++ )
//        {
//            VisiblePerson foundPerson = InvolvedPeople [index];
//            bool isCoincidence = person.Equals (foundPerson.Person);

//            if ( isCoincidence )
//            {
//                if ( _focused != null )
//                {
//                    _focused.IsFocused = false;
//                    _focused.IsSelected = false;
//                }
//                else
//                {
//                    _entireIsSelected = false;
//                }

//                SetSelectedToNull ();
//                _focused = foundPerson;

//                _focusedNumber = index;
//                PlaceHolder = personName;
//                EntireFontWeight = FontWeight.Normal;
//                HideDropListWithChange ();
//                break;
//            }
//        }
//    }


//    //internal void SetEntireList ()
//    //{
//    //    _choiceIsAbsent = false;

//    //    if ( _focused != null )
//    //    {
//    //        _focused.IsFocused = false;
//    //        _focused = null;
//    //    }

//    //    if ( _selected != null )
//    //    {
//    //        _selected.IsSelected = false;
//    //        _selected = null;
//    //    }

//    //    EntireIsSelected = true;
//    //    HideDropListWithChange ();
//    //    _focusedNumber = _focusedEdge - _maxVisibleCount;
//    //}


//    //private void SetPersonChosenState ()
//    //{
//    //    EntireIsSelected = false;
//    //    SinglePersonIsSelected = true;
//    //}





//    internal Person? FindPersonByStringPresentation ( string presentation )
//    {
//        if ( string.IsNullOrWhiteSpace (presentation) )
//        {
//            return null;
//        }

//        Person result = null;

//        foreach ( VisiblePerson person in InvolvedPeople )
//        {
//            bool isIntresting = person.Person.IsMatchingTo (presentation);

//            if ( isIntresting )
//            {
//                result = person.Person;
//                break;
//            }
//        }

//        return result;
//    }


//    //private void SetPersonList ()
//    //{
//    //    if ( ( InvolvedPeople != null ) && ( InvolvedPeople.Count > 0 ) )
//    //    {
//    //        int count = InvolvedPeople.Count;
//    //        PersonListHeight = _oneHeight * count;
//    //        bool listIsWhole = ( count == PeopleStorage.Count );
//    //        _scrollValue = 0;

//    //        if ( listIsWhole )
//    //        {
//    //            SetWholeList (count);
//    //        }
//    //        else
//    //        {
//    //            SetCutDownList (count);
//    //        }

//    //        _topLimit = _focusedNumber;
//    //        SetScrollerIfShould ();
//    //    }
//    //    else
//    //    {
//    //        FirstItemHeight = 0;
//    //        FirstIsVisible = false;
//    //        _allListMustBe = false;
//    //        _visibleHeightStorage = 0;
//    //        PersonListWidth = 0;
//    //        ScrollerWidth = 0;
//    //        IsPersonsScrollable = false;
//    //        PersonsScrollValue = 0;
//    //    }

//    //    SetReadiness ();
//    //}


//    //private void SetWholeList ( int personCount )
//    //{
//    //    _visibleHeightStorage = _oneHeight * ( Math.Min (_maxVisibleCount, personCount) + 1 );
//    //    FirstIsVisible = true;
//    //    _allListMustBe = true;
//    //    FirstItemHeight = _scrollingScratch;
//    //    PersonsScrollValue = _scrollingScratch;

//    //    if ( _focused != null )
//    //    {
//    //        _focused.IsFocused = false;
//    //        _focused.IsSelected = false;
//    //        _focused = null;
//    //    }

//    //    _focusedEdge = _edge;
//    //    _focusedNumber = -1;
//    //    TextboxIsReadOnly = false;
//    //    TextboxIsFocusable = true;
//    //    EntireIsSelected = true;

//    //    SetVisiblePeopleStartingFrom (0);
//    //}


//    //private void SetCutDownList ( int personCount )
//    //{
//    //    FirstItemHeight = 0;
//    //    FirstIsVisible = false;
//    //    _allListMustBe = false;

//    //    _visibleHeightStorage = _oneHeight * Math.Min (_maxVisibleCount, personCount);
//    //    EntireIsSelected = false;
//    //    PersonsScrollValue = 0;

//    //    _focusedNumber = 0;
//    //    _focused = InvolvedPeople [_focusedNumber];

//    //    _focused.IsFocused = true;

//    //    _focusedEdge = _edge;

//    //    SetVisiblePeopleStartingFrom (0);
//    //}


//    //private void SetVisiblePeopleStartingFrom ( int scratch )
//    //{
//    //    VisiblePeople.Clear ();
//    //    int limit = Math.Min (InvolvedPeople.Count, _maxVisibleCount);

//    //    for ( int index = 0; index < limit; index++ )
//    //    {
//    //        int personIndex = scratch + index;
//    //        VisiblePeople.Add (InvolvedPeople [personIndex]);
//    //    }
//    //}


//    //internal void SetInvolvedPeople ( List<VisiblePerson> involvedPeople )
//    //{
//    //    SetSelectedToNull ();
//    //    InvolvedPeople = involvedPeople;
//    //    _scrollValue = 0;
//    //    ShowDropDown ();
//    //}


//    //private void SetScrollerIfShould ()
//    //{
//    //    int personCount = InvolvedPeople.Count;

//    //    if ( personCount > _maxVisibleCount )
//    //    {
//    //        PersonListWidth = _withScroll - _widthDelta;
//    //        ScrollerWidth = _upperHeight;

//    //        double scrollerWorkAreaHeight = _visibleHeightStorage - ( ScrollerWidth * 2 );
//    //        double proportion = PersonListHeight / scrollerWorkAreaHeight;
//    //        RunnerHeight = _visibleHeightStorage / proportion;
//    //        RealRunnerHeight = RunnerHeight;

//    //        if ( RunnerHeight < _minRunnerHeight )
//    //        {
//    //            RunnerHeight = _minRunnerHeight;
//    //        }

//    //        RunnerYCoordinate = _upperHeight;
//    //        TopSpanHeight = 0;
//    //        BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
//    //        _runnerStep = BottomSpanHeight / ( InvolvedPeople.Count - _maxVisibleCount );
//    //        _scrollingLength = BottomSpanHeight;
//    //        IsPersonsScrollable = true;
//    //    }
//    //    else
//    //    {
//    //        PersonListWidth = _withoutScroll - _widthDelta;
//    //        ScrollerWidth = 0;
//    //        IsPersonsScrollable = false;
//    //    }
//    //}


//    //internal void ReductPersonList ( string input )
//    //{
//    //    ToZeroPersonSelection ();
//    //    RestrictInput (input);

//    //    if ( ( input == string.Empty ) )
//    //    {
//    //        RecoverVisiblePeople ();
//    //        ShowDropDown ();
//    //        return;
//    //    }

//    //    List<VisiblePerson> foundVisiblePeople = new List<VisiblePerson> ();

//    //    foreach ( VisiblePerson person in PeopleStorage )
//    //    {
//    //        person.IsFocused = false;

//    //        string entireName = person.Person.FullName;

//    //        if ( entireName.Contains (input, StringComparison.CurrentCultureIgnoreCase) )
//    //        {
//    //            foundVisiblePeople.Add (person);
//    //        }
//    //    }

//    //    SetInvolvedPeople (foundVisiblePeople);
//    //}


//    //private void SetSelectedToNull ()
//    //{
//    //    if ( _selected != null )
//    //    {
//    //        _selected.IsSelected = false;
//    //        _selected = null;
//    //    }
//    //}


//    //internal void SetUp ( string theme )
//    //{
//    //    SolidColorBrush correctColor = _defaultForegroundColor;
//    //    SolidColorBrush incorrectColor = _incorrectTemplateForeground;

//    //    if ( theme == "Dark" )
//    //    {
//    //        correctColor = _defaultBackgroundColor;
//    //        incorrectColor = new SolidColorBrush (new Avalonia.Media.Color (100, 255, 255, 255));
//    //    }

//    //    ObservableCollection<TemplateViewModel> templates = new ();

//    //    foreach ( KeyValuePair<Layout, KeyValuePair<string, List<string>>> layout   in   _badgeLayouts )
//    //    {
//    //        KeyValuePair<string, List<string>> sourceAndErrors = layout.Value;
//    //        bool correctLayoutHasEmptyMessage = ( sourceAndErrors.Value.Count < 1 );
//    //        List<string> errors = layout.Value.Value;
//    //        string source = layout.Value.Key;

//    //        templates.Add (new TemplateViewModel (new TemplateName (layout.Key.TemplateName, correctLayoutHasEmptyMessage)
//    //                                              , correctColor, incorrectColor, errors, source));
//    //    }

//    //    Templates = templates;
//    //}
//}



//public class TemplateName
//{
//    public string Name { get; private set; }
//    public bool IsFound { get; private set; }


//    public TemplateName ( string name, bool isFound )
//    {
//        Name = name;
//        IsFound = isFound;
//    }
//}



//public class TemplateViewModel
//{
//    private TemplateName TemplateName { get; set; }
//    public string SourcePath { get; private set; }

//    private string _name;
//    public string Name
//    {
//        get
//        {
//            return _name;
//        }
//        set
//        {
//            _name = value;
//        }
//    }

//    private SolidColorBrush _color;
//    public SolidColorBrush Color
//    {
//        get
//        {
//            return _color;
//        }
//        set
//        {
//            this.RaiseAndSetIfChanged (ref _color, value, nameof (Color));
//        }
//    }

//    private List<string> _correctnessMessage;
//    public List<string> CorrectnessMessage
//    {
//        get
//        {
//            return _correctnessMessage;
//        }
//        set
//        {
//            _correctnessMessage = value;
//        }
//    }


//    public TemplateViewModel ( TemplateName templateName, SolidColorBrush colorIfCorrect, SolidColorBrush colorIfIncorrect
//                              , List<string> correctnessMessage, string sourcePath )
//    {
//        TemplateName = templateName;
//        Name = templateName.Name;
//        CorrectnessMessage = correctnessMessage;
//        SourcePath = sourcePath;

//        bool isCorrect = ( correctnessMessage.Count == 0 );

//        if ( isCorrect )
//        {
//            Color = colorIfCorrect;
//        }
//        else
//        {
//            Color = colorIfIncorrect;
//        }
//    }
//}