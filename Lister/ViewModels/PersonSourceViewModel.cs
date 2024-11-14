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
using DynamicData;
using ExCSS;
using Avalonia;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;

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

        private FilePickerOpenOptions _filePickerOptions;
        private FilePickerOpenOptions FilePickerOptions => _filePickerOptions ??= new ()
        {
            Title = _filePickerTitle,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType ("Источники данных")
                {
                    Patterns = ["*.csv", "*.xlsx"]
                }
            ]
        };

        private bool _pathIsFromKeeper;
        private string _sourceFilePath;
        internal string SourceFilePath
        {
            get { return _sourceFilePath; }
            private set
            {
                string path = SetCorrespondingPersons (value);
                _pathIsFromKeeper = false;

                if ( ( SourceFilePath != null )   &&   ( SourceFilePath != string.Empty )   &&   ( path == string.Empty ) )
                {
                    path = SourceFilePath;
                }

                this.RaiseAndSetIfChanged (ref _sourceFilePath, path, nameof (SourceFilePath));
            }
        }

        internal bool peopleSettingOccured;


        public PersonSourceViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler
                                                                    , PersonChoosingViewModel personChoosing )
        {
            _uniformAssembler = singleTypeDocumentAssembler;
            _personChoosingVM = personChoosing;
            peopleSettingOccured = false;
        }


        internal void TrySetPath ( Type passerType, string? path )
        {
            bool pathExists = ( ( path != null )   &&   ( path != string.Empty ) );

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

                        return;
                    }
                }

                if ( passerType.Name == _viewTypeName )
                {
                    bool fileIsCorrect = CheckWhetherCorrectIfXSLX (path);

                    if ( ! fileIsCorrect )
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
                }
            }
            else
            {
                _pathIsFromKeeper = false;
                SourceFilePath = null;
            }
        }


        internal void PassView ( PersonSourceUserControl view )
        {
            _view = view;
        }


        internal async void ChooseFile ()
        {
            IReadOnlyList <IStorageFile> files = await MainWindow.CommonStorageProvider.OpenFilePickerAsync (FilePickerOptions);

            if ((files.Count == 1)   &&   ( files [0] is not null ))
            {
                string path = files [0].Path.LocalPath;

                bool fileIsCorrect = CheckWhetherCorrectIfXSLX (path);

                if ( ! fileIsCorrect )
                {
                    DeclineChosenFile (path);

                    if ( ( SourceFilePath != null )   &&   ( SourceFilePath != string.Empty ) )
                    {
                        SwitchPersonChoosingEnabling (true);
                    }

                    return;
                }

                SourceFilePath = path;
                TrySavePathInKeepingFile ();
                SwitchPersonChoosingEnabling (true);
            }
            else
            {
                SwitchPersonChoosingEnabling (false);
            }
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
            _personChoosingVM.TextboxIsReadOnly = ! shouldEnable;
            _personChoosingVM.TextboxIsFocusable = shouldEnable;
        }


        private bool CheckWhetherCorrectIfXSLX ( string path )
        {
            bool isOk = true;

            if ( path.Last () == 'x' )
            {
                IRowSource headersSource = App.services.GetService<IRowSource> ();

                List<string> headers = headersSource.GetRow (path, 0);

                for ( int index = 0; index < _xslxHeaders.Count; index++ )
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


        private string SetCorrespondingPersons ( string path )
        {
            bool valueIsSuitable = ! string.IsNullOrWhiteSpace(path);

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


//string path = files [0].Path.ToString ();
//int uriTypeLength = App.ResourceUriType.Length;
//path = path.Substring (uriTypeLength, path.Length - uriTypeLength);