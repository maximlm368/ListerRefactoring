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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
using static Lister.ViewModels.MainViewModel;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;

namespace Lister.ViewModels
{
    public class PersonChoosingViewModel : ViewModelBase
    {
        private double _withScroll = 454;
        private double _withoutScroll = 469;
        internal double WidthDelta { get; set; }

        private double _oneHeight;
        internal List <Person> People { get; set; }

        private ObservableCollection <Person> vP;
        internal ObservableCollection <Person> VisiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( VisiblePeople ) );
                SetPersonList ();
            }
        }

        private Person cP;
        internal Person ChosenPerson
        {
            get { return cP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref cP , value , nameof ( ChosenPerson ) );
            }
        }

        private double vH;
        internal double VisibleHeight
        {
            get { return vH; }
            set
            {
                this.RaiseAndSetIfChanged (ref vH, value, nameof (VisibleHeight));
            }
        }

        private double plW;
        internal double PersonListWidth
        {
            get { return plW; }
            set
            {
                this.RaiseAndSetIfChanged (ref plW, value, nameof (PersonListWidth));
            }
        }

        private double plH;
        internal double PersonListHeight
        {
            get { return plH; }
            set
            {
                this.RaiseAndSetIfChanged (ref plH, value, nameof (PersonListHeight));
            }
        }

        private double psV;
        internal double PersonsScrollValue
        {
            get { return psV; }
            set
            {
                this.RaiseAndSetIfChanged (ref psV, value, nameof (PersonsScrollValue));
            }
        }

        private double rH;
        internal double RunnerHeight
        {
            get { return rH; }
            set
            {
                this.RaiseAndSetIfChanged (ref rH, value, nameof (RunnerHeight));
            }
        }

        private double rTC;
        internal double RunnerTopCoordinate
        {
            get { return rTC; }
            set
            {
                this.RaiseAndSetIfChanged (ref rTC, value, nameof (RunnerTopCoordinate));
            }
        }

        private double tSH;
        internal double TopSpanHeight
        {
            get { return tSH; }
            set
            {
                this.RaiseAndSetIfChanged (ref tSH, value, nameof (TopSpanHeight));
            }
        }

        private double bSH;
        internal double BottomSpanHeight
        {
            get { return bSH; }
            set
            {
                this.RaiseAndSetIfChanged (ref bSH, value, nameof (BottomSpanHeight));
            }
        }

        private double sW;
        internal double ScrollerWidth
        {
            get { return sW; }
            set
            {
                this.RaiseAndSetIfChanged (ref sW, value, nameof (ScrollerWidth));
            }
        }

        private double sCL;
        internal double ScrollerCanvasLeft
        {
            get { return sCL; }
            set
            {
                this.RaiseAndSetIfChanged (ref sCL, value, nameof (ScrollerCanvasLeft));
            }
        }

        private bool isPS;
        public bool IsPersonsScrollable
        {
            get { return isPS; }
            set
            {
                this.RaiseAndSetIfChanged (ref isPS, value, nameof (IsPersonsScrollable));
            }
        }


        public PersonChoosingViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler )
        {
            _oneHeight = 24;
            ScrollerCanvasLeft = 454;
            VisiblePeople = new ObservableCollection<Person> ();
            People = new List<Person> ();
        }


        internal void ShiftScroller ( double shift )
        {
            ScrollerCanvasLeft -= shift;
        }


        internal Person ? FindPersonByStringPresentation ( string presentation ) 
        {
            if ( string.IsNullOrWhiteSpace (presentation) ) 
            {
                return null;
            }

            Person result = null;

            foreach ( Person person   in   VisiblePeople ) 
            {
                bool isIntresting = person.IsMatchingTo (presentation);
                
                if ( isIntresting ) 
                {
                    result = person;
                    break;
                }
            }

            return result;
        }


        private void SetPersonList ( ) 
        {
            int personCount = VisiblePeople. Count;
            PersonListHeight = _oneHeight * personCount;

            if ( personCount > 5 )
            {
                ShowScroller ();
            }
            else
            {
                VisibleHeight = _oneHeight * personCount;
                PersonListWidth = _withoutScroll - WidthDelta;
                ScrollerWidth = 0;
                IsPersonsScrollable = false;
            }
        }


        private void ShowScroller () 
        {
            VisibleHeight = _oneHeight * 5;
            PersonListWidth = _withScroll - WidthDelta;
            ScrollerWidth = 15;

            double scrollerWorkAreaHeight = VisibleHeight - (ScrollerWidth * 2);
            double proportion = PersonListHeight / scrollerWorkAreaHeight;
            RunnerHeight = VisibleHeight / proportion;

            if (RunnerHeight < 2) 
            {
                RunnerHeight = 2;
            }

            RunnerTopCoordinate = 15;
            TopSpanHeight = 0;
            BottomSpanHeight = scrollerWorkAreaHeight - RunnerHeight;
            PersonsScrollValue = 0;
            IsPersonsScrollable = true;
        }

        
    }
}




//private double tS;
//internal double PeopleTopShift
//{
//    get { return tS; }
//    set
//    {
//        this.RaiseAndSetIfChanged (ref tS, value, nameof (PeopleTopShift));
//    }
//}


//  private double plH = 89;