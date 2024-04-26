using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ContentAssembler;
using Lister.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Lister.Views
{
    public partial class PersonSourceFileChoosingUC : UserControl
    {
        internal PersonSourceFileChoosingUCViewModel viewModel { get; private set; }
        private Task<IReadOnlyList<IStorageFile>>? personsFile;
        private UserControl owner;


        public PersonSourceFileChoosingUC ( UserControl owner,  IUniformDocumentAssembler docAssembler )
        {
            InitializeComponent ();
            this.owner = owner;
            this.DataContext = new PersonSourceFileChoosingUCViewModel(docAssembler);
            this.viewModel = ( PersonSourceFileChoosingUCViewModel ) this.DataContext;
        }


        internal void OpenLikeExcel ( object sender, TappedEventArgs args )
        {
            string processFilePath = "";
            string beingEditedInLikeExcelFilePath = personsSourceFile.Text;

            var os = Environment.OSVersion.VersionString;
            bool currentOsIsWindow = os.Contains ("Windows")   ||   os.Contains ("windows");

            if ( currentOsIsWindow )
            {
                processFilePath = "c:\\Program Files\\Microsoft Office\\Office15\\EXCEL.EXE";
            }

            ProcessStartInfo procInfo = new ProcessStartInfo ();
            procInfo.FileName = processFilePath;
            procInfo.Arguments = beingEditedInLikeExcelFilePath;
            Process.Start (procInfo);
        }


        internal void ChooseFile ( object sender, TappedEventArgs args )
        {
            FilePickerOpenOptions options = new FilePickerOpenOptions ();
            List<FilePickerFileType> fileExtentions = new List<FilePickerFileType> ();
            fileExtentions.Add (new FilePickerFileType ("csv"));

            options.FileTypeFilter = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
            var window = TopLevel.GetTopLevel (this);
            personsFile = window.StorageProvider.OpenFilePickerAsync (new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = options.FileTypeFilter
            });

            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();
            personsFile.ContinueWith
                (
                   task =>
                   {
                       string result = task.Result [0].Path.ToString ();
                       personsSourceFile.Text = result;
                   }
                   , uiScheduler
                );
        }
    }
}
