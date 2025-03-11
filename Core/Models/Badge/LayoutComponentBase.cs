namespace Core.Models.Badge;

/// <summary>
/// Defines shared data and functionality for badge layout component such as image, shape or text.
/// </summary>
public abstract class LayoutComponentBase
{
    public double Width { get; protected set; }
    public double Height { get; protected set; }

    public double TopOffset { get; set; }
    public double LeftOffset { get; set; }

    public double WidthWithBorder { get; protected set; }
    public double HeightWithBorder { get; protected set; }

    public delegate void ChangedHandler ( LayoutComponentBase source );
    public event ChangedHandler ? Changed;


    public void Shift ( string direction, Thickness restrictions )
    {
        bool isShifted = false;

        if ( direction == "Left" )
        {
            isShifted = Shift ( 0, 1, restrictions );
        }
        else if ( direction == "Right" )
        {
            isShifted = Shift ( 0, -1, restrictions );
        }
        else if ( direction == "Up" )
        {
            isShifted = Shift ( 1, 0, restrictions );
        }
        else if ( direction == "Down" )
        {
            isShifted = Shift ( -1, 0, restrictions );
        }

        if ( isShifted ) Changed?.Invoke ( this );
    }


    public void Move ( double verticalDelta, double horizontalDelta, Thickness restrictions )
    {
        bool isShifted = false;

        isShifted = Shift ( verticalDelta, horizontalDelta, restrictions );
        
        if ( isShifted ) Changed?.Invoke ( this );
    }


    private bool Shift ( double verticalDelta, double horizontalDelta, Thickness restrictions )
    {
        double currentLeftOffset = LeftOffset - horizontalDelta;
        double currentTopOffset = TopOffset - verticalDelta;

        bool shiftIsUnpossible = ( currentLeftOffset < restrictions.Left )
                               || ( currentLeftOffset > restrictions.Right )
                               || ( currentTopOffset > restrictions.Bottom )
                               || ( currentTopOffset < restrictions.Top );

        if ( shiftIsUnpossible ) 
        {
            return false;
        }

        TopOffset -= verticalDelta;
        LeftOffset -= horizontalDelta;

        return true;
    }
}