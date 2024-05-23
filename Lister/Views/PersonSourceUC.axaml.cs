using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Lister.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class PersonSourceUserControl : UserControl
    {
        private TemplateChoosingUserControl _templateChoosingUserControl;
        private ZoomNavigationUserControl _zoomNavigationUserControl;
        private SceneUserControl _sceneUserControl;
        private PersonChoosingUserControl _personChoosingUserControl;


        public PersonSourceUserControl ()
        {
            InitializeComponent ();
        }


        internal void ChooseFile ( object sender, TappedEventArgs args )
        {
            ChooseFile ();
        }


        internal void ChooseFile ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();

            if ( key != "Q" )
            {
                return;
            }

            ChooseFile ();
        }


        private void ChooseFile ()
        {
            FilePickerFileType csvFileType = new FilePickerFileType ("Csv")
            {
                Patterns = new [] { "*.csv" },
                AppleUniformTypeIdentifiers = new [] { "public.image" },
                MimeTypes = new [] { "image/*" }
            };

            List<FilePickerFileType> fileExtentions = [];
            fileExtentions.Add (csvFileType);
            FilePickerOpenOptions options = new FilePickerOpenOptions ();
            options.FileTypeFilter = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
            options.Title = "Open Text File";
            options.AllowMultiple = false;
            var window = TopLevel.GetTopLevel (this);
            Task<IReadOnlyList<IStorageFile>> chosenFile = null;
            chosenFile = window.StorageProvider.OpenFilePickerAsync (options);
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            chosenFile.ContinueWith
                (
                   task =>
                   {
                       if ( task.Result.Count > 0 )
                       {
                           string result = task.Result [0].Path.ToString ();
                           //MainViewModel vm = viewModel;
                           MainViewModel vm = null;
                           result = result.Substring (8, result.Length - 8);
                           vm.SourceFilePath = result;

                           if ( vm.SourceFilePath != string.Empty )
                           {
                               editSourceFile.IsEnabled = true;
                               //setEntirePersonList.IsEnabled = true;   ///////////////////////////////////////////
                           }
                       }
                   }
                   , uiScheduler
                );
        }


        internal void OpenEditor ( object sender, TappedEventArgs args )
        {
            string filePath = personsSourceFile.Text;

            if ( string.IsNullOrWhiteSpace (filePath) )
            {
                return;
            }

            ProcessStartInfo procInfo = new ProcessStartInfo ()
            {
                FileName = filePath,
                UseShellExecute = true
            };
            try
            {
                Process.Start (procInfo);
            }
            catch ( System.ComponentModel.Win32Exception ex )
            {
            }
        }
    }
}
