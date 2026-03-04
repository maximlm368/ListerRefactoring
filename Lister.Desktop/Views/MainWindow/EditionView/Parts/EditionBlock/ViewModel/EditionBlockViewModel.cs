using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.Views.MainWindow.EditionView.Parts.Edition.ViewModel;

internal partial class EditionBlockViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isZoommerEnabled;

    [ObservableProperty]
    private bool _isSplitterEnabled;

    [ObservableProperty]
    private bool _isCancelEnabled;

    [ObservableProperty]
    private int _processableCount;

    [ObservableProperty]
    private int _incorrectCount;

    private BadgeViewModel? _processable;
    public BadgeViewModel? Processable
    {
        get => _processable;

        set
        {
            _processable = value;
            IsZoommerEnabled = false;
            IsCancelEnabled = _processable != null && _processable.IsChanged;

            if ( _processable != null )
            {
                _processable.Model.Changed += ( isChanged ) =>
                {
                    IsCancelEnabled = isChanged;
                };
            }

            OnPropertyChanged ();
        }
    }

    internal event Action? CancelHappend;
    internal event Action? DecreaseFontSizeHappend;
    internal event Action? IncreaseFontSizeHappend;
    internal event Action? SplitHappend;

    public EditionBlockViewModel ()
    {
        IsSplitterEnabled = false;
    }

    internal void RefreshIncorrectCount ( bool isReduced )
    {
        IncorrectCount = isReduced ? IncorrectCount - 1 : IncorrectCount + 1;
    }

    internal void RefreshState ( BadgeViewModel? newProcessable, int count, int incorrectCount )
    {
        Processable = newProcessable;
        ProcessableCount = count;
        IncorrectCount = incorrectCount;
        IsSplitterEnabled = false;
    }

    [RelayCommand]
    internal void Cancel ()
    {
        IsSplitterEnabled = false;
        IsZoommerEnabled = false;
        Processable?.ReleaseFocused ();
        CancelHappend?.Invoke ();
    }

    [RelayCommand]
    internal void Split ()
    {
        IsSplitterEnabled = false;
        SplitHappend?.Invoke ();
    }

    [RelayCommand]
    internal void IncreaseFontSize ()
    {
        IncreaseFontSizeHappend?.Invoke ();
    }

    [RelayCommand]
    internal void ReduceFontSize ()
    {
        DecreaseFontSizeHappend?.Invoke ();
    }

    internal void SetProcessableText ( string content, int elementNumber )
    {
        if ( Processable != null && Processable.FocusedLine != null )
        {
            IsZoommerEnabled = true;
            EnableSplitting ( content, elementNumber );
        }
    }

    private void EnableSplitting ( string content, int elementNumber )
    {
        TextLineViewModel? line = Processable?.GetCoincidence ( content, elementNumber );

        if ( line == null )
        {
            return;
        }

        string [] strings = content.Split ( [' ', '-'], StringSplitOptions.RemoveEmptyEntries );
        IsSplitterEnabled = strings.Length > 1;
    }
}
