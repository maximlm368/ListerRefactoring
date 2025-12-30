using Avalonia.Media;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.Extentions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using Lister.Core.Extentions;

namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;

internal sealed partial class PrintDialogViewModel : ReactiveObject
{
    private readonly string _linuxGetPrintersBash = "lpstat -p | awk '{print $2}'";
    private readonly string _linuxGetDefaultPrinterBash = "lpstat -d";
    private readonly string _emptyCopies;
    private readonly string _emptyPages;
    private readonly string _emptyPrinters;
    private readonly int _copiesMaxCount = 10;
    private readonly int _pagesStrMaxGlyphCount = 30;
    private readonly List<char> _pageNumsAcceptables = [' ', '-', ',', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    private readonly List<char> _copiesCountAcceptables = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    private readonly string _osName;
    private readonly List<char> _intAsCharList = [];
    private readonly List<int> _pageNumbers = [];
    private ParserStates _state;
    private int _rangeStart;
    private int _pagesAmount;
    private PrintAdjustingData? _adjusting;
    private bool _isPageInputPossable = false;

    private ObservableCollection<PrinterPresentation>? _printers;
    internal ObservableCollection<PrinterPresentation>? Printers
    {
        get { return _printers; }
        private set
        {
            this.RaiseAndSetIfChanged ( ref _printers, value, nameof ( Printers ) );
        }
    }

    private bool _isSomePages;
    internal bool IsSomePages
    {
        get { return _isSomePages; }
        private set
        {
            if ( !value )
            {
                PagesInString = string.Empty;
            }

            this.RaiseAndSetIfChanged ( ref _isSomePages, value, nameof ( IsSomePages ) );
        }
    }

    private string? _pagesInString;
    internal string? PagesInString
    {
        get { return _pagesInString; }
        set
        {
            value = value.RemoveUnacceptableGlyphs ( _pageNumsAcceptables );

            if ( !string.IsNullOrEmpty ( value ) && ( value.Length > _pagesStrMaxGlyphCount ) )
            {
                value = PagesInString;
            }

            PagesError = SetPagesFrom ( value );

            if ( !_isPageInputPossable && !string.IsNullOrWhiteSpace ( PagesError ) )
            {
                return;
            }

            for ( int index = 0; index < _pageNumbers.Count; index++ )
            {
                if ( _pageNumbers [index] > _pagesAmount )
                {
                    value = PagesInString;

                    break;
                }
            }

            _pagesInString = value;
        }
    }

    private SolidColorBrush? _pagesBorderColor = new ( new Color ( 255, 0, 0, 0 ) );
    internal SolidColorBrush? PagesBorderColor
    {
        get { return _pagesBorderColor; }
        set
        {
            this.RaiseAndSetIfChanged ( ref _pagesBorderColor, value, nameof ( PagesBorderColor ) );
        }
    }

    private string _pagesError = string.Empty;
    internal string PagesError
    {
        get { return _pagesError; }
        set
        {
            if ( string.IsNullOrEmpty ( value ) )
            {
                PagesBorderColor = new SolidColorBrush ( new Color ( 255, 0, 0, 0 ) );
            }
            else
            {
                PagesBorderColor = new SolidColorBrush ( new Color ( 255, 255, 0, 0 ) );
            }

            this.RaiseAndSetIfChanged ( ref _pagesError, value, nameof ( PagesError ) );
        }
    }

    private string? _copies;
    internal string? Copies
    {
        get { return _copies; }
        set
        {
            value = value.RemoveUnacceptableGlyphs ( _copiesCountAcceptables );

            if ( CopiesError == _emptyCopies )
            {
                CopiesError = string.Empty;
            }

            if ( !string.IsNullOrEmpty ( value ) )
            {
                if ( IsFirstDigitZero ( value ) || int.Parse ( value ) > _copiesMaxCount )
                {
                    (_copies, value) = (value, _copies);
                }
            }
            else
            {
                CopiesError = _emptyCopies;
            }

            this.RaiseAndSetIfChanged ( ref _copies, value, nameof ( Copies ) );
        }
    }

    private SolidColorBrush? _copiesBorderColor = new ( new Color ( 255, 0, 0, 0 ) );
    internal SolidColorBrush? CopiesBorderColor
    {
        get { return _copiesBorderColor; }
        set
        {
            this.RaiseAndSetIfChanged ( ref _copiesBorderColor, value, nameof ( CopiesBorderColor ) );
        }
    }

    private string? _copiesError;
    internal string? CopiesError
    {
        get { return _copiesError; }
        set
        {
            if ( string.IsNullOrEmpty ( value ) )
            {
                CopiesBorderColor = new SolidColorBrush ( new Color ( 255, 0, 0, 0 ) );
            }
            else
            {
                CopiesBorderColor = new SolidColorBrush ( new Color ( 255, 255, 0, 0 ) );
            }

            this.RaiseAndSetIfChanged ( ref _copiesError, value, nameof ( CopiesError ) );
        }
    }

    private string? _printersEmptyError;
    internal string? PrintersEmptyError
    {
        get { return _printersEmptyError; }
        set
        {
            this.RaiseAndSetIfChanged ( ref _printersEmptyError, value, nameof ( PrintersEmptyError ) );
        }
    }

    private bool _printingIsAvailable;
    internal bool PrintingIsAvailable
    {
        get { return _printingIsAvailable; }
        private set
        {
            this.RaiseAndSetIfChanged ( ref _printingIsAvailable, value, nameof ( PrintingIsAvailable ) );
        }
    }

    private int _selectedIndex;
    internal int SelectedIndex
    {
        get { return _selectedIndex; }
        private set
        {
            this.RaiseAndSetIfChanged ( ref _selectedIndex, value, nameof ( SelectedIndex ) );
        }
    }

    private bool _needClose;
    internal bool NeedClose
    {
        get { return _needClose; }
        private set
        {
            if ( _needClose == value )
            {
                _needClose = !_needClose;
            }

            this.RaiseAndSetIfChanged ( ref _needClose, value, nameof ( NeedClose ) );
        }
    }

    internal PrintDialogViewModel ( string emptyCopies, string emptyPages, string emptyPrinters, string osName )
    {
        _emptyCopies = emptyCopies;
        _emptyPages = emptyPages;
        _emptyPrinters = emptyPrinters;
        _osName = osName;
    }

    internal void OpenPrinterSettings ()
    {
        if ( _osName == "Windows"
             && Printers != null
             && Printers.Count > SelectedIndex
        )
        {
            System.Diagnostics.Process process = new ();
            System.Diagnostics.ProcessStartInfo startInfo = new ()
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/c rundll32 printui.dll,PrintUIEntry /e /n \"" + Printers [SelectedIndex].StringPresentation + "\""
            };
            process.StartInfo = startInfo;
            process.Start ();
        }
        else if ( _osName == "Linux" )
        {
            string command = "gnome-control-center -s Printers";
            PdfPrinterImplementation.ExecuteBashCommand ( command );
        }
    }

    internal void Print ()
    {
        if ( HasError ()
             || Printers == null
             || Printers.Count < SelectedIndex + 1
             || _adjusting == null
        )
        {
            return;
        }

        _adjusting.PrinterName = Printers [SelectedIndex].StringPresentation;

        if ( !IsSomePages )
        {
            _pageNumbers.Clear ();

            for ( int index = 0; index < _pagesAmount; index++ )
            {
                _pageNumbers.Add ( index );
            }
        }
        else
        {
            PagesError = SetPagesFrom ( PagesInString );

            if ( !string.IsNullOrWhiteSpace ( PagesError ) )
            {
                return;
            }

            for ( int index = 0; index < _pageNumbers.Count; index++ )
            {
                _pageNumbers [index] -= 1;
            }
        }

        _adjusting.PageNumbers = _pageNumbers;
        _adjusting.CopiesAmount = int.TryParse ( Copies, out int amount ) ? amount : 1;
        _adjusting.IsCancelled = false;
        NeedClose = true;
    }

    private bool HasError ()
    {
        if ( string.IsNullOrEmpty ( Copies ) )
        {
            CopiesError = _emptyCopies;

            return true;
        }

        if ( string.IsNullOrEmpty ( PagesInString ) && IsSomePages )
        {
            PagesError = _emptyPages;

            return true;
        }

        if ( !string.IsNullOrWhiteSpace ( _pagesInString ) && PagesError.Contains ( "Некорректный интервал" ) )
        {
            return true;
        }

        return false;
    }

    internal void Cancel ()
    {
        CopiesError = string.Empty;
        PagesError = string.Empty;

        if ( _adjusting != null )
        {
            _adjusting.IsCancelled = true;
        }

        NeedClose = true;
    }

    internal void AdjustPrinting ( int pageAmmount, PrintAdjustingData adjusting )
    {
        if ( _osName == "Windows" )
        {
            Printers = PreparePrintersListOnWindows ();
        }
        else if ( _osName == "Linux" )
        {
            Printers = PreparePrintersListOnLinux ();
        }

        HandleEmptyPrinters ( Printers );
        Copies = "1";

        _pagesAmount = pageAmmount;
        _adjusting = adjusting;
    }

    private string SetPagesFrom ( string? pageNumbersInString )
    {
        if ( _adjusting == null
             || _adjusting.IsCancelled
             || pageNumbersInString == null
        )
        {
            return string.Empty;
        }

        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;
        _intAsCharList.Clear ();
        _pageNumbers.Clear ();

        for ( int index = 0; index < pageNumbersInString.Length; index++ )
        {
            string errorMessage = HandleNextGlyph ( pageNumbersInString [index], index == pageNumbersInString.Length - 1 );

            if ( !string.IsNullOrWhiteSpace ( errorMessage ) )
            {
                return errorMessage;
            }
        }

        _pageNumbers.Sort ();

        return string.Empty;
    }

    private string HandleNextGlyph ( char glyph, bool isGlyphLast )
    {
        _isPageInputPossable = false;
        bool glyphIsInteger = _copiesCountAcceptables.Contains ( glyph );

        if ( glyphIsInteger )
        {
            if ( ( _state == ParserStates.BeforeOrBetweenOrAfterGlyph ) || ( _state == ParserStates.InsideIntegerOrFirstInRange ) )
            {
                if ( ( _state == ParserStates.BeforeOrBetweenOrAfterGlyph ) && ( glyph == '0' ) )
                {
                    return "Число не может начинаться с ноля";
                }

                _state = ParserStates.InsideIntegerOrFirstInRange;
                _intAsCharList.Add ( glyph );

                if ( isGlyphLast )
                {
                    if ( int.TryParse ( _intAsCharList.ToArray (), out int pageNumber ) )
                    {
                        _pageNumbers.Add ( pageNumber );
                    }
                }
            }
            else if ( _state == ParserStates.InRange )
            {
                if ( glyph == '0' )
                {
                    return "Число не может начинаться с ноля";
                }

                bool startIsInt = int.TryParse ( _intAsCharList.ToArray (), out _rangeStart );

                _intAsCharList.Clear ();
                _state = ParserStates.InRangeEnd;
                _intAsCharList.Add ( glyph );

                bool endIsInt = int.TryParse ( _intAsCharList.ToArray (), out int rangeEnd );

                if ( startIsInt && endIsInt && _rangeStart > rangeEnd )
                {
                    if ( ( rangeEnd * 10 ) > _pagesAmount )
                    {
                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }

                if ( isGlyphLast )
                {
                    for ( int index = _rangeStart; index <= rangeEnd; index++ )
                    {
                        _pageNumbers.Add ( index );
                    }

                    if ( _rangeStart > rangeEnd )
                    {
                        _isPageInputPossable = true;

                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }
            }
            else if ( _state == ParserStates.InRangeEnd )
            {
                _intAsCharList.Add ( glyph );
                bool endIsInt = int.TryParse ( _intAsCharList.ToArray (), out int rangeEnd );

                if ( endIsInt && _rangeStart > rangeEnd )
                {
                    if ( ( rangeEnd * 10 ) > _pagesAmount )
                    {
                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }

                if ( isGlyphLast )
                {
                    for ( int index = _rangeStart; index <= rangeEnd; index++ )
                    {
                        _pageNumbers.Add ( index );
                    }

                    if ( _rangeStart > rangeEnd )
                    {
                        _isPageInputPossable = true;

                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }
            }
            else if ( _state == ParserStates.AfterInteger )
            {
                return "Цифра не может идти после пробела";
            }
            else if ( _state == ParserStates.AfterSpaceWithoutPunctuation )
            {
                return "Отсутствует пунктуация";
            }
        }
        else if ( !glyphIsInteger )
        {
            switch ( glyph )
            {
                case ' ':

                    if ( _state == ParserStates.InsideIntegerOrFirstInRange )
                    {
                        _state = ParserStates.AfterInteger;
                    }
                    else if ( _state == ParserStates.InRangeEnd )
                    {
                        bool endIsInt = int.TryParse ( _intAsCharList.ToArray (), out int rangeEnd );

                        if ( endIsInt && _rangeStart > rangeEnd )
                        {
                            return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                        }

                        for ( int index = _rangeStart; index <= rangeEnd; index++ )
                        {
                            _pageNumbers.Add ( index );
                        }

                        _intAsCharList.Clear ();
                        _state = ParserStates.AfterSpaceWithoutPunctuation;
                    }
                    else if ( _state == ParserStates.InRange )
                    {
                        if ( isGlyphLast )
                        {
                            _isPageInputPossable = true;

                            return "Тире не может быть последним в строке";
                        }
                    }

                    break;
                case '-':

                    if ( ( _state == ParserStates.InsideIntegerOrFirstInRange ) || ( _state == ParserStates.AfterInteger ) )
                    {
                        _state = ParserStates.InRange;

                        if ( isGlyphLast )
                        {
                            _isPageInputPossable = true;

                            return "Тире не может быть последним в строке";
                        }
                    }
                    else if ( _state == ParserStates.BeforeOrBetweenOrAfterGlyph )
                    {
                        if ( _pageNumbers.Count < 1 )
                        {
                            return "Тире не может быть первым";
                        }
                        else
                        {
                            return "Тире может быть только внутри интервала";
                        }
                    }
                    else if ( _state == ParserStates.InRange )
                    {
                        return "Тире не может идти подряд";
                    }
                    else if ( _state == ParserStates.InRangeEnd )
                    {
                        return "Тире не может идти после интервала";
                    }
                    else if ( _state == ParserStates.AfterSpaceWithoutPunctuation )
                    {
                        return "Тире не может идти после интервала";
                    }

                    break;
                case ',':

                    if ( ( _state == ParserStates.InsideIntegerOrFirstInRange ) || ( _state == ParserStates.AfterInteger ) )
                    {
                        bool outIsInt = int.TryParse ( _intAsCharList.ToArray (), out int integer );

                        if ( outIsInt ) 
                        {
                            _pageNumbers.Add ( integer );
                        }

                        _intAsCharList.Clear ();
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if ( isGlyphLast )
                        {
                            _isPageInputPossable = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }
                    else if ( _state == ParserStates.InRangeEnd )
                    {
                        bool endIsInt = int.TryParse (_intAsCharList.ToArray (), out int rangeEnd);

                        if ( endIsInt && _rangeStart > rangeEnd )
                        {
                            return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                        }

                        for ( int index = _rangeStart; index <= rangeEnd; index++ )
                        {
                            _pageNumbers.Add ( index );
                        }

                        _intAsCharList.Clear ();
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if ( isGlyphLast )
                        {
                            _isPageInputPossable = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }
                    else if ( _state == ParserStates.BeforeOrBetweenOrAfterGlyph )
                    {
                        if ( _pageNumbers.Count < 1 )
                        {
                            return "Запятая не может быть первой";
                        }
                        else
                        {
                            return "Запятые не могут идти подряд";
                        }
                    }
                    else if ( _state == ParserStates.InRange )
                    {
                        return "Запятая не может идти после тире";
                    }
                    else if ( _state == ParserStates.AfterSpaceWithoutPunctuation )
                    {
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if ( isGlyphLast )
                        {
                            _isPageInputPossable = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }

                    break;
            }
        }

        return string.Empty;
    }

    private static bool IsFirstDigitZero ( string value )
    {
        bool firstDigitIsZero = false;

        char ch = value [0];

        if ( ch == '0' )
        {
            firstDigitIsZero = true;
        }

        return firstDigitIsZero;
    }

    private ObservableCollection<PrinterPresentation> PreparePrintersListOnWindows ()
    {
        ObservableCollection<PrinterPresentation> availablePrinters = [];

        if ( OperatingSystem.IsWindowsVersionAtLeast ( 6, 1 ) )
        {
            PrinterSettings settings = new ();
            string defaultPrinterName = settings.PrinterName;
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;
            int counter = 0;

            foreach ( string printer in printers )
            {
                availablePrinters.Add ( new PrinterPresentation ( printer ) );

                if ( defaultPrinterName == printer )
                {
                    SelectedIndex = counter;
                }

                counter++;
            }
        }

        return availablePrinters;
    }

    private ObservableCollection<PrinterPresentation> PreparePrintersListOnLinux ()
    {
        ObservableCollection<PrinterPresentation> availablePrinters = [];
        string defaultPrinterName = TerminalCommandExecuter.ExecuteCommand ( _linuxGetDefaultPrinterBash );
        string printersLine = TerminalCommandExecuter.ExecuteCommand ( _linuxGetPrintersBash );
        string [] printers = printersLine.Split ( ['\n'], StringSplitOptions.RemoveEmptyEntries );
        int counter = 0;

        foreach ( string printer in printers )
        {
            string prntr = printer.TrimLastNewLineChar ();

            availablePrinters.Add ( new PrinterPresentation ( prntr ) );

            if ( defaultPrinterName == prntr )
            {
                SelectedIndex = counter;
            }

            counter++;
        }

        return availablePrinters;
    }

    private void HandleEmptyPrinters ( ObservableCollection<PrinterPresentation>? printersList )
    {
        if ( printersList == null )
        {
            return;
        }

        bool printersIsEmpty = printersList.Count < 1;

        if ( printersIsEmpty )
        {
            PrintersEmptyError = _emptyPrinters;
        }
        else
        {
            PrintersEmptyError = "";
        }

        PrintingIsAvailable = !printersIsEmpty;
    }

    private enum ParserStates
    {
        BeforeOrBetweenOrAfterGlyph = 0,
        InsideIntegerOrFirstInRange = 1,
        InRange = 2,
        AfterInteger = 3,
        InRangeEnd = 4,
        SpaceAfterComma = 5,
        AfterSpaceWithoutPunctuation = 6
    }
}
