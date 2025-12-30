using Lister.Core.Models.Badge;

namespace Lister.Core.Document;

/// <summary>
/// Represents lines of badges for page.
/// Adds sent badge.
/// </summary>
public sealed class Page
{
    private static readonly double _topPadding;
    private static readonly double _leftPadding;
    private static readonly double _width;
    private static readonly double _height;

    public static event ComplatingHandler? Complated;

    private BadgeLine _fillableLine;

    public double Width { get; private set; }
    public double Height { get; private set; }
    public double ContentTopOffset { get; private set; }
    public double ContentLeftOffset { get; private set; }
    public List<BadgeLine> Lines { get; private set; }
    public int BadgeCount { get; private set; }
    public delegate void ComplatingHandler ( Page complated );

    static Page ()
    {
        _width = 825;
        _height = 1168;
        _topPadding = 40;
        _leftPadding = 60;
    }

    public Page ()
    {
        Lines = [];
        BadgeCount = 0;
        Width = _width;
        Height = _height;
        ContentTopOffset = _topPadding;
        ContentLeftOffset = _leftPadding;
        double usefullHeight = Height - 20;
        _fillableLine = new BadgeLine ( Width, usefullHeight, true );
        Lines.Add ( _fillableLine );
    }

    internal Page Add ( Badge badge )
    {
        Page fillablePage = this;
        AdditionSuccess additionSuccess = _fillableLine.AddBadge ( badge );

        if ( additionSuccess == AdditionSuccess.FailureByWidth )
        {
            double restHeight = GetRestHeight ();
            bool isFirstLine = ( Lines.Count == 0 );
            BadgeLine newLine = new ( Width, restHeight, isFirstLine );
            additionSuccess = newLine.AddBadge ( badge );

            if ( additionSuccess == AdditionSuccess.FailureByHeight )
            {
                fillablePage = new Page ();
                fillablePage.BadgeCount--;
                fillablePage.Add ( badge );
            }

            fillablePage.Lines.Add ( newLine );
            fillablePage._fillableLine = newLine;
        }

        if ( additionSuccess == AdditionSuccess.FailureByHeight )
        {
            Complated?.Invoke ( this );
            fillablePage = new Page ();
            fillablePage.BadgeCount--;
            fillablePage.Add ( badge );
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
