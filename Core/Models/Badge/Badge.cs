namespace Core.Models.Badge;

public class Badge
{
    private static int _lastId;
    private static Dictionary<int, Badge> _backup = new ();

    public Thickness Margin { get; internal set; }
    public int Id { get; private set; }
    public Person Person { get; private set; }
    public string BackgroundImagePath { get; private set; }
    public Layout Layout { get; private set; }
    public bool IsCorrect { get; private set; }
    public bool IsChanged { get; private set; }

    public delegate void RolledBackHandler ();
    public event RolledBackHandler ? RolledBack;

    public delegate void CorrectnessChangedHandler ();
    public event CorrectnessChangedHandler? CorrectnessChanged;


    private Badge ( ){}


    private Badge ( Person person, string backgroundImagePath, Layout layout )
    {
        Person = person;
        BackgroundImagePath = backgroundImagePath;
        Layout = layout;

        Dictionary<string, string> personProperties = Person.GetProperties ();

        Layout.SetUpComponents ( personProperties );
        IsCorrect = ! Layout.HasIncorrectLines;
        Layout.RolledBack += LayoutRolledBackHandler;
    }


    private void LayoutRolledBackHandler ()
    {
        RefreshCorrectness ();
        IsChanged = false;
        RolledBack?.Invoke ();
    }


    public static Badge ? GetBadge ( Person person, string backgroundImagePath, Layout layout )
    {
        bool isArgumentNull = ( person == null ) 
                              ||
                              backgroundImagePath == null
                              ||
                              layout == null;

        if ( isArgumentNull )
        {
            return null;
        }

        Badge result = new Badge ( person, backgroundImagePath, layout );
        result.Id = _lastId;
        _lastId++;

        return result;
    }


    public void ZeroProcessable ()
    {
        Layout?.ZeroProcessable ();
    }


    public static void ClearSharedData ()
    {
        _backup = new ();
        _lastId = 0;
    }


    public void Split ( TextLine splitable )
    {
        Layout.Split ( splitable );
        IsChanged = true;
    }


    public void SetProcessable ( LayoutComponentBase processableComponent )
    {
        SetBackup ();
        Layout.SetProcessable ( processableComponent );
    }


    private void SetBackup ()
    {
        if ( _backup.ContainsKey ( Id ) )
        {
            return;
        }

        Badge backup = new Badge ();
        backup.Layout = Layout.Clone ( false );
        backup.IsCorrect = IsCorrect;
        _backup.Add ( Id, backup );
    }


    public void CancelChanges ()
    {
        Layout.RollBackTo ( _backup [Id].Layout.Clone(false) );
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


    public void IncreaseFontSize ( )
    {
        IsChanged = true;
        Layout.IncreaseFontSize ( );
        RefreshCorrectness ();
    }


    public void ReduceFontSize ( )
    {
        IsChanged = true;
        Layout.ReduceFontSize ( );
        RefreshCorrectness ();
    }


    public void ResetProcessableContent ( string newContent )
    {
        IsChanged = true;
        Layout.ResetProcessableLineContent ( newContent );
        RefreshCorrectness();
    }


    private void RefreshCorrectness ( )
    {
        bool isCorrect = IsCorrect;
        IsCorrect = ! Layout.HasIncorrectLines;

        if ( isCorrect != IsCorrect ) 
        {
            CorrectnessChanged?.Invoke ();
        }
    }
}