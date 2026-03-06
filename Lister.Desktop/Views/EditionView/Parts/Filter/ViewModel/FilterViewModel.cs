using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Desktop.Entities.BadgeVM;
using Lister.Desktop.Extentions;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.EditionView.Parts.Filter.ViewModel;

internal partial class FilterViewModel : ObservableObject
{
    private readonly SolidColorBrush _white = new ( new Avalonia.Media.Color ( 255, 250, 250, 250 ) );
    private readonly SolidColorBrush _green = new ( new Avalonia.Media.Color ( 255, 97, 184, 97 ) );
    private readonly SolidColorBrush _red = new ( new Avalonia.Media.Color ( 255, 210, 54, 80 ) );

    private string _allFilter = string.Empty;
    private string _incorrectFilter = string.Empty;
    private string _correctFilter = string.Empty;
    private string _allTip = string.Empty;
    private string _correctTip = string.Empty;
    private string _incorrectTip = string.Empty;
    private readonly BadgeComparer _comparer = new ();
    private int _bottomLimit;

    private int _numberAmongIcons = 1;

    [ObservableProperty]
    private bool _narrowNavigationIsVisible;

    [ObservableProperty]
    private int _selectedFilterIndex;

    [ObservableProperty]
    private string? _switcherTip;

    [ObservableProperty]
    private SolidColorBrush? _switcherForeground;

    [ObservableProperty]
    private ObservableCollection<BadgeCorrectnessViewModel> _icons = [];

    [ObservableProperty]
    private int _processableCount;

    [ObservableProperty]
    private BadgeCorrectnessViewModel? _activeIcon;

    private List<BadgeViewModel> _currentCollection = [];
    internal List<BadgeViewModel> CurrentCollection
    {
        get => _currentCollection;

        private set
        {
            if ( value != null )
            {
                _currentCollection = value;
                NarrowNavigationIsVisible = _currentCollection.Count > 0;
            }
        }
    }

    private bool _isPreviousEnable;
    internal bool IsPreviousEnable
    {
        get => _isPreviousEnable;

        private set
        {
            _isPreviousEnable = value;
            OnPropertyChanged ();
        }
    }

    private bool _isNextEnable;
    internal bool IsNextEnable
    {
        get => _isNextEnable;

        private set
        {
            _isNextEnable = value;
            OnPropertyChanged ();
        }
    }

    private int _currentNumber;
    internal int CurrentNumber
    {
        get => _currentNumber;

        private set
        {
            _currentNumber = value;
            OnPropertyChanged ();
        }
    }

    internal List<BadgeViewModel> All { get; set; } = [];
    internal List<BadgeViewModel> Incorrects { get; set; } = [];
    internal List<BadgeViewModel> Corrects { get; set; } = [];
    internal Dictionary<int, BadgeViewModel?> Backup { get; private set; } = [];
    internal FilterState State { get; private set; } = FilterState.All;

    internal event Action<BadgeViewModel?, int, int>? WentToOther;
    internal event Action<FilterState>? FilterChanged;
    internal event Action? ScrollerHided;
    internal event Action? ScrollerShowed;

    public FilterViewModel ()
    {

    }

    internal void SetNames ( string [] names )
    {
        _allFilter = names [0];
        _correctFilter = names [1];
        _incorrectFilter = names [2];
        _allTip = names [3];
        _correctTip = names [4];
        _incorrectTip = names [5];
    }

    internal void SetUp ( List<BadgeViewModel> processables, List<BadgeViewModel> corrects, List<BadgeViewModel> incorrects ) 
    {
        SwitcherTip = _allTip;
        FilterNames = [_allFilter, _correctFilter, _incorrectFilter];

        All = processables;
        Incorrects = incorrects;
        Corrects = corrects;
        CurrentCollection = All;

        SetScrollerItemsCorrectWidth ( CurrentCollection.Count );
        CalcVisibleRange ( CurrentCollection != null ? CurrentCollection.Count : 0 );
        SetScroller ( CurrentCollection != null ? CurrentCollection.Count : 0 );

        IsNextEnable = true;
        SwitcherForeground = _white;
        ProcessableCount = CurrentCollection != null ? CurrentCollection.Count : 0;
        SelectedFilterIndex = 1;
        SelectedFilterIndex = 0;
    }

    internal void SetIcons ()
    {
        Icons = [];
        _iconsStorage?.Clear ();
        CurrentNumber = 1;

        if ( CurrentCollection != null && CurrentCollection.Count > 0 && _visibleRange > 0 )
        {
            for ( int index = 0; index < _visibleRange; index++ )
            {
                BadgeCorrectnessViewModel icon = new ( CurrentCollection.ElementAt ( index ), FilterIsExtended );
                icon.CalcStringPresentation ();
                Icons.Add ( icon );
                _iconsStorage?.Add ( icon );
                FadeIcon ( icon );
            }

            ActiveIcon = Icons [0];
            HighLightChosenIcon ( ActiveIcon );
        }
    }
}
