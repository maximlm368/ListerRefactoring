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

class VMBadge : ViewModelBase
{
    private const double coefficient = 1.1;
    internal Badge badgeModel { get; private set; }
    internal Bitmap imageBitmap { get; private set; }

    private string lN;
    internal string lastName
    {
        get { return lN; }
        set
        {
            this.RaiseAndSetIfChanged (ref lN, value, nameof (lastName));
        }
    }

    private string fsN;
    internal string firstAndSecondName
    {
        get { return fsN; }
        set
        {
            this.RaiseAndSetIfChanged (ref fsN, value, nameof (firstAndSecondName));
        }
    }

    private string dN;
    internal string departmentName
    {
        get { return dN; }
        set
        {
            this.RaiseAndSetIfChanged (ref dN, value, nameof (departmentName));
        }
    }

    private string pN;
    internal string positionName
    {
        get { return pN; }
        set
        {
            this.RaiseAndSetIfChanged (ref pN, value, nameof (positionName));
        }
    }

    private ObservableCollection<string> rL;
    internal ObservableCollection<string> reserveLastNameTextBlocks
    {
        get { return rL; }
        set
        {
            this.RaiseAndSetIfChanged (ref rL, value, nameof (reserveLastNameTextBlocks));
        }
    }

    private ObservableCollection<string> rFM;
    internal ObservableCollection<string> reserveFirstAndMiddleNamesTextBlocks
    {
        get { return rFM; }
        set
        {
            this.RaiseAndSetIfChanged (ref rFM, value, nameof (reserveFirstAndMiddleNamesTextBlocks));
        }
    }

    private ObservableCollection<string> rd;
    internal ObservableCollection<string> reserveDepartmentTextBlocks
    {
        get { return rd; }
        set
        {
            this.RaiseAndSetIfChanged (ref rd, value, nameof (reserveDepartmentTextBlocks));
        }
    }

    private ObservableCollection<string> rp;
    internal ObservableCollection<string> reservePositionTextBlocks
    {
        get { return rp; }
        set
        {
            this.RaiseAndSetIfChanged (ref rp, value, nameof (reservePositionTextBlocks));
        }
    }

    private double pH;
    internal double personalTextContainerHeight
    {
        get { return pH; }
        set
        {
            this.RaiseAndSetIfChanged (ref pH, value, nameof (personalTextContainerHeight));
        }
    }

    private double bW;
    internal double badgeWidth
    {
        get { return bW; }
        set
        {
            this.RaiseAndSetIfChanged (ref bW, value, nameof (badgeWidth));
        }
    }

    private double bH;
    internal double badgeHeight
    {
        get { return bH; }
        set
        {
            this.RaiseAndSetIfChanged (ref bH, value, nameof (badgeHeight));
        }
    }

    private double tS;
    internal double textAreaTopShift
    {
        get { return tS; }
        set
        {
            this.RaiseAndSetIfChanged (ref tS, value, nameof (textAreaTopShift));
        }
    }

    private double lS;
    internal double textAreaLeftShift
    {
        get { return lS; }
        set
        {
            this.RaiseAndSetIfChanged (ref lS, value, nameof (textAreaLeftShift));
        }
    }

    private double tW;
    internal double textAreaWidth
    {
        get { return tW; }
        set
        {
            this.RaiseAndSetIfChanged (ref tW, value, nameof (textAreaWidth));
        }
    }

    private double tH;
    internal double textAreaHeight
    {
        get { return tH; }
        set
        {
            this.RaiseAndSetIfChanged (ref tH, value, nameof (textAreaHeight));
        }
    }

    private double fFS;
    internal double firstLevelFontSize
    {
        get { return fFS; }
        set
        {
            this.RaiseAndSetIfChanged (ref fFS, value, nameof (firstLevelFontSize));
        }
    }

    private double sFS;
    internal double secondLevelFontSize
    {
        get { return sFS; }
        set
        {
            this.RaiseAndSetIfChanged (ref sFS, value, nameof (secondLevelFontSize));
        }
    }

    private double tFS;
    internal double thirdLevelFontSize
    {
        get { return tFS; }
        set
        {
            this.RaiseAndSetIfChanged (ref tFS, value, nameof (thirdLevelFontSize));
        }
    }

    private double fH;
    internal double firstLevelTBHeight
    {
        get { return fH; }
        set
        {
            this.RaiseAndSetIfChanged (ref fH, value, nameof (firstLevelTBHeight));
        }
    }

    private double sH;
    internal double secondLevelTBHeight
    {
        get { return sH; }
        set
        {
            this.RaiseAndSetIfChanged (ref sH, value, nameof (secondLevelTBHeight));
        }
    }

    private double thH;
    internal double thirdLevelTBHeight
    {
        get { return thH; }
        set
        {
            this.RaiseAndSetIfChanged (ref thH, value, nameof (thirdLevelTBHeight));
        }
    }

    internal bool isCorrect { get; private set; }


    internal VMBadge ( Badge badgeModel )
    {
        this.badgeModel = badgeModel;
        badgeWidth = badgeModel.badgeDescription. badgeDimensions. outlineSize. width;
        badgeHeight = badgeModel.badgeDescription. badgeDimensions. outlineSize. height;
        textAreaTopShift = badgeModel.badgeDescription. badgeDimensions. personTextAreaTopShiftOnBackground;
        textAreaLeftShift = badgeModel.badgeDescription. badgeDimensions. personTextAreaLeftShiftOnBackground;
        textAreaWidth = badgeModel.badgeDescription. badgeDimensions. personTextAreaSize. width;
        textAreaHeight = badgeModel.badgeDescription. badgeDimensions. personTextAreaSize. height;
        firstLevelFontSize = badgeModel.badgeDescription. badgeDimensions. firstLevelFontSize;
        secondLevelFontSize = badgeModel.badgeDescription. badgeDimensions. secondLevelFontSize;
        thirdLevelFontSize = badgeModel.badgeDescription. badgeDimensions. thirdLevelFontSize;
        firstLevelTBHeight = badgeModel.badgeDescription. badgeDimensions. firstLevelTBHeight;
        secondLevelTBHeight = badgeModel.badgeDescription. badgeDimensions. secondLevelTBHeight;
        thirdLevelTBHeight = badgeModel.badgeDescription. badgeDimensions. thirdLevelTBHeight;

        lastName = badgeModel.person. lastName;
        firstAndSecondName = badgeModel.person. firstName + " " + badgeModel.person. middleName;
        departmentName = badgeModel.person. department;
        positionName = badgeModel.person. position;
        reserveLastNameTextBlocks = new ObservableCollection<string> ();
        reserveFirstAndMiddleNamesTextBlocks = new ObservableCollection<string> ();
        reserveDepartmentTextBlocks = new ObservableCollection<string> ();
        reservePositionTextBlocks = new ObservableCollection<string> ();

        personalTextContainerHeight = 0;
        isCorrect = true;

        FitTextDataInContainer ();
    }

    public event PropertyChangedEventHandler? PropertyChanged;


    internal void ShowBackgroundImage () 
    {
        string path = badgeModel. backgroundImagePath;
        Uri uri = new Uri (path);
        this.imageBitmap = ImageHelper.LoadFromResource (uri);
    }


    internal void HideBackgroundImage ()
    {
        this.imageBitmap = null;
    }

    internal void ZoomOn ( double coefficient )
    {
        badgeWidth *= coefficient;
        badgeHeight *= coefficient;
        textAreaLeftShift *= coefficient;
        textAreaTopShift *= coefficient;
        textAreaWidth *= coefficient;
        textAreaHeight *= coefficient;
        firstLevelFontSize *= coefficient;
        secondLevelFontSize *= coefficient;
        thirdLevelFontSize *= coefficient;
        firstLevelTBHeight *= coefficient;
        secondLevelTBHeight *= coefficient;
        thirdLevelTBHeight *= coefficient;
    }


    internal void ZoomOut ( double coefficient )
    {
        badgeWidth /= coefficient;
        badgeHeight /= coefficient;
        textAreaLeftShift /= coefficient;
        textAreaTopShift /= coefficient;
        textAreaWidth /= coefficient;
        textAreaHeight /= coefficient;
        firstLevelFontSize /= coefficient;
        secondLevelFontSize /= coefficient;
        thirdLevelFontSize /= coefficient;
        firstLevelTBHeight /= coefficient;
        secondLevelTBHeight /= coefficient;
        thirdLevelTBHeight /= coefficient;
    }


    internal VMBadge Clone () 
    {
        VMBadge clone = new VMBadge (this.badgeModel);
        return clone;
    }


    internal void Zoom ( double coefficient )
    {
        if (coefficient != 1) 
        {
            ZoomOn (coefficient);
        }
    }


    private void FitTextDataInContainer ()
    {
        FitLastNameInContainer ();
        FitFirstAndMiddleNamesInContainer ();
        FitDepartmentInContainer ();
        FitPositionInContainer ();

        personalTextContainerHeight = personalTextContainerHeight + firstLevelTBHeight + secondLevelTBHeight
                                                                  + thirdLevelTBHeight * 2;

        for ( int lineCounter = 0; lineCounter < reserveLastNameTextBlocks.Count; lineCounter++ )
        {
            personalTextContainerHeight += firstLevelTBHeight;
        }

        for ( int lineCounter = 0; lineCounter < reserveFirstAndMiddleNamesTextBlocks.Count; lineCounter++ )
        {
            personalTextContainerHeight += secondLevelTBHeight;
        }

        for ( int lineCounter = 0; lineCounter < reserveDepartmentTextBlocks.Count; lineCounter++ )
        {
            personalTextContainerHeight += thirdLevelTBHeight;
        }

        for ( int lineCounter = 0; lineCounter < reservePositionTextBlocks.Count; lineCounter++ )
        {
            personalTextContainerHeight += thirdLevelTBHeight;
        }

        if ( personalTextContainerHeight >= textAreaHeight )
        {
            isCorrect = false;
        }
    }


    private void FitLastNameInContainer ()
    {
        string beingProcessedLine = lastName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (firstLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            lastName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                reserveLastNameTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private void FitFirstAndMiddleNamesInContainer ()
    {
        string beingProcessedLine = firstAndSecondName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (secondLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            firstAndSecondName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                reserveFirstAndMiddleNamesTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private void FitDepartmentInContainer ()
    {
        string beingProcessedLine = departmentName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (thirdLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            departmentName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                reserveDepartmentTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private void FitPositionInContainer ()
    {
        string beingProcessedLine = positionName;
        List<string> stack = new List<string> ();
        stack.Add (beingProcessedLine);

        while ( true )
        {
            string additionalLine = AddLineIfCurrentOverFlow (thirdLevelFontSize, stack);

            if ( additionalLine == string.Empty )
            {
                break;
            }
        }

        if ( stack.Count > 1 )
        {
            positionName = stack [0];

            for ( int stepCounter = 1; stepCounter < stack.Count; stepCounter++ )
            {
                reservePositionTextBlocks.Add (stack [stepCounter]);
            }
        }
    }


    private string AddLineIfCurrentOverFlow ( double fontSize, List<string> lineStack )
    {
        string beingProcessedLine = lineStack.Last ();
        int positionInStack = lineStack.Count - 1;
        string additionalLine = string.Empty;
        FormattedText formatted = new FormattedText
                    (beingProcessedLine, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, fontSize, null);
        double usefulTextBlockWidth = formatted.Width * coefficient;
        bool lineIsOverflow = ( usefulTextBlockWidth >= textAreaWidth );

        while ( lineIsOverflow )
        {
            List<string> splited = beingProcessedLine.SplitIntoRestAndLastWord ();

            if ( splited.Count > 0 )
            {
                beingProcessedLine = splited [0];
                additionalLine = splited [1] + " " + additionalLine;

                formatted = new FormattedText
                    (beingProcessedLine, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, fontSize, null);
                usefulTextBlockWidth = formatted.Width * coefficient;
                lineIsOverflow = ( usefulTextBlockWidth >= textAreaWidth );
            }
            else
            {
                break;
            }

        }

        if ( additionalLine != string.Empty )
        {
            lineStack.RemoveAt (positionInStack);
            lineStack.Add (beingProcessedLine);
            lineStack.Add (additionalLine);
        }

        return additionalLine;
    }
}