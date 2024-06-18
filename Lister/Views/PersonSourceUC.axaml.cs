using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class PersonSourceUserControl : UserControl
    {
        //private TemplateChoosingUserControl _templateChoosing;
        //private ZoomNavigationUserControl _zoomNavigation;
        //private SceneUserControl _scene;
        private PersonSourceViewModel _vm;
        private PersonChoosingUserControl _personChoosing;
        private TemplateChoosingUserControl _templateChoosing;
        private ZoomNavigationUserControl _zoomNavigation;
        private SceneUserControl _scene;


        public PersonSourceUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<PersonSourceViewModel> ();
            _vm = (PersonSourceViewModel) DataContext;
        }


        internal void ChooseFile ( object sender, TappedEventArgs args )
        {
            ChooseFile ();
        }


        internal void ChooseFile ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();

            //if ( key != "Space" )
            //{
            //    return;
            //}

            //ChooseFile ();

            if ( key == "Space" )
            {
                ChooseFile ( );
            }

        }


        private void ChooseFile ()
        {
            FilePickerFileType csvFileType = new FilePickerFileType ("Csv")
            {
                Patterns = new [] { "*.csv" },
                AppleUniformTypeIdentifiers = new [] { "public.image" },
                MimeTypes = new [] { "image/*" }
            };

            List <FilePickerFileType> fileExtentions = [];
            fileExtentions.Add (csvFileType);
            FilePickerOpenOptions options = new FilePickerOpenOptions ();
            options.FileTypeFilter = new ReadOnlyCollection <FilePickerFileType> (fileExtentions);
            options.Title = "Open Text File";
            options.AllowMultiple = false;
            var window = TopLevel.GetTopLevel (this);
            Task <IReadOnlyList<IStorageFile>> chosenFile = window.StorageProvider.OpenFilePickerAsync (options);
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

                               ModernMainView parent = (ModernMainView) this.Parent.Parent;
                               //parent.personChoosing. entirePersonListButton.IsEnabled = true;
                               parent.personChoosing. personTextBox.IsReadOnly = false;
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
