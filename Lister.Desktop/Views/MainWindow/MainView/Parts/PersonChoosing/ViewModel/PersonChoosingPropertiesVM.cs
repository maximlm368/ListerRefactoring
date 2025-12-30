using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Models;
using Lister.Core.Models.Badge;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;

internal partial class PersonChoosingViewModel : ObservableObject
{
    private static readonly SolidColorBrush _black = new ( Colors.Black );
    private static readonly SolidColorBrush _white = new ( Colors.White );
    private static readonly SolidColorBrush _gray = new ( Colors.Gray );
    private static readonly SolidColorBrush _blue = new ( Colors.Blue );
    private static SolidColorBrush _defaultBorderColor = _gray;
    private static SolidColorBrush _defaultBackgroundColor = _white;
    private static SolidColorBrush _defaultForegroundColor = _black;
    private static SolidColorBrush _selectedBorderColor = _blue;
    private static SolidColorBrush _selectedBackgroundColor = _white;
    private static SolidColorBrush _selectedForegroundColor = _black;
    private static SolidColorBrush _focusedBorderColor = _black;
    private static SolidColorBrush _focusedBackgroundColor = _gray;
    private static SolidColorBrush _incorrectTemplateForeground = _black;

    private readonly string? _placeHolder;
    private readonly double _minRunnerHeight = 10;
    private readonly double _upperHeight = 15;
    private readonly double _scrollingScratch = 32;
    private readonly int _maxVisibleCount = 4;
    private readonly double _oneHeight = 32;
    private readonly int _edge = 3;
    private double _withScroll = 457;
    private double _withoutScroll = 472;
    private Dictionary<Layout, KeyValuePair<string, List<string>>>? _badgeLayouts;
    private bool _allListMustBe = false;
    private double _widthDelta;
    private Timer? _timer;
    private VisiblePerson? _focused;
    private VisiblePerson? _selected;
    private int _focusedNumber;
    private int _focusedEdge;
    private bool _chosenPersonIsSetInSetter;
    private bool _choiceIsAbsent;

    private bool _entireIsSelected;
    internal bool EntireIsSelected
    {
        get
        {
            return _entireIsSelected;
        }

        private set
        {
            _entireIsSelected = value;

            if ( _entireIsSelected && !_choiceIsAbsent )
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

    internal List<VisiblePerson>? PeopleStorage { get; set; }

    [ObservableProperty]
    private ObservableCollection<TemplateViewModel>? _templates;

    private TemplateViewModel? _chosenTemplate;
    internal TemplateViewModel? ChosenTemplate
    {
        get
        {
            return _chosenTemplate;
        }

        set
        {
            if ( value == null || value.Color == null )
            {
                return;
            }

            _chosenTemplate = value;
            OnPropertyChanged ();

            SickTemplateIsSet = ( value.Color.Color.R != 0 )
                                || ( value.Color.Color.G != 0 )
                                || ( value.Color.Color.B != 0 );

            SetReadiness ();
        }
    }

    private List<VisiblePerson>? _involvedPeople;
    internal List<VisiblePerson>? InvolvedPeople
    {
        get
        {
            return _involvedPeople;
        }

        set
        {
            _involvedPeople = value;
            SetPersonList ();
        }
    }

    private Person? _chosenPerson;
    internal Person? ChosenPerson
    {
        get
        {
            return _chosenPerson;
        }

        private set
        {
            _chosenPerson = value;
            OnPropertyChanged ();

            _chosenPersonIsSetInSetter = true;

            if ( ChosenPerson == null )
            {
                SetEntireListChosenState ();
            }
            else
            {
                SetSinglePersonChosenState ();
            }
        }
    }

    [ObservableProperty]
    private bool _settingsIsComplated;

    private bool _sickTemplateIsSet;
    public bool SickTemplateIsSet
    {
        get
        {
            return _sickTemplateIsSet;
        }

        private set
        {
            _sickTemplateIsSet = true;
            OnPropertyChanged ();
        }
    }

    [ObservableProperty]
    private ObservableCollection<VisiblePerson>? _visiblePeople;

    private string? _placeholder;
    internal string? PlaceHolder
    {
        get
        {
            return _placeholder;
        }

        set
        {
            _placeholder = value;
            OnPropertyChanged ();
            CaretIndex = !string.IsNullOrWhiteSpace ( PlaceHolder ) ? PlaceHolder.Length : 0;
        }
    }

    [ObservableProperty]
    private int _caretIndex;

    [ObservableProperty]
    private int _selectionStart;

    [ObservableProperty]
    private int _selectionEnd;

    private double _visibleHeightStorage;

    [ObservableProperty]
    private double _visibleHeight;

    [ObservableProperty]
    private SolidColorBrush? _entireBorderColor;

    [ObservableProperty]
    private SolidColorBrush? _entireForegroundColor;

    [ObservableProperty]
    private SolidColorBrush? _entireBackgroundColor;

    [ObservableProperty]
    private FontWeight _entireFontWeight;

    [ObservableProperty]
    private double _personListWidth;

    [ObservableProperty]
    private double _personListHeight;

    [ObservableProperty]
    private double _personsScrollValue;

    internal double RealRunnerHeight { get; private set; }

    [ObservableProperty]
    private double _runnerHeight;

    [ObservableProperty]
    private double _runnerYCoordinate;

    [ObservableProperty]
    private double _topSpanHeight;

    [ObservableProperty]
    private double _bottomSpanHeight;

    [ObservableProperty]
    private double _scrollerWidth;

    [ObservableProperty]
    private double _scrollerCanvasLeft;

    [ObservableProperty]
    private double _firstItemHeight;

    private bool _firstIsVisible;
    public bool FirstIsVisible
    {
        get
        {
            return _firstIsVisible;
        }

        private set
        {
            _firstIsVisible = value;
            OnPropertyChanged ();

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

    [ObservableProperty]
    private bool _isPersonsScrollable;

    private bool _choiceIsDisabled;
    public bool ChoiceIsDisabled
    {
        get
        {
            return _choiceIsDisabled;
        }

        set
        {
            _choiceIsDisabled = value;
            ChoiceIsEnabled = !value;
            OnPropertyChanged ();
        }
    }

    [ObservableProperty]
    private bool _choiceIsEnabled;

    [ObservableProperty]
    private double _dropDownOpacity;
}
