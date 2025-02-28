using Core.BadgesProvider;
using Core.DataAccess.PeopleSource;
using Core.Models;
using Core.Models.Badge;
using Core.DataAccess;
using ExtentionsAndAuxiliary;
using System.Collections.ObjectModel;
using System;

namespace Core.DocumentBuilder;

public class DocumentBuilder 
{
    private List <Person> _people;
    private Layout _badgeLayout;
    private BadgesGetter _badgeGetter;
    private List<Page> _allPages;
    private int _badgeCount;

    public int IncorrectBadgeCount { get; private set; }
    public Page LastPage { get; private set; }

    public delegate void ComplatedPageHandler ( Page complated, bool mustBeReplacedByNext );
    public event ComplatedPageHandler ? ComplatedPage;


    public DocumentBuilder ( ITextWidthMeasurer widthMeasurer ) 
    {
        _badgeGetter = BadgesGetter.GetInstance ();
        Page.Complated += ( Page page ) => { ComplatedPage?.Invoke ( page, false ); };

        _allPages = new ();
        LastPage = new ();
        _allPages.Add ( LastPage );

        Layout.Measurer = widthMeasurer;
        TextLine.Measurer = widthMeasurer;
    }


    public List<Page> BuildAllPages ( string templateName, int limit )
    {
        List<Person> people = _badgeGetter.GetCurrentPeople ();

        if ( ( _badgeCount + people.Count ) >= limit )
        {
            return new List<Page>();
        }

        Page fillablePage = LastPage;
        bool lastIsNotEmpty = fillablePage.BadgeCount > 0;
        
        int lastNumber = people.Count - 1;

        List<Badge> incorrects = new List<Badge> ();

        for ( int index = 0;   index < people.Count;   index++ )
        {
            Badge builtIn = _badgeGetter.CreateSingleBadgeByTemplate ( templateName, people [index] );

            if ( (builtIn != null)   &&   ! builtIn.IsCorrect )
            {
                IncorrectBadgeCount++;
                incorrects.Add ( builtIn );
            }

            Page possibleNewPage = fillablePage.AddAndGetIncludingPage ( builtIn );
            bool timeToAddNewPage = ! possibleNewPage.Equals ( fillablePage );

            if ( timeToAddNewPage )
            {
                fillablePage = possibleNewPage;
                _allPages.Add ( fillablePage );
            }

            if ( index == lastNumber ) 
            {
                ComplatedPage?.Invoke ( possibleNewPage, true );
            }

            _badgeCount++;
        }

        LastPage = _allPages.Last ();

        return _allPages;
    }


    public List<Page> BuildInBadge ( string templateName, Person person )
    {
        Badge builtIn = _badgeGetter.CreateSingleBadgeByTemplate ( templateName, person );

        if ( ( builtIn != null )   &&   ! builtIn.IsCorrect )
        {
            IncorrectBadgeCount++;
        }

        Page possibleNewLastPage = LastPage.AddAndGetIncludingPage ( builtIn );

        bool timeToIncrementVisiblePageNumber = ! possibleNewLastPage.Equals ( LastPage );

        if ( timeToIncrementVisiblePageNumber )
        {
            LastPage = possibleNewLastPage;
            _allPages.Add ( LastPage );
        }

        _badgeCount++;

        return _allPages;
    }


    public void ClearAll ( )
    {
        _allPages = new ();
        LastPage = new Page ();
        _allPages.Add ( LastPage );
        _badgeCount = 0;
        IncorrectBadgeCount = 0;
        Badge.ClearCommonData ();

    }
}