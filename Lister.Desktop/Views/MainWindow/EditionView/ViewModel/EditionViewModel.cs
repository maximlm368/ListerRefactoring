using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.Extentions;
using Lister.Desktop.ModelMappings.BadgeVM;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

internal sealed partial class EditorViewModel : ObservableObject
{
    private readonly string _extentionToolTip;
    private readonly string _shrinkingToolTip;
    private readonly BadgeComparer _comparer;
    private readonly double _startScale = 1.5624;
    private double _scale = 1.5624;
    private readonly double _viewWidthh = 830;
    private readonly double _viewHeightt = 500;
    private readonly double _workAreaWidthh = 580;
    private readonly double _workAreaHeightt = 410;
    private readonly Dictionary<BadgeViewModel, double> _scaleStorage = [];
    private int _visibleRangeEnd;

    internal List<BadgeViewModel> AllNumbered { get; private set; } = [];
    internal List<BadgeViewModel> IncorrectNumbered { get; private set; } = [];
    internal List<BadgeViewModel> CorrectNumbered { get; private set; } = [];
    internal Dictionary<int, BadgeViewModel?> BackupNumbered { get; private set; } = [];

    [ObservableProperty]
    private double _workAreaWidth;

    [ObservableProperty]
    private double _workAreaHeight;

    [ObservableProperty]
    private double _viewWidth;

    [ObservableProperty]
    private double _viewHeight;

    [ObservableProperty]
    private Thickness _margin = new ( 0, 8 );

    private List<BadgeViewModel> _currentVisibleCollection;
    private List<BadgeViewModel> CurrentVisibleCollection
    {
        get => _currentVisibleCollection;

        set
        {
            if ( value != null )
            {
                TryEnableScroller ( value.Count );

                _currentVisibleCollection = value;
            }
            else
            {
                TryEnableScroller ( 0 );
            }
        }
    }

    [ObservableProperty]
    private int _incorrectBadgesCount;

    [ObservableProperty]
    private BadgeViewModel? _processableBadge;

    [ObservableProperty]
    private string? _focusedText;

    [ObservableProperty]
    private int _processableCount;

    [ObservableProperty]
    private bool _moversAreEnable;

    [ObservableProperty]
    private bool _splitterIsEnable;

    [ObservableProperty]
    private string _extenderContent;

    [ObservableProperty]
    private string? _extentionTip;

    [ObservableProperty]
    private ObservableCollection<BadgeCorrectnessViewModel> _visibleIcons = [];

    public delegate void BackingActivatedHandler ();
    public event BackingActivatedHandler? BackingActivated;

    public delegate void BackingComplatedHandler ();
    public event BackingComplatedHandler? BackingComplated;

    public delegate void PeopleGotEmptyHandler ();
    public event PeopleGotEmptyHandler? PeopleGotEmpty;

    private enum FilterChoosing
    {
        All = 0,
        Corrects = 1,
        Incorrects = 2
    }

    public EditorViewModel ( int incorrectBadgesAmmount, EditorViewModelArgs settingArgs )
    {
        _comparer = new BadgeComparer ();
        _extentionToolTip = settingArgs.ExtentionToolTip;
        _shrinkingToolTip = settingArgs.ShrinkingToolTip;
        _allFilter = settingArgs.AllFilter;
        _correctFilter = settingArgs.CorrectFilter;
        _incorrectFilter = settingArgs.IncorrectFilter;
        _allTip = settingArgs.AllTip;
        _correctTip = settingArgs.CorrectTip;
        _incorrectTip = settingArgs.IncorrectTip;
        _focusedFontsizeColor = settingArgs.FocusedFontSizeColor;
        _releasedFontsizeColor = settingArgs.ReleasedFontSizeColor;
        _focusedFontsizeBorderColor = settingArgs.FocusedFontSizeBorderColor;
        _releasedFontsizeBorderColor = settingArgs.ReleasedFontSizeBorderColor;
        ViewWidth = _viewWidthh;
        ViewHeight = _viewHeightt;
        ExtenderContent = "\uF060";
        WorkAreaWidth = _workAreaWidthh + _namesFilterWidthh;
        WorkAreaHeight = _workAreaHeightt;
        SetUpScrollBlock ( incorrectBadgesAmmount );
        SetUpZoommer ();
        SplitterIsEnable = false;
        FocusedFontSizeColor = _releasedFontsizeColor;
        FocusedFontSizeBorderColor = _releasedFontsizeBorderColor;

        _currentVisibleCollection = AllNumbered;
    }

    internal void HandleDialogOpenig ()
    {
        Margin = new Thickness ( 0, -( ViewHeight - 8 ) );
    }

    internal void HandleDialogClosing ()
    {
        Margin = new Thickness ( 0, 8 );
    }

    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        ViewWidth -= widthDifference;
        ViewHeight -= heightDifference;
        WorkAreaWidth -= widthDifference;
        WorkAreaHeight -= heightDifference;
        EntireBlockHeight -= heightDifference;
    }

    internal void CancelChanges ()
    {
        if ( ProcessableBadge == null )
        {
            return;
        }

        MoversAreEnable = false;
        SplitterIsEnable = false;
        ZoommerIsEnable = false;
        ProcessableBadge.CancelChanges ();
        ReleaseCaptured ();
    }

    internal void SetProcessables ( List<BadgeViewModel> processables )
    {
        if ( ( processables == null ) || ( processables.Count == 0 ) )
        {
            return;
        }

        _scaleStorage.Clear ();

        foreach ( BadgeViewModel badge in processables )
        {
            _scaleStorage.Add ( badge, badge.Scale );
            AllNumbered.Add ( badge );

            if ( !badge.IsCorrect )
            {
                IncorrectNumbered.Add ( badge );
            }

            if ( badge.IsCorrect )
            {
                CorrectNumbered.Add ( badge );
            }

            if ( !BackupNumbered.ContainsKey ( badge.Id ) )
            {
                BackupNumbered.Add ( badge.Id, null );
            }
        }

        AllNumbered.Sort ( _comparer );
        IncorrectNumbered.Sort ( _comparer );
        CorrectNumbered.Sort ( _comparer );
        CurrentVisibleCollection = AllNumbered;
        SetSliderWideness ();

        Dispatcher.UIThread.InvokeAsync
        (
            () =>
            {
                SetBeingProcessed ( AllNumbered.ElementAt ( 0 ) );
                EnableNavigationIfShould ();
                SetVisibleIcons ();
                ScrollOffset = 0;
                ProcessableCount = AllNumbered.Count;
                IncorrectBadgesCount = IncorrectNumbered.Count;
            }
        );
    }

    private void SetVisibleIcons ()
    {
        VisibleIcons ??= [];
        _visibleIconsStorage?.Clear ();

        if ( CurrentVisibleCollection != null && CurrentVisibleCollection.Count > 0 && _visibleRange > 0 )
        {
            for ( int index = 0; index < _visibleRange; index++ )
            {
                BadgeCorrectnessViewModel icon = new ( CurrentVisibleCollection.ElementAt ( index ), _extendedScrollableIconWidth, 
                    _shrinkedIconWidth, _correctnessWidthLimit, FilterIsExtended 
                );

                VisibleIcons.Add ( icon );
                _visibleIconsStorage?.Add ( icon );
                FadeIcon ( icon );

                if ( index == 0 )
                {
                    ActiveIcon = icon;
                    HighLightChosenIcon ( icon );
                }
            }
        }
    }

    private void FadeIcon ( BadgeCorrectnessViewModel? icon )
    {
        if ( icon == null ) 
        {
            return;
        }

        icon.BoundFontWeight = FontWeight.Normal;
        icon.CalcStringPresentation ( _correctnessWidthLimit );
    }

    private bool ChangesExist ()
    {
        foreach ( BadgeViewModel badge in AllNumbered )
        {
            if ( badge.IsChanged )
            {
                return true;
            }
        }

        return false;
    }

    [RelayCommand]
    internal void Back ()
    {
        if ( ChangesExist () )
        {
            BackingActivated?.Invoke ();
        }
        else
        {
            GoBack ();
        }
    }

    internal void GoBack ()
    {
        foreach ( KeyValuePair<BadgeViewModel, double> badgeToScale in _scaleStorage )
        {
            BadgeViewModel badge = badgeToScale.Key;
            double scale = badgeToScale.Value;

            if ( scale != badge.Scale )
            {
                badge.ZoomOut ( badge.Scale );
                SetOriginalScale ( badge, scale );
            }

            if ( badge.IsChanged )
            {
                badge.IsChanged = false;
            }
        }

        ReleaseCaptured ();
        _scale = _startScale;
        BackingComplated?.Invoke ();
    }

    private void SetBeingProcessed ( BadgeViewModel beingProcessed )
    {
        beingProcessed.Show ();
        ProcessableBadge = beingProcessed;
        SetToCorrectScale ( ProcessableBadge );
        BeingProcessedNumber = 1;
    }
}
