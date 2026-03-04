using Lister.Core.BadgesCreator.Abstractions;
using Lister.Core.PeopleAccess.Abstractions;
using Lister.Core.Document.AbstractServices;
using Lister.Core.Entities;
using Lister.Core.Entities.Badge;

namespace Lister.Core.Document;

/// <summary>
/// Single class, builds badges for template and for single person or all persons from source file,
/// returning pages as result.
/// Produces, save pdf file for result pages and sends these pages to printer. 
/// </summary>
public sealed class DocumentProcessor
{
    private static DocumentProcessor? _instance = null;

    private readonly IPdfCreator _pdfCreator;
    private readonly IPdfPrinter _pdfPrinter;
    private readonly BadgesCreator.BadgeCreator _badgeCreator;
    private int _badgeCount;

    private List<Page> _allPages = [];
    public int IncorrectBadgeCount { get; private set; }
    public Page LastPage { get; private set; }
    public delegate void ComplatedPageHandler ( Page complated, bool mustBeReplacedByNext );
    public event ComplatedPageHandler? ComplatedPage;

    private DocumentProcessor ( ITextWidthMeasurer widthMeasurer,
        IPdfCreator pdfCreator,
        IPdfPrinter pdfPrinter,
        IBadgeLayoutProvider badgeAppearenceProvider,
        IPeopleSourceFactory peopleSourceFactory
    )
    {
        _pdfCreator = pdfCreator;
        _pdfPrinter = pdfPrinter;
        _badgeCreator = BadgesCreator.BadgeCreator.GetInstance ( badgeAppearenceProvider, peopleSourceFactory );

        Page.Complated += page =>
        {
            ComplatedPage?.Invoke ( page, false );
        };

        LastPage = new ();
        _allPages.Add ( LastPage );

        Layout.Measurer = widthMeasurer;
        TextLine.Measurer = widthMeasurer;
    }

    public static DocumentProcessor GetInstance ( ITextWidthMeasurer widthMeasurer,
        IPdfCreator pdfCreator,
        IPdfPrinter pdfPrinter,
        IBadgeLayoutProvider badgeAppearenceProvider,
        IPeopleSourceFactory peopleSourceFactory
    )
    {
        _instance ??= new DocumentProcessor ( widthMeasurer, pdfCreator, pdfPrinter, badgeAppearenceProvider, peopleSourceFactory );

        return _instance;
    }

    public List<Page> BuildAllPages ( string templateName, int limit )
    {
        List<Person>? people = _badgeCreator.People;

        if ( people == null || ( _badgeCount + people.Count ) >= limit )
        {
            return [];
        }

        Page fillablePage = LastPage;
        bool lastIsNotEmpty = fillablePage.BadgeCount > 0;
        int lastNumber = people.Count - 1;
        List<Badge> incorrects = [];

        for ( int index = 0; index < people.Count; index++ )
        {
            Badge? builtIn = _badgeCreator.CreateBadgeByTemplate ( templateName, people [index] );

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

    public List<Page> BuildBadge ( string? templateName, Person? person )
    {
        Badge? builtIn = _badgeCreator.CreateBadgeByTemplate ( templateName, person );

        if ( builtIn == null )
        {
            return _allPages;
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
            _allPages.Add ( LastPage );
        }

        _badgeCount++;

        return _allPages;
    }

    public void Clear ()
    {
        _allPages = [];
        LastPage = new Page ();
        _allPages.Add ( LastPage );
        _badgeCount = 0;
        IncorrectBadgeCount = 0;
        Badge.ToZeroState ();
    }

    public bool CreateAndSavePdf ( string filePathToSave )
    {
        if ( _allPages.Count == 0 )
        {
            return false;
        }

        return _pdfCreator.CreateAndSave ( _allPages, filePathToSave );
    }

    public void Print ( string? printerName, List<int>? printablePageNumbers, int copiesAmount )
    {
        if ( string.IsNullOrWhiteSpace ( printerName )
            || printablePageNumbers == null
            || printablePageNumbers.Count == 0
        )
        {
            return;
        }

        List<Page> printables = [];

        for ( int index = 0; index < _allPages.Count; index++ )
        {
            if ( printablePageNumbers.Contains ( index ) )
            {
                printables.Add ( _allPages [index] );
            }
        }

        _pdfPrinter.Print ( printables, _pdfCreator, printerName, copiesAmount );
    }
}
