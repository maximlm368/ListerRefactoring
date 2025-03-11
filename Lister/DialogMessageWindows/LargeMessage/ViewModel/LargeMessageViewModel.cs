using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using View.Extentions;
using ReactiveUI;
using View.App;

namespace View.DialogMessageWindows.LargeMessage.ViewModel
{
    public class LargeMessageViewModel : ReactiveObject
    {
        private static string _warnImageName = "Icons/warning-alert.ico";

        private readonly int _maxHeight = 200;
        private readonly int _lineHeight = 16;
        private LargeMessageDialog _view;

        private Bitmap _warnImage;
        internal Bitmap WarnImage
        {
            get { return _warnImage; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _warnImage, value, nameof( WarnImage ) );
            }
        }

        private List<string> _messageLines;
        internal List<string> MessageLines
        {
            get { return _messageLines; }
            set
            {
                this.RaiseAndSetIfChanged( ref _messageLines, value, nameof( MessageLines ) );

                MessageMargin = new Thickness( 10, 0, 0, 0 );
            }
        }

        private string _message;
        internal string Message
        {
            get { return _message; }
            set
            {
                this.RaiseAndSetIfChanged( ref _message, value, nameof( Message ) );
            }
        }

        private string _errorsSource;
        internal string ErrorsSource
        {
            get { return _errorsSource; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _errorsSource, value, nameof( ErrorsSource ) );
            }
        }

        private Thickness _messageMargin;
        internal Thickness MessageMargin
        {
            get { return _messageMargin; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _messageMargin, value, nameof( MessageMargin ) );
            }
        }

        private double _windowHeight;
        internal double WindowHeight
        {
            get { return _windowHeight; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _windowHeight, value, nameof( WindowHeight ) );
            }
        }

        private double _windowWidth;
        internal double WindowWidth
        {
            get { return _windowWidth; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _windowWidth, value, nameof( WindowWidth ) );
            }
        }

        private double _lineWidth;
        internal double LineWidth
        {
            get { return _lineWidth; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _lineWidth, value, nameof( LineWidth ) );
            }
        }

        private double _messageWidth;
        internal double MessageWidth
        {
            get { return _messageWidth; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _messageWidth, value, nameof( MessageWidth ) );
            }
        }

        private double _messageContainerHeight;
        internal double MessageContainerHeight
        {
            get { return _messageContainerHeight; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _messageContainerHeight, value, nameof( MessageContainerHeight ) );
            }
        }

        private double _errorsContainerHeight;
        internal double ErrorsContainerHeight
        {
            get { return _errorsContainerHeight; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _errorsContainerHeight, value, nameof( ErrorsContainerHeight ) );
            }
        }

        private double _messageContainerWidth;
        internal double MessageContainerWidth
        {
            get { return _messageContainerWidth; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _messageContainerWidth, value, nameof( MessageContainerWidth ) );
            }
        }

        private double _messageHeight;
        internal double MessageHeight
        {
            get { return _messageHeight; }
            private set
            {
                this.RaiseAndSetIfChanged( ref _messageHeight, value, nameof( MessageHeight ) );
            }
        }


        public LargeMessageViewModel()
        {
            string correctnessIcon = ListerApp.ResourceFolderName + _warnImageName;
            WarnImage = ImageHelper.LoadFromResource( correctnessIcon );
        }


        internal void PassView(LargeMessageDialog view)
        {
            _view = view;
        }


        internal void Close()
        {
            _view.Shut();
        }


        internal void CopyCommand()
        {
            int fdf = 0;
        }


        internal void Set(List<string> message, string errorsSource)
        {
            ErrorsSource = errorsSource;
            MessageLines = message;

            Message = string.Empty;

            foreach (string line in MessageLines)
            {
                Message += line + "\n";
            }


            MessageHeight = message.Count * _lineHeight;

            double errorsContainerHeight = message.Count * _lineHeight;

            if (errorsContainerHeight > _maxHeight)
            {
                errorsContainerHeight = _maxHeight;
            }
            else if (errorsContainerHeight < _lineHeight)
            {
                errorsContainerHeight = _lineHeight;
            }

            ErrorsContainerHeight = errorsContainerHeight + 20;
            MessageContainerHeight = ErrorsContainerHeight + 49 + 35;
            WindowHeight = 20 + MessageContainerHeight + 51;

            LineWidth = 658;
            WindowWidth = 660;
            MessageWidth = GetLongest( message ) + 20;
            MessageContainerWidth = 592;
        }


        private double GetLongest(List<string> message)
        {
            double length = 0;

            foreach (string line in message)
            {
                FormattedText formatted = new FormattedText( line
                                           , System.Globalization.CultureInfo.CurrentCulture
                                           , FlowDirection.LeftToRight, Typeface.Default
                                           , 12, null );

                formatted.SetFontWeight( FontWeight.Bold );
                formatted.SetFontSize( 12 );
                formatted.SetFontFamily( new FontFamily( "Arial" ) );

                double currentLength = formatted.Width;

                if (currentLength > length)
                {
                    length = currentLength;
                }
            }

            return length;
        }

    }
}


//"required":["Width", "Height", "TopOffset", "LeftOffset", "Alignment", "FontSize", "FontFile"
//, "FontName", "Foreground", "FontWeight", "IsSplitable"],