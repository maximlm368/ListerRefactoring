using Avalonia;
using ReactiveUI;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel
{
    public partial class SceneViewModel : ReactiveObject
    {
        private readonly double _buttonWidth = 32;
        private readonly double _extention = 170;
        private readonly double _buttonBlockWidth = 64;
        private readonly double _workAreaLeftMargin = -74;
        private readonly string _extentionToolTip;
        private readonly string _shrinkingToolTip;
        private readonly string _fileIsOpenMessage;

        private bool _blockIsExtended = false;

        private double _hintWidth;
        internal double HintWidth
        {
            get { return _hintWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _hintWidth, value, nameof (HintWidth));
            }
        }

        private double _butonWidth;
        internal double ButtonWidth
        {
            get { return _butonWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _butonWidth, value, nameof (ButtonWidth));
            }
        }

        private double _buttonBlockWidht;
        internal double ButtonBlockWidth
        {
            get { return _buttonBlockWidht; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _buttonBlockWidht, value, nameof (ButtonBlockWidth));
            }
        }

        private Thickness _workAreaMargin;
        internal Thickness WorkAreaMargin
        {
            get { return _workAreaMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _workAreaMargin, value, nameof (WorkAreaMargin));
            }
        }

        private Thickness _extenderMargin;
        internal Thickness ExtenderMargin
        {
            get { return _extenderMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _extenderMargin, value, nameof (ExtenderMargin));
            }
        }

        private string _extentionTip;
        internal string ExtentionTip
        {
            get { return _extentionTip; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _extentionTip, value, nameof (ExtentionTip));
            }
        }

        private string _extenderContent;
        internal string ExtenderContent
        {
            get { return _extenderContent; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _extenderContent, value, nameof (ExtenderContent));
            }
        }

        private bool _pdfIsWanted;
        public bool PdfIsWanted
        {
            get { return _pdfIsWanted; }
            private set
            {
                if ( _pdfIsWanted == value )
                {
                    _pdfIsWanted = !_pdfIsWanted;
                }

                this.RaiseAndSetIfChanged (ref _pdfIsWanted, value, nameof (PdfIsWanted));
            }
        }

        private bool _printingIsWanted;
        public bool PrintingIsWanted
        {
            get { return _printingIsWanted; }
            private set
            {
                if ( _printingIsWanted == value )
                {
                    _printingIsWanted = !_printingIsWanted;
                }

                this.RaiseAndSetIfChanged (ref _printingIsWanted, value, nameof (PrintingIsWanted));
            }
        }


        private void SetButtonBlock ( )
        {
            ButtonWidth = _buttonWidth;
            HintWidth = 0;
            ButtonBlockWidth = _buttonBlockWidth;
            WorkAreaMargin = new Thickness (_workAreaLeftMargin, 0);
            ExtenderMargin = new Thickness (30, 8);
            ExtentionTip = _extentionToolTip;
            ExtenderContent = "\uF061";
        }


        internal void ExtendButtons ()
        {
            if ( _blockIsExtended )
            {
                HintWidth = 0;
                ButtonWidth -= _extention;
                ButtonBlockWidth -= _extention;
                WorkAreaMargin = new Thickness (( WorkAreaMargin. Left + _extention ), 0);
                ExtenderMargin = new Thickness (( ExtenderMargin.Left - _extention ), 8);
                ExtentionTip = _extentionToolTip;
                ExtenderContent = "\uF061";
                _blockIsExtended = false;
            }
            else 
            {
                HintWidth = _extention;
                ButtonWidth += _extention;
                ButtonBlockWidth += _extention;
                WorkAreaMargin = new Thickness (( WorkAreaMargin. Left - _extention ), 0);
                ExtenderMargin = new Thickness (( ExtenderMargin.Left + _extention ), 8);
                ExtentionTip = _shrinkingToolTip;
                ExtenderContent = "\uF060";
                _blockIsExtended = true;
            }
        }


        internal void EditIncorrectBadges ()
        {
            EditIncorrectsIsSelected = true;
        }


        internal void ClearBadges ()
        {
            ClearAllPages ();
            DisableButtons ();
            BadgesAreCleared = true;
            BadgesAreCleared = false;
        }


        internal void GeneratePdf ()
        {
            PdfIsWanted = true;
        }


        public void Print ()
        {
            PrintingIsWanted = true;
        }
    }
}




