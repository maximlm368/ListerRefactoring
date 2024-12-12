using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Lister.Views;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using MessageBox.Avalonia.Views;
using Avalonia.Interactivity;

namespace Lister.Views;

public partial class SplashWindow : BaseWindow
{
    public SplashWindow()
    {
        InitializeComponent();
    }
}