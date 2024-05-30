using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ContentAssembler;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class TemplateChoosingUserControl : UserControl
    {
        private const double coefficient = 1.1;
        private List<BadgeViewModel> _incorrectBadges;
        private ModernMainView _parent;
        private TemplateChoosingViewModel _vm;
        internal bool TemplateIsSelected { get; private set; }


        public TemplateChoosingUserControl ()
        {
            InitializeComponent ();
            _vm = (TemplateChoosingViewModel) DataContext;
            TemplateIsSelected = false;
        }


        internal void HandleTemplateChoosing ( object sender, SelectionChangedEventArgs args )
        {
            ComboBox comboBox = ( ComboBox ) sender;
            _vm.ChosenTemplate = ( FileInfo ) comboBox.SelectedItem;
            TemplateIsSelected = true;
            TryToEnableBadgeCreationButton ();
        }


        private void TryToEnableBadgeCreationButton ()
        {
            ModernMainView parent = this.Parent as ModernMainView;
            bool singleIsSelected = parent.personChoosing.SinglePersonIsSelected;
            bool entireListIsSelected = parent.personChoosing.EntirePersonListIsSelected;

            bool itsTimeToEnable = ( singleIsSelected   ||   entireListIsSelected )   &&   TemplateIsSelected;
            if ( itsTimeToEnable )
            {
                buildBadges.IsEnabled = true;
            }
        }


        internal void BuildBadges ( object sender, TappedEventArgs args )
        {
            ModernMainView parent = this.Parent as ModernMainView;
            bool singleIsSelected = parent.personChoosing.SinglePersonIsSelected;
            bool entireListIsSelected = parent.personChoosing.EntirePersonListIsSelected;

            if ( singleIsSelected )
            {
                _vm.BuildSingleBadge ();
            }
            if ( entireListIsSelected )
            {
                _vm.BuildBadges ();
            }

            _incorrectBadges = _vm.IncorrectBadges;

            ZoomNavigationUserControl zoomNavigation = parent.zoomNavigation;

            zoomNavigation.EnableZoom ();
            zoomNavigation.SetEnablePageNavigation ();
            clearBadges.IsEnabled = true;
            save.IsEnabled = true;
            print.IsEnabled = true;
        }


        internal void ClearBadges ( object sender, TappedEventArgs args )
        {
            ModernMainView parent = this.Parent as ModernMainView;
            ZoomNavigationUserControl zoomNavigation = parent.zoomNavigation;

            _vm.ClearAllPages ();
            clearBadges.IsEnabled = false;
            save.IsEnabled = false;
            print.IsEnabled = false;
            zoomNavigation.DisableButtons ();
        }


        internal void GeneratePdf ( object sender, TappedEventArgs args )
        {
            List<FilePickerFileType> fileExtentions = [];
            fileExtentions.Add (FilePickerFileTypes.Pdf);
            FilePickerSaveOptions options = new ();
            options.Title = "Open Text File";
            options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
            var window = TopLevel.GetTopLevel (this);
            Task<IStorageFile> chosenFile = window.StorageProvider.SaveFilePickerAsync (options);
            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            chosenFile.ContinueWith
                (
                   task =>
                   {
                       if ( task.Result != null )
                       {
                           string result = task.Result.Path.ToString ();
                           result = result.Substring (8, result.Length - 8);
                           Task<bool> pdf = _vm.GeneratePdf (result);
                           pdf.ContinueWith
                               (
                               task =>
                               {
                                   if ( pdf.Result == false )
                                   {
                                       string message = "Выбраный файл открыт в другом приложении. Закройте его и повторите.";

                                       int idOk = Winapi.MessageBox (0, message, "", 0);
                                       //GeneratePdf (result);
                                   }
                                   else
                                   {
                                       Process fileExplorer = new Process ();
                                       fileExplorer.StartInfo.FileName = "explorer.exe";
                                       result = result.ExtractPathWithoutFileName ();
                                       result = result.Replace ('/', '\\');
                                       fileExplorer.StartInfo.Arguments = result;
                                       fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                                       fileExplorer.Start ();
                                   }
                               }
                               );
                       }
                   }
                );
        }


        internal void Print ( object sender, TappedEventArgs args )
        {
            _vm.Print ();
        }



    }
}



//internal void PassNeighbours ( PersonSourceUserControl personSource, PersonChoosingUserControl personChoosing
//                             , ZoomNavigationUserControl zoomNavigation, SceneUserControl scene )
//{
//    _personChoosing = personChoosing;
//    _personSource = personSource;
//    _scene = scene;
//    _zoomNavigation = zoomNavigation;
//}


//private PersonSourceUserControl _personSource;
//private ZoomNavigationUserControl _zoomNavigation;
//private SceneUserControl _scene;
//private PersonChoosingUserControl _personChoosing;

//private bool _singlePersonIsSelected = false;
//private bool _entirePersonListIsSelected = false;

//private bool personListIsDropped = false;
//private bool templateListIsDropped = false;
//private bool personSelectionGotFocus = false;
//private bool textStackIsMesuared = false;
//private bool openedViaButton = false;
//private bool cursorIsOverPersonList = false;
//private bool selectionIsChanged = false;
//private Window owner;
//private Person selectedPerson;
//private ushort maxScalability;
//private ushort minScalability;
//private short scalabilityDepth;
//private readonly short scalabilityStep;
//private short maxDepth;
//private short minDepth;
//private double personContainerHeight;
//private double maxPersonListHeight;
//private double minPersonListHeight;