﻿using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Lister.Desktop.Extentions;
using Lister.Desktop.CoreModelReflections;
using Lister.Desktop.CoreModelReflections.BadgeVM;
using Lister.Desktop.Views.MainWindow.EditionView.ViewModel;

namespace View.EditionView.ViewModel;

public partial class BadgeEditorViewModel : ReactiveObject
{
    internal static event BackingToMainViewHandler BackingToMainViewEvent;

    private readonly string _extentionToolTip;
    private readonly string _shrinkingToolTip;
    private readonly BadgeComparer _comparer;
    private readonly double _startScale = 1.5624;

    private double _scale = 1.5624;
    private double _viewWidth = 830;
    private double _viewHeight = 500;
    private double _workAreaWidth = 580;
    private double _workAreaHeight = 410;
    private PageViewModel _firstPage;
    private Dictionary <BadgeViewModel, double> _scaleStorage;
    private TextLineViewModel _splittable;
    private TextLineViewModel _focusedLine;
    private bool _incorrectsAreSet = false;
    private BadgeEditorView _view;
    private int _visibleRangeStart = 0;
    private int _visibleRangeEnd;

    internal double WidthDelta;
    internal double HeightDelta;
    internal List<BadgeViewModel> AllNumbered { get; private set; }
    internal List<BadgeViewModel> IncorrectNumbered { get; private set; }
    internal List<BadgeViewModel> CorrectNumbered { get; private set; }
    internal Dictionary<int, BadgeViewModel> BackupNumbered { get; private set; }

    private double _workAreaWidt;
    internal double WorkAreaWidth
    {
        get { return _workAreaWidt; }
        set
        {
            this.RaiseAndSetIfChanged (ref _workAreaWidt, value, nameof (WorkAreaWidth));
        }
    }

    private double _workAreaHeightt;
    internal double WorkAreaHeight
    {
        get { return _workAreaHeightt; }
        set
        {
            this.RaiseAndSetIfChanged (ref _workAreaHeightt, value, nameof (WorkAreaHeight));
        }
    }

    private double _viewWidthh;
    internal double ViewWidth
    {
        get { return _viewWidthh; }
        set
        {
            this.RaiseAndSetIfChanged (ref _viewWidthh, value, nameof (ViewWidth));
        }
    }

    private double _viewHeightt;
    internal double ViewHeight
    {
        get { return _viewHeightt; }
        set
        {
            this.RaiseAndSetIfChanged (ref _viewHeightt, value, nameof (ViewHeight));
        }
    }

    private Thickness _margin;
    public Thickness Margin
    {
        get { return _margin; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _margin, value, nameof (Margin));
        }
    }

    private List <BadgeViewModel> _currentVisibleCollection;
    private List <BadgeViewModel> CurrentVisibleCollection 
    {
        get { return _currentVisibleCollection; }

        set 
        {
            if ( value != null )
            {
                TryEnableScroller (value.Count);
            }
            else 
            {
                TryEnableScroller (0);
            }

            _currentVisibleCollection = value;
        }
    }

    private int _incorrectBadgesCount;
    internal int IncorrectBadgesCount
    {
        get { return _incorrectBadgesCount; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _incorrectBadgesCount, value, nameof (IncorrectBadgesCount));
        }
    }

    private BadgeViewModel _processableBadge;
    internal BadgeViewModel BeingProcessedBadge
    {
        get { return _processableBadge; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _processableBadge, value, nameof (BeingProcessedBadge));
        }
    }

    private string _focusedText;
    internal string FocusedText
    {
        get { return _focusedText; }
        set
        {
            this.RaiseAndSetIfChanged (ref _focusedText, value, nameof (FocusedText));
        }
    }

    private int _processableCount;
    internal int ProcessableCount
    {
        get { return _processableCount; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _processableCount, value, nameof (ProcessableCount));
        }
    }

    private bool _moversEnabled;
    internal bool MoversAreEnable
    {
        get { return _moversEnabled; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _moversEnabled, value, nameof (MoversAreEnable));
        }
    }

    private bool _splitterEnabled;
    internal bool SplitterIsEnable
    {
        get { return _splitterEnabled; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _splitterEnabled, value, nameof (SplitterIsEnable));
        }
    }

    private string _extenderContent;
    internal string ExtenderContent
    {
        get { return _extenderContent; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _extenderContent, value, nameof (ExtenderContent));
        }
    }

    private string _extentionTip;
    internal string ExtentionTip
    {
        get { return _extentionTip; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _extentionTip, value, nameof (ExtentionTip));
        }
    }


    private static void OnBackingToMainView ()
    {
        if ( BackingToMainViewEvent == null )
        {
            return;
        }

        BackingToMainViewEvent ();
    }


    public BadgeEditorViewModel ( int incorrectBadgesAmmount, EditorViewModelArgs settingArgs )
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
        _focusedFontSizeColor = settingArgs.FocusedFontSizeColor;
        _releasedFontSizeColor = settingArgs.ReleasedFontSizeColor;
        _focusedFontSizeBorderColor = settingArgs.FocusedFontSizeBorderColor;
        _releasedFontSizeBorderColor = settingArgs.ReleasedFontSizeBorderColor;
        ViewWidth = _viewWidth;
        ViewHeight = _viewHeight;
        ExtenderContent = "\uF060";
        WorkAreaWidth = _workAreaWidth + _namesFilterWidth;
        WorkAreaHeight = _workAreaHeight;
        SetUpScrollBlock ( incorrectBadgesAmmount );
        SetUpZoommer ();
        SplitterIsEnable = false;
        FocusedFontSizeColor = _releasedFontSizeColor;
        FocusedFontSizeBorderColor = _releasedFontSizeBorderColor;
        Margin = new Thickness (0, 8);
    }


    internal void HandleDialogOpenig ()
    {
        Margin = new Thickness (0, -ViewHeight);
    }


    internal void HandleDialogClosing ()
    {
        Margin = new Thickness (0, 8);
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
        if ( BeingProcessedBadge == null ) 
        {
            return;
        }

        MoversAreEnable = false;
        SplitterIsEnable = false;
        ZoommerIsEnable = false;
        BeingProcessedBadge.CancelChanges ();
        ReleaseCaptured ();
    }


    internal void SetProcessables ( List <BadgeViewModel> processables, PageViewModel firstPage )
    {
        if ( ( processables == null ) || ( processables.Count == 0 ) ) return;

        _firstPage = firstPage;
        AllNumbered = new ();
        IncorrectNumbered = new ();
        CorrectNumbered = new ();
        BackupNumbered = new ();
        _scaleStorage = new ();

        foreach ( BadgeViewModel badge in processables )
        {
            _scaleStorage.Add ( badge, badge.Scale );
            AllNumbered.Add ( badge );

            if ( ! badge.IsCorrect )
            {
                IncorrectNumbered.Add (badge);
            }

            if ( badge.IsCorrect  )
            {
                CorrectNumbered.Add (badge);
            }

            if ( ! BackupNumbered.ContainsKey (badge.Id) )
            {
                BackupNumbered.Add (badge.Id, null);
            }
        }

        AllNumbered.Sort (_comparer);
        IncorrectNumbered.Sort (_comparer);
        CorrectNumbered.Sort (_comparer);
        CurrentVisibleCollection = AllNumbered;
        SetSliderWideness ();

        Dispatcher.UIThread.InvokeAsync
        (
            () =>
            {
                SetBeingProcessed (AllNumbered.ElementAt (0));
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
        VisibleIcons = new ObservableCollection<BadgeCorrectnessViewModel> ();
        _visibleIconsStorage = new ObservableCollection<BadgeCorrectnessViewModel> ();

        if ( ( CurrentVisibleCollection.Count > 0 ) && ( _visibleRange > 0 ) )
        {
            for ( int index = 0;   index < _visibleRange;   index++ )
            {
                BadgeViewModel boundBadge = CurrentVisibleCollection.ElementAt ( index );
                BadgeCorrectnessViewModel icon = new BadgeCorrectnessViewModel ( boundBadge, _extendedScrollableIconWidth
                                                               , _shrinkedIconWidth, _correctnessWidthLimit, FilterIsExtended );
                VisibleIcons.Add ( icon );
                _visibleIconsStorage.Add ( icon );
                FadeIcon ( icon );

                if ( index == 0 )
                {
                    ActiveIcon = icon;
                    HighLightChosenIcon ( icon );
                }
            }
        }
    }


    private void FadeIcon ( BadgeCorrectnessViewModel icon )
    {
        icon.BoundFontWeight = FontWeight.Normal;
        icon.CalcStringPresentation ( _correctnessWidthLimit );
    }


    internal void PassView ( BadgeEditorView view )
    {
        _view = view;
    }


    private bool ChangesExist ( )
    {
        bool exist = false;

        foreach ( BadgeViewModel badge   in   AllNumbered )
        {
            if ( badge.IsChanged )
            {
                exist = true;
                return exist;
            }
        }

        return exist;
    }


    internal void GoBackCommand ()
    {
        bool changesExist = ChangesExist ();

        if ( changesExist )
        {
            _view.CheckBacking ();
        }
        else
        {
            GoBack ();
        }
    }


    internal void ComplateGoBack ( BadgeEditorView caller )
    {
        if ( _view.Equals(caller) )
        {
            GoBack();
        }
        
    }


    private void GoBack ()
    {
        foreach ( KeyValuePair <BadgeViewModel, double> badgeToScale   in   _scaleStorage )
        {
            BadgeViewModel badge = badgeToScale.Key;
            double scale = badgeToScale.Value;

            if ( scale != badge.Scale ) 
            {
                badge.ZoomOut (badge.Scale);
                SetOriginalScale (badge, scale);
            }

            if ( badge.IsChanged ) 
            {
                badge.IsChanged = false;
            }
        }

        ReleaseCaptured ();
        OnBackingToMainView ();
        _scale = _startScale;
        _view.CompleteBacking ();
        _firstPage.Show ();
    }


    private void SetBeingProcessed ( BadgeViewModel beingProcessed )
    {
        beingProcessed.Show ();
        BeingProcessedBadge = beingProcessed;
        SetToCorrectScale (BeingProcessedBadge);
        BeingProcessedNumber = 1;
    }



    private enum FilterChoosing 
    {
       All = 0,
       Corrects = 1,
       Incorrects = 2
    }
}
