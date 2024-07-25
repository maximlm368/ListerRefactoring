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
        public static string _resourceUriFolderName = "//Resources//";
        private static string _warnImageName = "warning-alert.ico";

        private readonly int _lineHeight = 16;
        private int _topMargin = 54;

        private SolidColorBrush lB;
        internal SolidColorBrush LineBackground
        {
            get { return lB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref lB, value, nameof (LineBackground));
            }
        }

        private SolidColorBrush cB;
        internal SolidColorBrush CanvasBackground
        {
            get { return cB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cB, value, nameof (CanvasBackground));
            }
        }

        private Bitmap wI;
        internal Bitmap WarnImage
        {
            get { return wI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wI, value, nameof (WarnImage));
            }
        }

        private List<string> mL;
        internal List<string> MessageLines
        {
            get { return mL; }
            set
            {
                this.RaiseAndSetIfChanged (ref mL, value, nameof (MessageLines));

                foreach ( string line in MessageLines )
                {
                    _topMargin -= _lineHeight;
                }

                messageMargin = new Thickness (10, _topMargin, 0, 0);
            }
        }

        private Thickness mM;
        internal Thickness messageMargin
        {
            get { return mM; }
            private set
            {
                this.RaiseAndSetIfChanged (ref mM, value, nameof (messageMargin));
            }
        }


        public MessageViewModel () 
        {
            string directoryPath = System.IO.Directory.GetCurrentDirectory ();
            string correctnessIcon = "file:///" + directoryPath + _resourceUriFolderName + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);

            CanvasBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 240, 240, 240));
            LineBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 220, 220, 220));
        }
    }
}