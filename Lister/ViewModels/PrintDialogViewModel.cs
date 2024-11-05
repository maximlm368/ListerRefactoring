using Avalonia.Media.Imaging;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Lister.Extentions;
using Lister.Views;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Interactivity;
using static QuestPDF.Helpers.Colors;
using System.Collections.Specialized;
using ExtentionsAndAuxiliary;

namespace Lister.ViewModels
{
    public partial class PrintDialogViewModel : ViewModelBase
    {
        private static string _warnImageName = "warning-alert.ico";
        public static bool IsClosed { get; private set; }

        private readonly string _linuxGetPrintersBash = "lpstat -p | awk '{print $2}'";
        private readonly string _linuxGetDefaultPrinterBash = "lpstat -d";

        private readonly string _emptyCopies = "Количество копий не может быть пустым";
        private readonly string _emptyPages = "Список страниц не может быть пустым";

        private readonly int _copiesMaxCount = 10;
        private readonly int _pagesStringMaxGlyphCount = 30;

        private readonly List<char> _pageNumsAcceptables
                                   = new List<char> () { ' ', '-', ',', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly List<char> _copiesCountAcceptables
                                   = new List<char> () { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        //private int _pagesStringGlyphCount;
        private List<char> _numAsChars;
        private List<int> _pageNumbers;
        private List<int> _currentRange;
        private ParserStates _state;
        private int _rangeStart;
        private PrintDialog _view;
        private int _passedPagesAmount;
        private PrintAdjustingData _printingAdjusting;
        private List <int> _chosenPagesToPrint;
        private bool _pagesHinderPrinting;
        private bool _copiesHinderPrinting;
        private string _pagesListError;

        private SolidColorBrush lB;
        public SolidColorBrush LineBackground
        {
            get { return lB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref lB, value, nameof (LineBackground));
            }
        }

        private SolidColorBrush cB;
        public SolidColorBrush CanvasBackground
        {
            get { return cB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cB, value, nameof (CanvasBackground));
            }
        }

        private Bitmap wI;
        public Bitmap WarnImage
        {
            get { return wI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wI, value, nameof (WarnImage));
            }
        }

        private ObservableCollection <PrinterPresentation> _printers;
        public ObservableCollection <PrinterPresentation> Printers
        {
            get { return _printers; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _printers, value, nameof (Printers));
            }
        }

        private bool some;
        public bool Some
        {
            get { return some; }
            private set
            {
                if ( ! value )
                {
                    Pages = string.Empty;
                }

                this.RaiseAndSetIfChanged (ref some, value, nameof (Some));
            }
        }

        private string ps;
        public string Pages
        {
            get { return ps; }
            private set
            {
                PagesError = string.Empty;
                value = RemoveUnacceptableGlyph (value, _pageNumsAcceptables);

                if ( ( ! string.IsNullOrEmpty(value))   &&   (value.Length > _pagesStringMaxGlyphCount) ) 
                {
                    value = Pages;
                }

                try
                {
                    List<int> pageNumbers;
                    pageNumbers = GetPagesFromString (value);

                    SceneViewModel sceneVM = App.services.GetRequiredService<SceneViewModel> ();

                    for ( int index = 0;   index < pageNumbers.Count;   index++ )
                    {
                        int currentNum = pageNumbers [index];

                        if ( currentNum > ( _passedPagesAmount - 1 ) )
                        {
                            value = Pages;
                            break;
                        }
                    }
                }
                catch ( TransparentForTypingParsingException ex )
                {
                }
                catch ( ParsingException ex )
                {
                    value = Pages;
                }

                _pagesHinderPrinting = false;
                this.RaiseAndSetIfChanged (ref ps, value, nameof (Pages));
            }
        }

        private SolidColorBrush pBC;
        public SolidColorBrush PagesBorderColor
        {
            get { return pBC; }
            set
            {
                this.RaiseAndSetIfChanged (ref pBC, value, nameof (PagesBorderColor));
            }
        }

        private string perr;
        public string PagesError
        {
            get { return perr; }
            set
            {
                if ( string.IsNullOrEmpty (value) )
                {
                    PagesBorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
                }
                else
                {
                    PagesBorderColor = new SolidColorBrush (new Color (255, 255, 0, 0));
                }

                this.RaiseAndSetIfChanged (ref perr, value, nameof (PagesError));
            }
        }

        private string copies;
        public string Copies
        {
            get { return copies; }
            set
            {
                value = RemoveUnacceptableGlyph (value, _copiesCountAcceptables);
                _copiesHinderPrinting = false;

                if ( CopiesError == _emptyCopies ) 
                {
                    CopiesError = string.Empty;
                }

                if ( ! string.IsNullOrEmpty(value) )
                {
                    if ( (IsFirstDigitZero(value))   ||   ( Int32.Parse(value) > _copiesMaxCount ) ) 
                    {
                        value = Copies;
                    }
                }

                this.RaiseAndSetIfChanged (ref copies, value, nameof (Copies));
            }
        }

        private SolidColorBrush cBC;
        public SolidColorBrush CopiesBorderColor
        {
            get { return cBC; }
            set
            {
                this.RaiseAndSetIfChanged (ref cBC, value, nameof (CopiesBorderColor));
            }
        }

        private string cerr;
        public string CopiesError
        {
            get { return cerr; }
            set
            {
                if ( string.IsNullOrEmpty (value) )
                {
                    CopiesBorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
                }
                else
                {
                    CopiesBorderColor = new SolidColorBrush (new Color (255, 255, 0, 0));
                }

                this.RaiseAndSetIfChanged (ref cerr, value, nameof (CopiesError));
            }
        }

        private int sI;
        public int SelectedIndex
        {
            get { return sI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sI, value, nameof (SelectedIndex));
            }
        }


        public PrintDialogViewModel ( )
        {
            string correctnessIcon = App.ResourceDirectoryUri + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);

            CanvasBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 240, 240, 240));
            LineBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 220, 220, 220));

            PagesBorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            CopiesBorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
        }


        public void PagesLostFocus ( )
        {
            //try
            //{
            //    GetPagesFromString (Pages);
            //}
            //catch (ParsingException ex) 
            //{
            //    PagesError = ex.Message;
            //    _pagesListError = ex.Message;
            //    _pagesHinderPrinting = true;
            //}

            if ( string.IsNullOrEmpty (Pages) )
            {
                PagesError = _emptyPages;
                _pagesHinderPrinting = true;
            }
        }


        public void PagesGotFocus ()
        {
            PagesError = string.Empty;
        }


        public void CopiesLostFocus ( )
        {
            if ( string.IsNullOrEmpty(Copies) ) 
            {
                CopiesError = _emptyCopies;
                _copiesHinderPrinting = true;
            }
        }


        public void CopiesGotFocus ()
        {
            CopiesError = string.Empty;
        }


        public void Print ()
        {
            //if ( _pagesHinderPrinting ) 
            //{
            //    PagesError = _pagesListError;
            //    return;
            //}

            //if ( _copiesHinderPrinting )
            //{
            //    CopiesError = _emptyCopies;
            //    return;
            //}

            try
            {
                if ( string.IsNullOrEmpty (Copies) )
                {
                    throw new ParsingException (_emptyCopies);
                }

                if ( string.IsNullOrEmpty (Pages)   &&   Some )
                {
                    throw new ParsingException (_emptyPages);
                }

                PrinterPresentation printer = Printers [SelectedIndex];
                _printingAdjusting.PrinterName = printer.StringPresentation;

                if ( ! Some )
                {
                    _chosenPagesToPrint = new List<int> ();

                    for ( int index = 0;   index < _passedPagesAmount;   index++ )
                    {
                        _chosenPagesToPrint.Add (index);
                    }
                }
                else
                {
                    _chosenPagesToPrint = GetPagesFromString (Pages);
                }

                _printingAdjusting.PageNumbers = _chosenPagesToPrint;
                _printingAdjusting.CopiesAmount = Int32.Parse (Copies);
                _printingAdjusting.Cancelled = false;
                IsClosed = true;
                _view.Close ();
            }
            catch (ParsingException ex) 
            {
                if ( ex.Message == _emptyPages )
                {
                    PagesError = ex.Message;
                }
                else if ( ex.Message == _emptyCopies ) 
                {
                    CopiesError = ex.Message;
                }
            }
        }


        private List <int> ? GetPagesFromString ( string pageNumbers )
        {
            if ( _printingAdjusting.Cancelled   ||   (pageNumbers == null) ) 
            {
                return null;
            }

            List <int> result = new ();
            _state = ParserStates.BeforeBetweenAfter;
            _numAsChars = new ();
            _pageNumbers = new ();

            for ( int index = 0;   index < pageNumbers. Length;   index++ ) 
            {
                bool isGlyphLast = false;

                if ( index == pageNumbers. Length - 1 ) 
                {
                    isGlyphLast = true;
                }

                char glyph = pageNumbers [index];
                ParsePageNumbers (glyph, isGlyphLast);
            }

            _pageNumbers.Sort ();

            for ( int index = 0;   index < _pageNumbers.Count;   index++ )
            {
                result.Add (_pageNumbers [index] - 1);
            }

            return result;
        }


        private void ParsePageNumbers ( char glyph, bool isGlyphLast )
        {
            bool glyphIsInteger = _copiesCountAcceptables.Contains ( glyph );

            if (glyphIsInteger)
            {
                int num = Int32.Parse (glyph.ToString ());

                if ( ( _state == ParserStates.BeforeBetweenAfter )  ||  ( _state == ParserStates.InsideIntegerOrFirstInRange ) )
                {
                    if ( ( _state == ParserStates.BeforeBetweenAfter )   &&   ( glyph == '0' ) )
                    {
                        throw new ParsingException ("Число не может начинаться с ноля");
                    }

                    _state = ParserStates.InsideIntegerOrFirstInRange;
                    _numAsChars.Add (glyph);

                    if ( isGlyphLast )
                    {
                        string presentation = "";

                        foreach ( char ch   in   _numAsChars )
                        {
                            presentation += ch;
                        }

                        int integer = Int32.Parse (presentation);

                        _pageNumbers.Add (integer);
                    }
                }
                else if ( _state == ParserStates.InRange )
                {
                    if ( glyph == '0' )
                    {
                        throw new ParsingException ("Число не может начинаться с ноля");
                    }

                    string presentation = "";

                    foreach ( char ch   in   _numAsChars )
                    {
                        presentation += ch;
                    }

                    _rangeStart = Int32.Parse (presentation);

                    _numAsChars = new ();
                    _state = ParserStates.InRangeEnd;
                    _numAsChars.Add (glyph);

                    presentation = "";

                    foreach ( char ch in _numAsChars )
                    {
                        presentation += ch;
                    }

                    int rangeEnd = Int32.Parse (presentation);

                    if ( _rangeStart > rangeEnd )
                    {
                        if ( ( rangeEnd * 10 ) > _passedPagesAmount )
                        {
                            throw new ParsingException ("Некорректный интервал");
                        }
                        //else
                        //{
                        //    throw new TransparentForTypingParsingException ("Некорректный интервал");
                        //}
                    }

                    if ( isGlyphLast )
                    {
                        for ( int index = _rangeStart;   index <= rangeEnd;   index++ )
                        {
                            _pageNumbers.Add (index);
                        }
                    }
                }
                else if ( _state == ParserStates.InRangeEnd )
                {
                    _numAsChars.Add (glyph);

                    string presentation = "";

                    foreach ( char ch   in   _numAsChars )
                    {
                        presentation += ch;
                    }

                    int rangeEnd = Int32.Parse (presentation);

                    if ( _rangeStart > rangeEnd )
                    {
                        if ( ( rangeEnd * 10 ) > _passedPagesAmount )
                        {
                            throw new ParsingException ("Некорректный интервал");
                        }
                        //else
                        //{
                        //    throw new TransparentForTypingParsingException ("Некорректный интервал");
                        //}
                    }

                    if ( isGlyphLast )
                    {
                        for ( int index = _rangeStart;   index <= rangeEnd;   index++ )
                        {
                            _pageNumbers.Add (index);
                        }
                    }
                }
                else if ( _state == ParserStates.AfterInteger )
                {
                    throw new ParsingException ("Цифра не может идти после пробела");
                }
            }
            else if ( ! glyphIsInteger )
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
                            string presentation = "";

                            foreach ( char ch   in   _numAsChars )
                            {
                                presentation += ch;
                            }

                            int rangeEnd = Int32.Parse (presentation);

                            if ( _rangeStart > rangeEnd )
                            {
                                throw new ParsingException ("Некорректный интервал");
                            }

                            for ( int index = _rangeStart;   index <= rangeEnd;   index++ )
                            {
                                _pageNumbers.Add (index);
                            }

                            _numAsChars = new ();
                            _state = ParserStates.BeforeBetweenAfter;
                        }

                        break;
                    case '-':

                        if ( ( _state == ParserStates.InsideIntegerOrFirstInRange )  ||  ( _state == ParserStates.AfterInteger ) )
                        {
                            _state = ParserStates.InRange;

                            if ( isGlyphLast )
                            {
                                throw new TransparentForTypingParsingException ("Тире не может быть последним в строке");
                            }
                        }
                        else if ( _state == ParserStates.BeforeBetweenAfter )
                        {
                            if ( _pageNumbers.Count < 1 )
                            {
                                throw new ParsingException ("Тире не может быть первым");
                            }
                            else
                            {
                                throw new ParsingException ("Тире может быть только внутри интервала");
                            }
                        }
                        else if ( _state == ParserStates.InRange )
                        {
                            throw new ParsingException ("Тире не может идти подряд");
                        }
                        else if ( _state == ParserStates.InRangeEnd )
                        {
                            throw new ParsingException ("Тире не может идти после интервала");
                        }

                        break;
                    case ',':

                        if ( ( _state == ParserStates.InsideIntegerOrFirstInRange )  ||  ( _state == ParserStates.AfterInteger ) )
                        {
                            string presentation = "";

                            foreach ( char ch   in   _numAsChars )
                            {
                                presentation += ch;
                            }

                            _numAsChars = new ();

                            int integer = Int32.Parse (presentation);

                            _pageNumbers.Add (integer);

                            _state = ParserStates.BeforeBetweenAfter;

                            if ( isGlyphLast )
                            {
                                throw new TransparentForTypingParsingException ("Запятая не может быть последней в строке");
                            }
                        }
                        else if ( _state == ParserStates.InRangeEnd )
                        {
                            string presentation = "";

                            foreach ( char ch   in   _numAsChars )
                            {
                                presentation += ch;
                            }

                            int rangeEnd = Int32.Parse (presentation);

                            if ( _rangeStart > rangeEnd )
                            {
                                throw new ParsingException ("Некорректный интервал");
                            }

                            for ( int index = _rangeStart;   index <= rangeEnd;   index++ )
                            {
                                _pageNumbers.Add (index);
                            }

                            _numAsChars = new ();
                            _state = ParserStates.BeforeBetweenAfter;

                            if ( isGlyphLast )
                            {
                                throw new TransparentForTypingParsingException ("Запятая не может быть последней в строке");
                            }
                        }
                        else if ( _state == ParserStates.BeforeBetweenAfter )
                        {
                            if ( _pageNumbers.Count < 1 )
                            {
                                throw new ParsingException ("Запятая не может быть первой");
                            }
                            else
                            {
                                throw new ParsingException ("Запятые не могут идти подряд");
                            }
                        }
                        else if ( _state == ParserStates.InRange )
                        {
                            throw new ParsingException ("Запятая не может идти после тире");
                        }

                        break;
                }
            }
        }


        private string RemoveUnacceptableGlyph ( string value, List<char> acceptableGlyphs )
        {
            bool glyphIsIncorrect = true;

            for ( int index = 0;   index < value.Length;   index++ )
            {
                char ch = value [index];

                glyphIsIncorrect = acceptableGlyphs.Contains ( ch );

                if ( ! glyphIsIncorrect )
                {
                    value = value.Remove (index, 1);
                    break;
                }
            }

            return value;
        }


        private bool IsFirstDigitZero ( string value )
        {
            bool firstDigitIsZero = false;

            char ch = value [0];

            if ( ch == '0' )
            {
                firstDigitIsZero = true;
            }

            return firstDigitIsZero;
        }


        public void Cancel ()
        {
            CopiesError = string.Empty;
            PagesError = string.Empty;

            _printingAdjusting.Cancelled = true;
            IsClosed = true;
            SceneViewModel sceneVM = App.services.GetRequiredService<SceneViewModel> ();
            sceneVM.HandleDialogClosing ();

            _view.Close ();
        }


        public void Prepare ()
        {
            IsClosed = false;

            ObservableCollection <PrinterPresentation> printersList = new ();

            if ( App.OsName == "Windows" )
            {
                PrinterSettings settings = new PrinterSettings ();
                string defaultPrinterName = settings.PrinterName;

                PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;

                int counter = 0;

                foreach ( string printer   in   printers )
                {
                    printersList.Add (new PrinterPresentation (printer));

                    if ( defaultPrinterName == printer )
                    {
                        SelectedIndex = counter;
                    }

                    counter++;
                }
            }
            else if ( App.OsName == "Linux" )
            {
                string defaultPrinterName = App.ExecuteBashCommand (_linuxGetDefaultPrinterBash);

                string printersLine = App.ExecuteBashCommand (_linuxGetPrintersBash);
                List<string> printers = printersLine.SplitBySeparators (new () { '\n' });

                int counter = 0;

                foreach ( string printer   in   printers )
                {
                    string prntr = printer.TrimLastNewLineChar ();

                    printersList.Add (new PrinterPresentation (prntr));

                    if ( defaultPrinterName == prntr )
                    {
                        SelectedIndex = counter;
                    }

                    counter++;
                }
            }

            Printers = printersList;
            Copies = "1";
        }


        public void TakeAmmountAndAdjusting ( int pageAmmount, PrintAdjustingData printAdjusting )
        {
            _passedPagesAmount = pageAmmount;
            _printingAdjusting = printAdjusting;
        }


        public void TakeView ( PrintDialog view )
        {
            _view = view;
        }



        private enum ParserStates 
        {
            BeforeBetweenAfter = 0,
            InsideIntegerOrFirstInRange = 1,
            InRange = 2,
            AfterInteger = 3,
            InRangeEnd = 4
        }
    }



    public class ParsingException : Exception
    {
        public ParsingException ( string message ) : base (message) { }
    }



    public class TransparentForTypingParsingException : ParsingException
    {
        public TransparentForTypingParsingException ( string message ) : base (message) { }
    }



    public class IncorrectStringStartException : ParsingException
    {
        public IncorrectStringStartException ( string message ) : base (message) { }
    }



    public class PrinterPresentation : ViewModelBase 
    {
        private string sP;
        public string StringPresentation
        {
            get { return sP; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sP, value, nameof (StringPresentation));
            }
        }


        public PrinterPresentation ( string printerName ) 
        {
            StringPresentation = printerName;
        }
    }
}