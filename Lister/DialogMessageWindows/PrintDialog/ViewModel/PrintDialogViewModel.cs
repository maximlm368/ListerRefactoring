using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using View.App;
using View.Extentions;
using View.CoreAbstractionsImplimentations.DocumentProcessor;

namespace View.DialogMessageWindows.PrintDialog.ViewModel;

internal partial class PrintDialogViewModel : ReactiveObject
{
    private readonly string _warnImagePath;

    private readonly string _linuxGetPrintersBash = "lpstat -p | awk '{print $2}'";
    private readonly string _linuxGetDefaultPrinterBash = "lpstat -d";

    private readonly string _emptyCopies;
    private readonly string _emptyPages;
    private readonly string _emptyPrinters;

    private readonly int _copiesMaxCount = 10;
    private readonly int _pagesStringMaxGlyphCount = 30;

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
    private List<int> _chosenPagesToPrint;
    private bool _pagesHinderPrinting;
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

    private Bitmap _warnImage;
    internal Bitmap WarnImage
    {
        get { return _warnImage; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _warnImage, value, nameof( WarnImage ) );
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
            if (!value)
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
            value = RemoveUnacceptableGlyph( value, _pageNumsAcceptables );

            if (!string.IsNullOrEmpty( value ) && value.Length > _pagesStringMaxGlyphCount)
            {
                value = Pages;
            }

            try
            {
                List<int> pageNumbers;
                pageNumbers = GetPagesFromString( value );

                for (int index = 0; index < pageNumbers.Count; index++)
                {
                    int currentNum = pageNumbers[index];

                    if (currentNum > _passedPagesAmount - 1)
                    {
                        value = Pages;
                        break;
                    }
                }
            }
            catch (TransparentForTypingParsingException ex) { }
            catch (ParsingException ex)
            {
                string changer = value;
                value = _pages;
                _pages = changer;
            }

            _pagesHinderPrinting = false;
            this.RaiseAndSetIfChanged( ref _pages, value, nameof( Pages ) );
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


    internal PrintDialogViewModel(string warnImagePath, string emptyCopies, string emptyPages, string emptyPrinters
                                , string osName)
    {
        _warnImagePath = warnImagePath;
        _emptyCopies = emptyCopies;
        _emptyPages = emptyPages;
        _emptyPrinters = emptyPrinters;
        _osName = osName;

        string correctnessIcon = ListerApp.ResourceFolderName + _warnImagePath;
        WarnImage = ImageHelper.LoadFromResource( correctnessIcon );

        CanvasBackground = new SolidColorBrush( new Color( 255, 240, 240, 240 ) );
        LineBackground = new SolidColorBrush( new Color( 255, 220, 220, 220 ) );

        PagesBorderColor = new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
        CopiesBorderColor = new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
    }


    internal void PagesLostFocus()
    {
        try
        {
            GetPagesFromString( Pages );
        }
        catch (ParsingException ex)
        {
            PagesError = ex.Message;
            _pagesListError = ex.Message;
            _pagesHinderPrinting = true;
        }

        if (string.IsNullOrEmpty( Pages ))
        {
            PagesError = _emptyPages;
            _pagesHinderPrinting = true;
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


    internal void Print()
    {
        try
        {
            if (string.IsNullOrEmpty( Copies ))
            {
                throw new ParsingException( _emptyCopies );
            }

            if (string.IsNullOrEmpty( Pages )   &&   Some)
            {
                throw new ParsingException( _emptyPages );
            }

            PrinterPresentation printer = Printers[SelectedIndex];
            _printingAdjusting.PrinterName = printer.StringPresentation;

            if ( ! Some )
            {
                _chosenPagesToPrint = new List<int>();

                for (int index = 0;   index < _passedPagesAmount;   index++)
                {
                    _chosenPagesToPrint.Add( index );
                }
            }
            else
            {
                _chosenPagesToPrint = GetPagesFromString( Pages );
            }

            _printingAdjusting.PageNumbers = _chosenPagesToPrint;
            _printingAdjusting.CopiesAmount = int.Parse( Copies );
            _printingAdjusting.Cancelled = false;
            NeedClose = true;
        }
        catch (ParsingException ex)
        {
            if (ex.Message == _emptyPages)
            {
                PagesError = ex.Message;
            }
            else if (ex.Message == _emptyCopies)
            {
                CopiesError = ex.Message;
            }
        }
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


    private List<int> GetPagesFromString(string pageNumbers)
    {
        List<int> result = new();

        if (_printingAdjusting.Cancelled || pageNumbers == null)
        {
            return result;
        }

        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;
        _numAsChars = new();
        _pageNumbers = new();

        for (int index = 0; index < pageNumbers.Length; index++)
        {
            bool isGlyphLast = false;

            if (index == pageNumbers.Length - 1)
            {
                isGlyphLast = true;
            }

            char glyph = pageNumbers[index];
            ParsePageNumbers( glyph, isGlyphLast );
        }

        _pageNumbers.Sort();

        for (int index = 0; index < _pageNumbers.Count; index++)
        {
            result.Add( _pageNumbers[index] - 1 );
        }

        return result;
    }


    private void ParsePageNumbers(char glyph, bool isGlyphLast)
    {
        bool glyphIsInteger = _copiesCountAcceptables.Contains( glyph );

        if (glyphIsInteger)
        {
            int num = int.Parse( glyph.ToString() );

            if ((_state == ParserStates.BeforeOrBetweenOrAfterGlyph)   ||   (_state == ParserStates.InsideIntegerOrFirstInRange))
            {
                if ((_state == ParserStates.BeforeOrBetweenOrAfterGlyph)   &&   (glyph == '0'))
                {
                    throw new ParsingException( "Число не может начинаться с ноля" );
                }

                _state = ParserStates.InsideIntegerOrFirstInRange;
                _numAsChars.Add( glyph );

                if (isGlyphLast)
                {
                    string presentation = "";

                    foreach (char ch in _numAsChars)
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
                    throw new ParsingException( "Число не может начинаться с ноля" );
                }

                string presentation = "";

                foreach (char ch in _numAsChars)
                {
                    presentation += ch;
                }

                _rangeStart = int.Parse( presentation );

                _numAsChars = new();
                _state = ParserStates.InRangeEnd;
                _numAsChars.Add( glyph );

                presentation = "";

                foreach (char ch   in   _numAsChars)
                {
                    presentation += ch;
                }

                int rangeEnd = int.Parse( presentation );

                if (_rangeStart > rangeEnd)
                {
                    if ((rangeEnd * 10) > _passedPagesAmount)
                    {
                        throw new ParsingException( "Некорректный интервал" );
                    }
                }

                if (isGlyphLast)
                {
                    for (int index = _rangeStart; index <= rangeEnd; index++)
                    {
                        _pageNumbers.Add( index );
                    }
                }
            }
            else if (_state == ParserStates.InRangeEnd)
            {
                _numAsChars.Add( glyph );

                string presentation = "";

                foreach (char ch   in   _numAsChars)
                {
                    presentation += ch;
                }

                int rangeEnd = int.Parse( presentation );

                if (_rangeStart > rangeEnd)
                {
                    if ((rangeEnd * 10) > _passedPagesAmount)
                    {
                        throw new ParsingException( "Некорректный интервал" );
                    }
                }

                if (isGlyphLast)
                {
                    for (int index = _rangeStart;   index <= rangeEnd;   index++)
                    {
                        _pageNumbers.Add( index );
                    }
                }
            }
            else if (_state == ParserStates.AfterInteger)
            {
                throw new ParsingException( "Цифра не может идти после пробела" );
            }
        }
        else if ( ! glyphIsInteger )
        {
            switch (glyph)
            {
                case ' ':

                    if (_state == ParserStates.InsideIntegerOrFirstInRange)
                    {
                        _state = ParserStates.AfterInteger;
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
                            throw new ParsingException( "Некорректный интервал" );
                        }

                        for (int index = _rangeStart;   index <= rangeEnd;   index++)
                        {
                            _pageNumbers.Add( index );
                        }

                        _numAsChars = new();
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;
                    }

                    break;
                case '-':

                    if ((_state == ParserStates.InsideIntegerOrFirstInRange)   ||   (_state == ParserStates.AfterInteger))
                    {
                        _state = ParserStates.InRange;

                        if (isGlyphLast)
                        {
                            throw new TransparentForTypingParsingException( "Тире не может быть последним в строке" );
                        }
                    }
                    else if (_state == ParserStates.BeforeOrBetweenOrAfterGlyph)
                    {
                        if (_pageNumbers.Count < 1)
                        {
                            throw new ParsingException( "Тире не может быть первым" );
                        }
                        else
                        {
                            throw new ParsingException( "Тире может быть только внутри интервала" );
                        }
                    }
                    else if (_state == ParserStates.InRange)
                    {
                        throw new ParsingException( "Тире не может идти подряд" );
                    }
                    else if (_state == ParserStates.InRangeEnd)
                    {
                        throw new ParsingException( "Тире не может идти после интервала" );
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
                            throw new TransparentForTypingParsingException( "Запятая не может быть последней в строке" );
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
                            throw new ParsingException( "Некорректный интервал" );
                        }

                        for (int index = _rangeStart;   index <= rangeEnd;   index++)
                        {
                            _pageNumbers.Add( index );
                        }

                        _numAsChars = new();
                        _state = ParserStates.BeforeOrBetweenOrAfterGlyph;

                        if (isGlyphLast)
                        {
                            throw new TransparentForTypingParsingException( "Запятая не может быть последней в строке" );
                        }
                    }
                    else if (_state == ParserStates.BeforeOrBetweenOrAfterGlyph)
                    {
                        if (_pageNumbers.Count < 1)
                        {
                            throw new ParsingException( "Запятая не может быть первой" );
                        }
                        else
                        {
                            throw new ParsingException( "Запятые не могут идти подряд" );
                        }
                    }
                    else if (_state == ParserStates.InRange)
                    {
                        throw new ParsingException( "Запятая не может идти после тире" );
                    }

                    break;
            }
        }
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
        InRangeEnd = 4
    }



    private class ParsingException : Exception
    {
        public ParsingException ( string message ) : base ( message ) { }
    }



    private class TransparentForTypingParsingException : ParsingException
    {
        public TransparentForTypingParsingException ( string message ) : base ( message ) { }
    }
}



internal class PrinterPresentation : ReactiveObject
{
    private string sP;
    internal string StringPresentation
    {
        get { return sP; }
        private set
        {
            this.RaiseAndSetIfChanged ( ref sP, value, nameof ( StringPresentation ) );
        }
    }


    internal PrinterPresentation ( string printerName )
    {
        StringPresentation = printerName;
    }
}
