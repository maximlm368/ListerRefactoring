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
        private static string _warnImageName = "Icons/warning-alert.ico";

        private DialogWindow _view;

        private Bitmap _warnImage;
        internal Bitmap WarnImage
        {
            get { return _warnImage; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _warnImage, value, nameof (WarnImage));
            }
        }


        public DialogViewModel ( DialogWindow view )
        {
            _view = view;

            string correctnessIcon = App.ResourceDirectoryUri + _warnImageName;
            Uri correctUri = new Uri (correctnessIcon);
            WarnImage = ImageHelper.LoadFromResource (correctUri);
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
