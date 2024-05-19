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
    internal class PersonChoosingViewModel : ViewModelBase
    {
        internal List<Person> people { get; private set; }
        internal List<VMBadge> incorrectBadges { get; private set; }

        private ObservableCollection<Person> vP;
        internal ObservableCollection<Person> visiblePeople
        {
            get { return vP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref vP , value , nameof ( visiblePeople ) );
            }
        }

        private Person cP;
        internal Person chosenPerson
        {
            get { return cP; }
            set
            {
                this.RaiseAndSetIfChanged ( ref cP , value , nameof ( chosenPerson ) );
            }
        }


        internal PersonChoosingViewModel ( )
        {
            
            visiblePeople = new ObservableCollection<Person> ( );
            people = new List<Person> ( );
            incorrectBadges = new List<VMBadge> ( );
        }




    }
}
