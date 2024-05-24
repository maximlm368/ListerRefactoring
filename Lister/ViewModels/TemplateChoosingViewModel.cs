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


namespace Lister.ViewModels;

internal class TemplateChoosingViewModel : ViewModelBase
{
    private IUniformDocumentAssembler uniformAssembler;

    private List<FileInfo> templatesField;
    internal List<FileInfo> Templates
    {
        get
        {
            return templatesField;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref templatesField, value, nameof (Templates));
        }
    }

    private FileInfo cT;
    internal FileInfo ChosenTemplate
    {
        set
        {
            bool valueIsSuitable = ( value.Name != string.Empty ) && ( value != null );

            if ( valueIsSuitable )
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (ChosenTemplate));
            }
        }
        get
        {
            return cT;
        }
    }


    internal TemplateChoosingViewModel () 
    {
        Templates = uniformAssembler.GetBadgeModels ();
    }
}