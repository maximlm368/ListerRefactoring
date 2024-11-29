using Avalonia;
using ContentAssembler;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls;
using Avalonia.Media;
using System.Windows.Input;
using System.Text;
using System.Net.WebSockets;
using System.ComponentModel;
using Lister.Views;
using Lister.Extentions;
using System.Collections.ObjectModel;
using static QuestPDF.Helpers.Colors;
using Avalonia.Controls.Shapes;
using DynamicData;
using ReactiveUI;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;
using QuestPDF.Helpers;
using System;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using Avalonia.Remote.Protocol.Viewport;
using MessageBox.Avalonia.Views;
using AvaloniaEdit;
using Avalonia.Threading;


namespace Lister.ViewModels;

public class BadgesBuildingViewModel : ReactiveObject
{
    private static readonly string _title = "Ошибка";
    private static readonly string _saveTitle = "Сохранение документа";
    private static readonly string _suggestedFileNames = "Badge";
    public static bool BuildingOccured { get; private set; }

    private WaitingViewModel _waitingVM;

    //private ObservableCollection <TemplateViewModel> _templates;
    //internal ObservableCollection <TemplateViewModel> Templates
    //{
    //    get
    //    {
    //        return _templates;
    //    }
    //    set
    //    {
    //        this.RaiseAndSetIfChanged (ref _templates, value, nameof (Templates));
    //    }
    //}

    private bool _buildingIsPossible;
    internal bool BuildingIsPossible
    {
        set
        {
            this.RaiseAndSetIfChanged (ref _buildingIsPossible, value, nameof (BuildingIsPossible));
        }
        get
        {
            return _buildingIsPossible;
        }
    }

    private bool _buildButtonIsTapped;
    internal bool BuildButtonIsTapped
    {
        private set
        {
            if ( value )
            {
                this.RaiseAndSetIfChanged (ref _buildButtonIsTapped, value, nameof (BuildButtonIsTapped));
            }
            else 
            {
                _buildButtonIsTapped = false;
            }
        }
        get
        {
            return _buildButtonIsTapped;
        }
    }


    public BadgesBuildingViewModel ( )
    {
    }


    internal void TryToEnableBadgeCreation ( bool shouldEnable )
    {
        BuildingIsPossible = shouldEnable;
    }


    internal void ChangeAccordingTheme ( string theme )
    {
        //SolidColorBrush foundColor = new SolidColorBrush (MainWindow.black);
        //SolidColorBrush unfoundColor = new SolidColorBrush (new Color (100, 0, 0, 0));

        //if ( theme == "Dark" ) 
        //{
        //    foundColor  = new SolidColorBrush (MainWindow.white);
        //    unfoundColor = new SolidColorBrush (new Color (100, 255, 255, 255));
        //}

        //ObservableCollection <TemplateViewModel> templates = new ();

        //foreach ( TemplateName name   in   _templateNames )
        //{
        //    SolidColorBrush brush;

        //    if ( name.IsFound )
        //    {
        //        brush = foundColor;
        //    }
        //    else
        //    {
        //        brush = unfoundColor;
        //    }

        //    templates.Add (new TemplateViewModel (name, brush));
        //}

        //Templates = templates;
    }


    internal void BuildBadges ()
    {
        BuildButtonIsTapped = true;

        BuildingOccured = true;

        BadgeEditorViewModel.BackingToMainViewEvent += ()=>
        { 
            BuildingOccured = false; 
        };

        BuildButtonIsTapped = false;
    }


    //internal void Build ()
    //{
    //    if ( ShouldBuildEntireList )
    //    {
    //        TappedBadgesBuildingButton = 1;
    //        ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();
    //        mainViewModel.SetWaiting ();
    //    }
    //    else
    //    {
    //        BuildSingleBadge ();
    //        _sceneVM.EnableButtons ();
    //    }
    //}


    //internal void BuildAllBadges ()
    //{
    //    _buildingIsLocked = true;
    //    BuildingIsPossible = false;

    //    Task task = new Task
    //    (
    //        () =>
    //        {
    //            bool buildingIsCompleted = _sceneVM.BuildBadges (_personChoosingVM.ChosenTemplate.Name);

    //            _sceneVM.EnableButtons ();

    //            _buildingIsLocked = false;
    //            TappedBadgesBuildingButton = 0;

    //            if ( ! buildingIsCompleted )
    //            {
    //                Dispatcher.UIThread.Invoke
    //                (() =>
    //                {
    //                    TappedBadgesBuildingButton = 0;
    //                    ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();
    //                    mainViewModel.EndWaiting ();

    //                    var messegeDialog = new MessageDialog (ModernMainView.Instance);
    //                    messegeDialog.Message = _buildingLimitIsExhaustedMessage;
    //                    WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
    //                    waitingVM.HandleDialogOpenig ();
    //                    messegeDialog.ShowDialog (MainWindow.Window);
    //                });
    //            }
    //        }
    //    );

    //    task.Start ();
    //}


    //internal void BuildSingleBadge ()
    //{
    //    _buildingIsLocked = true;

        

    //    bool buildingIsCompleted = _sceneVM.BuildSingleBadge (_personChoosingVM.ChosenTemplate. Name);
    //    _buildingIsLocked = false;

    //    if ( ! buildingIsCompleted )
    //    {
    //        var messegeDialog = new MessageDialog (ModernMainView.Instance);
    //        messegeDialog.Message = _buildingLimitIsExhaustedMessage;
    //        WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
    //        waitingVM.HandleDialogOpenig ();
    //        messegeDialog.ShowDialog (MainWindow.Window);
    //    }
    //}


    //private void TryToEnableBadgeCreationButton ()
    //{
    //    if ( _personChoosingVM == null )
    //    {
    //        _personChoosingVM = App.services.GetRequiredService <PersonChoosingViewModel> ();
    //    }

    //    bool buildingIsPossible = ( _personChoosingVM.ChosenTemplate != null )   &&   _personChoosingVM.BuildingIsPossible;

    //    if ( buildingIsPossible )
    //    {
    //        BuildingIsPossible = true;
    //    }
    //    else 
    //    {
    //        BuildingIsPossible = false;
    //    }
    //}


    //internal void BuildDuringWaiting ( )
    //{
    //    if ( ModernMainViewModel.MainViewIsWaiting    &&   ! _buildingIsLocked )
    //    {
    //        BuildAllBadges ();
    //    }
    //}
}



