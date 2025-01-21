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
    public class MessageViewModel : ViewModelBase
    {
        private static string _warnImageName = "Icons/warning-alert.ico";

        private readonly int _lineHeight = 16;
        private int _topMargin = 54;
        private MessageDialog _view;

        private Bitmap wI;
        internal Bitmap WarnImage
        {
            get { return wI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wI, value, nameof (WarnImage));
            }
        }

        private List<string> _messageLines;
        internal List<string> MessageLines
        {
            get { return _messageLines; }
            set
            {
                this.RaiseAndSetIfChanged (ref _messageLines, value, nameof (MessageLines));

                foreach ( string line in MessageLines )
                {
                    _topMargin -= _lineHeight;
                }

                MessageMargin = new Thickness (18, _topMargin, 0, 0);
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


        public MessageViewModel ()
        {
            string correctnessIcon = App.ResourceDirectoryUri + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);
        }


        internal void PassView ( MessageDialog view )
        {
            _view = view;
        }


        internal void Close ()
        {
            _view.Shut ();
        }
    }
}