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

namespace Lister.ViewModels
{
    public partial class PrintDialogViewModel : ViewModelBase
    {
        private static readonly string _notIntegerGlyph = "";
        private static readonly string _outOfRange = "";
        private static readonly string _glyphRepiting = "";
        private static string _warnImageName = "warning-alert.ico";
        public static bool IsClosed { get; private set; }


        private List<char> _numAsChars;
        private List<int> _pageNumbers;
        private List<int> _currentRange;
        private ParserStates _state;
        private int _rangeStart;
        private PrintDialog _view;
        private int _passedPagesAmount;
        private PrintAdjustingData _printingAdjusting;
        private List <int> _chosenPagesToPrint;

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

        //private bool all;
        //public bool All
        //{
        //    get { return all; }
        //    private set
        //    {
        //        //this.RaiseAndSetIfChanged (ref all, value, nameof (All));

        //        all = value;
        //    }
        //}

        private bool some;
        public bool Some
        {
            get { return some; }
            private set
            {
                if ( !value )
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
                this.RaiseAndSetIfChanged (ref ps, value, nameof (Pages));
            }
        }

        private string copies;
        public string Copies
        {
            get { return copies; }
            set
            {
                this.RaiseAndSetIfChanged (ref copies, value, nameof (Copies));
            }
        }

        private int sI;
        public int SelectedIndex
        {
            get { return sI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sI, value, nameof (SelectedIndex));
                int dff = 0;
            }
        }


        public PrintDialogViewModel ( )
        {
            string correctnessIcon = App.ResourceDirectoryUri + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);

            CanvasBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 240, 240, 240));
            LineBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 220, 220, 220));
        }


        public void Print ()
        {
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
                _chosenPagesToPrint = GetPagesFromString ();
            }

            _printingAdjusting.PageNumbers = _chosenPagesToPrint;

            try
            {
                _printingAdjusting.CopiesAmount = Int32.Parse (Copies);
            }
            catch ( FormatException ex )
            {
                throw new ParsingExeption (_notIntegerGlyph);
            }

            _printingAdjusting.Cancelled = false;
            IsClosed = true;
            _view.Close ();
        }


        private List <int> GetPagesFromString ()
        {
            List <int> result = new ();
            _state = ParserStates.BeforeBetweenAfter;
            _numAsChars = new ();
            _pageNumbers = new ();

            for ( int index = 0;   index < Pages. Length;   index++ ) 
            {
                bool isGlyphLast = false;

                if ( index == Pages. Length - 1 ) 
                {
                    isGlyphLast = true;
                }

                char glyph = Pages [index];
                ParsePageNumbers (glyph, isGlyphLast);
            }

            for ( int index = 0;   index < _pageNumbers.Count;   index++ )
            {
                if ( _pageNumbers [index] > _passedPagesAmount ) 
                {
                    throw new ParsingExeption (_outOfRange);
                }

                result.Add (_pageNumbers [index] - 1);
            }

            return result;
        }


        private void ParsePageNumbers ( char glyph, bool isGlyphLast )
        {
            try
            {
                int num = Int32.Parse (glyph.ToString());

                if ( (_state == ParserStates.BeforeBetweenAfter)   ||   ( _state == ParserStates.InsideIntegerOrFirstInRange ) )
                {
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
                    string presentation = "";

                    foreach ( char ch   in   _numAsChars ) 
                    {
                        presentation += ch;
                    }

                    _rangeStart = Int32.Parse (presentation);

                    _numAsChars = new ();
                    _state = ParserStates.InRangeEnd;
                    _numAsChars.Add (glyph);

                    if ( isGlyphLast )
                    {
                        presentation = "";

                        foreach ( char ch   in   _numAsChars )
                        {
                            presentation += ch;
                        }

                        int rangeEnd = Int32.Parse (presentation);

                        for ( int index = _rangeStart; index <= rangeEnd; index++ )
                        {
                            _pageNumbers.Add (index);
                        }
                    }
                }
                else if ( _state == ParserStates.InRangeEnd )
                {
                    _numAsChars.Add (glyph);

                    if ( isGlyphLast )
                    {
                        string presentation = "";

                        foreach ( char ch   in   _numAsChars )
                        {
                            presentation += ch;
                        }

                        int rangeEnd = Int32.Parse (presentation);

                        for ( int index = _rangeStart;   index <= rangeEnd;   index++ )
                        {
                            _pageNumbers.Add (index);
                        }
                    }
                }
            }
            catch ( FormatException ex ) 
            {
                if ( isGlyphLast   &&   ( _state == ParserStates.InRange ) )
                {
                    throw new ParsingExeption (_notIntegerGlyph);
                }

                switch ( glyph )
                {
                    case ' ':

                        if ( _state == ParserStates.InsideIntegerOrFirstInRange )
                        {
                            _state = ParserStates.AfterInteger;
                        }
                        else if ( _state == ParserStates.InRangeEnd )
                        {
                            //_numAsChars.Add (glyph);

                            string presentation = "";

                            foreach ( char ch   in   _numAsChars )
                            {
                                presentation += ch;
                            }

                            int rangeEnd = Int32.Parse (presentation);

                            for ( int index = _rangeStart;   index <= rangeEnd;   index++ )
                            {
                                _pageNumbers.Add (index);
                            }

                            _numAsChars = new ();
                            _state = ParserStates.BeforeBetweenAfter;
                        }

                        break;
                    case '-':

                        if ((_state == ParserStates.InsideIntegerOrFirstInRange)   ||   ( _state == ParserStates.AfterInteger ))
                        {
                            _state = ParserStates.InRange;
                        }
                        else 
                        {
                            throw new ParsingExeption (_glyphRepiting);
                        }

                        break;
                    case ',':

                        if ( ( _state == ParserStates.InsideIntegerOrFirstInRange ) || ( _state == ParserStates.AfterInteger ) )
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
                        }
                        else if ( _state == ParserStates.InRangeEnd )
                        {
                            //_numAsChars.Add (glyph);

                            string presentation = "";

                            foreach ( char ch in _numAsChars )
                            {
                                presentation += ch;
                            }

                            int rangeEnd = Int32.Parse (presentation);

                            for ( int index = _rangeStart; index <= rangeEnd; index++ )
                            {
                                _pageNumbers.Add (index);
                            }

                            _numAsChars = new ();
                            _state = ParserStates.BeforeBetweenAfter;
                        }
                        else if ( _state == ParserStates.BeforeBetweenAfter ) 
                        {
                        }
                        else
                        {
                            throw new ParsingExeption (_glyphRepiting);
                        }

                        break;
                }
            }
        }


        public void Cancel ()
        {
            _printingAdjusting.Cancelled = true;
            IsClosed = true;
            _view.Close ();
        }


        public void Prepare ()
        {
            IsClosed = false;

            var printersList = new ObservableCollection <PrinterPresentation> ();

            PrinterSettings settings = new PrinterSettings ();
            string defaultPrinterName = settings.PrinterName;

            var printers = PrinterSettings.InstalledPrinters;

            int counter = 0;

            foreach ( string printer   in   printers )
            {
                printersList.Add (new PrinterPresentation (printer));

                if (defaultPrinterName == printer) 
                {
                    SelectedIndex = counter;
                }

                counter++;
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



    public class ParsingExeption : Exception
    {
        public ParsingExeption ( string message ) : base (message) { }
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