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
using SkiaSharp;
using QuestPDF.Helpers;

namespace Lister.ViewModels;

class VMTextBlock 
{
    HorizontalAlignment HorAlignment { get; set; }

    internal VMTextBlock () 
    {
        HorAlignment = HorizontalAlignment.Center;
    }
}