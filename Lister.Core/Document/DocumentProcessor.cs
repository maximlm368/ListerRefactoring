using Lister.Core.Document.AbstractComponents;
using Lister.Core.Entities;
using Lister.Core.Entities.Badge;
using Lister.Core.Infrastructure.PeopleAccess;
using Lister.Core.Infrastructure.ResultWithdrawing;

namespace Lister.Core.Document;

/// <summary>
/// Single class, builds badges for template and for single person or all persons from source file,
/// returning pages as result. 
/// </summary>
public sealed class DocumentProcessor
{
    private static DocumentProcessor? _instance = null;
    private static PeopleSourceFactory? _peopleSourceFactory;

    private readonly string _osName;
    private readonly PdfCreator _pdfCreator;
    private readonly Printer _printer;
    private int _badgeCount;

    public List<Person>? People { get; private set; }
    public List<Page> AllPages { get; private set; } = [];
    public int IncorrectBadgeCount { get; private set; }
    public Page LastPage { get; private set; }
    public delegate void ComplatedPageHandler ( Page complated, bool mustBeReplacedByNext );
    public event ComplatedPageHandler? ComplatedPage;

    private DocumentProcessor ( ITextWidthMeasurer widthMeasurer, string osName )
    {
        _osName = osName;
        _pdfCreator = PdfCreator.GetInstance ( _osName );
        _printer = Printer.GetInstance ( _osName );
        _peopleSourceFactory = PeopleSourceFactory.GetInstance ();

        Page.Complated += page =>
        {
            ComplatedPage?.Invoke ( page, false );
        };

        LastPage = new ();
        AllPages.Add ( LastPage );

        Layout.Measurer = widthMeasurer;
        TextLine.Measurer = widthMeasurer;
    }

    public static DocumentProcessor GetInstance ( ITextWidthMeasurer widthMeasurer, string osName )
    {
        _instance ??= new DocumentProcessor ( widthMeasurer, osName );

        return _instance;
    }

    public List<Page> BuildAllPages ( int limit, Layout? layout, string? background )
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
            Badge? built = 
                ( layout != null && People[index] != null ) ? Badge.GetBadge ( People [index], background, layout.Clone ( true ) ) : null;

            if ( built == null )
            {
                continue;
            }

            if ( !built.IsCorrect )
            {
                IncorrectBadgeCount++;
                incorrects.Add ( built );
            }

            Page possibleNewPage = fillablePage.Add ( built );
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

    public List<Page> BuildBadge ( Person? person, Layout? layout, string? background )
    {
        Badge? built = ( layout != null && person != null ) ? Badge.GetBadge ( person, background, layout.Clone ( true ) ) : null;

        if ( built == null )
        {
            return AllPages;
        }

        if ( !built.IsCorrect )
        {
            IncorrectBadgeCount++;
        }

        Page possibleNewLastPage = LastPage.Add ( built );
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

    public void Print ( List<Page> printables, string printerName, int copiesAmount )
    {
        _printer.Print ( printables, _pdfCreator, printerName, copiesAmount );
    }

    public bool CreateAndSave ( List<Page> pages, string filePathToSave )
    {
        return _pdfCreator.CreateAndSave ( pages, filePathToSave );
    }
}
