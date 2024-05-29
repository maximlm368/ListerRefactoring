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
        internal List <Person> People { get; set; }

        private ObservableCollection <Person> vP;
        internal ObservableCollection <Person> VisiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( VisiblePeople ) );
                SetPersonList (value);
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

        private double plH = 100;
        internal double PersonListHeight
        {
            get { return plH; }
            set
            {
                this.RaiseAndSetIfChanged (ref plH, value, nameof (PersonListHeight));
            }
        }

        private double plW = 454;
        internal double PersonListWidth
        {
            get { return plW; }
            set
            {
                this.RaiseAndSetIfChanged (ref plW, value, nameof (PersonListWidth));
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


        public PersonChoosingViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler
                                         , ContentAssembler.Size pageSize )
        {
            
            VisiblePeople = new ObservableCollection <Person> ( );
            People = new List <Person> ( );
            
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


        private void SetPersonList ( ObservableCollection <Person> persons ) 
        {
            if ( persons == null ) 
            {
                return;
            }

            if ( persons.Count <= 5 ) 
            {
                PersonListHeight = 20 * persons.Count;
                PersonListWidth = 469;
                ScrollerWidth = 0;
            }
            else 
            {
                PersonListHeight = 20 * 5;
                PersonListWidth = 454;
                ScrollerWidth = 15;

                RunnerTopCoordinate = 15;
                //RunnerHeight = 
            }
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