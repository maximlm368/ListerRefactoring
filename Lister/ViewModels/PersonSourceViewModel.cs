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
using MessageBox.Avalonia.Views;
using Avalonia.Media;

namespace Lister.ViewModels
{
    public class PersonSourceViewModel : ViewModelBase
    {
        private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении. Закройте его и повторите выбор.";
        private static readonly string _fileIsAbsentMessage = "Файл не найден.";
        private static readonly string _viewTypeName = "PersonSourceUserControl";
        private static readonly string _filePickerTitle = "Выбор файла";
        
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

        private SolidColorBrush bB;
        internal SolidColorBrush BorderBrush
        {
            get { return bB; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bB, value, nameof (BorderBrush));
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


        internal void ChangeAccordingTheme ( string theme )
        {
            BorderBrush = new SolidColorBrush (MainWindow.black);

            if ( theme == "Dark" )
            {
                BorderBrush = new SolidColorBrush (MainWindow.white);
            }
        }


        internal void SetPath ( Type passerType, string ? path )
        {
            if ( (path != null)   &&   ( path != string.Empty ) ) 
            {
                FileInfo fileInfo = new FileInfo ( path );

                if ( fileInfo.Exists ) 
                {
                    try
                    {
                        FileStream stream = fileInfo.OpenRead ();
                        stream.Close ();
                    }
                    catch ( System.IO.IOException ex ) 
                    {
                        SourceFilePath = null;
                        EditorIsEnable = false;

                        return;
                    }
                }
            }

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


        internal void ChooseFile ()
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
            options.Title = _filePickerTitle;
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
                           int uriTypeLength = App.ResourceUriType. Length;
                           result = result.Substring (uriTypeLength, result.Length - uriTypeLength);
                           SourceFilePath = result;

                           if ( ( SourceFilePath != null )   &&   (SourceFilePath != string.Empty) )
                           {
                               EditorIsEnable = true;
                               _personChoosingVM.TextboxIsReadOnly = false;
                               _personChoosingVM.TextboxIsFocusable = true;

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
                           else 
                           {
                               EditorIsEnable = false;
                               _personChoosingVM.TextboxIsReadOnly = true;
                               _personChoosingVM.TextboxIsFocusable = false;
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
                    _personChoosingVM.SetPersons (persons);
                    peopleSettingOccured = true;

                    return path;
                }
                catch ( IOException ex )
                {
                    var messegeDialog = new MessageDialog ();

                    if ( !_pathIsFromKeeper )
                    {
                        messegeDialog.Message = _fileIsOpenMessage;
                        messegeDialog.ShowDialog (MainWindow.Window);
                    }

                    return string.Empty;
                }
            }
            else
            {
                _personChoosingVM.SetPersons (null);
            }

            return string.Empty;
        }
    }
}
