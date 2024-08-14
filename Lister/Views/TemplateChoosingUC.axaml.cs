using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ContentAssembler;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class TemplateChoosingUserControl : ReactiveUserControl <TemplateChoosingViewModel>
    {
        private static readonly string _jsonError = 
        "Невозможно загрузить этот шаблон.Обратитесь к разработчику по телефону 324-708";
        private static TemplateViewModel _chosen;
        private ModernMainView _parent;
        private TemplateChoosingViewModel _vm;
        private string _theme;


        public TemplateChoosingUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<TemplateChoosingViewModel> ();
            _vm = ( TemplateChoosingViewModel ) DataContext;
            Loaded += OnLoaded;
            ActualThemeVariantChanged += ThemeChanged;
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _vm.ChosenTemplate = _chosen;

            if ( _chosen != null ) 
            {
                templateChoosing.PlaceholderText = _vm.ChosenTemplate. Name;
            }

            _vm.PassView (this);
            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme (_theme);
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null ) 
            {
                return;
            }

            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme ( _theme );
        }


        internal void CloseCustomCombobox ( object sender, GotFocusEventArgs args )
        {
            _parent = this.Parent.Parent as ModernMainView;
            PersonChoosingUserControl personChoosing = _parent. personChoosing;
            
            if ( personChoosing != null ) 
            {
                personChoosing.CloseCustomCombobox ();
            }
        }


        internal void HandleClosing ( object sender, EventArgs args )
        {
            TemplateViewModel chosen = templateChoosing.SelectedItem as TemplateViewModel;

            if ( chosen == null )
            {
                return;
            }

            bool templateIsIncorrect = (chosen.Color.Color.A == 100);

            if ( templateIsIncorrect )
            {
                _vm.ChosenTemplate = null;
                _chosen = null;
                var messegeDialog = new MessageDialog ();
                messegeDialog.Message = _jsonError;
                messegeDialog.ShowDialog (MainWindow._mainWindow);
                messegeDialog.Focusable = true;
                messegeDialog.Focus ();
            }
            else 
            {
                _vm.ChosenTemplate = chosen;
                _chosen = chosen;
            }
        }


        internal void SetCursorWait ( )
        {
            Cursor = new Cursor (StandardCursorType.Wait);
        }


        internal void SetCursorArrow ()
        {
            Cursor = new Cursor (StandardCursorType.Arrow);
        }


        //internal void BuildBadges ( object sender, TappedEventArgs args )
        //{
        //    ModernMainView parent = this.Parent.Parent as ModernMainView;

        //    _vm.Build ();
        //    ZoomNavigationUserControl zoomNavigation = parent.zoomNavigation;
        //    zoomNavigation.EnableZoom ();
        //    zoomNavigation.SetEnablePageNavigation ();
        //    clearBadges.IsEnabled = true;
        //    save.IsEnabled = true;
        //    print.IsEnabled = true;
        //}


        //internal void ClearBadges ( object sender, TappedEventArgs args )
        //{
        //    ModernMainView parent = this.Parent.Parent as ModernMainView;
        //    ZoomNavigationUserControl zoomNavigation = parent.zoomNavigation;
        //    SceneUserControl scene = parent.scene;
        //    SceneViewModel sceneVM = scene.DataContext as SceneViewModel;

        //    _vm.ClearAllPages ();
        //    clearBadges.IsEnabled = false;
        //    save.IsEnabled = false;
        //    print.IsEnabled = false;
        //    zoomNavigation.DisableButtons ();
        //    sceneVM.EditionMustEnable = false;
        //}


        //internal void GeneratePdf ( object sender, TappedEventArgs args )
        //{
        //    List<FilePickerFileType> fileExtentions = [];
        //    fileExtentions.Add (FilePickerFileTypes.Pdf);
        //    FilePickerSaveOptions options = new ();
        //    options.Title = "Open Text File";
        //    options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
        //    var window = TopLevel.GetTopLevel (this);
        //    Task<IStorageFile> chosenFile = window.StorageProvider.SaveFilePickerAsync (options);
        //    TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

        //    chosenFile.ContinueWith
        //        (
        //           task =>
        //           {
        //               if ( task.Result != null )
        //               {
        //                   string result = task.Result.Path.ToString ();
        //                   result = result.Substring (8, result.Length - 8);
        //                   Task<bool> pdf = _vm.GeneratePdf (result);
        //                   pdf.ContinueWith
        //                       (
        //                       task =>
        //                       {
        //                           if ( pdf.Result == false )
        //                           {
        //                               string message = "Выбраный файл открыт в другом приложении. Закройте его и повторите.";

        //                               int idOk = Winapi.MessageBox (0, message, "", 0);
        //                               //GeneratePdf (result);
        //                           }
        //                           else
        //                           {
        //                               Process fileExplorer = new Process ();
        //                               fileExplorer.StartInfo.FileName = "explorer.exe";
        //                               result = result.ExtractPathWithoutFileName ();
        //                               result = result.Replace ('/', '\\');
        //                               fileExplorer.StartInfo.Arguments = result;
        //                               fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
        //                               fileExplorer.Start ();
        //                           }
        //                       }
        //                       );
        //               }
        //           }
        //        );
        //}


        //internal void Print ( object sender, TappedEventArgs args )
        //{
        //    _vm.Print ();
        //}

    }
}
