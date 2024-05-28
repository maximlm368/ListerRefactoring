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
        internal List <Person> People { get; private set; }

        private ObservableCollection <Person> vP;
        internal ObservableCollection <Person> VisiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( VisiblePeople ) );
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

        private double tS;
        internal double PeopleTopShift
        {
            get { return tS; }
            set
            {
                this.RaiseAndSetIfChanged (ref tS, value, nameof (PeopleTopShift));
            }
        }

        private double plH = 89;
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


        public PersonChoosingViewModel ( IUniformDocumentAssembler singleTypeDocumentAssembler
                                         , ContentAssembler.Size pageSize )
        {
            
            VisiblePeople = new ObservableCollection <Person> ( );
            People = new List <Person> ( );
            
        }


        internal Person ? FindPersonByStringPresentation ( string presentation ) 
        {
            if ( string.IsNullOrWhiteSpace(presentation) ) 
            {
                return null;
            }

            Person result = null;

            foreach ( Person person  in  VisiblePeople ) 
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

    }
}
