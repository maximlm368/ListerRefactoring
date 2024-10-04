using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ContentAssembler;
using ReactiveUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static QuestPDF.Helpers.Colors;
using Lister.Extentions;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Layout;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lister.ViewModels;

public class ViewModelBase : ReactiveObject
{
    //public event PropertyChangedEventHandler? PropertyChanged;

    ////[NotifyPropertyChangedInvocator]
    //protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    //{
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}
}


//class VMPerson
//{
//    internal string entireName { get; private set; }
//    internal Person person { get; private set; }

//    internal VMPerson(Person person)
//    {
//        entireName = person.lastName + " " + person.firstName + " " + person.middleName;
//        this.person = person;
//    }
//}


 





//protected virtual void OnPropertyChanged ( string propertyName )
//{
//    if ( PropertyChanged != null )
//    {
//        PropertyChanged?.Invoke (this, new System.ComponentModel.PropertyChangedEventArgs (propertyName));
//    }
//    else
//    {
//        throw new Exception ("empty delegate in VMBadge  " + propertyName);
//    }
//}