using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ContentAssembler;
using Lister.ViewModels;

namespace Lister.Views;

public partial class MainWindow : Window
{
    private PixelSize screenSize;
    private double currentWidth;
    private double currentHeight;


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
        ///


        this.Opened += OnOpened;
        this.Content = new MainView( this,  docAssembler);
        this.SizeChanged += OnSizeChanged;
        currentWidth = this.Width;
        currentHeight = this.Height;
        this.Tapped += HandleTapping;
    }


    internal void SetWidth (int width) 
    {
        MainView mainView = ( MainView ) Content;
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


    private void OnSizeChanged ( object? sender , SizeChangedEventArgs e )
    {
        MainView mainView = ( MainView ) Content;
        double newWidth = e.NewSize.Width;
        double newHeight = e.NewSize.Height;
        double widthDifference = currentWidth - newWidth;
        double heightDifference = currentHeight - newHeight;
        currentWidth = newWidth;
        currentHeight = newHeight;
        mainView.personList.Width -= widthDifference;
        mainView.personTyping.Width -= widthDifference;
        mainView.comboboxFrame.Width -= widthDifference;
        mainView.workArea.Height -= heightDifference;
    }


    internal void HandleTapping ( object sender, TappedEventArgs args )
    {
        MainView mainView = ( MainView ) Content;

        //POINT cursorCoordinates = new POINT ();
        //CursorViaWinapi.GetCursorPos (ref cursorCoordinates);
        //int coordinateX = cursorCoordinates.x;
        //int coordinateY = cursorCoordinates.y;

        //ContentAssembler.Size pointOfReference = new ContentAssembler.Size (99, 63);
        //int x = 99;
        //int y = 63;

        //ContentAssembler.Size targetSize = mainView.GetCustomComboboxDimensions ();
        //int targetWidth = ( int ) targetSize.width;
        //int targetHeight = ( int ) targetSize.height;

        //bool cursorIsOutsideTarget = coordinateX < ( x );
        //cursorIsOutsideTarget = cursorIsOutsideTarget && ( coordinateX > ( x + targetWidth ) );
        //cursorIsOutsideTarget = cursorIsOutsideTarget && ( coordinateY < y );
        //cursorIsOutsideTarget = cursorIsOutsideTarget && ( coordinateY > ( y + targetHeight ) );

        //if ( cursorIsOutsideTarget )
        //{
        //    mainView.CloseCustomCombobox ();
        //}

        mainView.CloseCustomCombobox ();
    }
}
