using Core.BadgesCreator.Abstractions;
using Core.DataAccess.Abstractions;
using Core.DocumentProcessor.Abstractions;
using Core.Models;
using Core.Models.Badge;

namespace Core.DocumentProcessor;

/// <summary>
/// Single class, builds badges for template and for single person or all persons from source file,
/// returning pages as result.
/// Produces, save pdf file for result pages and sends these pages to printer. 
/// </summary>

public class DocumentProcessor 
{
    private static DocumentProcessor _instance = null;

    private IPdfCreator _pdfCreator;
    private IPdfPrinter _pdfPrinter;
    private BadgesCreator.BadgeCreator _badgeCreator;
    private List<Page> _allPages;
    private int _badgeCount;

    public int IncorrectBadgeCount { get; private set; }
    public Page LastPage { get; private set; }

    public delegate void ComplatedPageHandler ( Page complated, bool mustBeReplacedByNext );
    public event ComplatedPageHandler ? ComplatedPage;


    private DocumentProcessor ( ITextWidthMeasurer widthMeasurer, IPdfCreator pdfCreator, IPdfPrinter pdfPrinter
                                , IBadgeLayoutProvider badgeAppearenceProvider, IPeopleSourceFactory peopleSourceFactory ) 
    {
        _pdfCreator = pdfCreator;
        _pdfPrinter = pdfPrinter;
        _badgeCreator = BadgesCreator.BadgeCreator.GetInstance (badgeAppearenceProvider, peopleSourceFactory);
        Page.Complated += ( Page page ) => { ComplatedPage?.Invoke ( page, false ); };

        _allPages = new ();
        LastPage = new ();
        _allPages.Add ( LastPage );

        Layout.Measurer = widthMeasurer;
        TextLine.Measurer = widthMeasurer;
    }


    public static DocumentProcessor GetInstance ( ITextWidthMeasurer widthMeasurer, IPdfCreator pdfCreator
                                                , IPdfPrinter pdfPrinter, IBadgeLayoutProvider badgeAppearenceProvider
                                                , IPeopleSourceFactory peopleSourceFactory )
    {
        if ( _instance == null ) 
        {
            _instance = new DocumentProcessor ( widthMeasurer, pdfCreator, pdfPrinter, badgeAppearenceProvider
                                                                                          , peopleSourceFactory );
        }

        return _instance;
    }

    public List<Page> BuildAllPages ( string templateName, int limit )
    {
        List<Person> people = _badgeCreator.GetCurrentPeople ();

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
            Badge builtIn = _badgeCreator.CreateSingleBadgeByTemplate ( templateName, people [index] );

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


    public List<Page> BuildBadge ( string templateName, Person person )
    {
        Badge builtIn = _badgeCreator.CreateSingleBadgeByTemplate ( templateName, person );

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


    public void Clear ( )
    {
        _allPages = new ();
        LastPage = new Page ();
        _allPages.Add ( LastPage );
        _badgeCount = 0;
        IncorrectBadgeCount = 0;
        Badge.ClearSharedData ();

    }


    public bool CreatePdfAndSave ( string filePathToSave )
    {
        bool isNothingToDo = ( _allPages == null )
                             ||
                             ( _allPages.Count == 0 );

        if ( isNothingToDo )
        {
            return false;
        }

        return _pdfCreator.CreateAndSave ( _allPages, filePathToSave );
    }


    public void Print ( string printerName, List<int> printablePageNumbers, int copiesAmount )
    {
        List<Page> printables = new ();

        for ( int index = 0;   index < _allPages.Count;   index++ ) 
        {
            if ( printablePageNumbers.Contains(index) ) 
            {
                printables.Add ( _allPages[index] );
            }
        }

        _pdfPrinter.Print ( printables, _pdfCreator, printerName, copiesAmount );
    }
}