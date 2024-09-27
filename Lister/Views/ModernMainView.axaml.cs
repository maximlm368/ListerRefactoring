using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class ModernMainView : UserControl
        //ReactiveUserControl <ModernMainViewModel>
    {
        private static double _widthDelta;
        private static double _heightDelta;
        private ModernMainViewModel _vm;
        private bool _isFirstTimeLoading = true;
        internal static readonly string _sourcePathKeeper = "keeper.txt";
        internal static bool pathIsSet;
        internal static ModernMainView Instance { get; private set; }
        internal static int TappedEditorBuildingButton { get; private set; }

        internal static double ProperWidth { get; private set; }
        internal static double ProperHeight { get; private set; }

        internal BadgeEditorView EditorView { get; private set; }
        private List<BadgeViewModel> _incorrectBadges;
        private PageViewModel _firstPage;


        public ModernMainView ()
        {
            InitializeComponent ();
            Instance = this;

            ProperWidth = Width;
            ProperHeight = Height;

            Loaded += OnLoaded;
            //LayoutUpdated += AfterWaiting;

            DataContext = App.services.GetRequiredService<ModernMainViewModel> ();
            _vm = ( ModernMainViewModel ) DataContext;
            _vm.PassView (this);

            waiting.Margin = new Avalonia.Thickness ( 0, -460 );
            waiting.IsVisible = false;

            //buttonPanel.Margin = new Avalonia.Thickness (0, -260);

            LayoutUpdated += LayoutUpdatedHandler;


            this.AddHandler (UserControl.TappedEvent, PreventPasting, RoutingStrategies.Tunnel);
        }


        internal void LayoutUpdatedHandler ( object sender, EventArgs args )
        {
            _vm.LayoutUpdated ();
        }


        private void PreventPasting ( object sender, TappedEventArgs args )
        {
            args.Handled = true;
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            if ( !_isFirstTimeLoading )
            {
                return;
            }

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
                catch ( IndexOutOfRangeException ex ) 
                {
                    personSource.SetPath (null);
                }
            }
            else 
            {
                File.Create(keeperPath).Close();
                personSource.SetPath (null);
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

            waiting.ChangeSize ( heightDifference, widthDifference );

            //waiting.Padding = new Avalonia.Thickness (0, heightDifference);

            personChoosing.AdjustComboboxWidth (widthDifference);
            personChoosing.CloseCustomCombobox ();
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


        //internal void EditIncorrectBadgess ( List<BadgeViewModel> incorrectBadges )
        //{
        //    ModernMainView mainView = this;
        //    MainWindow window = MainWindow.GetMainWindow ();

        //    if ( ( window != null )   &&   ( incorrectBadges.Count > 0 ) )
        //    {
        //        BadgeEditorView editorView = new BadgeEditorView ();

        //        editorView.SetProperSize (ModernMainView.ProperWidth, ModernMainView.ProperHeight);
        //        window.CancelSizeDifference ();

        //        editorView.PassIncorrectBadges (incorrectBadges);
        //        editorView.PassBackPoint (mainView);
        //        _isFirstTimeLoading = false;
        //        window.Content = editorView;
        //    }
        //}


        internal void EditIncorrectBadges ( List <BadgeViewModel> incorrectBadges, PageViewModel firstPage )
        {
            _incorrectBadges = incorrectBadges;
            _firstPage = firstPage;
            ModernMainView mainView = this;
            MainWindow window = MainWindow.GetMainWindow ();

            if ( ( window != null )   &&   ( incorrectBadges.Count > 0 ) )
            {
                EditorView = new BadgeEditorView ( TemplateChoosingViewModel.BuildingOccured, incorrectBadges.Count );
                EditorView.SetProperSize (ModernMainView.ProperWidth, ModernMainView.ProperHeight);
                window.CancelSizeDifference ();
                TappedEditorBuildingButton = 1;

                _vm.SetWaiting ();
            }
        }


        internal void BuildEditor ( )
        {
            ModernMainView mainView = this;
            MainWindow window = MainWindow.GetMainWindow ();

            Task task = new Task
            (
                () =>
                {
                    EditorView.PassIncorrectBadges (_incorrectBadges, _firstPage);
                    EditorView.PassBackPoint (mainView);
                    _isFirstTimeLoading = false;

                    Dispatcher.UIThread.InvokeAsync 
                    (
                        () =>
                        {
                            ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                            modernMV.EndWaiting ();
                            window.Content = EditorView; 
                        }
                    );

                    ModernMainViewModel.MainViewIsWaiting = false;
                    TappedEditorBuildingButton = 0;
                }
            );

            task.Start ();


            //Thread thread = new Thread 
            //    (
            //        () => 
            //        {
            //            EditorView.PassIncorrectBadges (_incorrectBadges);
            //            //Dispatcher.UIThread.InvokeAsync (() => { EditorView.PassIncorrectBadges (_incorrectBadges); });

            //            EditorView.PassBackPoint (mainView);
            //            _isFirstTimeLoading = false;

            //            ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
            //            modernMV.EndWaiting ();

            //            Dispatcher.UIThread.InvokeAsync (() => { window.Content = EditorView; });

            //            TappedButton = 0;
            //        }
            //    );

            //thread.Start ();
        }


        //internal void Hide ()
        //{
        //    waiting.IsVisible = false;
        //}
    }
}




