using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.Components.Navigator.ViewModel;
using Lister.Desktop.Components.Zoomer.ViewModel;
using Lister.Desktop.Entities.BadgeVM;
using Lister.Desktop.Extentions;
using Lister.Desktop.Views.EditionView.Parts.Edition.ViewModel;
using Lister.Desktop.Views.EditionView.Parts.Filter.ViewModel;
using Lister.Desktop.Views.EditionView.Parts.WorkArea.ViewModel;
using Lister.Desktop.Views.WaitingView.ViewModel;
using System.Collections.ObjectModel;

namespace Lister.Desktop.Views.EditionView.ViewModel;

internal sealed partial class EditorViewModel : ObservableObject
{
    public static event Action? BackingComplated;
    public static event Action<Action, Action>? BackingActivated;

    private readonly BadgeComparer _comparer;
    private readonly Dictionary<BadgeViewModel, double> _scaleStorage = [];

    [ObservableProperty]
    private ObservableCollection<string>? _filterNames;

    internal List<BadgeViewModel> All { get; private set; } = [];
    internal List<BadgeViewModel> Incorrects { get; private set; } = [];
    internal List<BadgeViewModel> Corrects { get; private set; } = [];
    internal Dictionary<int, BadgeViewModel?> BackupNumbered { get; private set; } = [];
    internal NavigatorViewModel Navigator { get; private set; }
    internal ZoomerViewModel Zoomer { get; private set; }

    private List<BadgeViewModel> _currentVisibleCollection;
    private List<BadgeViewModel> CurrentVisibleCollection
    {
        get => _currentVisibleCollection;

        set
        {
            if ( value != null )
            {
                _currentVisibleCollection = value;
            }
        }
    }

    internal EditionBlockViewModel Editor { get; private set; }
    internal WorkAreaViewModel WorkArea { get; private set; }
    internal FilterViewModel Filter { get; private set; }
    internal WaitingViewModel Waiting { get; private set; }

    public EditorViewModel ( EditionBlockViewModel editor, WorkAreaViewModel workArea, FilterViewModel filter,
        NavigatorViewModel navigator, ZoomerViewModel zoomer, WaitingViewModel waiting )
    {
        Editor = editor;
        WorkArea = workArea;
        Filter = filter;
        Navigator = navigator;
        Zoomer = zoomer;
        Waiting = waiting;

        _comparer = new BadgeComparer ();
        _currentVisibleCollection = All;

        SetNavigatorEvents ();
        SetZoomerEvents ();

        Filter.FilterChanged += ToAppropriateState;
        Filter.WentToOther += SetProcessable;

        WorkArea.ElementGotFocus += Editor.SetProcessableText;
        WorkArea.ElementLostFocus += ReleaseEditor;

        WorkArea.CorrectnessChanged += ( gotCorrect ) => 
        {
            Editor.RefreshIncorrectCount ( gotCorrect );
        };

        Editor.IncreaseFontSizeHappend += WorkArea.IncreaseFontSize;
        Editor.DecreaseFontSizeHappend += WorkArea.DecreaseFontSize;
        Editor.SplitHappend += WorkArea.Split;
        Editor.CancelHappend += WorkArea.CancelChanges;
    }

    private void SetNavigatorEvents ()
    {
        Navigator.WentToFirst += Filter.ToFirst;
        Navigator.WentToLast += Filter.ToLast;

        Navigator.WentToNext += ( number ) =>
        {
            Filter.ToNext ();
        };

        Navigator.WentToPrevious += ( number ) =>
        {
            Filter.ToPrevious ();
        };
    }

    private void SetZoomerEvents ()
    {
        Zoomer.ZoomedOn += WorkArea.ZoomOn;
        Zoomer.ZoomedOut += WorkArea.ZoomOut;
    }

    private void ReleaseEditor () 
    {
        Editor.IsSplitterEnabled = false;
        Editor.IsZoommerEnabled = false;
    }

    private void SetProcessable ( BadgeViewModel? processable, int numberInCollection, int collectionCount ) 
    {
        if ( numberInCollection > CurrentVisibleCollection.Count )
        {
            return;
        }

        WorkArea.SetProcessable ( processable );
        Editor.RefreshState ( processable, collectionCount, Incorrects.Count );
        Navigator?.EnableNavigation ( collectionCount, numberInCollection );
        Zoomer?.EnableZoom ( );

        if ( Filter.State == FilterState.Corrects )
        {
            Editor.IncorrectCount = 0;
        }
    }

    private void ToAppropriateState ( FilterState filterState )
    {
        List<BadgeViewModel>? badges = null;

        if ( filterState == FilterState.All )
        {
            Editor.Processable = All [0];
            WorkArea.SetProcessable ( All [0] );
            Editor.IncorrectCount = Incorrects.Count;
            Editor.ProcessableCount = All.Count;
            badges = CurrentVisibleCollection = All;
        }
        else if ( filterState == FilterState.Incorrects )
        {
            Editor.Processable = Incorrects.Count > 0 ? Incorrects [0] : null;
            WorkArea.SetProcessable ( Incorrects.Count > 0 ? Incorrects [0] : null );
            Editor.IncorrectCount = Incorrects.Count;
            Editor.ProcessableCount = Incorrects.Count;
            badges = CurrentVisibleCollection = Incorrects;
        }
        else if ( filterState == FilterState.Corrects )
        {
            Editor.Processable = Corrects.Count > 0 ? Corrects [0] : null;
            WorkArea.SetProcessable (Corrects.Count > 0 ? Corrects [0] : null);
            Editor.IncorrectCount = 0;
            Editor.ProcessableCount = Corrects.Count;
            badges = CurrentVisibleCollection = Corrects;
        }

        Navigator?.EnableNavigation ( badges == null ? 0 : badges.Count, badges != null && badges.Count > 0 ? 1 : 0 );
        Zoomer?.EnableZoom ( );
    }

    private static void SetOriginalScale ( BadgeViewModel beingPrecessed, double scale )
    {
        if ( scale != 1 )
        {
            beingPrecessed.ZoomOn ( scale );
        }
    }

    internal void HandleDialogOpenig ()
    {
        Waiting?.Show ();
    }

    internal void HandleDialogClosing ()
    {
        Waiting?.Hide ();
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
            All.Add ( badge );

            if ( !badge.IsCorrect )
            {
                Incorrects.Add ( badge );
            }

            if ( badge.IsCorrect )
            {
                Corrects.Add ( badge );
            }

            if ( !BackupNumbered.ContainsKey ( badge.Id ) )
            {
                BackupNumbered.Add ( badge.Id, null );
            }
        }

        All.Sort ( _comparer );
        Incorrects.Sort ( _comparer );
        Corrects.Sort ( _comparer );

        Editor.ProcessableCount = All.Count;
        Editor.IncorrectCount = Incorrects.Count;
        Editor.Processable = All [0];

        WorkArea.SetUp ( All [0] );
        
        Dispatcher.UIThread.InvokeAsync
        (
            () =>
            {
                Filter.SetUp ( All, Corrects, Incorrects );
                Filter.SetIcons ();
            }
        );
    }

    internal void FocusedToSide ( string direction )
    {
        WorkArea?.FocusedToSide ( direction );
    }

    private bool IsChangesExist ()
    {
        foreach ( BadgeViewModel badge in All )
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
        if ( IsChangesExist () )
        {
            HandleDialogOpenig ();
            BackingActivated?.Invoke (HandleDialogClosing, GoBack);
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

        WorkArea.ReleaseCaptured ();
        BackingComplated?.Invoke ();
    }
}
