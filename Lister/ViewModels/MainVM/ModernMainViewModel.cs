using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Media;
using Avalonia.VisualTree;
using ColorTextBlock.Avalonia;
using ContentAssembler;
using DocumentFormat.OpenXml.Vml;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister.ViewModels
{
    public class ModernMainViewModel : ReactiveObject
    {
        public static bool MainViewIsWaiting { get; set; }

        private PersonSourceViewModel _personSourceViewModel;
        private PersonChoosingViewModel _personChoosingViewModel;
        private BadgesBuildingViewModel _badgesBuildingViewModel;
        private PageNavigationZoomerViewModel _zoomNavigationViewModel;
        private SceneViewModel _sceneViewModel;
        private ModernMainView _view;


        public ModernMainViewModel ( )
        {
            _personChoosingViewModel = App.services.GetRequiredService<PersonChoosingViewModel> ();
            _personSourceViewModel = App.services.GetRequiredService<PersonSourceViewModel> ();
            _badgesBuildingViewModel = App.services.GetRequiredService<BadgesBuildingViewModel> ();
            _zoomNavigationViewModel = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();
            _sceneViewModel = App.services.GetRequiredService<SceneViewModel> ();


            _personSourceViewModel.PropertyChanged += PersonSourceChanged;
            _personChoosingViewModel.PropertyChanged += AllAreReadyChanged;
            _badgesBuildingViewModel.PropertyChanged += BuildButtonTapped;
            _sceneViewModel.PropertyChanged += SceneHasChanged;
            _zoomNavigationViewModel.PropertyChanged += ZoomNavigationChanged;
        }


        private void PersonSourceChanged ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "SourceFilePath" ) 
            {
                PersonSourceViewModel personSource = ( PersonSourceViewModel ) sender;

                _personChoosingViewModel.SetPersonsFromFile (personSource.SourceFilePath);
            }
        }


        private void AllAreReadyChanged ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "AllAreReady" )
            {
                PersonChoosingViewModel personChoosingViewModel = (PersonChoosingViewModel) sender;

                _badgesBuildingViewModel.TryToEnableBadgeCreation ( personChoosingViewModel.AllAreReady );
            }
        }


        private void BuildButtonTapped ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "BuildButtonIsTapped" ) 
            {
                _sceneViewModel.Build (_personChoosingViewModel.ChosenTemplate. Name, _personChoosingViewModel.ChosenPerson);
            }
        }


        private void SceneHasChanged ( object? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "BadgesAreCleared" )
            {
                _zoomNavigationViewModel.ToZeroState ();
            }
            else if ( args.PropertyName == "BuildingIsOccured" )
            {
                SceneViewModel sceneViewModel = (SceneViewModel) sender;

                _zoomNavigationViewModel.EnableZoomIfPossible ( sceneViewModel.BuildingIsOccured );
                _zoomNavigationViewModel.SetEnablePageNavigation (sceneViewModel.PageCount, sceneViewModel.VisiblePageNumber);
            }
            else if ( args.PropertyName == "EditIncorrectsIsSelected" )
            {

            }
        }


        private void ZoomNavigationChanged ( object? sender, PropertyChangedEventArgs args )
        {
            PageNavigationZoomerViewModel zoomerNavigator = ( PageNavigationZoomerViewModel ) sender;

            if ( args.PropertyName == "ZoomDegree" )
            {
                _sceneViewModel.Zoom (zoomerNavigator.ZoomDegree);
            }
            else if ( args.PropertyName == "VisiblePageNumber" ) 
            {
                _sceneViewModel.ShowPageWithNumber (zoomerNavigator.VisiblePageNumber);
            }
        }






        internal void HandleDialogClosing ()
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.HandleDialogClosing ();
        }


        internal void ResetIncorrects ( )
        {
            _sceneViewModel.ResetIncorrects ();
        }


        internal void PassView ( ModernMainView view )
        {
            _view = view;

            WaitingView wv = _view. waiting;
        }


        internal void SetWaiting ( )
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService <WaitingViewModel> ();
            waitingVM.Show ();
            MainViewIsWaiting = true;
        }


        internal void EndWaiting ()
        {
            _badgesBuildingViewModel.BuildingIsPossible = true;
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.Hide ();
            MainViewIsWaiting = false;
        }


        internal void EndWaitingPdfOrPrint ()
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.Hide ();
            MainViewIsWaiting = false;
        }


        internal void LayoutUpdated ( )
        {
            if ( SceneViewModel.EntireListBuildingIsChosen == 1 )
            {
                _sceneViewModel.BuildDuringWaiting ();
                return;
            }
            else if ( SceneViewModel.TappedPdfGenerationButton == 1 )
            {
                _sceneViewModel.GeneratePdfDuringWaiting ();
                return;
            }
            else if ( SceneViewModel.TappedPrintButton == 1 )
            {
                _sceneViewModel.PrintDuringWaiting ();
                return;
            }
            else if ( ModernMainView.TappedGoToEditorButton == 1 )
            {
                _view.BuildEditor ();
            }
        }
    }


}

//  IUniformDocumentAssembler singleTypeDocumentAssembler, ContentAssembler.Size pageSize

//public override void Send ( string message, List<object> args, LittleViewModel littleVM )
//{
//    if ( _personChoosingVM == (littleVM as PersonChoosingViewModel) ) 
//    {

//    }
//}

//public abstract class Mediator : ViewModelBase
//{
//    public abstract void Send ( string message, List<object> ? args, LittleViewModel littleVM );
//}



//public abstract class LittleViewModel : ViewModelBase
//{
//    protected Mediator _mediator;


//    public void SetMediator ( Mediator mediator )
//    {
//        this._mediator = mediator;
//    }


//    public virtual void Send ( string message, List<object> ? args )
//    {
//        _mediator.Send (message, args, this);
//    }


//    public abstract void Notify ( string message, List<object> ? args );
//}


//Binding personSourceBinding = new Binding ();

//personSourceBinding.Source = _personSourceVM;
//personSourceBinding.Mode = BindingMode.OneWay;
//personSourceBinding.Path = "SourceFilePath";

//var expression = this.Bind (PersonSourceProperty, personSourceBinding);


//public static readonly AvaloniaProperty PersonSourceProperty;

//public string PersonSource
//{
//    get 
//    { 
//        return (string) GetValue (PersonSourceProperty); 
//    }

//    set 
//    {
//        //_personChoosingVM.SetCorrespondingPersons (value);

//        SetValue (PersonSourceProperty, value);
//    }
//}


//static ModernMainViewModel () 
//{
//    //AvaloniaPropertyRegistry.Instance.Register (typeof (ModernMainViewModel), PersonSourceProperty);

//    PersonSourceProperty = AvaloniaProperty.Register<ModernMainViewModel,string> ("PersonSource");
//}