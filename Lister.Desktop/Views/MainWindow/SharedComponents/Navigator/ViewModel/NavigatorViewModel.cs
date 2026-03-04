using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.MainWindow.SharedComponents.Navigator.ViewModel;

public partial class NavigatorViewModel : ObservableObject
{
    private bool _isZeroState = true;

    [ObservableProperty]
    private bool _isFirstEnable;

    [ObservableProperty]
    private bool _isPreviousEnable;

    [ObservableProperty]
    private bool _isNextEnable;

    [ObservableProperty]
    private bool _isLastEnable;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private bool _isEmptyNumberVisible;

    private int _currentNumber;
    internal int CurrentNumber
    {
        get => _currentNumber;

        set
        {
            if ( value <= Count && value >= 0 || value == 1 )
            {

            }

            _currentNumber = value;
            EnableNavigation ();
            CurrentNumberChanged?.Invoke ( _currentNumber );
        }
    }

    internal event Action WentToFirst;
    internal event Action WentToLast;
    internal event Action<int> WentToNext;
    internal event Action<int> WentToPrevious;
    internal event Action<int> WentToNumber;
    internal event Action<int> CurrentNumberChanged;

    public NavigatorViewModel ( )
    {
        ToZeroState ();
    }

    internal void ToZeroState ()
    {
        IsFirstEnable = false;
        IsPreviousEnable = false;
        IsNextEnable = false;
        IsLastEnable = false;

        _isZeroState = true;

        CurrentNumber = 1;
        Count = 0;
    }

    internal void EnableNavigation ( int count, int currentVisibleNumber )
    {
        if ( count >= 0 )
        {
            Count = count;
            CurrentNumber = currentVisibleNumber;
            EnableNavigation ();
        }
    }

    private void EnableNavigation ()
    {
        if ( _isZeroState ) 
        {
            _isZeroState = false;

            return;
        }

        IsEmptyNumberVisible = false;

        if ( CurrentNumber > 1 && CurrentNumber == Count )
        {
            IsFirstEnable = true;
            IsPreviousEnable = true;
            IsNextEnable = false;
            IsLastEnable = false;
        }
        else if ( CurrentNumber > 1 && CurrentNumber < Count )
        {
            IsFirstEnable = true;
            IsPreviousEnable = true;
            IsNextEnable = true;
            IsLastEnable = true;
        }
        else if ( CurrentNumber == 1 && Count == 1 )
        {
            IsFirstEnable = false;
            IsPreviousEnable = false;
            IsNextEnable = false;
            IsLastEnable = false;
        }
        else if ( CurrentNumber == 1 && Count > 1 )
        {
            IsFirstEnable = false;
            IsPreviousEnable = false;
            IsNextEnable = true;
            IsLastEnable = true;
        }
        else if ( Count == 0 )
        {
            IsFirstEnable = false;
            IsPreviousEnable = false;
            IsNextEnable = false;
            IsLastEnable = false;

            if ( CurrentNumber == 0 )
            {
                IsEmptyNumberVisible = true;
            }
        }
    }

    [RelayCommand]
    internal void ToFirst ()
    {
        if ( CurrentNumber > 1 )
        {
            CurrentNumber = 1;
        }

        WentToFirst?.Invoke ();
    }

    [RelayCommand]
    internal void ToPrevious ()
    {
        if ( CurrentNumber > 1 )
        {
            CurrentNumber--;
        }

        WentToPrevious?.Invoke ( CurrentNumber );
    }

    [RelayCommand]
    internal void ToNext ()
    {
        if ( CurrentNumber < Count )
        {
            CurrentNumber++;
        }

        WentToNext?.Invoke ( CurrentNumber );
    }

    [RelayCommand]
    internal void ToLast ()
    {
        if ( CurrentNumber < Count )
        {
            CurrentNumber = Count;
        }

        WentToLast?.Invoke ();
    }

    internal void ToNumber ( int number )
    {
        CurrentNumber = number;
        WentToNumber?.Invoke ( CurrentNumber );
    }

    internal void RecoverPageCounterIfEmpty ()
    {
        CurrentNumber = 1;
    }
}
