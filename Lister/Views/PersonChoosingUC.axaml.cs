using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
using Avalonia.VisualTree;
using ContentAssembler;
using DynamicData;
using Lister.ViewModels;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Lister.Views
{
    public partial class PersonChoosingUserControl : UserControl
    {
        private static SolidColorBrush _unfocusedColor = new SolidColorBrush (MainWindow.white);

        private ModernMainView _parent;
        private readonly int _inputLimit = 100;
        private string _previousText;
        private bool _buttonIsPressed = false;
        private bool _cursorIsOverPersonList = false;
        private bool _personListIsDropped = false;
        private bool _runnerIsCaptured = false;
        private bool _scrollingCausedByTapping = false;
        private bool _runnerShiftCaused = false;
        private double _capturingY;
        private double _shiftScratch = 0;
        private PersonChoosingViewModel _viewModel;
        private string _textBoxText = string.Empty;
        private string _theme;

        private SolidColorBrush _personTBBackground = new SolidColorBrush (MainWindow.black);


        public PersonChoosingUserControl ()
        {
            InitializeComponent ();

            DataContext = App.services.GetRequiredService<PersonChoosingViewModel> ();
            _viewModel = (PersonChoosingViewModel) DataContext;

            Loaded += OnLoaded;
            //ActualThemeVariantChanged += ThemeChanged;

            personTextBox.AddHandler (TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel);
        }


        private void PreventPasting ( object sender, PointerReleasedEventArgs args )
        {
            var point = args.GetCurrentPoint (sender as Control);
            var x = point.Position.X;
            var y = point.Position.Y;

            args.Handled = true;
        }


        internal void AdjustComboboxWidth ( double shift, bool shouldChangeComboboxWidth )
        {
            personTextBox.Width -= shift;
            visiblePersons.Width -= shift;
            listFrame.Width -= shift;

            if ( shouldChangeComboboxWidth ) 
            {
                personList.Width -= shift;
                _viewModel.ShiftScroller (shift);
            }
        }


        internal void AcceptEntirePersonList ( object sender, TappedEventArgs args )
        {
            _viewModel.SetEntireList ();
        }


        internal void CustomComboboxGotFocus ( object sender, GotFocusEventArgs args )
        {
            if ( personTextBox.Text == null )
            {
                return;
            }

            personTextBox.SelectionStart = personTextBox.Text.Length;
            personTextBox.SelectionEnd = personTextBox.Text.Length;
        }

        #region Drop

        internal void CustomComboboxLostFocus ( object sender, RoutedEventArgs args )
        {
            CloseCustomCombobox ();
        }


        internal void CloseCustomCombobox ( )
        {
            _buttonIsPressed = false;

            if ( _personListIsDropped )
            {
                _personListIsDropped = false;
                _viewModel.HideDropListWithoutChange ();
            }
        }


        internal void DropOrPickUpPersonsOrScroll ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();

            if ( key == "Return" )
            {
                DropOrPickUp ();
                return;
            }

            if ( key == "Escape" )
            {
                PickUp ();
                return;
            }

            if ( key == "Up" )
            {
                ScrollByKey (true);
                return;
            }

            if ( key == "Down" )
            {
                ScrollByKey (false);
            }
        }


        internal void DropOrPickUpPersons ( object sender, TappedEventArgs args )
        {
            _buttonIsPressed = true;
            DropOrPickUp ();
            personTextBox.Focus (NavigationMethod.Tab);
        }


        private void DropOrPickUp ()
        {
            if ( personTextBox.Text == null )
            {
                return;
            }

            if ( _personListIsDropped )
            {
                _personListIsDropped = false;
                _viewModel.HideDropListWithChange ();
            }
            else
            {
                _personListIsDropped = true;
                _viewModel.ShowDropDown ();
            }
        }


        private void PickUp ()
        {
            if ( _personListIsDropped )
            {
                _personListIsDropped = false;
                _viewModel.HideDropListWithoutChange ();
            }
        }

        #endregion Drop

        #region PersonListReduction

        internal void HandlePersonListReduction ( object sender, KeyEventArgs args )
        {
            string key = args.Key.ToString ();
            bool keyIsUnimpacting = IsKeyUnimpacting (key);

            if ( keyIsUnimpacting )
            {
                return;
            }

            _viewModel.ToZeroPersonSelection ();
            TextBox textBox = ( TextBox ) sender;
            string input = textBox.Text;

            if ( input != null )
            {
                RestrictInput (input);

                if ( ( input == string.Empty ) )
                {
                    RecoverVisiblePeople ();
                    _personListIsDropped = true;
                    return;
                }

                List <VisiblePerson> foundVisiblePeople = new List <VisiblePerson> ();

                foreach ( VisiblePerson person   in   _viewModel.PeopleStorage )
                {
                    person.BorderBrushColor = _unfocusedColor;
                    string entireName = person.Person. StringPresentation;

                    if ( entireName.Contains (input, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        foundVisiblePeople.Add (person);
                    }
                }

                _viewModel.SetInvolvedPeople (foundVisiblePeople);
                _personListIsDropped = true;
            }
        }


        private void RestrictInput ( string input )
        {
            if ( input.Length >= _inputLimit )
            {
                personTextBox.Text = _previousText;
                input = _previousText;
            }
            else 
            {
                _previousText = input;
            }
        }


        private bool IsKeyUnimpacting ( string key )
        {
            bool keyIsUnimpacting = key == "Tab";
            keyIsUnimpacting = keyIsUnimpacting || ( key == "LeftShift" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "RightShift" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Left" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Up" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Right" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Down" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Return" );
            keyIsUnimpacting = keyIsUnimpacting || ( key == "Escape" );
            return keyIsUnimpacting;
        }


        private void RecoverVisiblePeople ()
        {
            _viewModel.RecoverVisiblePeople ();
            _personListIsDropped = true;
        }

        #endregion PersonListReduction

        #region Choosing

        internal void HandleChoosingByTapping ( object sender, TappedEventArgs args )
        {
            Label chosenLabel = ( Label ) sender;
            string chosenName = ( string ) chosenLabel.Content;
            _viewModel.SetChosenPerson (chosenName);
        }

        #endregion


        #region Scrolling

        internal void ScrollByWheel ( object sender, PointerWheelEventArgs args )
        {
            bool isDirectionUp = args.Delta.Y > 0;
            _viewModel.ScrollByWheel ( isDirectionUp );
        }


        internal void ScrollByTapping ( object sender, PointerPressedEventArgs args )
        {
            Canvas activator = sender as Canvas;
            bool isDirectionUp = (activator.Name == "upper");
            int count = personList.ItemCount;
            _scrollingCausedByTapping = true;
            _viewModel.ScrollByButton ( isDirectionUp, count );
        }


        internal void ShiftRunner ( object sender, PointerPressedEventArgs args )
        {
            Canvas activator = sender as Canvas;
            _shiftScratch = args.GetPosition ( activator ).Y;
            double limit = 0;
            bool isDirectionUp = activator.Name == "topSpan";

            if ( isDirectionUp ) 
            {
                limit = args.GetPosition (activator).Y;
            }
            else 
            {
                limit = bottomSpan.Height - args.GetPosition (activator).Y;
            }

            _runnerShiftCaused = true;
            _viewModel.ShiftRunner ( isDirectionUp, limit );
        }


        internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
        {
            _runnerIsCaptured = true;
            Point inRunnerRelativePosition = args.GetPosition (( Canvas ) args.Source);
            _capturingY = inRunnerRelativePosition.Y;
        }


        internal void ReleaseScrollingLeverage ( )
        {
            if ( _scrollingCausedByTapping    ||   _runnerIsCaptured )
            {
                if ( _runnerIsCaptured )
                {
                    _runnerIsCaptured = false;
                }

                if ( _scrollingCausedByTapping )
                {
                    _scrollingCausedByTapping = false;
                }

                _scrollingCausedByTapping = false;
                _viewModel.StopScrolling ();
            }
            else 
            {
                if ( !_runnerShiftCaused )
                {
                    CloseCustomCombobox ();
                }
                else 
                {
                    _runnerShiftCaused = false;
                }
            }
        }


        internal void MoveRunner ( object sender, PointerEventArgs args )
        {
            if ( _runnerIsCaptured ) 
            {
                Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
                double runnerVerticalDelta = _capturingY - pointerPosition.Y;
                _viewModel.MoveRunner ( runnerVerticalDelta );
            }
        }


        private void ScrollByKey ( bool isDirectionUp )
        {
            _viewModel.ScrollByKey ( isDirectionUp );
        }

        #endregion Scrolling


        internal void HandleClosing ( object sender, EventArgs args )
        {
            TemplateViewModel chosen = templateChoosing.SelectedItem as TemplateViewModel;

            if ( chosen == null )
            {
                return;
            }

            _viewModel.ChosenTemplate = chosen;
        }


        internal void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _theme = ActualThemeVariant.Key.ToString ();
            _viewModel.SetUp (_theme);
        }


        internal void ThemeChanged ( object sender, EventArgs args )
        {
            if ( ActualThemeVariant == null )
            {
                return;
            }

            _theme = ActualThemeVariant.Key.ToString ();
            _viewModel.SetUp (_theme);
        }
    }
}


