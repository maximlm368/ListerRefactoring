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

namespace Lister.ViewModels
{
    public class DialogViewModel : ViewModelBase
    {
        private static string _warnImageName = "warning-alert.ico";

        private DialogWindow _view;

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


        public DialogViewModel ( DialogWindow view )
        {
            _view = view;

            string workDirectory = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            string directoryPath = containingDirectory.FullName;

            string correctnessIcon = App.ResourceUriType + directoryPath + App.ResourceUriFolderName + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);

            CanvasBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 240, 240, 240));
            LineBackground = new SolidColorBrush (new Avalonia.Media.Color (255, 220, 220, 220));
        }


        internal void ChooseYes ()
        {
            _view.ChooseYes ();
        }


        internal void ChooseNo ()
        {
            _view.ChooseNo ();
        }
    }
}
