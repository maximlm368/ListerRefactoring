using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using ContentAssembler;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Lister.Views
{
    public partial class TemplateChoosingUserControl : ReactiveUserControl <TemplateChoosingViewModel>
    {
        private static readonly string _jsonError = 
        "Невозможно загрузить этот шаблон.Обратитесь к разработчику по телефону 324-708";

        private static TemplateViewModel _chosen;

        private ModernMainView _parent;
        private TemplateChoosingViewModel _vm;
        private string _theme;


        public TemplateChoosingUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<TemplateChoosingViewModel> ();
            _vm = ( TemplateChoosingViewModel ) DataContext;
            Loaded += OnLoaded;
            ActualThemeVariantChanged += ThemeChanged;

            //templateChoosing.IsEnabled = false;

        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            //_vm.ChosenTemplate = _chosen;

            //if ( _chosen != null ) 
            //{
            //    templateChoosing.PlaceholderText = _vm.ChosenTemplate. Name;
            //}

            //_vm.PassView (this);
            //_theme = ActualThemeVariant.Key.ToString ();
            //_vm.ChangeAccordingTheme (_theme);
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null ) 
            {
                return;
            }

            _theme = ActualThemeVariant.Key.ToString ();
            _vm.ChangeAccordingTheme ( _theme );
        }


        internal void CloseCustomCombobox ( object sender, GotFocusEventArgs args )
        {
            _parent = this.Parent.Parent as ModernMainView;
            PersonChoosingUserControl personChoosing = _parent. personChoosing;
            
            if ( personChoosing != null ) 
            {
                personChoosing.CloseCustomCombobox ();
            }
        }


        internal void HandleClosing ( object sender, EventArgs args )
        {
            //TemplateViewModel chosen = templateChoosing.SelectedItem as TemplateViewModel;

            //if ( chosen == null )
            //{
            //    return;
            //}

            //bool templateIsIncorrect = (chosen.Color.Color.A == 100);

            //if ( templateIsIncorrect )
            //{
            //    _vm.ChosenTemplate = null;
            //    _chosen = null;
            //    var messegeDialog = new MessageDialog ();
            //    messegeDialog.Message = _jsonError;
            //    messegeDialog.ShowDialog (MainWindow._mainWindow);
            //    messegeDialog.Focusable = true;
            //    messegeDialog.Focus ();
            //}
            //else 
            //{
            //    _vm.ChosenTemplate = chosen;
            //    _chosen = chosen;
            //}
        }


        //internal void Tapped ( object sender, TappedEventArgs args )
        //{
        //    var templateChoosingVM = App.services.GetRequiredService<TemplateChoosingViewModel> ();

        //    templateChoosingVM.GeneratePdf ();
        //}


        //internal void SetWaiting ( )
        //{
        //    _parent = this.Parent.Parent as ModernMainView;
        //    _parent.Show ();
        //}


        //internal void CancelWaiting ()
        //{
        //    _parent = this.Parent.Parent as ModernMainView;
        //    _parent.Hide ();
        //}


        //internal void BuildAll ( string templateName )
        //{
        //    SceneViewModel sceneVM = App.services.GetRequiredService<SceneViewModel> ();
        //    sceneVM.BuildBadges ( templateName );


        //}


        //internal void BuildSingle ()
        //{
        //    _vm.BuildSingleBadge ();
        //}


        //internal void Pressed ( object sender, PointerPressedEventArgs args )
        //{
        //    SetWaiting ();

        //    Stopwatch sw = Stopwatch.StartNew ();

        //}


        //internal void Released ( object sender, PointerReleasedEventArgs args )
        //{
        //    _vm.Build ();
        //}
    }
}
