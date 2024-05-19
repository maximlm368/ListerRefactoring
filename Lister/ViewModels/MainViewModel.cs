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


namespace Lister.ViewModels;

class MainViewModel : ViewModelBase
{
    private IUniformDocumentAssembler uniformAssembler;
    ConverterToPdf converter;
    internal List<Person> people { get; private set; }
    internal List<VMBadge> incorrectBadges { get; private set; }

    private ObservableCollection<Person> vP;
    internal ObservableCollection<Person> visiblePeople 
    {
        get {  return vP; }
        set 
        {
            this.RaiseAndSetIfChanged (ref vP, value, nameof (visiblePeople));
        }
    }

    private Person cP;
    internal Person chosenPerson 
    {
        get {  return cP; }
        set 
        {
            this.RaiseAndSetIfChanged (ref cP, value, nameof (chosenPerson));
        }
    }

    private List<FileInfo> templatesField;
    internal List<FileInfo> templates 
    {
        get 
        {
            return templatesField;
        }
        set 
        {
            this.RaiseAndSetIfChanged (ref templatesField, value, nameof (templates));
        }
    }

    private FileInfo cT;
    internal FileInfo chosenTemplate 
    {
        set 
        {
            bool valueIsSuitable = (value.Name != string.Empty)    &&    (value != null);

            if (valueIsSuitable)
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (chosenTemplate));
            }
        }
        get 
        {
            return cT;
        }
    }

    private string sFP;
    internal string sourceFilePath
    {
        get { return sFP; }
        set
        {
            string path = SetPersonsFilePath ( value );
            this.RaiseAndSetIfChanged ( ref sFP , path , nameof ( sourceFilePath ) );
        }
    }

    private List<VMPage> allPages;
    private VMPage lastPage;
    private VMPage vPage;
    internal VMPage visiblePage
    {
        get { return vPage; }
        set
        {
            this.RaiseAndSetIfChanged (ref vPage, value, nameof (visiblePage));
        }
    }

    private int vpN;
    internal int visiblePageNumber
    {
        get { return vpN; }
        set
        {
            this.RaiseAndSetIfChanged (ref vpN, value, nameof (visiblePageNumber));
        }
    }

    private string zoomDV;
    internal string zoomDegreeInView
    {
        get { return zoomDV; }
        set
        {
            this.RaiseAndSetIfChanged (ref zoomDV, value, nameof (zoomDegreeInView));
        }
    }
    private double zoomDegree;

    private ContentAssembler.Size pageSize;
    private string procentSymbol;
    private double documentScale;
    private double scalabilityCoefficient;
    private List<int> scaleCorrespondingPages;

    private int sw;
    internal int screenWidth
    {
        get { return sw; }
        set
        {
            this.RaiseAndSetIfChanged (ref sw, value, nameof (screenWidth));
        }
    }


    internal MainViewModel (IUniformDocumentAssembler singleTypeDocumentAssembler,  ContentAssembler.Size pageSize) 
    {
        this.uniformAssembler = singleTypeDocumentAssembler;
        converter = new ConverterToPdf ();
        templates = uniformAssembler.GetBadgeModels();
        visiblePeople = new ObservableCollection<Person>();
        people = new List<Person>();
        incorrectBadges = new List<VMBadge> ();
        allPages = new List<VMPage>();
        this.pageSize = pageSize;
        visiblePageNumber = 1;
        procentSymbol = "%";
        zoomDegree = 100;
        zoomDegreeInView = zoomDegree.ToString () + " " + procentSymbol;
        documentScale = 1;
        scalabilityCoefficient = 1.25;
        scaleCorrespondingPages = new List<int> ();

        
    }


    internal int GetPageCount () 
    {
        return allPages.Count;
    }


    internal string SetPersonsFilePath ( string value )
    {
        bool valueIsSuitable = ( value != null ) && ( value != string.Empty );

        if ( valueIsSuitable )
        {
            visiblePeople.Clear ( );
            people.Clear ( );

            try
            {
                List<Person> persons = uniformAssembler.GetPersons ( value );

                foreach ( var person in persons )
                {
                    visiblePeople.Add ( person );
                    people.Add ( person );
                }

                return value;
            }
            catch ( IOException ex )
            {
                int idOk = Winapi.MessageBox ( 0 , "Выбраный файл открыт в другом приложении. Закройте его." , "" , 0 );
                return string.Empty;
            }
        }

        return string.Empty;
    }


    internal void SetWidth ( int screenWidth )
    {
        this.screenWidth = screenWidth;
    }


    internal Task<bool> GeneratePdf ( string fileToSave )
    {
        List<VMBadge> allBadges = GetAllBadges ();
        Task<bool> task = new Task<bool> (() => { return converter.SaveAsFile (allBadges, fileToSave); });
        task.Start ();
        return task;
    }


    public void Print ( )
    {
        List<VMBadge> allBadges = GetAllBadges();
        string fileToSave = @"intermidiate.pdf";
        Task pdf = new Task (() => { converter.SaveAsFile (allBadges, fileToSave); });
        pdf.Start ();
        pdf.ContinueWith
               (
                  savingTask =>
                  {
                      int length = converter. intermidiateFiles.Count;

                      ProcessStartInfo info = new ()
                      {
                          FileName = fileToSave,
                          Verb = "Print",
                          UseShellExecute = true,
                          ErrorDialog = false,
                          CreateNoWindow = true,
                          WindowStyle = ProcessWindowStyle.Minimized
                      };

                      Process.Start (info)?.WaitForExit (20_000);
                      File.Delete (fileToSave);
                  }
               );
    }


    private List<VMBadge> GetAllBadges () 
    {
        List<VMBadge> allBadges = new List<VMBadge> ();

        for ( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ )
        {
            int badgePairCounter = 0;

            while ( true )
            {
                try
                {
                    allBadges.Add (allPages [pageCounter].evenBadges [badgePairCounter]);
                    allBadges.Add (allPages [pageCounter].oddBadges [badgePairCounter]);
                    badgePairCounter++;
                }
                catch ( ArgumentOutOfRangeException e )
                {
                    break;
                }
            }
        }

        return allBadges;
    }


    //internal void PrintViaWinapi ()
    //{
    //    bool badgesNotCreated = ( converter.bytes == null );

    //    if ( badgesNotCreated )
    //    {
    //        GeneratePdf ();
    //    }

    //    IEnumerable<byte []> printableData = converter.bytes;

    //    PRINTDLGA pd = new PRINTDLGA ();
    //    pd.lStructSize = Marshal.SizeOf<PRINTDLGA> ();
    //    IntPtr unmanagedPrintDlg = Marshal.AllocHGlobal (pd.lStructSize);
    //    Marshal.StructureToPtr (pd, unmanagedPrintDlg, false);

    //    bool isDialogSuccessfull = PrintDlg (unmanagedPrintDlg);
    //    pd = Marshal.PtrToStructure<PRINTDLGA> (unmanagedPrintDlg);
    //    int error = GetLastError ();

    //    DOCINFOA docInfo = new DOCINFOA ();
    //    docInfo.cbSize = Marshal.SizeOf<DOCINFOA> ();
    //    docInfo.lpszDocName = Marshal.StringToHGlobalUni ("My Document");
    //    docInfo.lpszOutput = IntPtr.Zero;
    //    docInfo.lpszDatatype = Marshal.StringToHGlobalUni ("RAW");
    //    docInfo.fwType = 0;

    //    IntPtr pointerToDocInfo = Marshal.AllocHGlobal (docInfo.cbSize);
    //    Marshal.StructureToPtr (docInfo, pointerToDocInfo, false);

    //    int res = StartDocA (pd.hDC, pointerToDocInfo);
    //    error = GetLastError ();

    //    foreach ( byte [] pageBytes in printableData )
    //    {

    //        var handlePointer = pd.hDC;
    //        error = 0;
    //        res = StartPage (pd.hDC);
    //        error = GetLastError ();

    //        //BITMAPINFO bitmapInfo = new BITMAPINFO ();
    //        //int bitmapinfoSize = Marshal.SizeOf<BITMAPINFO> ();

    //        //57 data start
    //        //49 data length

    //        //byte [] length = pageBytes.SubArray (49, 4);
    //        byte [] length = pageBytes.SubArray (8253, 4);
    //        int bigEndLength = BitConverter.ToInt32 (length, 0);
    //        int dataLength = BinaryPrimitives.ReverseEndianness (bigEndLength);






    //        byte first = pageBytes [8257];
    //        first = pageBytes [8258];
    //        first = pageBytes [8259];
    //        first = pageBytes [8260];




    //        IntPtr pointerToBitmapFileHeader = Marshal.AllocHGlobal (14);
    //        Marshal.Copy (pageBytes, 0, pointerToBitmapFileHeader, 14);



    //        BITMAPFILEHEADER bitmapFileHeader = 
    //                (BITMAPFILEHEADER) Marshal.PtrToStructure(pointerToBitmapFileHeader, typeof(BITMAPFILEHEADER));
    //        short type = bitmapFileHeader.bfType;
    //        int size = bitmapFileHeader.bfSize;
    //        int bitsStartAdress = (int) bitmapFileHeader.bfOffBits;

    //        IntPtr pointerToBitmapInfo = Marshal.AllocHGlobal (56);
    //        Marshal.Copy (pageBytes, 14, pointerToBitmapInfo, 56);
    //        BITMAPINFOHEADER bi = new BITMAPINFOHEADER ();
    //        Marshal.PtrToStructure (pointerToBitmapInfo, bi);

    //        IntPtr pointerToBits = Marshal.AllocHGlobal (pageBytes.Length - bitsStartAdress);
    //        Marshal.Copy (pageBytes, bitsStartAdress, pointerToBits, pageBytes.Length - bitsStartAdress);

    //        //int copiedLinesAmount = StretchDIBits
    //        //      (pd.hDC, 0, 0, 794, 1123, 0, 0, 794, 1123, pointerToBits, pointerToBitmapInfo, 1, 13369376);

    //        int copiedLinesAmount = SetDIBitsToDevice
    //              (pd.hDC, 0, 0, 794, 1123, 0, 0, 794, 1123, pointerToBits, pointerToBitmapInfo, 1);

    //        int errNumber = GetLastError ();
    //        EndPage (pd.hDC);
    //    }

    //    EndDoc (pd.hDC);
    //    DeleteObject (pd.hDC);
    //}


    [DllImport ("Printer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int DllMain ( );


    [DllImport ("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox ( IntPtr hWnd, string lpText, string lpCaption, uint uType );


    [DllImport ("Comdlg32.dll", CharSet = CharSet.Auto)]
    private static extern bool PrintDlg ( [In, Out] IntPtr lppd );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int StretchDIBits (IntPtr hdc, int xDest, int yDest, int DestWidth, int DestHeight, int xSrc, int ySrc, 
                                          int SrcWidth, int SrcHeight, IntPtr lpBits, IntPtr lpbmi, uint iUsage, int rop);


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int SetDIBitsToDevice ( IntPtr hdc, int xDest, int yDest, int DestWidth
                                                , int DestHeight, int xSrc, int ySrc, uint StartScan
                                                , uint cLines, IntPtr lpvBits, IntPtr lpbmi, uint ColorUse );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int StartDocA (IntPtr hdc, IntPtr docInfo);


    [DllImport ("Spoolss.dll", CharSet = CharSet.Auto)]
    private static extern int StartDocPrinter ( int hdc, int docInfoVersion, IntPtr docInfo );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int StartPage ( IntPtr hdc );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int EndPage ( IntPtr hdc );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int EndDoc ( IntPtr hdc );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int DeleteObject ( IntPtr hdc );


    [DllImport ("Comdlg32.dll", CharSet = CharSet.Auto)]
    private static extern int CommDlgExtendedError ();


    [DllImport ("Kernel32.dll", CharSet = CharSet.Auto)]
    private static extern int GetLastError ();


    public delegate IntPtr PrintHookProc ( IntPtr hWnd, UInt16 msg, Int32 wParam, Int32 lParam );

    public delegate IntPtr SetupHookProc ( IntPtr hWnd, UInt16 msg, Int32 wParam, Int32 lParam );


    //internal IntPtr PrintHookProcImpl ( IntPtr hPrintDlgWnd,
    //     UInt16 msg, Int32 wParam, Int32 lParam )
    //{
    //    // Evaluates the message parameter to determine
    //    // windows event notifications.
    //    switch ( msg )
    //    {
    //        case WndMsg.WM_INITDIALOG:
    //            {
    //                // save the handle to print dialog window for future use.
    //                this.Handle = hPrintDlgWnd;
    //                // window z-order position
    //                UInt32 nZOrder = ( this.TopMost ) ? WndZOrder.HWND_TOPMOST :
    //                                                  WndZOrder.HWND_TOP;
    //                // set this dialog z-order to topmost.
    //                Win32.SetWindowPos (
    //                    this.Handle,         // handle to window
    //                    nZOrder,             // placement-order handle
    //                    0, 0, 0, 0,          // dont-cares since,
    //                                         // this window options is set to
    //                                         // SWP_NOSIZE
    //                    WndPos.SWP_NOSIZE |
    //                    WndPos.SWP_NOMOVE    // window-positioning options
    //                );
    //                break;
    //            }
    //    }
    //    return IntPtr.Zero;
    //}


    internal void VisualiseNextPage ( )
    {
        if ( visiblePageNumber < allPages.Count )
        {
            visiblePage.HideBadges ();
            visiblePageNumber++;
            visiblePage = allPages [visiblePageNumber - 1];
            visiblePage.ShowBadges ();
        }
    }


    internal void VisualisePreviousPage ( )
    {
        if (visiblePageNumber > 1) 
        {
            visiblePage.HideBadges ();
            visiblePageNumber--;
            visiblePage = allPages [visiblePageNumber - 1];
            visiblePage.ShowBadges ();
        }
    }


    internal void VisualiseLastPage ()
    {
        if ( visiblePageNumber < allPages.Count )
        {
            visiblePage.HideBadges ();
            visiblePageNumber = allPages.Count;
            visiblePage = allPages [visiblePageNumber - 1]; 
            visiblePage.ShowBadges ();
        }
    }


    internal void VisualiseFirstPage ()
    {
        if ( visiblePageNumber > 1 )
        {
            visiblePage.HideBadges ();
            visiblePageNumber = 1;
            visiblePage = allPages [visiblePageNumber - 1];
            visiblePage.ShowBadges ();
        }
    }


    internal int VisualisePageWithNumber ( int pageNumber )
    {
        int result = visiblePageNumber;
        bool notTheSamePage = visiblePageNumber != pageNumber;
        bool inRange = pageNumber <= allPages.Count;

        if ( notTheSamePage && inRange )
        {
            visiblePage.HideBadges ();
            visiblePageNumber = pageNumber;
            visiblePage = allPages [visiblePageNumber - 1];
            visiblePage.ShowBadges ();
            result = pageNumber;
        }

        return result;
    }


    internal void BuildBadges ()
    {
        string pathInAvalonia = "avares://Lister/Assets";

        if ( chosenTemplate == null )
        {
            return;
        }

        string fileName = chosenTemplate. FullName.ExtractFileNameFromPath ();
        string badgeModelName = pathInAvalonia + "/" + fileName;
        List<Badge> requiredBadges = uniformAssembler.CreateBadgesByModel (badgeModelName);

        if ( requiredBadges.Count > 0 ) 
        {
            List<VMBadge> allBadges = new List<VMBadge> ();

            for ( int badgeCounter = 0;   badgeCounter < requiredBadges.Count;   badgeCounter++ )
            {
                VMBadge beingProcessedVMBadge = new VMBadge (requiredBadges [badgeCounter]);
                allBadges.Add (beingProcessedVMBadge);

                if ( ! beingProcessedVMBadge.isCorrect ) 
                {
                    incorrectBadges.Add (beingProcessedVMBadge);
                }
            }

            List<VMPage> newPages = VMPage.PlaceIntoPages (allBadges, pageSize, documentScale, lastPage);
            bool placingStartedOnLastPage = (lastPage != null)   &&   lastPage.Equals (newPages [0]);

            if (placingStartedOnLastPage) 
            {
                visiblePageNumber = allPages.Count;

                // page number 0 corresponds last page of previous building,  VMPage.PlaceIntoPages() method
                // added badges on it
                newPages.RemoveAt (0);
            }

            allPages.AddRange (newPages);
            lastPage = allPages.Last ();
            visiblePage = allPages [visiblePageNumber - 1];
            visiblePage.ShowBadges ();
        }  
    }


    internal void BuildSingleBadge ()
    {
        string pathInAvalonia = "avares://Lister/Assets";
        string fileName = chosenTemplate. FullName.ExtractFileNameFromPath ();
        string badgeModelName = pathInAvalonia + "/" + fileName;
        Person goalPerson = chosenPerson;
        Badge requiredBadge = uniformAssembler.CreateSingleBadgeByModel (badgeModelName, goalPerson);
        VMBadge goalVMBadge = new VMBadge (requiredBadge);

        if ( ! goalVMBadge.isCorrect )
        {
            incorrectBadges.Add (goalVMBadge);
        }

        bool itIsFirstBadgeBuildingInCurrentAppRun = (visiblePage==null);

        if ( itIsFirstBadgeBuildingInCurrentAppRun ) 
        {
            VMBadge badgeExample = goalVMBadge.Clone ();
            visiblePage = new VMPage (pageSize, badgeExample, documentScale);
            lastPage = visiblePage;
            allPages.Add (visiblePage);
        }

        bool placingStartedAfterEntireListAddition = ! lastPage.Equals (visiblePage);

        if (placingStartedAfterEntireListAddition) 
        {
            visiblePage.HideBadges ();
            visiblePage = lastPage;
            visiblePage.ShowBadges ();
        }

        VMPage possibleNewVisiblePage = lastPage.AddBadge (goalVMBadge, true);
        bool timeToIncrementVisiblePageNumber = ! possibleNewVisiblePage.Equals (lastPage);

        if ( timeToIncrementVisiblePageNumber ) 
        {
            visiblePage.HideBadges ();
            visiblePage = possibleNewVisiblePage;
            lastPage = visiblePage;
            allPages.Add (possibleNewVisiblePage);
            visiblePage.ShowBadges ();
        }

        visiblePageNumber = allPages.Count;
        goalVMBadge.ShowBackgroundImage ();
    }


    internal bool VerifyBadgeCorrectness(Badge badge ) 
    {


        return false;
    }


    internal event PropertyChangedEventHandler PropertyChanged;


    protected virtual void OnPropertyChanged ( string propertyName )
    {
        if ( PropertyChanged != null )
        {
            PropertyChanged?.Invoke (this, new System.ComponentModel.PropertyChangedEventArgs (propertyName));
        }
        //else
        //{
        //    throw new Exception ("empty delegate in mainViewModel  " + propertyName);
        //}
    }


    //private void SetPersonsFilePath (string value)
    //{
    //    bool valueIsSuitable = (value != string.Empty)   &&   (value != null);

    //    if (valueIsSuitable)
    //    {
    //        value = value.Substring(8, value.Length - 8);

    //        visiblePeople.Clear();
    //        people.Clear();
    //        var persons = this.uniformAssembler.GetPersons(value);

    //        foreach ( var person in persons ) 
    //        {
    //            VMPerson vmPerson = new VMPerson(person);
    //            visiblePeople.Add(vmPerson);
    //            people.Add(vmPerson);
    //        }
    //    }
    //}


    internal void ClearAllPages ()
    {
        if ( allPages.Count > 0 ) 
        {
            for ( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ )
            {
                allPages [pageCounter].Clear ();
            }

            visiblePage = allPages [0];
            allPages = new List<VMPage> ();
            lastPage = visiblePage;
            allPages.Add (lastPage);
            visiblePageNumber = 1;
        }
    }


    internal void ZoomOnDocument ( short step ) 
    {
        documentScale *= scalabilityCoefficient;

        if ( visiblePage != null )
        {
            for ( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ )
            {
                allPages [pageCounter].ZoomOn (scalabilityCoefficient);
            }

            visiblePage.ZoomOnExampleBadge (scalabilityCoefficient);
            zoomDegree *= scalabilityCoefficient;
            short zDegree = ( short ) zoomDegree;
            zoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
        }
    }


    internal void ZoomOutDocument ( short step )
    {
        documentScale /= scalabilityCoefficient;

        if ( visiblePage != null )
        {
            for ( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ )
            {
                allPages [pageCounter].ZoomOut (scalabilityCoefficient);
            }

            visiblePage.ZoomOutExampleBadge (scalabilityCoefficient);
            zoomDegree /= scalabilityCoefficient;
            short zDegree = ( short ) zoomDegree;
            zoomDegreeInView = zDegree.ToString () + " " + procentSymbol;
        }
    }

}



//[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
//class PRINTDLG
//{
//    public Int32 lStructSize;
//    public IntPtr hwndOwner;
//    public IntPtr hDevMode;
//    public IntPtr hDevNames;
//    public IntPtr hDC = IntPtr.Zero;
//    public Int32 Flags;
//    public Int16 FromPage = 0;
//    public Int16 ToPage = 0;
//    public Int16 MinPage = 0;
//    public Int16 MaxPage = 0;
//    public Int16 Copies = 0;
//    public IntPtr hInstance = IntPtr.Zero;
//    public IntPtr lCustData = IntPtr.Zero;
//    public IntPtr lpfnPrintHook;
//    public IntPtr lpfnSetupHook = IntPtr.Zero;
//    public IntPtr lpPrintTemplateName = IntPtr.Zero;
//    public IntPtr lpSetupTemplateName = IntPtr.Zero;
//    public IntPtr hPrintTemplate = IntPtr.Zero;
//    public IntPtr hSetupTemplate = IntPtr.Zero;
//}



//[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
//public struct DOCINFO
//{
//    public int cbSize;
//    public IntPtr lpszDocName;
//    public IntPtr lpszOutput;
//    public IntPtr lpszDatatype;
//    public Int32 fwType;
//}



//[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
//public struct BITMAPINFOHEADER
//{
//    public int biSize;
//    public long biWidth;
//    public long biHeight;
//    public short biPlanes;
//    public short biBitCount;
//    public short biCompression;
//    public short biSizeImage;
//    public long biXPelsPerMeter;
//    public long biYPelsPerMeter;
//    public short biClrUsed;
//    public short biClrImportant;
//}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
//[System.Runtime.InteropServices.ComVisible (false)]
public struct PRINTDLGA
{
    //[MarshalAs (UnmanagedType.I4)]
    public Int32 lStructSize;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hwndOwner;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hDevMode;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hDevNames;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hDC;
    //[MarshalAs (UnmanagedType.I4)]
    public Int32 Flags;
    //[MarshalAs (UnmanagedType.I2)]
    public Int16 FromPage;
    //[MarshalAs (UnmanagedType.I2)]
    public Int16 ToPage;
    //[MarshalAs (UnmanagedType.I2)]
    public Int16 MinPage;
    //[MarshalAs (UnmanagedType.I2)]
    public Int16 MaxPage;
    //[MarshalAs (UnmanagedType.I2)]
    public Int16 Copies;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hInstance;
    //[MarshalAs ( UnmanagedType.I8 )]
    public IntPtr lCustData;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr lpfnPrintHook;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr lpfnSetupHook;
    //[MarshalAs ( UnmanagedType.LPStr )]
    public IntPtr lpPrintTemplateName;
    //[MarshalAs ( UnmanagedType.LPStr )]
    public IntPtr lpSetupTemplateName;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hPrintTemplate;
    //[MarshalAs ( UnmanagedType.LPStruct )]
    public IntPtr hSetupTemplate;

    public PRINTDLGA ()
    {
        lStructSize = 0;
        hwndOwner = IntPtr.Zero;
        hDevMode = IntPtr.Zero;
        hDevNames = IntPtr.Zero;
        Flags = 0x00000100 | 0x00100000 | 0x00080000 | 0x00000004;
        hDC = IntPtr.Zero;
        FromPage = 1;
        ToPage = 20;
        MinPage = 1;
        MaxPage = 20;
        Copies = 1;
        hInstance = IntPtr.Zero;
        lCustData = IntPtr.Zero;
        lpfnPrintHook = IntPtr.Zero;
        lpfnSetupHook = IntPtr.Zero;
        lpPrintTemplateName = IntPtr.Zero;
        lpSetupTemplateName = IntPtr.Zero;
        hPrintTemplate = IntPtr.Zero;
        hSetupTemplate = IntPtr.Zero;
    }
}


[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
public struct DOCINFOA
{
    public int cbSize;
    public IntPtr lpszDocName;
    public IntPtr lpszOutput;
    public IntPtr lpszDatatype;
    public Int32 fwType;
}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
public struct BITMAPINFOHEADER
{
    public int biSize;
    public long biWidth;
    public long biHeight;
    public short biPlanes;
    public short biBitCount;
    public short biCompression;
    public short biSizeImage;
    public long biXPelsPerMeter;
    public long biYPelsPerMeter;
    public short biClrUsed;
    public short biClrImportant;
}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
public struct BITMAPINFO
{
    BITMAPINFOHEADER bmiHeader;
    RGBQUAD [] bmiColors;
}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
public struct RGBQUAD
{
    public byte rgbBlue;
    public byte rgbGreen;
    public byte rgbRed;
    public byte rgbReserved;
}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
public struct BITMAPFILEHEADER
{
    public short bfType;
    public int bfSize;
    public short bfReserved1;
    public short bfReserved2;
    public int bfOffBits;
}



[DebuggerDisplay ("{Value}")]
public readonly unsafe struct PWSTR : IEquatable<PWSTR>
{
    public readonly char* Value;

    public PWSTR ( char* value ) => Value = value;

    public static implicit operator char* ( PWSTR value ) => value.Value;

    public static implicit operator PWSTR ( char* value ) => new (value);

    public static bool operator == ( PWSTR left, PWSTR right ) => left.Value == right.Value;

    public static bool operator != ( PWSTR left, PWSTR right ) => !( left == right );

    public bool Equals ( PWSTR other ) => Value == other.Value;

    public override bool Equals ( object obj ) => obj is PWSTR other && Equals (other);

    public override int GetHashCode () => ( int ) Value;

    public override string ToString () => new PCWSTR (Value).ToString ();

    public static implicit operator PCWSTR ( PWSTR value ) => new (value.Value);

    public int Length => new PCWSTR (Value).Length;

    public Span<char> AsSpan () => Value is null ? default : new Span<char> (Value, Length);
}



[DebuggerDisplay ("{" + nameof (DebuggerDisplay) + "}")]
public readonly unsafe struct PCWSTR : IEquatable<PCWSTR>
{
    public readonly char* Value;

    public PCWSTR ( char* value ) => Value = value;

    public static explicit operator char* ( PCWSTR value ) => value.Value;

    public static implicit operator PCWSTR ( char* value ) => new (value);

    public bool Equals ( PCWSTR other ) => Value == other.Value;

    public override bool Equals ( object obj ) => obj is PCWSTR other && Equals (other);

    public override int GetHashCode () => ( int ) Value;

    public int Length
    {
        get
        {
            var p = Value;

            if ( p is null )
                return 0;

            while ( *p != '\0' )
                p++;

            return checked(( int ) ( p - Value ));
        }
    }

    public override string ToString () => Value is null ? string.Empty : new string (Value);

    public ReadOnlySpan<char> AsSpan () => Value is null ? default : new ReadOnlySpan<char> (Value, Length);

    private string DebuggerDisplay => ToString ();
}



internal class DropDownOrPickUpListCommand : ICommand
{
    private bool DroppingIsDone;


    public DropDownOrPickUpListCommand()
    {
        DroppingIsDone = false;
    }

    public event EventHandler? CanExecuteChanged 
    {
        add {  }
        remove { }
    }

    public bool CanExecute(object? parameter)
    {
        throw new NotImplementedException();
    }

    public void Execute(object? parameter)
    {
        throw new NotImplementedException();
    }
}




