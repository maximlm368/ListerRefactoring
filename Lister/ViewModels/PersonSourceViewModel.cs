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
using Avalonia.Interactivity;

namespace Lister.ViewModels
{
    public class PersonSourceViewModel : ReactiveObject
    {
        private IReadOnlyList<string> _patterns;
        private readonly string _pickerTitle;
        private readonly string _sourcePathKeeper;
        private readonly string _filePickerTitle;
        private IReadOnlyList<string> _xslxHeaders;

        private IUniformDocumentAssembler _uniformAssembler;
        private bool _isFirstTimeLoading = true;
        private string _declinedFilePath;

        private FilePickerOpenOptions _filePickerOptions;
        private FilePickerOpenOptions FilePickerOptions => _filePickerOptions ??= new ()
        {
            Title = _filePickerTitle,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType (_pickerTitle)
                {
                    Patterns = _patterns
                }
            ]
        };

        private string _sourceFilePath;
        internal string SourceFilePath
        {
            get { return _sourceFilePath; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _sourceFilePath, value, nameof(SourceFilePath));
            }
        }

        private bool _personsFileIsOpen;
        public bool FileIsOpen
        {
            get { return _personsFileIsOpen; }
            private set
            {
                if ( _personsFileIsOpen == value )
                {
                    _personsFileIsOpen = !_personsFileIsOpen;
                }

                this.RaiseAndSetIfChanged (ref _personsFileIsOpen, value, nameof (FileIsOpen));
            }
        }

        private bool _fileIsDeclined;
        internal bool FileIsDeclined
        {
            get { return _fileIsDeclined; }
            private set
            {
                if ( _fileIsDeclined == value )
                {
                    _fileIsDeclined = !_fileIsDeclined;
                }

                this.RaiseAndSetIfChanged (ref _fileIsDeclined, value, nameof (FileIsDeclined));
            }
        }

        internal string FilePath { get; private set; }


        public PersonSourceViewModel ( List<string> args, List<string> patterns, List<string> xslxHeaders )
        {
            _pickerTitle = args [0];
            _sourcePathKeeper = args [1];
            _filePickerTitle = args [2];

            _patterns = patterns;
            _xslxHeaders = xslxHeaders;

            _uniformAssembler = App.services.GetRequiredService<IUniformDocumentAssembler> ();
        }


        internal void OnLoaded ( )
        {
            if ( !_isFirstTimeLoading )
            {
                return;
            }

            string workDirectory = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            string directory = containingDirectory.FullName;
            string keeper = directory + _sourcePathKeeper;
            FileInfo fileInf = new FileInfo (keeper);

            if ( fileInf.Exists )
            {
                string [] lines = File.ReadAllLines (keeper);

                try
                {
                    SetPath (lines [0]);
                }
                catch ( IndexOutOfRangeException ex )
                {
                    SetPath (null);
                }
            }
            else
            {
                File.Create (keeper).Close ();
                SetPath (null);
            }

            _isFirstTimeLoading = false;
        }


        private void SetPath ( string ? path )
        {
            bool pathExists = (( path != null )   &&   ( path != string.Empty ));

            if ( pathExists )
            {
                CheckIfOpenOrIncorrectAndSave (path, false);
            }
            else
            {
                SourceFilePath = null;
            }
        }


        internal async void ChooseFile ()
        {
            IReadOnlyList <IStorageFile> files = 
                await MainWindow.CommonStorageProvider.OpenFilePickerAsync (FilePickerOptions);

            if ((files.Count == 1)   &&   ( files [0] is not null ))
            {
                string path = files [0].Path. LocalPath;

                CheckIfOpenOrIncorrectAndSave (path, true);
            }
        }


        private void CheckIfOpenOrIncorrectAndSave ( string path, bool shouldSave )
        {
            FileInfo fileInfo = new FileInfo (path);

            if ( fileInfo.Exists )
            {
                try
                {
                    FileStream stream = fileInfo.OpenRead ();
                    stream.Close ();
                }
                catch ( IOException ex )
                {
                    FileIsOpen = true;
                    return;
                }
            }

            XlsxFileState fileState = CheckWhetherCorrectIfXSLX (path);

            if ( fileState == XlsxFileState.IsIncorrect )
            {
                DeclineIncorrectFile (path);
                return;
            }
            else if ( fileState == XlsxFileState.IsOpen )
            {
                FileIsOpen = true;
                return;
            }
            else if ( fileState == XlsxFileState.NotXlsxFile )
            {
                SourceFilePath = path;

                if ( shouldSave ) 
                {
                    SavePath ();
                }
            }
        }


        private void SavePath ()
        {
            string workDirectory = @"./";
            DirectoryInfo containingDirectory = new DirectoryInfo (workDirectory);
            string directoryPath = containingDirectory.FullName;
            string keeperPath = directoryPath + _sourcePathKeeper;
            FileInfo fileInfo = new FileInfo (keeperPath);

            if ( fileInfo.Exists )
            {
                List<string> lines = new List<string> ();
                lines.Add (SourceFilePath);
                File.WriteAllLines (keeperPath, lines);
            }
        }


        private XlsxFileState CheckWhetherCorrectIfXSLX ( string path )
        {
            XlsxFileState fileState = XlsxFileState.NotXlsxFile;

            if ( path.Last () == 'x' )
            {
                try 
                {
                    IRowSource headersSource = App.services.GetService<IRowSource> ();
                    List<string> headers = headersSource.GetRow (path, 0);

                    for ( int index = 0; index < _xslxHeaders.Count; index++ )
                    {
                        bool isNotCoincident = ( headers [index] != _xslxHeaders [index] );

                        if ( isNotCoincident )
                        {
                            fileState = XlsxFileState.IsIncorrect;

                            break;
                        }
                    }
                }
                catch ( IOException ex )
                {
                    fileState = XlsxFileState.IsOpen;
                }
            }

            return fileState;
        }


        private void DeclineIncorrectFile ( string filePath )
        {
            FilePath = filePath;
            FileIsDeclined = true;
        }


        internal void DeclineKeepedFileIfIncorrect ( )
        {
            if ( string.IsNullOrWhiteSpace(_declinedFilePath) ) 
            {
                return;
            }

            DeclineIncorrectFile (_declinedFilePath);
        }


        internal void EmptySourcePath ()
        {
            SourceFilePath = string.Empty;
        }


        private enum XlsxFileState 
        {
            NotXlsxFile = 0,
            IsOpen = 1,
            IsIncorrect = 2
        }
    }
}


//string path = files [0].Path.ToString ();
//int uriTypeLength = App.ResourceUriType.Length;
//path = path.Substring (uriTypeLength, path.Length - uriTypeLength);


//private string SetCorrespondingPersons ( string path )
//{
//    bool valueIsSuitable = ! string.IsNullOrWhiteSpace(path);

//    if ( valueIsSuitable )
//    {
//        try
//        {
//            List <Person> persons = _uniformAssembler.GetPersons (path);
//            _personChoosingVM.SetPersons (persons);

//            return path;
//        }
//        catch ( IOException ex )
//        {
//            var messegeDialog = new MessageDialog (ModernMainView.Instance);

//            if ( ! _pathIsFromKeeper )
//            {
//                messegeDialog.Message = _fileIsOpenMessage;
//                WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
//                waitingVM.HandleDialogOpenig ();
//                messegeDialog.ShowDialog (MainWindow.Window);
//            }

//            return string.Empty;
//        }
//    }
//    else
//    {
//        _personChoosingVM.SetPersons (( List<Person>? )null);
//    }

//    return string.Empty;
//}