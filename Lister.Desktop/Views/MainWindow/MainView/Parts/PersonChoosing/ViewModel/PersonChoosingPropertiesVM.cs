using Avalonia.Media;
using Lister.Core.Models;
using Lister.Core.Models.Badge;
using Lister.Desktop.CoreModelReflections;
using Lister.Desktop.CoreModelReflections.BadgeVM;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;

public partial class PersonChoosingViewModel : ReactiveObject
{
    private static SolidColorBrush _defaultBorderColor;
    private static SolidColorBrush _defaultBackgroundColor;
    private static SolidColorBrush _defaultForegroundColor;
    private static SolidColorBrush _selectedBorderColor;
    private static SolidColorBrush _selectedBackgroundColor;
    private static SolidColorBrush _selectedForegroundColor;
    private static SolidColorBrush _focusedBorderColor;
    private static SolidColorBrush _focusedBackgroundColor;

    private readonly string _placeHolder;
    private readonly double _minRunnerHeight = 10;
    private readonly double _upperHeight = 15;
    private readonly double _scrollingScratch = 32;
    private readonly int _maxVisibleCount = 4;
    private readonly double _oneHeight = 32;
    private readonly int _edge = 3;

    private double _withScroll = 457;
    private double _withoutScroll = 472;
    private SolidColorBrush _incorrectTemplateForeground;
    private Dictionary<Layout, KeyValuePair<string, List<string>>> _badgeLayouts;
    private bool _allListMustBe = false;
    private double _widthDelta;
    private Timer _timer;
    private VisiblePerson _focused;
    private VisiblePerson _selected;
    private int _focusedNumber;
    private int _focusedEdge;
    private int _topLimit;
    private bool _chosenPersonIsSetInSetter;
    private bool _choiceIsAbsent;

    internal bool ScrollingIsOccured { get; set; }
    internal bool SinglePersonIsSelected { get; private set; }

    private bool _entireIsSelected;
    internal bool EntireIsSelected
    {
        get { return _entireIsSelected; }
        private set
        {
            _entireIsSelected = value;

            if ( _entireIsSelected   &&   !_choiceIsAbsent )
            {
                PlaceHolder = _placeHolder;
                EntireBackgroundColor = _selectedBackgroundColor;
                EntireBorderColor = _selectedBorderColor;
                EntireForegroundColor = _selectedForegroundColor;
                EntireFontWeight = Avalonia.Media.FontWeight.Bold;
            }
            else
            {
                EntireBackgroundColor = _defaultBackgroundColor;
                EntireBorderColor = _defaultBorderColor;
                EntireForegroundColor = _defaultForegroundColor;
                EntireFontWeight = Avalonia.Media.FontWeight.Normal;
            }
        }
    }

    internal bool BuildingIsPossible { get; private set; }
    internal List <VisiblePerson> PeopleStorage { get; set; }

    private ObservableCollection <TemplateViewModel> _templates;
    internal ObservableCollection <TemplateViewModel> Templates
    {
        get
        {
            return _templates;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref _templates, value, nameof (Templates));
        }
    }

    private bool _isOpen;
    internal bool IsOpen
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _isOpen, value, nameof (_isOpen));
        }
        get
        {
            return _isOpen;
        }
    }

    private Avalonia.Thickness _chosenTemplatePadding;
    public Avalonia.Thickness ChosenTemplatePadding
    {
        get
        {
            return _chosenTemplatePadding;
        }
        private set
        {
            this.RaiseAndSetIfChanged (ref _chosenTemplatePadding, value, nameof (ChosenTemplatePadding));
        }
    }

    private SolidColorBrush _chosenTemplateColor;
    public SolidColorBrush ChosenTemplateColor
    {
        get
        {
            return _chosenTemplateColor;
        }
        private set
        {
            this.RaiseAndSetIfChanged (ref _chosenTemplateColor, value, nameof (ChosenTemplateColor));
        }
    }

    private TemplateViewModel _chosenTemplate;
    internal TemplateViewModel ChosenTemplate
    {
        set
        {
            bool valueIsSuitable = (( value != null ) && ( value.Name != string.Empty ));

            if ( valueIsSuitable )
            {
                foreach ( TemplateViewModel template   in   Templates )
                {
                    bool isCoincident = ( template.Name == value.Name );

                    if ( isCoincident )
                    {
                        ChosenTemplateColor = value.Color;

                        if ( _chosenTemplate == value )
                        {
                            _chosenTemplate = null;
                        }

                        this.RaiseAndSetIfChanged (ref _chosenTemplate, value, nameof (ChosenTemplate));

                        SickTemplateIsSet = ( value.Color.Color.R != 0 )
                                             || ( value.Color.Color.G != 0 )
                                             || ( value.Color.Color.B != 0 );
                        SetReadiness ();
                    }
                }
            }
        }

        get
        {
            return _chosenTemplate;
        }
    }

    private List<VisiblePerson> _sortedInvolvedPeople;
    private List <VisiblePerson> _involvedPeople;
    internal List <VisiblePerson> InvolvedPeople
    {
        get { return _involvedPeople; }
        set
        {
            _involvedPeople = value;
            SetPersonList ();
        }
    }

    private Person _chosenPerson;
    internal Person ? ChosenPerson
    {
        get { return _chosenPerson; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _chosenPerson, value, nameof (ChosenPerson));

            _chosenPersonIsSetInSetter = true;

            if ( ChosenPerson == null )
            {
                SetEntireListChosenState ();
            }
            else
            {
                SetPersonChosenState ();
            }
        }
    }

    private bool _allAreSelected;
    public bool AllAreReady
    {
        get { return _allAreSelected; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _allAreSelected, value, nameof (AllAreReady));
        }
    }

    private bool _sickTemplateIsSet;
    public bool SickTemplateIsSet
    {
        get { return _sickTemplateIsSet; }
        private set
        {
            if ( _sickTemplateIsSet == value )
            {
                _sickTemplateIsSet = !_sickTemplateIsSet;
            }

            this.RaiseAndSetIfChanged (ref _sickTemplateIsSet, value, nameof (SickTemplateIsSet));
        }
    }

    private ObservableCollection <VisiblePerson> _visiblePeople;
    internal ObservableCollection <VisiblePerson> VisiblePeople
    {
        get { return _visiblePeople; }
        set
        {
            this.RaiseAndSetIfChanged (ref _visiblePeople, value, nameof (VisiblePeople));
        }
    }

    private string _placeholder;
    internal string PlaceHolder
    {
        get { return _placeholder; }
        set
        {
            this.RaiseAndSetIfChanged (ref _placeholder, value, nameof (PlaceHolder));
            CaretIndex = PlaceHolder.Length;
        }
    }

    private int _caretIndex;
    internal int CaretIndex
    {
        get { return _caretIndex; }
        set
        {
            this.RaiseAndSetIfChanged (ref _caretIndex, value, nameof (CaretIndex));
        }
    }

    private int _selectionStart;
    internal int SelectionStart
    {
        get { return _selectionStart; }
        set
        {
            this.RaiseAndSetIfChanged (ref _selectionStart, value, nameof (SelectionStart));
        }
    }

    private int _selectionEnd;
    internal int SelectionEnd
    {
        get { return _selectionEnd; }
        set
        {
            this.RaiseAndSetIfChanged (ref _selectionEnd, value, nameof (SelectionEnd));
        }
    }

    private double _visibleHeightStorage;
    private double _visibleHeight;
    internal double VisibleHeight
    {
        get { return _visibleHeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _visibleHeight, value, nameof (VisibleHeight));
        }
    }

    private SolidColorBrush _colorForEntireList;
    internal SolidColorBrush EntireBorderColor
    {
        get { return _colorForEntireList; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _colorForEntireList, value, nameof (EntireBorderColor));
        }
    }

    private SolidColorBrush _entireForegroundColor;
    internal SolidColorBrush EntireForegroundColor
    {
        get { return _entireForegroundColor; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _entireForegroundColor, value, nameof (EntireForegroundColor));
        }
    }

    private SolidColorBrush _entireBackgroundColor;
    internal SolidColorBrush EntireBackgroundColor
    {
        get { return _entireBackgroundColor; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _entireBackgroundColor, value, nameof (EntireBackgroundColor));
        }
    }

    private FontWeight _entireFontWeight;
    internal FontWeight EntireFontWeight
    {
        get { return _entireFontWeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _entireFontWeight, value, nameof (EntireFontWeight));
        }
    }

    private double _personListWidth;
    internal double PersonListWidth
    {
        get { return _personListWidth; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _personListWidth, value, nameof (PersonListWidth));
        }
    }

    private double _personListHeight;
    internal double PersonListHeight
    {
        get { return _personListHeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _personListHeight, value, nameof (PersonListHeight));
        }
    }

    private double _personScrollValue;
    internal double PersonsScrollValue
    {
        get { return _personScrollValue; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _personScrollValue, value, nameof (PersonsScrollValue));
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

    private double _runnerYCoordinate;
    internal double RunnerYCoordinate
    {
        get { return _runnerYCoordinate; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _runnerYCoordinate, value, nameof (RunnerYCoordinate));
        }
    }

    private double _topSpanHeight;
    internal double TopSpanHeight
    {
        get { return _topSpanHeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _topSpanHeight, value, nameof (TopSpanHeight));
        }
    }

    private double _bottomSpanHeight;
    internal double BottomSpanHeight
    {
        get { return _bottomSpanHeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _bottomSpanHeight, value, nameof (BottomSpanHeight));
        }
    }

    private double _scrollerWidth;
    internal double ScrollerWidth
    {
        get { return _scrollerWidth; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _scrollerWidth, value, nameof (ScrollerWidth));
        }
    }

    private double _scrollerCanvasLeft;
    internal double ScrollerCanvasLeft
    {
        get { return _scrollerCanvasLeft; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _scrollerCanvasLeft, value, nameof (ScrollerCanvasLeft));
        }
    }

    private double _firstItemHeight;
    internal double FirstItemHeight
    {
        get { return _firstItemHeight; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _firstItemHeight, value, nameof (FirstItemHeight));
        }
    }

    private bool _firstIsVisible;
    public bool FirstIsVisible
    {
        get { return _firstIsVisible; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _firstIsVisible, value, nameof (FirstIsVisible));

            if ( value )
            {
                FirstItemHeight = 32;
            }
            else
            {
                FirstItemHeight = 0;
            }
        }
    }

    private bool _isPersonsScrollable;
    public bool IsPersonsScrollable
    {
        get { return _isPersonsScrollable; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _isPersonsScrollable, value, nameof (IsPersonsScrollable));
        }
    }

    private bool _textboxIsReadOnly;
    public bool TextboxIsReadOnly
    {
        get { return _textboxIsReadOnly; }
        set
        {
            this.RaiseAndSetIfChanged (ref _textboxIsReadOnly, value, nameof (TextboxIsReadOnly));
        }
    }

    private bool _textboxIsFocusable;
    public bool TextboxIsFocusable
    {
        get { return _textboxIsFocusable; }
        set
        {
            this.RaiseAndSetIfChanged (ref _textboxIsFocusable, value, nameof (TextboxIsFocusable));
        }
    }

    private double _dropDownOpacity;
    public double DropDownOpacity
    {
        get { return _dropDownOpacity; }
        set
        {
            this.RaiseAndSetIfChanged (ref _dropDownOpacity, value, nameof (DropDownOpacity));
        }
    }


    private void SetPropertiesByConfig ()
    {

    }
}