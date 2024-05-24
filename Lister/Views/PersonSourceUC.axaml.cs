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
        private TemplateChoosingUserControl _templateChoosing;
        private ZoomNavigationUserControl _zoomNavigation;
        private SceneUserControl _scene;
        private PersonChoosingUserControl _personChoosing;
        private PersonSourceViewModel _vm;


        public PersonSourceUserControl ()
        {
            InitializeComponent ();
            _vm = (PersonSourceViewModel) DataContext;
        }


        internal void PassNeighbours ( SceneUserControl scene, PersonChoosingUserControl personChoosing
                                     , ZoomNavigationUserControl zoomNavigation, TemplateChoosingUserControl templateChoosing )
        {
            _personChoosing = personChoosing;
            _scene = scene;
            _zoomNavigation = zoomNavigation;
            _templateChoosing = templateChoosing;
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
                           result = result.Substring (8, result.Length - 8);
                           _vm.SourceFilePath = result;

                           if ( _vm.SourceFilePath != string.Empty )
                           {
                               editSourceFile.IsEnabled = true;
                               _personChoosing. setEntirePersonList.IsEnabled = true;
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
