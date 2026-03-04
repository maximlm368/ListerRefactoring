using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Core.Extentions;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.Extentions;
using System.Collections.ObjectModel;
using System.Drawing.Printing;

namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;

public sealed partial class PrintDialogViewModel : ObservableObject
{
    private readonly string _linuxGetPrintersBash = "lpstat -p | awk '{print $2}'";
    private readonly string _linuxGetDefaultPrinterBash = "lpstat -d";
    private readonly string _incorrectIntervalError = "Некорректный интервал. Начальный номер должен быть не больше конечного.";
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
    private bool _isLastGlyphUsable = false;

    [ObservableProperty]
    private ObservableCollection<PrinterPresentation>? _printers;

    [ObservableProperty]
    private bool _printingIsAvailable;

    [ObservableProperty]
    private bool _isPrintersEmpty;

    internal bool HasCopiesError = false;

    internal bool HasPagesError = false;

    [ObservableProperty]
    private int _selectedIndex;

    [ObservableProperty]
    private PrinterPresentation _selectedPrinter;

    private bool _hasPageSelection;
    internal bool HasPageSelection
    {
        get => _hasPageSelection;

        private set
        {
            if ( value )
            {

            }
            else
            {
                Pages = string.Empty;
                PagesError = string.Empty;
            }

            _hasPageSelection = value;
            OnPropertyChanged ();
        }
    }

    private string? _pages;
    internal string? Pages
    {
        get => _pages;

        set
        {
            value = value.RemoveUnacceptableGlyphs ( _pageNumsAcceptables );

            if ( !string.IsNullOrEmpty ( value ) && ( value.Length > _pagesStrMaxGlyphCount ) )
            {
                value = Pages;
            }
            else if ( string.IsNullOrEmpty ( _pages ) && value == string.Empty )
            {
                PagesError = "Список страниц не может быть пустым.";

                return;
            }

            string pagesError = PagesError;
            PagesError = SetPagesFrom ( value );

            if ( !_isLastGlyphUsable )
            {
                PagesError = pagesError;

                if ( string.IsNullOrEmpty ( value ) )
                {
                    _pages = value;
                    OnPropertyChanged ();
                }

                return;
            }

            for ( int index = 0; index < _pageNumbers.Count; index++ )
            {
                if ( _pageNumbers [index] > _pagesAmount )
                {
                    value = Pages;

                    break;
                }
            }

            _pages = value;

            if ( string.IsNullOrEmpty ( _pages ) && string.IsNullOrEmpty ( PagesError ) )
            {
                PagesError = "Список страниц не может быть пустым.";
            }

            OnPropertyChanged ();
        }
    }

    private string _pagesError = string.Empty;
    internal string PagesError
    {
        get => _pagesError;

        private set
        {
            _pagesError = value;
            HasPagesError = !string.IsNullOrEmpty ( _pagesError );
            OnPropertyChanged ();
        }
    }

    private string? _copies = "1";
    internal string? Copies
    {
        get => _copies;

        set
        {
            value = value.RemoveUnacceptableGlyphs ( _copiesCountAcceptables );
            HasCopiesError = false;

            if ( !string.IsNullOrEmpty ( value ) )
            {
                if ( IsFirstGlyphZero ( value ) || 
                    ( int.TryParse ( value, out int intResult ) && intResult > _copiesMaxCount ) 
                )
                {
                    value = _copies;
                    HasCopiesError = true;
                }
            }
            else
            {
                HasCopiesError = true;
            }

            _copies = value;
            OnPropertyChanged ();
        }
    }

    private bool _isClosing;
    internal bool IsClosing
    {
        get => _isClosing;

        private set
        {
            if ( _isClosing == value )
            {
                _isClosing = !_isClosing;
            }

            OnPropertyChanged ();
        }
    }

    public PrintDialogViewModel ( string osName )
    {
        _osName = osName;
    }

    [RelayCommand]
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
                Arguments = "/c rundll32 printui.dll,PrintUIEntry /e /n \"" + SelectedPrinter.StringPresentation + "\""
            };
            process.StartInfo = startInfo;
            process.Start ();
        }
        else if ( _osName == "Linux" )
        {
            string command = "gnome-control-center -s Printers";
            Printer.ExecuteBashCommand ( command );
        }
    }

    [RelayCommand]
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

        _adjusting.PrinterName = SelectedPrinter.StringPresentation;

        if ( !HasPageSelection )
        {
            _pageNumbers.Clear ();

            for ( int index = 0; index < _pagesAmount; index++ )
            {
                _pageNumbers.Add ( index );
            }
        }
        else
        {
            PagesError = SetPagesFrom ( Pages );

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
        IsClosing = true;
    }

    private bool HasError ()
    {
        if ( string.IsNullOrEmpty ( Copies ) ||
            string.IsNullOrEmpty ( Pages ) && HasPageSelection ||
            !string.IsNullOrWhiteSpace ( _pages ) && PagesError.Contains ( _incorrectIntervalError )
        )
        {
            return true;
        }

        return false;
    }

    [RelayCommand]
    internal void Cancel ()
    {
        HasCopiesError = false;
        PagesError = string.Empty;

        if ( _adjusting != null )
        {
            _adjusting.IsCancelled = true;
        }

        IsClosing = true;
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
        _isLastGlyphUsable = false;
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

                if ( startIsInt && endIsInt && _rangeStart > rangeEnd && ( rangeEnd * 10 ) > _pagesAmount )
                {
                    return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                }

                if ( isGlyphLast )
                {
                    for ( int index = _rangeStart; index <= rangeEnd; index++ )
                    {
                        _pageNumbers.Add ( index );
                    }

                    _isLastGlyphUsable = _rangeStart > rangeEnd;

                    if ( rangeEnd > _pagesAmount || _isLastGlyphUsable )
                    {
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

                    _isLastGlyphUsable = _rangeStart > rangeEnd;

                    if ( rangeEnd > _pagesAmount || _isLastGlyphUsable )
                    {
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
                            _isLastGlyphUsable = true;

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
                            _isLastGlyphUsable = true;

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
                            _isLastGlyphUsable = true;

                            return "Запятая не может быть последней в строке";
                        }
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
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if ( isGlyphLast )
                        {
                            _isLastGlyphUsable = true;

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
                            _isLastGlyphUsable = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }

                    break;
            }
        }

        _isLastGlyphUsable = true;

        return string.Empty;
    }

    private static bool IsFirstGlyphZero ( string value )
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
        IsPrintersEmpty = printersList == null || printersList.Count < 1;
        PrintingIsAvailable = !IsPrintersEmpty;
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
