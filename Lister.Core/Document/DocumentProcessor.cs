using Lister.Core.BadgesCreator.AbstractComponents;
using Lister.Core.Document.AbstractComponents;
using Lister.Core.Entities;
using Lister.Core.Entities.Badge;
using Lister.Core.PeopleAccess;

namespace Lister.Core.Document;

/// <summary>
/// Single class, builds badges for template and for single person or all persons from source file,
/// returning pages as result. 
/// </summary>
public sealed class DocumentProcessor
{
    private static DocumentProcessor? _instance = null;
    private static IPeopleSourceFactory? _peopleSourceFactory;

    private readonly BadgesCreator.BadgeCreator _badgeCreator;
    private int _badgeCount;

    public List<Person>? People { get; private set; }
    public List<Page> AllPages { get; private set; } = [];
    public int IncorrectBadgeCount { get; private set; }
    public Page LastPage { get; private set; }
    public delegate void ComplatedPageHandler ( Page complated, bool mustBeReplacedByNext );
    public event ComplatedPageHandler? ComplatedPage;

    private DocumentProcessor ( ITextWidthMeasurer widthMeasurer, IBadgeLayoutProvider badgeLayoutProvider,
        IPeopleSourceFactory peopleSourceFactory
    )
    {
        _badgeCreator = BadgesCreator.BadgeCreator.GetInstance ( badgeLayoutProvider );
        _peopleSourceFactory = peopleSourceFactory;

        Page.Complated += page =>
        {
            ComplatedPage?.Invoke ( page, false );
        };

        LastPage = new ();
        AllPages.Add ( LastPage );

        Layout.Measurer = widthMeasurer;
        TextLine.Measurer = widthMeasurer;
    }

    public static DocumentProcessor GetInstance ( ITextWidthMeasurer widthMeasurer,
        IBadgeLayoutProvider badgeAppearenceProvider,
        IPeopleSourceFactory peopleSourceFactory
    )
    {
        _instance ??= new DocumentProcessor ( widthMeasurer, badgeAppearenceProvider, peopleSourceFactory );

        return _instance;
    }

    public List<Page> BuildAllPages ( string templateName, int limit )
    {
        if ( People == null || ( _badgeCount + People.Count ) >= limit )
        {
            return [];
        }

        Page fillablePage = LastPage;
        bool lastIsNotEmpty = fillablePage.BadgeCount > 0;
        int lastNumber = People.Count - 1;
        List<Badge> incorrects = [];

        for ( int index = 0; index < People.Count; index++ )
        {
            Badge? builtIn = _badgeCreator.CreateBadgeByTemplate ( templateName, People [index] );

            if ( builtIn == null )
            {
                continue;
            }

            if ( !builtIn.IsCorrect )
            {
                IncorrectBadgeCount++;
                incorrects.Add ( builtIn );
            }

            Page possibleNewPage = fillablePage.Add ( builtIn );
            bool timeToAddNewPage = !possibleNewPage.Equals ( fillablePage );

            if ( timeToAddNewPage )
            {
                fillablePage = possibleNewPage;
                AllPages.Add ( fillablePage );
            }

            if ( index == lastNumber )
            {
                ComplatedPage?.Invoke ( possibleNewPage, true );
            }

            _badgeCount++;
        }

        LastPage = AllPages.Last ();

        return AllPages;
    }

    public List<Page> BuildBadge ( string? templateName, Person? person )
    {
        Badge? builtIn = _badgeCreator.CreateBadgeByTemplate ( templateName, person );

        if ( builtIn == null )
        {
            return AllPages;
        }

        if ( !builtIn.IsCorrect )
        {
            IncorrectBadgeCount++;
        }

        Page possibleNewLastPage = LastPage.Add ( builtIn );
        bool timeToIncrementVisiblePageNumber = !possibleNewLastPage.Equals ( LastPage );

        if ( timeToIncrementVisiblePageNumber )
        {
            LastPage = possibleNewLastPage;
            AllPages.Add ( LastPage );
        }

        _badgeCount++;

        return AllPages;
    }

    public void Clear ()
    {
        AllPages = [];
        LastPage = new Page ();
        AllPages.Add ( LastPage );
        _badgeCount = 0;
        IncorrectBadgeCount = 0;
        Badge.ToZeroState ();
    }

    public bool TrySetPeopleFrom ( string source, int limit )
    {
        IPeopleSource? peopleSource = _peopleSourceFactory?.GetPeopleSource ( source );
        List<Person>? people = peopleSource?.Get ( source, limit );

        if ( people != null ) 
        {
            People = people;

            return true;
        }

        return false;
    }
}
