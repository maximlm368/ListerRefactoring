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
using static Lister.ViewModels.MainViewModel;


namespace Lister.ViewModels;

class MainViewModel : ViewModelBase
{
    private IUniformDocumentAssembler uniformAssembler;
    ConverterToPdf converter;
    private string personsSourceFilePath;
    private string chosenTemplateFilePath;
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
            SetPersonsFilePath (value);
            this.RaiseAndSetIfChanged (ref sFP, value, nameof (sourceFilePath));
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
    private int zoomDegree;

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


    internal void SetPersonsFilePath ( string value )
    {
        bool valueIsSuitable = (value != null)   &&   (value != string.Empty);

        if ( valueIsSuitable )
        {
            value = value.Substring (8, value.Length - 8);
            visiblePeople.Clear ();
            people.Clear ();
            var persons = uniformAssembler.GetPersons (value);

            foreach ( var person in persons )
            {
                visiblePeople.Add (person);
                people.Add (person);
            }
        }
    }


    internal void SetWidth ( int screenWidth )
    {
        this.screenWidth = screenWidth;
    }


    internal void GeneratePdf ( )
    {
        //string fileToSave = "D:\\MML\\Lister\\Lister\\pdfFromDotNet.pdf";
        string fileToSave = "./pdfFromDotNet.pdf";

        List<VMBadge> allBadges = new List<VMBadge> ();

        for( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ ) 
        {
            int badgePairCounter = 0;

            while (true)
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

        //Task task = new Task (() => { converter.ConvertToExtention (allBadges, fileToSave); });
        //task.Start ();
        converter.ConvertToExtention (allBadges, fileToSave);
    }


    internal unsafe void Print ()
    {
        bool badgesNotCreated = (converter. bytes == null);

        if ( badgesNotCreated ) 
        {
            GeneratePdf ();
        }

        IEnumerable<byte[]> printableData = converter. bytes;

        PRINTDLG pd = new PRINTDLG ();
        pd.lStructSize = Marshal.SizeOf<PRINTDLG> ();
        
        IntPtr unmanagedPrintDlg = Marshal.AllocHGlobal (pd.lStructSize);
        Marshal.StructureToPtr (pd, unmanagedPrintDlg, true);
        //long* printDlgPointer = ( long* ) unmanagedPrintDlg.ToPointer ();

        //PRINTDLG* pointer = &pd;

        //Process notepade = Process.Start ("notepade");
        //IntPtr handle = notepade.Handle;
        
        bool isDialogSuccessfull = PrintDlg (unmanagedPrintDlg);

        DOCINFO docInfo = new DOCINFO ();
        docInfo.cbSize = Marshal.SizeOf<DOCINFO> ();
        docInfo.lpszDocName = Marshal.StringToHGlobalUni ("My Document");
        docInfo.lpszOutput = 0;
        docInfo.lpszDatatype = Marshal.StringToHGlobalUni ("RAW");
        docInfo.fwType = 0;

        StartDoc(pd.hDC, docInfo);

        foreach ( byte[] pageBytes in printableData) 
        {
            var handlePointer = pd.hDC;
            StartPage (pd.hDC);

            BITMAPINFO bitmapInfo = new BITMAPINFO ();
            bitmapInfo.biSize = Marshal.SizeOf<BITMAPINFO> ();
            bitmapInfo.biHeight = 1123;
            bitmapInfo.biWidth = 974;
            bitmapInfo.biPlanes = 1;
            bitmapInfo.biBitCount = 24;
            bitmapInfo.biCompression = 0;

            int copiedLinesAmount = StretchDIBits (pd.hDC, 0, 0, 974, 1123, 0, 0, 974, 1123, pageBytes, bitmapInfo, 0, 13369376);
            int errNumber = GetLastError ();
            EndPage(pd.hDC);
        }

        EndDoc (pd.hDC);
        DeleteObject (pd.hDC);
    }


    [DllImport ("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox ( IntPtr hWnd, string lpText, string lpCaption, uint uType );


    [DllImport ("Comdlg32.dll", CharSet = CharSet.Auto)]
    private static unsafe extern bool PrintDlg ( [In, Out] IntPtr lppd );


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int StretchDIBits (IntPtr hdc, int xDest, int yDest, int DestWidth, int DestHeight, int xSrc, int ySrc, 
                                            int SrcWidth, int SrcHeight, byte[] lpBits, BITMAPINFO lpbmi, uint iUsage, int rop);


    [DllImport ("Gdi32.dll", CharSet = CharSet.Auto)]
    private static extern int StartDoc (IntPtr hdc, DOCINFO docInfo);


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


    internal void VisualisePageWithNumber ( int pageNumber )
    {
        bool notTheSamePage = visiblePageNumber != pageNumber;
        bool inRange = pageNumber <= allPages.Count;

        if ( notTheSamePage && inRange )
        {
            visiblePage.HideBadges ();
            visiblePageNumber = pageNumber;
            visiblePage = allPages [visiblePageNumber - 1];
            visiblePage.ShowBadges ();
        }
    }


    internal void BuildBadges ()
    {
        string pathInAvalonia = "avares://Lister/Assets";

        if ( chosenTemplate == null )
        {
            return;
        }

        string fileName = chosenTemplate.FullName.ExtractFileNameFromPath ();
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


    internal void ZoomOnDocument () 
    {
        documentScale *= scalabilityCoefficient;

        if ( visiblePage != null )
        {
            for ( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ )
            {
                allPages [pageCounter].ZoomOn (scalabilityCoefficient);
            }

            visiblePage.ZoomOnExampleBadge (scalabilityCoefficient);
            double zD = zoomDegree * scalabilityCoefficient;
            zoomDegree = (int) zD;
            zoomDegreeInView = zoomDegree.ToString () + " " + procentSymbol;
        }
    }


    internal void ZoomOutDocument ()
    {
        documentScale /= scalabilityCoefficient;

        if ( visiblePage != null )
        {
            for ( int pageCounter = 0;   pageCounter < allPages.Count;   pageCounter++ )
            {
                allPages [pageCounter].ZoomOut (scalabilityCoefficient);
            }

            visiblePage.ZoomOutExampleBadge (scalabilityCoefficient);
            double zD = zoomDegree / scalabilityCoefficient;
            zoomDegree = ( int ) zD;
            zoomDegreeInView = zoomDegree.ToString () + " " + procentSymbol;
        }
    }

}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
//[System.Runtime.InteropServices.ComVisible (false)]
public unsafe struct PRINTDLG
{
    [MarshalAs (UnmanagedType.I4)]
    public Int32 lStructSize;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hwndOwner;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hDevMode;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hDevNames;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hDC;
    [MarshalAs (UnmanagedType.I4)]
    public Int32 Flags = 0x00000100 | 0x00100000 | 0x00080000 | 0x00000004;
    [MarshalAs (UnmanagedType.I2)]
    public Int16 FromPage;
    [MarshalAs (UnmanagedType.I2)]
    public Int16 ToPage;
    [MarshalAs (UnmanagedType.I2)]
    public Int16 MinPage;
    [MarshalAs (UnmanagedType.I2)]
    public Int16 MaxPage;
    [MarshalAs (UnmanagedType.I2)]
    public Int16 Copies;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hInstance;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr lCustData;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr lpfnPrintHook;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr lpfnSetupHook;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr lpPrintTemplateName;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr lpSetupTemplateName;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hPrintTemplate;
    //[MarshalAs (UnmanagedType.I8)]
    public IntPtr hSetupTemplate;

    public PRINTDLG ()
    {
        lStructSize = 0;
        hwndOwner = IntPtr.Zero;
        hDevMode = IntPtr.Zero;
        hDevNames = IntPtr.Zero;
        Flags = 0;
        hDC = IntPtr.Zero;
        FromPage = 0;
        ToPage = 0;
        MinPage = 0;
        MaxPage = 0;
        Copies = 0;
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
public struct DOCINFO
{
    public int cbSize;
    public IntPtr lpszDocName;
    public IntPtr lpszOutput;
    public IntPtr lpszDatatype;
    public Int32 fwType;
}



[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8)]
public struct BITMAPINFO
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




