using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContentAssembler;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using Avalonia.Media;
using System.Globalization;
using System.Reflection.Metadata;
using ExtentionsAndAuxiliary;
using Microsoft.VisualBasic;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using System.Linq.Expressions;
using Lister.Views;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;

namespace Lister.ViewModels
{
    public class LargeMessageViewModel : ViewModelBase
    {
        private static string _warnImageName = "Icons/warning-alert.ico";

        private readonly int _lineHeight = 16;
        private int _topMargin = 54;
        private int _topMarginForLarge = 0;
        private LargeMessageDialog _view;

        private readonly int _minHeight = 70;
        private readonly int _maxHeight = 270;

        private SolidColorBrush _lineBackground;
        internal SolidColorBrush LineBackground
        {
            get { return _lineBackground; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _lineBackground, value, nameof (LineBackground));
            }
        }

        private SolidColorBrush _canvasBackground;
        internal SolidColorBrush CanvasBackground
        {
            get { return _canvasBackground; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _canvasBackground, value, nameof (CanvasBackground));
            }
        }

        private Bitmap _warnImage;
        internal Bitmap WarnImage
        {
            get { return _warnImage; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _warnImage, value, nameof (WarnImage));
            }
        }

        private List<string> _messageLines;
        internal List<string> MessageLines
        {
            get { return _messageLines; }
            set
            {
                this.RaiseAndSetIfChanged (ref _messageLines, value, nameof (MessageLines));

                MessageMargin = new Thickness (10, 0, 0, 0);
            }
        }

        private string _errorsSource;
        internal string ErrorsSource
        {
            get { return _errorsSource; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _errorsSource, value, nameof (ErrorsSource));
            }
        }

        private Thickness _messageMargin;
        internal Thickness MessageMargin
        {
            get { return _messageMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _messageMargin, value, nameof (MessageMargin));
            }
        }

        private double _windowHeight;
        internal double WindowHeight
        {
            get { return _windowHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _windowHeight, value, nameof (WindowHeight));
            }
        }

        private double _windowWidth;
        internal double WindowWidth
        {
            get { return _windowWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _windowWidth, value, nameof (WindowWidth));
            }
        }

        private double _lineWidth;
        internal double LineWidth
        {
            get { return _lineWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _lineWidth, value, nameof (LineWidth));
            }
        }

        private double _buttonCanvasLeft;
        internal double ButtonCanvasLeft
        {
            get { return _buttonCanvasLeft; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _buttonCanvasLeft, value, nameof (ButtonCanvasLeft));
            }
        }

        private double _messageWidth;
        internal double MessageWidth
        {
            get { return _messageWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _messageWidth, value, nameof (MessageWidth));
            }
        }

        private double _messageContainerHeight;
        internal double MessageContainerHeight
        {
            get { return _messageContainerHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _messageContainerHeight, value, nameof (MessageContainerHeight));
            }
        }

        private double _errorsContainerHeight;
        internal double ErrorsContainerHeight
        {
            get { return _errorsContainerHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _errorsContainerHeight, value, nameof (ErrorsContainerHeight));
            }
        }

        private double _messageContainerWidth;
        internal double MessageContainerWidth
        {
            get { return _messageContainerWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _messageContainerWidth, value, nameof (MessageContainerWidth));
            }
        }

        private double _messageHeight;
        internal double MessageHeight
        {
            get { return _messageHeight; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _messageHeight, value, nameof (MessageHeight));
            }
        }


        public LargeMessageViewModel () 
        {
            string correctnessIcon = App.ResourceDirectoryUri + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);
            CanvasBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 240, 240, 240));
            LineBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 220, 220, 220));
        }


        internal void PassView ( LargeMessageDialog view )
        {
            _view = view;
        }


        internal void Close (  )
        {
            _view.Shut ();
        }


        internal void Set ( List<string> message, string errorsSource )
        {
            ErrorsSource = errorsSource;
            MessageLines = message;
            MessageHeight = message.Count * _lineHeight + 20;

            double errorsContainerHeight = message.Count * _lineHeight;

            if ( errorsContainerHeight > _maxHeight )
            {
                errorsContainerHeight = _maxHeight;
            }
            else if ( errorsContainerHeight < _minHeight ) 
            {
                errorsContainerHeight = _minHeight;
            }

            ErrorsContainerHeight = errorsContainerHeight;
            MessageContainerHeight = ErrorsContainerHeight + 35 + 35;
            WindowHeight = 20 + MessageContainerHeight + 39;

            LineWidth = 658;
            WindowWidth = 660;
            ButtonCanvasLeft = 570;
            MessageWidth = GetLongest (message) + 20;
            MessageContainerWidth = 600;
        }


        private double GetLongest ( List<string> message )
        {
            double length = 0;

            foreach ( string line   in   message ) 
            {
                FormattedText formatted = new FormattedText (line
                                           , System.Globalization.CultureInfo.CurrentCulture
                                           , FlowDirection.LeftToRight, Typeface.Default
                                           , 12, null);

                formatted.SetFontWeight (Avalonia.Media.FontWeight.Bold);
                formatted.SetFontSize (12);
                formatted.SetFontFamily (new FontFamily ("Arial"));

                double currentLength = formatted.Width;

                if ( currentLength > length ) 
                {
                    length = currentLength;
                }
            }

            return length;
        }


        //internal void SetStandard ( List<string> message )
        //{
        //    _isLarge = false;
        //    message.Add ("");
        //    MessageLines = message;
        //    SetStandard ();

        //    MessageHeight = message.Count * _lineHeight;
        //}


        //private void SetStandard ( )
        //{
        //    WindowHeight = 130;
        //    WindowWidth = 360;
        //    LineWidth = 358;
        //    MessageContainerHeight = 70;
        //    ButtonCanvasLeft = 270;
        //    MessageWidth = 323;
        //}
    }
}


//"required":["Width", "Height", "TopOffset", "LeftOffset", "Alignment", "FontSize", "FontFile"
//, "FontName", "Foreground", "FontWeight", "IsSplitable"],