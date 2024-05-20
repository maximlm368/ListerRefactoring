using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ContentAssembler;
using System.Net.Http;
using System.IO;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Interactivity;
using SkiaSharp;
using System.Security.Authentication.ExtendedProtection;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Splat;
using System.Collections.ObjectModel;
using Lister.ViewModels;
using ContentAssembler;
using DataGateway;
using Avalonia.Markup.Xaml.Templates;
using Lister.Extentions;
using Avalonia.Layout;
using static QuestPDF.Helpers.Colors;
using DynamicData;
using System.Runtime.InteropServices;
using ExtentionsAndAuxiliary;

namespace Lister.Views
{
    public partial class PersonSourceChoosingUC : UserControl
    {
        internal PersonSourceChoosingViewModel viewModel { get; private set; }
        private UserControl owner;


        public PersonSourceChoosingUC ( )
        {
            InitializeComponent ( );
        }


        internal void SetOwnerAndModel ( UserControl owner , IUniformDocumentAssembler docAssembler )
        {
            this.owner = owner;
            this.DataContext = new PersonSourceChoosingViewModel ( docAssembler );
            this.viewModel = ( PersonSourceChoosingViewModel ) this.DataContext;
        }


        internal void OpenEditor ( object sender , TappedEventArgs args )
        {
            //string filePath = personsSourceFile.Text;

            string filePath = "";

            if ( string.IsNullOrWhiteSpace ( filePath ) )
            {
                return;
            }

            ProcessStartInfo procInfo = new ProcessStartInfo ( )
            {
                FileName = filePath ,
                UseShellExecute = true
            };
            try
            {
                Process.Start ( procInfo );
            }
            catch ( System.ComponentModel.Win32Exception ex )
            {
            }
        }


        internal void ChooseFile ( object sender , TappedEventArgs args )
        {
            ChooseFile ( );
        }


        internal void ChooseFile ( object sender , KeyEventArgs args )
        {
            string key = args.Key.ToString ( );

            if ( key != "Q" )
            {
                return;
            }

            ChooseFile ( );
        }


        private void ChooseFile ( )
        {
            FilePickerFileType csvFileType = new FilePickerFileType ( "Csv" )
            {
                Patterns = new [ ] { "*.csv" } ,
                AppleUniformTypeIdentifiers = new [ ] { "public.image" } ,
                MimeTypes = new [ ] { "image/*" }
            };

            List<FilePickerFileType> fileExtentions = [ ];
            fileExtentions.Add ( csvFileType );
            FilePickerOpenOptions options = new FilePickerOpenOptions ( );
            options.FileTypeFilter = new ReadOnlyCollection<FilePickerFileType> ( fileExtentions );
            options.Title = "Open Text File";
            options.AllowMultiple = false;
            var window = TopLevel.GetTopLevel ( this );
            Task<IReadOnlyList<IStorageFile>> chosenFile = null;
            chosenFile = window.StorageProvider.OpenFilePickerAsync ( options );
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ( );

            chosenFile.ContinueWith
                (
                   task =>
                   {
                       if ( task.Result.Count > 0 )
                       {
                           string result = task.Result [ 0 ].Path.ToString ( );
                           PersonSourceChoosingViewModel vm = viewModel;
                           result = result.Substring ( 8 , result.Length - 8 );
                           vm.sourceFilePath = result;

                           if ( vm.sourceFilePath != string.Empty )
                           {
                               //editSourceFile.IsEnabled = true;
                               //setEntirePersonList.IsEnabled = true;
                           }
                       }
                   }
                   , uiScheduler
                );
        }
    }
}
