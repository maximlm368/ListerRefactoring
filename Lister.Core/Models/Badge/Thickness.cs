namespace Lister.Core.Models.Badge;

public class Thickness 
{
    public double Left { get; private set; }
    public double Top { get; private set; }
    public double Right { get; private set; }
    public double Bottom { get; private set; }


    public Thickness ( )
    {
        Left = 0;
        Top = 0;
        Right = 0;
        Bottom = 0;
    }


    public Thickness ( double thickness )
    {
        Left = thickness;
        Top = thickness;
        Right = thickness;
        Bottom = thickness;
    }


    public Thickness ( double left, double top ) 
    {
        Left = left;
        Top = top;
        Right = 0;
        Bottom = 0;
    }


    public Thickness ( double left, double top, double right, double bottom )
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}