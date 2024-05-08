using Avalonia;
using Avalonia.Controls;
using ContentAssembler;
using Lister.ViewModels;

namespace Lister.Views;

public partial class MainWindow : Window
{
    private PixelSize screenSize;

    public MainWindow (IUniformDocumentAssembler docAssembler)
    {
        InitializeComponent();

        //// Adjust the window size based on screen resolution
        //double screenWidth = SystemParameters.PrimaryScreenWidth;
        //double screenHeight = SystemParameters.PrimaryScreenHeight;
        //double desiredWidth = screenWidth * 0.8;  // 80% of screen width
        //double desiredHeight = screenHeight * 0.8; // 80% of screen height

        //Width = desiredWidth;
        //Height = desiredHeight;

        //// Adjust the size of controls inside the window
        //double scaleFactor = Math.Min (screenWidth, screenHeight) / 1920; // Define a scaling factor based on a reference screen resolution
        //button.FontSize *= scaleFactor;
        //textBox.FontSize *= scaleFactor;
        //// ... Adjust other controls accordingly

        this.Opened += OnOpened;
        this.Content = new MainView( this,  docAssembler);
        
    }


    internal void SetWidth (int width) 
    {
        MainView mainView = (MainView) Content;
        mainView.SetWidth (width);
    }


    internal void SetSize ( PixelSize size )
    {
        screenSize = size;
    }


    private void OnOpened ( object? sender, EventArgs e )
    {
        int windowWidth = ( int ) this.DesiredSize.Width / 2;
        int windowHeight = ( int ) this.DesiredSize.Height / 2;
        int x = ( screenSize.Width - windowWidth ) / 2;
        int y = ( screenSize.Height - windowHeight ) / 2;
        //this.Position = new Avalonia.PixelPoint (x, y);
        int wqw = 0;
    }
}
