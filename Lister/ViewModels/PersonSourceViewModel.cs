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
using Microsoft.Extensions.DependencyInjection;
using DataGateway;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister.ViewModels
{
    public class PersonSourceViewModel : ViewModelBase
    {
        private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении. Закройте его и повторите выбор.";
        private static readonly string _incorrectXSLX = " - некорректный файл.";
        private static readonly string _fileIsAbsentMessage = "Файл не найден.";
        private static readonly string _viewTypeName = "PersonSourceUserControl";
        private static readonly string _filePickerTitle = "Выбор файла";
        private static readonly List<string> _xslxHeaders = new List<string> () { "№", "Фамилия", "Имя", "Отчество"
                                                                                , "Отделение", "Должность" };
        
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
                string path = SetCorrespondingPersons ( value );
                _pathIsFromKeeper = false;

                if ((SourceFilePath != null)   &&   (SourceFilePath != string.Empty)   &&   (path == string.Empty)) 
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


        internal void TrySetPath ( Type passerType, string ? path )
        {
            bool pathExists = (( path != null )   &&   ( path != string.Empty ));

            if ( pathExists )
            {
                FileInfo fileInfo = new FileInfo (path);

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

                if ( passerType.Name == _viewTypeName )
                {
                    bool fileIsCorrect = CheckIfIncorrectXSLX (path);

                    if ( !fileIsCorrect )
                    {
                        DeclineChosenFile (path);

                        if ( ( SourceFilePath != null ) && ( SourceFilePath != string.Empty ) )
                        {
                            SwitchPersonChoosingEnabling (false);
                        }

                        return;
                    }

                    _pathIsFromKeeper = true;
                    SourceFilePath = path;
                    EditorIsEnable = true;
                }
            }
            else 
            {
                _pathIsFromKeeper = false;
                SourceFilePath = null;
                EditorIsEnable = false;
            }
        }


        internal void PassView ( PersonSourceUserControl view )
        {
            _view = view;
        }


        internal void ChooseFile ()
        {
            FilePickerOpenOptions options = SetFilePikerOptions ();
            Task <IReadOnlyList <IStorageFile>> chosenFile = MainWindow.CommonStorageProvider.OpenFilePickerAsync (options);
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            chosenFile.ContinueWith
            (
                task =>
                {
                    if ( task.Result. Count > 0 )
                    {
                        string result = task.Result [0].Path.ToString ();
                        int uriTypeLength = App.ResourceUriType. Length;
                        result = result.Substring (uriTypeLength, result.Length - uriTypeLength);

                        if ( ( result != null )   &&   (result != string.Empty) )
                        {
                            bool fileIsCorrect = CheckIfIncorrectXSLX (result);

                            if ( ! fileIsCorrect )
                            {
                                DeclineChosenFile (result);

                                if ( ( SourceFilePath != null ) && ( SourceFilePath != string.Empty ) )
                                {
                                    SwitchPersonChoosingEnabling (true);
                                }

                                return;
                            }

                            SourceFilePath = result;
                            TrySavePathInKeepingFile ();
                            SwitchPersonChoosingEnabling (true);
                        }
                        else 
                        {
                            SwitchPersonChoosingEnabling (false);
                        }
                    }
                }
                , uiScheduler
            );
        }


        private void TrySavePathInKeepingFile () 
        {
            string workDirectory = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            string directoryPath = containingDirectory.FullName;
            string keeperPath = directoryPath + ModernMainView._sourcePathKeeper;
            FileInfo fileInfo = new FileInfo (keeperPath);

            if ( fileInfo.Exists )
            {
                List<string> lines = new List<string> ();
                lines.Add (SourceFilePath);
                File.WriteAllLines (keeperPath, lines);
            }
        }


        private void SwitchPersonChoosingEnabling ( bool shouldEnable )
        {
            EditorIsEnable = shouldEnable;
            _personChoosingVM.TextboxIsReadOnly = ! shouldEnable;
            _personChoosingVM.TextboxIsFocusable = shouldEnable;
        }


        private bool CheckIfIncorrectXSLX ( string path )
        {
            bool isOk = true;

            if ( path.Last () == 'x' )
            {
                IRowSource headersSource = App.services.GetService<IRowSource> ();

                List<string> headers = headersSource.GetRow (path, 0);

                for ( int index = 0;   index < _xslxHeaders.Count;   index++ )
                {
                    bool isNotCoincident = ( headers [index] != _xslxHeaders [index] );

                    if ( isNotCoincident )
                    {
                        isOk = false;

                        break;
                    }
                }
            }

            return isOk;
        }


        private void DeclineChosenFile ( string filePath )
        {
            var messegeDialog = new MessageDialog (ModernMainView.Instance);
            messegeDialog.Message = filePath + _incorrectXSLX;
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.HandleDialogOpenig ();
            messegeDialog.ShowDialog (MainWindow.Window);
            SourceFilePath = string.Empty;
        }


        private FilePickerOpenOptions SetFilePikerOptions ( )
        {
            FilePickerOpenOptions result = new FilePickerOpenOptions ();

            FilePickerFileType csvFileType = new FilePickerFileType ("Csv")
            {
                Patterns = new [] { "*.csv" },
                AppleUniformTypeIdentifiers = new [] { "public.image" },
                MimeTypes = new [] { "image/*" }
            };

            FilePickerFileType xlsxFileType = new FilePickerFileType ("Xlsx")
            {
                Patterns = new [] { "*.xlsx" },
                AppleUniformTypeIdentifiers = new [] { "public.image" },
                MimeTypes = new [] { "image/*" }
            };

            List<FilePickerFileType> fileExtentions = [];
            fileExtentions.Add (csvFileType);
            fileExtentions.Add (xlsxFileType);

            result.FileTypeFilter = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
            result.Title = _filePickerTitle;
            result.AllowMultiple = false;

            return result;
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


        private string SetCorrespondingPersons ( string path )
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
                    var messegeDialog = new MessageDialog (ModernMainView.Instance);

                    if ( ! _pathIsFromKeeper )
                    {
                        messegeDialog.Message = _fileIsOpenMessage;
                        WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                        waitingVM.HandleDialogOpenig ();
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
