using Avalonia.Media;
using Avalonia.Media.Imaging;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.Extentions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using Lister.Desktop.App;

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
    private readonly List<char> _pageNumsAcceptables
                               = new List<char>() { ' ', '-', ',', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    private readonly List<char> _copiesCountAcceptables
                               = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    private readonly string _osName;

    private List<char> _numAsChars;
    private List<int> _pageNumbers;
    private List<int> _currentRange;
    private ParserStates _state;
    private int _rangeStart;
    private int _passedPagesAmount;
    private PrintAdjustingData _printingAdjusting;
    private bool _copiesHinderPrinting;
    private string _pagesListError;

    private SolidColorBrush _lineBackgraund;
    internal SolidColorBrush LineBackground
    {
        get { return _lineBackgraund; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _lineBackgraund, value, nameof( LineBackground ) );
        }
    }

    private SolidColorBrush _canvasBackground;
    internal SolidColorBrush CanvasBackground
    {
        get { return _canvasBackground; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _canvasBackground, value, nameof( CanvasBackground ) );
        }
    }

    private ObservableCollection<PrinterPresentation> _printers;
    internal ObservableCollection<PrinterPresentation> Printers
    {
        get { return _printers; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _printers, value, nameof( Printers ) );
        }
    }

    private bool _some;
    internal bool Some
    {
        get { return _some; }
        private set
        {
            if ( ! value )
            {
                Pages = string.Empty;
            }

            this.RaiseAndSetIfChanged( ref _some, value, nameof( Some ) );
        }
    }

    private string _pages;
    internal string Pages
    {
        get { return _pages; }
        set
        {
            PagesError = string.Empty;
            value = RemoveUnacceptableGlyph ( value, _pageNumsAcceptables );

            if ( ! string.IsNullOrEmpty( value )   &&   (value.Length > _pagesStrMaxGlyphCount))
            {
                value = Pages;
            }

            string errMessage = null;
            bool canContinue = false;
            GetPagesFromString ( value, out errMessage, out canContinue );

            if ( ! canContinue   &&   ! string.IsNullOrWhiteSpace ( errMessage ) ) return;

            for ( int index = 0;   index < _pageNumbers.Count;   index++ )
            {
                if ( _pageNumbers [index] > _passedPagesAmount )
                {
                    value = Pages;
                    break;
                }
            }

            _pages = value;
        }
    }

    private SolidColorBrush _pagesBorderColor;
    internal SolidColorBrush PagesBorderColor
    {
        get { return _pagesBorderColor; }
        set
        {
            this.RaiseAndSetIfChanged( ref _pagesBorderColor, value, nameof( PagesBorderColor ) );
        }
    }

    private string _pagesError;
    internal string PagesError
    {
        get { return _pagesError; }
        set
        {
            if (string.IsNullOrEmpty( value ))
            {
                PagesBorderColor = new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
            }
            else
            {
                PagesBorderColor = new SolidColorBrush( new Color( 255, 255, 0, 0 ) );
            }

            this.RaiseAndSetIfChanged( ref _pagesError, value, nameof( PagesError ) );
        }
    }

    private string _copies;
    internal string Copies
    {
        get { return _copies; }
        set
        {
            value = RemoveUnacceptableGlyph( value, _copiesCountAcceptables );
            _copiesHinderPrinting = false;

            if (CopiesError == _emptyCopies)
            {
                CopiesError = string.Empty;
            }

            if (!string.IsNullOrEmpty( value ))
            {
                if (IsFirstDigitZero( value ) || int.Parse( value ) > _copiesMaxCount)
                {
                    string changer = value;
                    value = _copies;
                    _copies = changer;
                }
            }

            this.RaiseAndSetIfChanged( ref _copies, value, nameof( Copies ) );
        }
    }

    private SolidColorBrush _copiesBorderColor;
    internal SolidColorBrush CopiesBorderColor
    {
        get { return _copiesBorderColor; }
        set
        {
            this.RaiseAndSetIfChanged( ref _copiesBorderColor, value, nameof( CopiesBorderColor ) );
        }
    }

    private string _copiesError;
    internal string CopiesError
    {
        get { return _copiesError; }
        set
        {
            if (string.IsNullOrEmpty( value ))
            {
                CopiesBorderColor = new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
            }
            else
            {
                CopiesBorderColor = new SolidColorBrush( new Color( 255, 255, 0, 0 ) );
            }

            this.RaiseAndSetIfChanged( ref _copiesError, value, nameof( CopiesError ) );
        }
    }

    private string _printersEmptyError;
    internal string PrintersEmptyError
    {
        get { return _printersEmptyError; }
        set
        {
            this.RaiseAndSetIfChanged( ref _printersEmptyError, value, nameof( PrintersEmptyError ) );
        }
    }

    private bool _printingIsAvailable;
    internal bool PrintingIsAvailable
    {
        get { return _printingIsAvailable; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _printingIsAvailable, value, nameof( PrintingIsAvailable ) );
        }
    }

    private int _selectedIndex;
    internal int SelectedIndex
    {
        get { return _selectedIndex; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _selectedIndex, value, nameof( SelectedIndex ) );
        }
    }

    private bool _needClose;
    internal bool NeedClose
    {
        get { return _needClose; }
        private set
        {
            if (_needClose == value)
            {
                _needClose = !_needClose;
            }

            this.RaiseAndSetIfChanged( ref _needClose, value, nameof( NeedClose ) );
        }
    }


    internal PrintDialogViewModel( string emptyCopies, string emptyPages, string emptyPrinters, string osName)
    {
        _emptyCopies = emptyCopies;
        _emptyPages = emptyPages;
        _emptyPrinters = emptyPrinters;
        _osName = osName;

        CanvasBackground = new SolidColorBrush( new Color( 255, 240, 240, 240 ) );
        LineBackground = new SolidColorBrush( new Color( 255, 220, 220, 220 ) );
        PagesBorderColor = new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
        CopiesBorderColor = new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
    }


    internal void PagesLostFocus()
    {
        string errMessage = null;
        bool canContinue = false;
        GetPagesFromString ( Pages, out errMessage, out canContinue );

        if ( ! string.IsNullOrWhiteSpace(errMessage) )
        {
            PagesError = errMessage;
            _pagesListError = errMessage;
        }

        if (string.IsNullOrEmpty( Pages ))
        {
            PagesError = _emptyPages;
        }
    }


    internal void PagesGotFocus()
    {
        PagesError = string.Empty;
    }


    internal void CopiesLostFocus()
    {
        if (string.IsNullOrEmpty( Copies ))
        {
            CopiesError = _emptyCopies;
            _copiesHinderPrinting = true;
        }
    }


    internal void CopiesGotFocus()
    {
        CopiesError = string.Empty;
    }


    internal void OpenPrinterSettings()
    {
        if ( _osName == "Windows" )
        {
            string printerName = Printers [SelectedIndex].StringPresentation;
            System.Diagnostics.Process process = new System.Diagnostics.Process ();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo ();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c rundll32 printui.dll,PrintUIEntry /e /n \"" + printerName + "\"";
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
        if ( HasError () ) return;

        PrinterPresentation printer = Printers [SelectedIndex];
        _printingAdjusting.PrinterName = printer.StringPresentation;

        if ( ! Some )
        {
            _pageNumbers = new List<int> ();

            for ( int index = 0;   index < _passedPagesAmount;   index++ )
            {
                _pageNumbers.Add ( index );
            }
        }
        else
        {
            string errMessage = null;
            bool canContinue = false;
            GetPagesFromString ( Pages, out errMessage, out canContinue );

            if ( ! string.IsNullOrWhiteSpace ( errMessage ) )
            {
                PagesError = errMessage;

                return;
            }

            for ( int index = 0;   index < _pageNumbers.Count;   index++ ) 
            {
                _pageNumbers [index] -= 1;
            }
        }

        _printingAdjusting.PageNumbers = _pageNumbers;
        _printingAdjusting.CopiesAmount = int.Parse ( Copies );
        _printingAdjusting.Cancelled = false;
        NeedClose = true;
    }


    private bool HasError ()
    {
        if ( string.IsNullOrEmpty ( Copies ) )
        {
            CopiesError = _emptyCopies;

            return true;
        }

        if ( string.IsNullOrEmpty ( Pages )   &&   Some )
        {
            PagesError = _emptyPages;

            return true;
        }

        if ( ! string.IsNullOrWhiteSpace ( _pages )   &&   PagesError.Contains ("Некорректный интервал") )
        {
            return true;
        }

        return false;
    }


    internal void Cancel ()
    {
        CopiesError = string.Empty;
        PagesError = string.Empty;
        _printingAdjusting.Cancelled = true;
        NeedClose = true;
    }


    internal void Prepare ()
    {
        ObservableCollection<PrinterPresentation> printersList = null;

        if ( _osName == "Windows" )
        {
            printersList = PreparePrintersListOnWindows ();
        }
        else if ( _osName == "Linux" )
        {
            printersList = PreparePrintersListOnLinux ();
        }

        HandleEmptyPrinters ( printersList );
        Printers = printersList;
        Copies = "1";
    }


    internal void TakeAmmountAndAdjusting ( int pageAmmount, PrintAdjustingData printAdjusting )
    {
        _passedPagesAmount = pageAmmount;
        _printingAdjusting = printAdjusting;
    }


    private void GetPagesFromString (string pageNumbers, out string errMessage, out bool canContinue)
    {
        List<int> result = new();
        errMessage = string.Empty;
        canContinue = false;

        if (_printingAdjusting.Cancelled   ||   (pageNumbers == null))
        {
            return;
        }

        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;
        _numAsChars = new();
        _pageNumbers = new();

        for (int index = 0;   index < pageNumbers.Length;   index++)
        {
            bool isGlyphLast = ( index == pageNumbers.Length - 1 );
            char glyph = pageNumbers[index];
            string errorMessage = CalcNumbersReturningErrMessage ( glyph, isGlyphLast, out canContinue );

            if ( errorMessage != string.Empty ) 
            {
                errMessage = errorMessage;
                
                return;
            }
        }

        _pageNumbers.Sort();
    }


    private string CalcNumbersReturningErrMessage (char glyph, bool isGlyphLast, out bool canContinue)
    {
        bool glyphIsInteger = _copiesCountAcceptables.Contains( glyph );
        canContinue = false;
        if (glyphIsInteger)
        {
            int num = int.Parse( glyph.ToString() );

            if ((_state == ParserStates.BeforeOrBetweenOrAfterGlyph)   ||   (_state == ParserStates.InsideIntegerOrFirstInRange))
            {
                if ((_state == ParserStates.BeforeOrBetweenOrAfterGlyph)   &&   (glyph == '0'))
                {
                    return "Число не может начинаться с ноля";
                }

                _state = ParserStates.InsideIntegerOrFirstInRange;
                _numAsChars.Add( glyph );

                if (isGlyphLast)
                {
                    string presentation = "";

                    foreach (char ch   in   _numAsChars)
                    {
                        presentation += ch;
                    }

                    int integer = int.Parse( presentation );

                    _pageNumbers.Add( integer );
                }
            }
            else if (_state == ParserStates.InRange)
            {
                if (glyph == '0')
                {
                    return "Число не может начинаться с ноля";
                }

                string presentation = "";

                foreach (char ch   in   _numAsChars)
                {
                    presentation += ch;
                }

                _rangeStart = int.Parse ( presentation );
                _numAsChars = new();
                _state = ParserStates.InRangeEnd;
                _numAsChars.Add ( glyph );

                presentation = "";

                foreach (char ch   in   _numAsChars)
                {
                    presentation += ch;
                }

                int rangeEnd = int.Parse ( presentation );

                if (_rangeStart > rangeEnd)
                {
                    if ((rangeEnd * 10) > _passedPagesAmount)
                    {
                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }

                if (isGlyphLast)
                {
                    for (int index = _rangeStart;   index <= rangeEnd;   index++)
                    {
                        _pageNumbers.Add ( index );
                    }

                    if ( _rangeStart > rangeEnd )
                    {
                        canContinue = true;

                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }
            }
            else if (_state == ParserStates.InRangeEnd)
            {
                _numAsChars.Add ( glyph );

                string presentation = "";

                foreach (char ch   in   _numAsChars)
                {
                    presentation += ch;
                }

                int rangeEnd = int.Parse ( presentation );

                if (_rangeStart > rangeEnd)
                {
                    if ((rangeEnd * 10) > _passedPagesAmount)
                    {
                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }

                if (isGlyphLast)
                {
                    for (int index = _rangeStart;   index <= rangeEnd;   index++)
                    {
                        _pageNumbers.Add ( index );
                    }

                    if ( _rangeStart > rangeEnd )
                    {
                        canContinue = true;

                        return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                    }
                }
            }
            else if (_state == ParserStates.AfterInteger)
            {
                return "Цифра не может идти после пробела";
            }
            else if ( _state == ParserStates.AfterSpaceWithoutPunctuation )
            {
                return "Отсутствует пунктуация";
            }
        }
        else if( ! glyphIsInteger )
        {
            switch (glyph)
            {
                case ' ':

                    if ( _state == ParserStates.InsideIntegerOrFirstInRange )
                    {
                        _state = ParserStates.AfterInteger;
                    }
                    else if ( _state == ParserStates.InRangeEnd )
                    {
                        string presentation = "";

                        foreach ( char ch in _numAsChars )
                        {
                            presentation += ch;
                        }

                        int rangeEnd = int.Parse ( presentation );

                        if ( _rangeStart > rangeEnd )
                        {
                            return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                        }

                        for ( int index = _rangeStart; index <= rangeEnd; index++ )
                        {
                            _pageNumbers.Add ( index );
                        }

                        _numAsChars = new ();
                        _state = ParserStates.AfterSpaceWithoutPunctuation;
                    }
                    else if ( _state == ParserStates.InRange ) 
                    {
                        if ( isGlyphLast )
                        {
                            canContinue = true;

                            return "Тире не может быть последним в строке";
                        }
                    }

                    break;
                case '-':

                    if (( _state == ParserStates.InsideIntegerOrFirstInRange )   ||   ( _state == ParserStates.AfterInteger ))
                    {
                        _state = ParserStates.InRange;

                        if ( isGlyphLast )
                        {
                            canContinue = true;

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
                    else if ( _state == ParserStates.AfterSpaceWithoutPunctuation) 
                    {
                        return "Тире не может идти после интервала";
                    }

                    break;
                case ',':

                    if ((_state == ParserStates.InsideIntegerOrFirstInRange)   ||   (_state == ParserStates.AfterInteger))
                    {
                        string presentation = "";

                        foreach (char ch   in   _numAsChars)
                        {
                            presentation += ch;
                        }

                        _numAsChars = new();
                        int integer = int.Parse( presentation );
                        _pageNumbers.Add( integer );
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if (isGlyphLast)
                        {
                            canContinue = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }
                    else if (_state == ParserStates.InRangeEnd)
                    {
                        string presentation = "";

                        foreach (char ch   in   _numAsChars)
                        {
                            presentation += ch;
                        }

                        int rangeEnd = int.Parse( presentation );

                        if (_rangeStart > rangeEnd)
                        {
                            return "Некорректный интервал. Начальный номер должен быть не больше конечного.";
                        }

                        for (int index = _rangeStart;   index <= rangeEnd;   index++)
                        {
                            _pageNumbers.Add( index );
                        }

                        _numAsChars = new();
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if (isGlyphLast)
                        {
                            canContinue = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }
                    else if (_state == ParserStates.BeforeOrBetweenOrAfterGlyph)
                    {
                        if (_pageNumbers.Count < 1)
                        {
                            return "Запятая не может быть первой";
                        }
                        else
                        {
                            return "Запятые не могут идти подряд";
                        }
                    }
                    else if (_state == ParserStates.InRange)
                    {
                        return "Запятая не может идти после тире";
                    }
                    else if ( _state == ParserStates.AfterSpaceWithoutPunctuation )
                    {
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if ( isGlyphLast )
                        {
                            canContinue = true;

                            return "Запятая не может быть последней в строке";
                        }
                    }

                    break;
            }
        }

        return string.Empty;
    }


    private string RemoveUnacceptableGlyph(string value, List<char> acceptableGlyphs)
    {
        if (string.IsNullOrWhiteSpace( value ))
        {
            return string.Empty;
        }

        bool glyphIsIncorrect = true;

        for (int index = 0;   index < value.Length;   index++)
        {
            char ch = value[index];

            glyphIsIncorrect = acceptableGlyphs.Contains( ch );

            if ( ! glyphIsIncorrect )
            {
                value = value.Remove( index, 1 );
                break;
            }
        }

        return value;
    }


    private bool IsFirstDigitZero(string value)
    {
        bool firstDigitIsZero = false;

        char ch = value[0];

        if (ch == '0')
        {
            firstDigitIsZero = true;
        }

        return firstDigitIsZero;
    }


    private ObservableCollection<PrinterPresentation> PreparePrintersListOnWindows()
    {
        ObservableCollection<PrinterPresentation> printersList = new();

        PrinterSettings settings = new PrinterSettings();
        string defaultPrinterName = settings.PrinterName;

        PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;

        int counter = 0;

        foreach (string printer   in   printers)
        {
            printersList.Add( new PrinterPresentation( printer ) );

            if (defaultPrinterName == printer)
            {
                SelectedIndex = counter;
            }

            counter++;
        }

        return printersList;
    }


    private ObservableCollection<PrinterPresentation> PreparePrintersListOnLinux()
    {
        ObservableCollection<PrinterPresentation> printersList = new();

        string defaultPrinterName = TerminalCommandExecuter.ExecuteCommand( _linuxGetDefaultPrinterBash );

        string printersLine = TerminalCommandExecuter.ExecuteCommand( _linuxGetPrintersBash );

        string[] printers = printersLine.Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries );

        int counter = 0;

        foreach (string printer   in   printers)
        {
            string prntr = printer.TrimLastNewLineChar();

            printersList.Add( new PrinterPresentation( prntr ) );

            if (defaultPrinterName == prntr)
            {
                SelectedIndex = counter;
            }

            counter++;
        }

        return printersList;
    }


    private void HandleEmptyPrinters(ObservableCollection<PrinterPresentation> printersList)
    {
        bool printersIsEmpty = printersList.Count < 1;

        if (printersIsEmpty)
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
