using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Lister.Desktop.Views.MainWindow.SharedComponents.ButtonsBlock.ViewModel
{
    public partial class ButtonsBlockViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _narrowIsVisible = true;

        [ObservableProperty]
        private bool _largeIsVisible = false;

        private bool _pdfIsWanted;
        public bool PdfIsWanted
        {
            get { return _pdfIsWanted; }
            private set
            {
                _pdfIsWanted = value;
                OnPropertyChanged ();
            }
        }

        private bool _printingIsWanted;
        public bool PrintingIsWanted
        {
            get { return _printingIsWanted; }
            private set
            {
                _printingIsWanted = value;
                OnPropertyChanged ();
            }
        }

        internal static event Action? ToEditionRequired;
        internal event Action? PrintRequired;
        internal event Action? GeneratePdfRequired;
        internal event Action? ClearTapped;

        [RelayCommand]
        internal void Extend ()
        {
            NarrowIsVisible = false;
            LargeIsVisible = true;
        }

        [RelayCommand]
        internal void Shrink ()
        {
            NarrowIsVisible = true;
            LargeIsVisible = false;
        }

        [RelayCommand]
        internal void Edit ()
        {
            ToEditionRequired?.Invoke ();
        }

        [RelayCommand]
        internal void ClearPages ()
        {
            ClearTapped?.Invoke ();
        }

        [RelayCommand]
        internal void GeneratePdf ()
        {
            GeneratePdfRequired?.Invoke ();
        }

        [RelayCommand]
        public void Print ()
        {
            PrintRequired?.Invoke ();
        }
    }
}
