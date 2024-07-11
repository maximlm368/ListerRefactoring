using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Lister.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class ModernMainView : UserControl
    {
        private static double _widthDelta;
        private static double _heightDelta;
        internal static readonly string _sourcePathKeeper = "keeper.txt";
        internal static bool pathIsSet;
        internal static ModernMainView Instance { get; private set; }

        internal static double ProperWidth { get; private set; }
        internal static double ProperHeight { get; private set; }
        

        public ModernMainView ()
        {
            InitializeComponent ();
            Instance = this;

            ProperWidth = Width;
            ProperHeight = Height;

            Loaded += OnLoaded;
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            string workDirectory = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            string directoryPath = containingDirectory.FullName;
            string keeperPath = directoryPath + _sourcePathKeeper;
            FileInfo fileInf = new FileInfo (keeperPath);

            if ( fileInf.Exists )
            {
                string [] lines = File.ReadAllLines (keeperPath);
                
                try
                {
                    personSource.SetPath (lines [0]);
                }
                catch ( IndexOutOfRangeException ex ) { }
            }
            else 
            {
                File.Create(keeperPath).Close();
            }
        }


        internal void ReleaseRunner () 
        {
            personChoosing.ReleaseScrollingLeverage ();
        }


        internal void CloseCustomCombobox ()
        {
            personChoosing.CloseCustomCombobox ();
        }


        internal void ChangeSize ( double widthDifference, double heightDifference )
        {
            Width -= widthDifference;
            Height -= heightDifference;
            ProperWidth = Width;
            ProperHeight = Height;
            scene.Width -= widthDifference;
            scene.Height -= heightDifference;
            scene. workArea.Width -= widthDifference;
            _widthDelta -= widthDifference;
            scene.workArea.Height -= heightDifference;
            _heightDelta -= heightDifference;
            personChoosing.AdjustComboboxWidth (widthDifference);
        }


        internal void SetProperSize ( double properWidth, double properHeight )
        {
            double widthDifference = Width - properWidth;
            double heightDifference = Height - properHeight;

            Width = properWidth;
            Height = properHeight;
            ProperWidth = Width;
            ProperHeight = Height;

            scene.workArea.Width -= widthDifference;
            scene.workArea.Height -= heightDifference;
            personChoosing.AdjustComboboxWidth (widthDifference);
        }


        internal void ResetIncorrects ( )
        {
            ModernMainViewModel vm = DataContext as ModernMainViewModel;
            
            if ( vm != null ) 
            {
                vm.ResetIncorrects ( );
            }
        }


        internal void EditIncorrectBadges ( List<BadgeViewModel> incorrectBadges )
        {
            ModernMainView mainView = this;
            MainWindow window = MainWindow.GetMainWindow ();

            if ( ( window != null )   &&   ( incorrectBadges.Count > 0 ) )
            {
                BadgeEditorView editorView = new BadgeEditorView ();

                editorView.SetProperSize (ModernMainView.ProperWidth, ModernMainView.ProperHeight);
                window.CancelSizeDifference ();

                editorView.PassIncorrectBadges (incorrectBadges);
                editorView.PassBackPoint (mainView);
                window.Content = editorView;
            }
        }
    }
}




