using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Drawing;
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
using Avalonia.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using DataGateway;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Lister.ViewModels
{
    public partial class PersonChoosingViewModel : ViewModelBase
    {
        private readonly int _inputLimit = 50;

        private bool _fileNotFound;
        public bool FileNotFound
        {
            get { return _fileNotFound; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _fileNotFound, value, nameof (FileNotFound));
            }
        }


        public PersonChoosingViewModel ( string placeHolder, SolidColorBrush entireListColor
                                        , SolidColorBrush incorrectTemplateColor, List <SolidColorBrush> defaultColors
                                        , List <SolidColorBrush> focusedColors, List <SolidColorBrush> selectedColors )
        {
            _placeHolder = placeHolder;
            _entireListColor = entireListColor;
            _incorrectTemplateForeground = incorrectTemplateColor;

            _defaultBackgroundColor = defaultColors [0];
            _defaultBorderColor = defaultColors [1];
            _defaultForegroundColor = defaultColors [2];

            _selectedBackgroundColor = selectedColors [0];
            _selectedBorderColor = selectedColors [1];
            _selectedForegroundColor = selectedColors [2];

            _focusedBackgroundColor = focusedColors [0];
            _focusedBorderColor = focusedColors [1];

            VisiblePeople = new ObservableCollection<VisiblePerson> ();
            ScrollerCanvasLeft = _withScroll;
            PersonsScrollValue = _oneHeight;
            TextboxIsReadOnly = true;
            TextboxIsFocusable = false;

            _focusedEdge = _edge;

            IBadgeAppearenceProvider badgeAppearenceProvider = App.services.GetRequiredService<IBadgeAppearenceProvider> ();

            if ( _badgeLayouts == null )
            {
                _badgeLayouts = badgeAppearenceProvider.GetBadgeLayouts ();
            }

            ChosenTemplatePadding = new Thickness (4, 0);
        }


        internal void RefreshTemplateChoosingAppearence ()
        {
            if ( ChosenTemplate != null )
            {
                ChosenTemplateColor = ChosenTemplate.Color;
            }
        }


        internal void SetPersonsFromFile ( string ? path )
        {
            bool valueIsSuitable = ! string.IsNullOrWhiteSpace (path);

            if ( valueIsSuitable )
            {
                IUniformDocumentAssembler documentAssembler = App.services.GetService<IUniformDocumentAssembler> ();

                try
                {
                    List <Person> persons = documentAssembler.GetPersons (path);
                    SetPersonsFromNewSource (persons);
                    SwitchPersonChoosingEnabling (true);
                }
                catch ( Exception ex ) 
                {
                    FileNotFound = true;
                    SetPersonsFromNewSource (null);
                    SwitchPersonChoosingEnabling (false);
                }
            }
            else
            {
                SetPersonsFromNewSource (null);
                SwitchPersonChoosingEnabling (false);
            }
        }


        private void SwitchPersonChoosingEnabling ( bool shouldEnable )
        {
            TextboxIsReadOnly = ! shouldEnable;
            TextboxIsFocusable = shouldEnable;
        }


        #region DropDown

        internal void HideDropListWithChange ()
        {
            if ( _selected != null )
            {
                _selected.IsSelected = false;
                _selected = null;
            }

            if ( _focused == null )
            {
                if ( !_chosenPersonIsSetInSetter )
                {
                    ChosenPerson = null;
                }

                PlaceHolder = _placeHolder;
            }
            else
            {
                //if ( !_chosenPersonIsSetInSetter )
                //{
                //    ChosenPerson = _focused.Person;
                //}
                
                ChosenPerson = _focused.Person;
                PlaceHolder = ChosenPerson.StringPresentation;

                _selected = _focused;
                _selected.IsSelected = true;
                _focused.IsFocused = true;
            }

            _chosenPersonIsSetInSetter = false;
            HideDropListWithoutChange ();
            SetReadiness ();
        }


        internal void HideDropListWithoutChange ()
        {
            if ( ( InvolvedPeople. Count == 0 )   &&   ( PeopleStorage. Count > 0 ) )
            {
                RecoverVisiblePeople ();
                EntireFontWeight = FontWeight.Bold;
                PlaceHolder = _placeHolder;
                ChosenPerson = null;
            }

            DropDownOpacity = 0;
            VisibleHeight = 0;
            FirstItemHeight = 0;
            FirstIsVisible = false;
        }


        internal void ShowDropDown ()
        {
            DropDownOpacity = 1;
            VisibleHeight = _visibleHeightStorage;
            FirstIsVisible = _allListMustBe;

            if ( _allListMustBe )
            {
                FirstItemHeight = _scrollingScratch;
            }
        }
        #endregion


        private void SetPersonsFromNewSource ( List <Person> ? persons )
        {
            if ( persons == null )
            {
                return;
            }

            List <VisiblePerson> peopleStorage = new ();
            List <VisiblePerson> involvedPeople = new ();

            foreach ( Person person   in   persons )
            {
                if ( person.IsEmpty () )
                {
                    continue;
                }

                VisiblePerson visiblePerson = new VisiblePerson (person);
                peopleStorage.Add (visiblePerson);
                involvedPeople.Add (visiblePerson);
            }

            PeopleStorage = peopleStorage;
            InvolvedPeople = involvedPeople;
            SinglePersonIsSelected = false;
            ChosenPerson = null;
        }


        internal void SetChosenPerson ( string personName )
        {
            Person person = FindPersonByStringPresentation (personName);
            int seekingScratch = _focusedNumber - _maxVisibleCount;

            if ( seekingScratch < 0 )
            {
                seekingScratch = 0;
            }

            int seekingEnd = _focusedNumber + _maxVisibleCount;

            if ( seekingScratch > InvolvedPeople.Count )
            {
                seekingScratch = InvolvedPeople.Count;
            }

            for ( int index = seekingScratch;   index <= seekingEnd;   index++ )
            {
                VisiblePerson foundPerson = InvolvedPeople [index];
                bool isCoincidence = person.Equals (foundPerson.Person);

                if ( isCoincidence )
                {
                    if ( _focused != null )
                    {
                        _focused.IsFocused = false;
                        _focused.IsSelected = false;
                    }
                    else
                    {
                        _entireIsSelected = false;
                    }

                    SetSelectedToNull ();
                    _focused = foundPerson;
                    
                    _focusedNumber = index;
                    PlaceHolder = personName;
                    EntireFontWeight = FontWeight.Normal;
                    HideDropListWithChange ();
                    break;
                }
            }
        }


        internal void SetEntireList ()
        {
            if ( _focused != null )
            {
                _focused.IsFocused = false;
                _focused = null;
            }

            if ( _selected != null )
            {
                _selected.IsSelected = false;
                _selected = null;
            }

            EntirePersonListIsSelected = true;
            HideDropListWithChange ();
            _focusedNumber = _focusedEdge - _maxVisibleCount;
        }


        private void SetPersonChoosingConsequences ()
        {
            EntirePersonListIsSelected = false;
            SinglePersonIsSelected = true;
        }


        private void SetEntireListChoosingConsequences ()
        {
            EntirePersonListIsSelected = true;
            SinglePersonIsSelected = false;
        }


        internal void ShiftScroller ( double shift )
        {
            _widthDelta += shift;
            ScrollerCanvasLeft -= shift;
            PersonListWidth -= shift;
            _withoutScroll -= shift;
            _withScroll -= shift;
        }


        internal Person ? FindPersonByStringPresentation ( string presentation )
        {
            if ( string.IsNullOrWhiteSpace (presentation) )
            {
                return null;
            }

            Person result = null;

            foreach ( VisiblePerson person   in   InvolvedPeople )
            {
                bool isIntresting = person.Person.IsMatchingTo (presentation);

                if ( isIntresting )
                {
                    result = person.Person;
                    break;
                }
            }

            return result;
        }


        private void SetPersonList ()
        {
            if ( ( InvolvedPeople != null ) && ( InvolvedPeople.Count > 0 ) )
            {
                int count = InvolvedPeople.Count;
                PersonListHeight = _oneHeight * count;
                bool listIsWhole = ( count == PeopleStorage.Count );
                _scrollValue = 0;

                if ( listIsWhole )
                {
                    SetWholeList (count);
                }
                else
                {
                    SetCutDownList (count);
                }

                _topLimit = _focusedNumber;
                SetScrollerIfShould ();
            }
            else
            {
                FirstItemHeight = 0;
                FirstIsVisible = false;
                _allListMustBe = false;
                _visibleHeightStorage = 0;
                PersonListWidth = 0;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
                PersonsScrollValue = 0;
            }

            SetReadiness ();
        }


        private void SetWholeList ( int personCount )
        {
            _visibleHeightStorage = _oneHeight * ( Math.Min (_maxVisibleCount, personCount) + 1 );
            FirstIsVisible = true;
            _allListMustBe = true;
            FirstItemHeight = _scrollingScratch;
            PersonsScrollValue = _scrollingScratch;

            if ( _focused != null ) 
            {
                _focused.IsFocused = false;
                _focused.IsSelected = false;
                _focused = null;
            }

            _entireIsSelected = true;
            _focusedNumber = -1;
            _focusedEdge = _edge;

            TextboxIsReadOnly = false;
            TextboxIsFocusable = true;

            PlaceHolder = _placeHolder;

            EntireFontWeight = FontWeight.Bold;

            SetVisiblePeople (0);
        }


        private void SetCutDownList ( int personCount )
        {
            FirstItemHeight = 0;
            FirstIsVisible = false;
            _allListMustBe = false;

            _visibleHeightStorage = _oneHeight * Math.Min (_maxVisibleCount, personCount);
            EntirePersonListIsSelected = false;
            PersonsScrollValue = 0;

            _focusedNumber = 0;
            _focused = InvolvedPeople [_focusedNumber];

            _focused.IsFocused = true;

            _focusedEdge = _edge;

            SetVisiblePeople (0);
        }


        private void SetVisiblePeople ( int scratch )
        {
            VisiblePeople.Clear ();
            int limit = Math.Min (InvolvedPeople.Count, _maxVisibleCount);

            for ( int index = 0;   index < limit;   index++ )
            {
                int personIndex = scratch + index;
                VisiblePeople.Add (InvolvedPeople [personIndex]);
            }
        }


        internal void SetInvolvedPeople ( List<VisiblePerson> involvedPeople )
        {
            SetSelectedToNull ();
            InvolvedPeople = involvedPeople;
            _scrollValue = 0;
            ShowDropDown ();
        }


        private void SetScrollerIfShould ()
        {
            int personCount = InvolvedPeople.Count;

            if ( personCount > _maxVisibleCount )
            {
                PersonListWidth = _withScroll - _widthDelta;
                ScrollerWidth = _upperHeight;

                double scrollerWorkAreaHeight = _visibleHeightStorage - ( ScrollerWidth * 2 );
                double proportion = PersonListHeight / scrollerWorkAreaHeight;
                RunnerHeight = _visibleHeightStorage / proportion;
                RealRunnerHeight = RunnerHeight;

                if ( RunnerHeight < _minRunnerHeight )
                {
                    RunnerHeight = _minRunnerHeight;
                }

                RunnerTopCoordinate = _upperHeight;
                TopSpanHeight = 0;
                BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
                _runnerStep = BottomSpanHeight / ( InvolvedPeople.Count - _maxVisibleCount );
                _scrollingLength = BottomSpanHeight;
                IsPersonsScrollable = true;
            }
            else
            {
                PersonListWidth = _withoutScroll - _widthDelta;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
            }
        }


        private void SetReadiness ()
        {
            if ( ( InvolvedPeople != null ) && ( InvolvedPeople.Count > 0 ) )
            {
                bool areReady = ( SinglePersonIsSelected || EntirePersonListIsSelected ) && ( ChosenTemplate != null );

                if ( areReady )
                {
                    AllAreReady = true;
                }
                else
                {
                    AllAreReady = false;
                }
            }
        }


        internal void ToZeroPersonSelection ()
        {
            SinglePersonIsSelected = false;
            EntirePersonListIsSelected = false;

            AllAreReady = false;
        }


        internal void ReductPersonList ( string input )
        {
            ToZeroPersonSelection ();
            RestrictInput (input);

            if ( ( input == string.Empty ) )
            {
                RecoverVisiblePeople ();
                return;
            }

            List <VisiblePerson> foundVisiblePeople = new List <VisiblePerson> ();

            foreach ( VisiblePerson person   in   PeopleStorage )
            {
                person.IsFocused = false;

                string entireName = person.Person.StringPresentation;

                if ( entireName.Contains (input, StringComparison.CurrentCultureIgnoreCase) )
                {
                    foundVisiblePeople.Add (person);
                }
            }

            SetInvolvedPeople (foundVisiblePeople);
        }


        private void RestrictInput ( string input )
        {
            if ( input.Length > _inputLimit )
            {
                string ph = PlaceHolder;
                PlaceHolder = "";
                PlaceHolder = ph;
            }
            else
            {
                PlaceHolder = input;
            }
        }


        internal void RecoverVisiblePeople ()
        {
            SetSelectedToNull ();
            _scrollValue = _scrollingScratch;
            List<VisiblePerson> recovered = new List<VisiblePerson> ();

            foreach ( VisiblePerson person   in   PeopleStorage )
            {
                person.IsFocused = false;
                recovered.Add (person);
            }

            InvolvedPeople = recovered;
            ShowDropDown ();
        }


        private void SetSelectedToNull ()
        {
            if ( _selected != null )
            {
                _selected.IsSelected = false;
                _selected = null;
            }
        }


        internal void SetUp ( string theme )
        {
            SolidColorBrush correctColor = _defaultForegroundColor;
            SolidColorBrush incorrectColor = _incorrectTemplateForeground;

            if ( theme == "Dark" )
            {
                correctColor = _defaultBackgroundColor;
                incorrectColor = new SolidColorBrush (new Avalonia.Media.Color (100, 255, 255, 255));
            }

            ObservableCollection <TemplateViewModel> templates = new ();

            foreach ( KeyValuePair<BadgeLayout, KeyValuePair<string, List<string>>> layout   in   _badgeLayouts )
            {
                KeyValuePair<string, List<string>> sourceAndErrors = layout.Value;
                bool correctLayoutHasEmptyMessage = ( sourceAndErrors.Value.Count < 1 );
                List<string> errors = layout.Value.Value;
                string source = layout.Value.Key;

                templates.Add (new TemplateViewModel (new TemplateName (layout.Key.TemplateName, correctLayoutHasEmptyMessage)
                                                      , correctColor, incorrectColor, errors, source));
            }

            Templates = templates;
        }
    }



    public class TemplateName
    {
        public string Name { get; private set; }
        public bool IsFound { get; private set; }


        public TemplateName ( string name, bool isFound )
        {
            Name = name;
            IsFound = isFound;
        }
    }



    public class TemplateViewModel : ViewModelBase
    {
        private TemplateName TemplateName { get; set; }
        public string SourcePath { get; private set; }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                this.RaiseAndSetIfChanged (ref _name, value, nameof (Name));
            }
        }

        private SolidColorBrush _color;
        public SolidColorBrush Color
        {
            get
            {
                return _color;
            }
            set
            {
                this.RaiseAndSetIfChanged (ref _color, value, nameof (Color));
            }
        }

        private List<string> _correctnessMessage;
        public List<string> CorrectnessMessage
        {
            get
            {
                return _correctnessMessage;
            }
            set
            {
                _correctnessMessage = value;
            }
        }


        public TemplateViewModel ( TemplateName templateName, SolidColorBrush colorIfCorrect, SolidColorBrush colorIfIncorrect
                                  , List<string> correctnessMessage, string sourcePath )
        {
            TemplateName = templateName;
            Name = templateName.Name;
            CorrectnessMessage = correctnessMessage;
            SourcePath = sourcePath;

            bool isCorrect = ( correctnessMessage.Count == 0 );

            if ( isCorrect )
            {
                Color = colorIfCorrect;
            }
            else 
            {
                Color = colorIfIncorrect;
            }
        }
    }
}


