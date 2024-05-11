﻿using Avalonia;
using Avalonia.Controls;
using ContentAssembler;
using Lister.ViewModels;
using System.Runtime.InteropServices;

namespace Lister.Views;

public partial class MainWindow : Window
{
    private PixelSize screenSize;
    private double currentWidth;

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
        this.SizeChanged += OnSizeChanged;
        currentWidth = this.Width;
        
        
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


    private void OnSizeChanged ( object? sender , SizeChangedEventArgs e )
    {
        MainView mainView = ( MainView ) Content;
        double newWidth = e.NewSize.Width;
        double difference = currentWidth - newWidth;
        currentWidth = newWidth;
        mainView.personChoosing.Width -= difference;
        //mainView.personTypping.Width -= difference;
    }
}



[StructLayout ( LayoutKind.Sequential )]
public struct POINT { public int x; public int y; }
