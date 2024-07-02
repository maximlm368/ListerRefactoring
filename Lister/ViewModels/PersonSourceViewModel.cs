using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ContentAssembler;
using Lister.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuestPDF.Helpers.Colors;

namespace Lister.ViewModels
{
    public class PersonSourceViewModel : ViewModelBase
    {
        private IUniformDocumentAssembler _uniformAssembler;
        private PersonChoosingViewModel _personChoosingVM;
        private PersonSourceUserControl _view;

        private string sFP;
        internal string SourceFilePath
        {
            get { return sFP; }
            set
            {
                string path = SetPersonsFilePath ( value );
                this.RaiseAndSetIfChanged ( ref sFP , path , nameof ( SourceFilePath ) );
            }
        }

        private bool eE;
        internal bool EditorIsEnable
        {
            get { return eE; }
            set
            {
                this.RaiseAndSetIfChanged (ref eE, value, nameof (EditorIsEnable));
            }
        }

        internal bool peopleSettingOccured;


        public PersonSourceViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler
                                                                    , PersonChoosingViewModel personChoosing ) 
        {
            _uniformAssembler = singleTypeDocumentAssembler;
            _personChoosingVM = personChoosing;
            EditorIsEnable = false;
            peopleSettingOccured = false;
        }


        internal void PassView ( PersonSourceUserControl view )
        {
            _view = view;
        }


        internal void ChooseFile ( )
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
            Task <IReadOnlyList <IStorageFile>> chosenFile = MainWindow.CommonStorageProvider.OpenFilePickerAsync (options);
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            chosenFile.ContinueWith
                (
                   task =>
                   {
                       if ( task.Result.Count > 0 )
                       {
                           string result = task.Result [0].Path.ToString ();
                           result = result.Substring (8, result.Length - 8);
                           SourceFilePath = result;

                           if ( SourceFilePath != string.Empty )
                           {
                               EditorIsEnable = true;
                               _personChoosingVM.TextboxIsReadOnly = false;
                           }
                       }
                   }
                   , uiScheduler
                );
        }


        internal void OpenEditor ( )
        {
            string filePath = SourceFilePath;

            if ( string.IsNullOrWhiteSpace (SourceFilePath) )
            {
                return;
            }

            ProcessStartInfo procInfo = new ProcessStartInfo ()
            {
                FileName = SourceFilePath,
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


        internal string SetPersonsFilePath ( string value )
        {
            bool valueIsSuitable = ( value != null )   &&   ( value != string.Empty );

            if ( valueIsSuitable )
            {
                try
                {
                    List <Person> persons = _uniformAssembler.GetPersons (value);
                    ObservableCollection <VisiblePerson> visible = new ();
                    List <Person> people = new ();

                    foreach ( var person   in   persons )
                    {
                        VisiblePerson visiblePerson = new VisiblePerson (person);
                        visible.Add (visiblePerson);
                        people.Add (person);
                    }

                    peopleSettingOccured = true;
                    _personChoosingVM.People = people;
                    _personChoosingVM.VisiblePeople = visible;
                    return value;
                }
                catch ( IOException ex )
                {
                    int idOk = Winapi.MessageBox ( 0 , "Выбраный файл открыт в другом приложении. Закройте его." , "" , 0 );
                    return string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
