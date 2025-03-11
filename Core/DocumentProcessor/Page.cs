using Core.Models.Badge;

namespace Core.DocumentProcessor;

/// <summary>
/// Represents lines of badges on page.
/// Adds sent badge.
/// </summary>
public class Page
{
    private static double _topOffsetOfContent;
    private static double _leftOffsetOfContent;
    private static double _width;
    private static double _height;

    private BadgeLine _fillableLine;

    public double Width { get; private set; }
    public double Height { get; private set; }

    public double ContentTopOffset { get; private set; }
    public double ContentLeftOffset { get; private set; }

    public List <BadgeLine> Lines { get; private set; }
    public int BadgeCount { get; private set; }

    public delegate void ComplatingHandler ( Page complated );
    public static event ComplatingHandler ? Complated;


    static Page ()
    {
        _width = 794;
        _height = 1123;
        
        _topOffsetOfContent = 20;
        _leftOffsetOfContent = 45;
    }


    public Page ( )
    {
        Lines = new ();
        BadgeCount = 0;
        Width = _width;
        Height = _height;
        ContentTopOffset = _topOffsetOfContent;
        ContentLeftOffset = _leftOffsetOfContent;
        double usefullHeight = Height - 20;

        _fillableLine = new BadgeLine ( Width, usefullHeight, true );
        Lines.Add ( _fillableLine );
    }


    internal Page AddAndGetIncludingPage ( Badge badge )
    {
        Page fillablePage = this;
        AdditionSuccess additionSuccess = _fillableLine.AddBadge ( badge );

        if ( additionSuccess == AdditionSuccess.FailureByWidth )
        {
            double restHeight = GetRestHeight ();
            bool isFirstLine = ( Lines.Count == 0 );
            BadgeLine newLine = new BadgeLine ( Width, restHeight, isFirstLine );
            additionSuccess = newLine.AddBadge ( badge );

            if ( additionSuccess == AdditionSuccess.FailureByHeight )
            {
                fillablePage = new Page ( );
                fillablePage.BadgeCount--;
                fillablePage.AddAndGetIncludingPage ( badge );
            }

            fillablePage.Lines.Add ( newLine );
            fillablePage._fillableLine = newLine;
        }

        if ( additionSuccess == AdditionSuccess.FailureByHeight )
        {
            Complated?.Invoke ( this );
            fillablePage = new Page ( );
            fillablePage.BadgeCount--;
            fillablePage.AddAndGetIncludingPage ( badge );
        }

        fillablePage.BadgeCount++;

        return fillablePage;
    }


    private double GetRestHeight ()
    {
        double summaryHeight = 0;

        foreach ( BadgeLine line in Lines )
        {
            summaryHeight += line.Height;
        }

        double restHeight = Height - summaryHeight;
        return restHeight;
    }


    internal void Clear ()
    {
        BadgeCount = 0;

        foreach ( BadgeLine line in Lines )
        {
            line.Clear ();
        }
    }
}
