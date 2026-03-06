using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using Lister.Desktop.ExecutersForCoreAbstractions.PeopleAccess;
using Lister.Desktop.Services;

namespace Lister.Desktop.Views.MainView.Parts.PersonSource.ViewModel;

public class PersonSourceViewModel ( string pickerTitle, string filePickerTitle, List<string> patterns, List<string> xslxHeaders,
    int limit, string sourceKeeper, DocumentProcessor documentProcessor, string configPath
) : ObservableObject
{
    private readonly string _pickerTitle = pickerTitle;
    private readonly string _filePickerTitle = filePickerTitle;
    private readonly string _configPath = configPath;
    private readonly IReadOnlyList<string> _patterns = patterns;
    private readonly List<string> _xslxHeaders = xslxHeaders;
    private string? _sourcePathKeeper = sourceKeeper;
    private bool _isFirstTimeLoading = true;
    private readonly string? _declinedFilePath;
    private readonly DocumentProcessor _documentProcessor = documentProcessor;

    private FilePickerOpenOptions? _filePickerOptions;
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

    private string? _sourceFilePath;
    internal string? SourceFilePath
    {
        get => _sourceFilePath;
        
        private set
        {
            _sourcePathKeeper = value;
            _sourceFilePath = value;
            OnPropertyChanged ();
        }
    }

    private bool _fileIsOpen;
    public bool FileIsOpen
    {
        get => _fileIsOpen;
        
        private set
        {
            if ( _fileIsOpen == value )
            {
                _fileIsOpen = !_fileIsOpen;
            }

            OnPropertyChanged ();
        }
    }

    private bool _fileIsDeclined;
    internal bool FileIsDeclined
    {
        get => _fileIsDeclined;
        
        private set
        {
            if ( _fileIsDeclined == value )
            {
                _fileIsDeclined = !_fileIsDeclined;
            }

            OnPropertyChanged ();
        }
    }

    private bool _fileIsTooBig;
    internal bool FileIsTooBig
    {
        get => _fileIsTooBig;

        private set
        {
            if ( _fileIsTooBig == value )
            {
                _fileIsTooBig = !_fileIsTooBig;
            }

            OnPropertyChanged ();
        }
    }

    private readonly int _personsLimitForSource = limit;

    internal string? FilePath { get; private set; }

    internal void OnLoaded ()
    {
        if ( !_isFirstTimeLoading )
        {
            return;
        }

        try
        {
            SetPath ( _sourcePathKeeper );
        }
        catch ( IndexOutOfRangeException )
        {
            SetPath ( null );
        }

        _isFirstTimeLoading = false;
    }

    internal async void ChooseFile ()
    {
        IReadOnlyList<IStorageFile> files = await MainWindow.MainWindow.CommonStorageProvider.OpenFilePickerAsync ( FilePickerOptions );

        if ( files.Count == 1 && files [0] is not null )
        {
            string path = files [0].Path.LocalPath;
            TryUse ( path, true );
        }
    }

    internal void DeclineKeepedFileIfIncorrect ()
    {
        if ( string.IsNullOrWhiteSpace ( _declinedFilePath ) )
        {
            return;
        }

        DeclineIncorrectFile ( _declinedFilePath );
    }

    internal void EmptySourcePath ()
    {
        SourceFilePath = string.Empty;
    }

    private void SetPath ( string? path )
    {
        if ( !string.IsNullOrWhiteSpace ( path ) )
        {
            TryUse ( path, false );
        }
        else
        {
            SourceFilePath = null;
        }
    }

    private void TryUse ( string path, bool shouldSave )
    {
        FileInfo fileInfo = new ( path );

        if ( fileInfo.Exists )
        {
            try
            {
                FileStream stream = fileInfo.OpenWrite ();
                stream.Close ();
            }
            catch ( IOException )
            {
                FileIsOpen = true;

                return;
            }
        }
        else
        {
            SourceFilePath = null;

            return;
        }

        XlsxFileState fileState = CheckCorrectnessIfXSLX ( path );

        if ( fileState == XlsxFileState.IsIncorrect )
        {
            DeclineIncorrectFile ( path );

            return;
        }
        else if ( !_documentProcessor.TrySetPeopleFrom ( path, _personsLimitForSource ) )
        {
            FilePath = path;
            FileIsTooBig = true;

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
                JsonProcessor.WritePersonSource ( _configPath, SourceFilePath );
            }
        }
    }

    private XlsxFileState CheckCorrectnessIfXSLX ( string path )
    {
        XlsxFileState fileState = XlsxFileState.NotXlsxFile;

        if ( path.Last () == 'x' )
        {
            try
            {
                PeopleXlsxSource headersSource = PeopleXlsxSource.GetInstance ();
                List<string> headers = PeopleXlsxSource.GetRow ( path );

                for ( int index = 0; index < _xslxHeaders.Count; index++ )
                {
                    bool isNotCoincident = headers [index] != _xslxHeaders [index];

                    if ( isNotCoincident )
                    {
                        fileState = XlsxFileState.IsIncorrect;

                        break;
                    }
                }
            }
            catch ( IOException )
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

    private enum XlsxFileState
    {
        NotXlsxFile = 0,
        IsOpen = 1,
        IsIncorrect = 2,
        IsTooBig = 3
    }
}
