using System.Collections.ObjectModel;
using Core.Models.Badge;

namespace Core.DocumentBuilder;

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
        BuildingSuccess additionSuccess = _fillableLine.AddBadge ( badge );

        if ( additionSuccess == BuildingSuccess.FailureByWidth )
        {
            double restHeight = GetRestHeight ();
            bool isFirstLine = ( Lines.Count == 0 );
            BadgeLine newLine = new BadgeLine ( Width, restHeight, isFirstLine );
            additionSuccess = newLine.AddBadge ( badge );

            if ( additionSuccess == BuildingSuccess.FailureByHeight )
            {
                fillablePage = new Page ( );
                fillablePage.BadgeCount--;
                fillablePage.AddAndGetIncludingPage ( badge );
            }

            fillablePage.Lines.Add ( newLine );
            fillablePage._fillableLine = newLine;
        }

        if ( additionSuccess == BuildingSuccess.FailureByHeight )
        {
            Complated?.Invoke ( this );
            fillablePage = new Page ( );
            fillablePage.BadgeCount--;
            fillablePage.AddAndGetIncludingPage ( badge );
        }

        fillablePage.BadgeCount++;

        return fillablePage;
    }


    //internal static Page AddAndGetIncludingPage ( Page page, Badge badge )
    //{
    //    Page fillablePage = page;
    //    BuildingSuccess additionSuccess = _fillableLine.AddBadge ( badge );

    //    if ( additionSuccess == BuildingSuccess.FailureByWidth )
    //    {
    //        double restHeight = GetRestHeight ();
    //        bool isFirstLine = ( Lines.Count == 0 );
    //        BadgeLine newLine = new BadgeLine ( Width, restHeight, isFirstLine );
    //        additionSuccess = newLine.AddBadge ( badge );

    //        if ( additionSuccess == BuildingSuccess.FailureByHeight )
    //        {
    //            fillablePage = new Page ();
    //            fillablePage.AddAndGetIncludingPage ( badge );
    //        }

    //        fillablePage.Lines.Add ( newLine );
    //        fillablePage._fillableLine = newLine;
    //    }

    //    if ( additionSuccess == BuildingSuccess.FailureByHeight )
    //    {
    //        Complated?.Invoke ( this );
    //        fillablePage = new Page ();
    //        fillablePage.AddAndGetIncludingPage ( badge );
    //    }

    //    fillablePage.BadgeCount++;
    //    return fillablePage;
    //}


    //private bool IsTimeForNewPage ()
    //{
    //    double restHeight = GetRestHeight ();
    //    bool itIsTime = restHeight > _fillableLine.Height;
    //    return itIsTime;
    //}


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


    //private static List <Page> Place ( List <Badge> placebleBadges, Page fillablePage )
    //{
    //    List<Page> result = new ();

    //    for ( int index = 0;   index < placebleBadges.Count;   index++ )
    //    {
    //        Page posibleNewPage = fillablePage.AddAndGetIncludingPage ( placebleBadges [index] );
    //        bool timeToAddNewPage = ! posibleNewPage.Equals ( fillablePage );

    //        if ( timeToAddNewPage )
    //        {
    //            result.Add ( fillablePage );
    //            fillablePage = posibleNewPage;
    //        }

    //        if ( index == placebleBadges.Count - 1 )
    //        {
    //            result.Add ( fillablePage );
    //            Complated?.Invoke ( fillablePage );
    //        }
    //    }

    //    return result;
    //}
}
