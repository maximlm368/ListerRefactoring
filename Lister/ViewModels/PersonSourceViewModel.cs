using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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
        private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении, закройте его.";
        private static readonly string _fileIsAbsentMessage = "Файл не найден.";
        private static readonly string _viewTypeName = "PersonSourceUserControl";
        
        private IUniformDocumentAssembler _uniformAssembler;
        private PersonChoosingViewModel _personChoosingVM;
        private PersonSourceUserControl _view;

        private bool _pathIsFromKeeper;
        private string sFP;
        internal string SourceFilePath
        {
            get { return sFP; }
            private set
            {
                string path = SetPersonsFilePath ( value );
                _pathIsFromKeeper = false;

                if ( (SourceFilePath != null)   &&   (SourceFilePath != string.Empty)   &&   (path == string.Empty) ) 
                {
                    path = SourceFilePath;
                }

                this.RaiseAndSetIfChanged ( ref sFP , path , nameof ( SourceFilePath ) );
            }
        }

        private bool eE;
        internal bool EditorIsEnable
        {
            get { return eE; }
            private set
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


        internal void SetPath ( Type passerType, string path )
        {
            if ( passerType.Name == _viewTypeName ) 
            {
                _pathIsFromKeeper = true;
                SourceFilePath = path;
                EditorIsEnable = true;
            }
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

                               string workDirectory = @"./";
                               DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
                               string directoryPath = containingDirectory.FullName;
                               string keeperPath = directoryPath + ModernMainView._sourcePathKeeper;
                               FileInfo fileInf = new FileInfo (keeperPath);

                               if ( fileInf.Exists )
                               {
                                   List<string> lines = new List<string> ();
                                   lines.Add (SourceFilePath);
                                   File.WriteAllLines (keeperPath, lines);
                               }
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


        private string SetPersonsFilePath ( string path )
        {
            bool valueIsSuitable = ( path != null )   &&   ( path != string.Empty );

            if ( valueIsSuitable )
            {
                try
                {
                    List <Person> persons = _uniformAssembler.GetPersons (path);
                    ObservableCollection <VisiblePerson> visible = new ();
                    List <Person> people = new ();
                    List <VisiblePerson> visiblePeopleStorage = new ();
                    //ObservableCollection <VisiblePerson> visiblePeopleReserve = new ();

                    foreach ( var person   in   persons )
                    {
                        VisiblePerson visiblePerson = new VisiblePerson (person);
                        visible.Add (visiblePerson);
                        visiblePeopleStorage.Add (visiblePerson);
                        //visiblePeopleReserve.Add (visiblePerson);
                        people.Add (person);
                    }

                    peopleSettingOccured = true;
                    _personChoosingVM.People = people;
                    _personChoosingVM.VisiblePeopleStorage = visiblePeopleStorage;
                    //_personChoosingVM.VisiblePeopleReserve = visiblePeopleReserve;
                    _personChoosingVM.VisiblePeople = visible;
                    return path;
                }
                catch ( IOException ex )
                {
                    var messegeDialog = new MessageDialog ();

                    if ( ! _pathIsFromKeeper )
                    {
                        messegeDialog.Message = _fileIsOpenMessage;
                        messegeDialog.ShowDialog (MainWindow._mainWindow);
                    }

                    return string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
