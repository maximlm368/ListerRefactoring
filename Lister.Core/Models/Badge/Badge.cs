namespace Lister.Core.Models.Badge;

/// <summary>
/// Represents data of badge containing background image, person and layout as set of movable components.
/// </summary>
public sealed class Badge
{
    private static int _lastId;
    private static Dictionary<int, Layout> _backup = [];

    /// <summary>
    /// Is margin in badge line of page for one unit (may be on display or printer) thickness frame.
    /// </summary>
    public Thickness? Margin { get; internal set; }
    public int Id { get; private set; }
    public Person Person { get; private set; }
    public string? BackgroundImagePath { get; private set; }
    public Layout Layout { get; private set; }
    public bool IsCorrect { get; private set; }
    public bool IsChanged { get; private set; }
    public delegate void RolledBackHandler ();
    public event RolledBackHandler? RolledBack;
    public delegate void CorrectnessChangedHandler ();
    public event CorrectnessChangedHandler? CorrectnessChanged;

    private Badge ( Person person, string backgroundImagePath, Layout layout )
    {
        Person = person;
        BackgroundImagePath = backgroundImagePath;
        Layout = layout;
        Dictionary<string, string> personProperties = Person.GetProperties ();
        Layout.SetUpComponents ( personProperties );
        IsCorrect = !Layout.HasIncorrectLines;
        Layout.RolledBack += LayoutRolledBackHandler;
    }

    private void LayoutRolledBackHandler ()
    {
        RefreshCorrectness ();
        IsChanged = false;
        RolledBack?.Invoke ();
    }

    public static Badge? GetBadge ( Person person, string backgroundImagePath, Layout layout )
    {
        if ( person == null
             || backgroundImagePath == null
             || layout == null
        )
        {
            return null;
        }

        Badge result = new ( person, backgroundImagePath, layout )
        {
            Id = _lastId
        };

        _lastId++;

        return result;
    }

    public void ZeroProcessable ()
    {
        Layout?.ZeroProcessable ();
    }

    public static void ClearSharedData ()
    {
        _backup = [];
        _lastId = 0;
    }

    public void Split ( TextLine splitable )
    {
        Layout?.Split ( splitable );
        IsChanged = true;
    }

    public void SetProcessable ( LayoutComponentBase processableComponent )
    {
        SetBackup ();
        Layout?.SetProcessable ( processableComponent );
    }

    private void SetBackup ()
    {
        if ( _backup.ContainsKey ( Id ) )
        {
            return;
        }

        _backup.Add ( Id, Layout.Clone ( false ) );
    }

    public void CancelChanges ()
    {
        Layout?.RollBackTo ( _backup [Id].Clone ( false ) );
    }

    public void ShiftProcessable ( string direction )
    {
        IsChanged = true;
        Layout.ShiftProcessable ( direction );
        RefreshCorrectness ();
    }

    public void MoveProcessable ( double verticalDelta, double horizontalDelta )
    {
        IsChanged = true;
        Layout.MoveProcessable ( verticalDelta, horizontalDelta );
        RefreshCorrectness ();
    }

    public void IncreaseFontSize ()
    {
        IsChanged = true;
        Layout.IncreaseFontSize ();
        RefreshCorrectness ();
    }

    public void ReduceFontSize ()
    {
        IsChanged = true;
        Layout.ReduceFontSize ();
        RefreshCorrectness ();
    }

    public void ResetProcessableContent ( string newContent )
    {
        IsChanged = true;
        Layout.ResetProcessableLineContent ( newContent );
        RefreshCorrectness ();
    }

    private void RefreshCorrectness ()
    {
        bool isCorrect = IsCorrect;
        IsCorrect = !Layout.HasIncorrectLines;

        if ( isCorrect != IsCorrect )
        {
            CorrectnessChanged?.Invoke ();
        }
    }
}