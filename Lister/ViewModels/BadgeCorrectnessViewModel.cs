using Avalonia.Media;
using Avalonia.Media.Imaging;
using Lister.Extentions;
using Lister.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class BadgeCorrectnessViewModel : ViewModelBase
    {
        private static string _correctnessIcon = "GreenCheckMarker.jpg";
        private static string _incorrectnessIcon = "RedCross.png";

        private bool cr;
        internal bool Correctness
        {
            get { return cr; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cr, value, nameof (Correctness));
            }
        }

        private Bitmap crI;
        internal Bitmap CorrectnessIcon
        {
            get { return crI; }
            private set
            {
                this.RaiseAndSetIfChanged (ref crI, value, nameof (CorrectnessIcon));
            }
        }

        private IBrush clr;
        internal IBrush BorderColor
        {
            get { return clr; }
            set
            {
                this.RaiseAndSetIfChanged (ref clr, value, nameof (BorderColor));
            }
        }

        internal BadgeViewModel BoundBadge { get; private set; }


        internal BadgeCorrectnessViewModel ( bool isCorrect, BadgeViewModel boundBadge ) 
        {
            BoundBadge = boundBadge;

            if ( isCorrect )
            {
                Correctness = true;
                string correctnessIcon = App.ResourceDirectoryUri + _correctnessIcon;
                Uri uri = new Uri (correctnessIcon);
                CorrectnessIcon = ImageHelper.LoadFromResource (uri);
            }
            else
            {
                Correctness = false;
                string incorrectnessIcon = App.ResourceDirectoryUri + _incorrectnessIcon;
                Uri uri = new Uri (incorrectnessIcon);
                CorrectnessIcon = ImageHelper.LoadFromResource (uri);
            }

            BorderColor = new SolidColorBrush (new Color (255, 255, 255, 255));
        }
    }
}
