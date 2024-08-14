using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Globalization;
using System.Reactive.Subjects;

namespace Lister.Views
{
    public partial class SceneUserControl : UserControl
    {
        private SceneViewModel _vm;
        private double _widthDelta;
        private double _heightDelta;


        public SceneUserControl ()
        {
            InitializeComponent ();
            DataContext = App.services.GetRequiredService<SceneViewModel> ();
            _vm = (SceneViewModel) DataContext;
            var window = TopLevel.GetTopLevel (this);
            _vm.PassView (this);
            this.Margin = new Avalonia.Thickness (5);

            Loaded += OnLoaded;
        }


        private void OnLoaded ( object sender, RoutedEventArgs args )
        {
            _vm.SetEdition ();
        }


        internal void EditIncorrectBadges ( List <BadgeViewModel> incorrectBadges )
        {
            ModernMainView mainView = ModernMainView.Instance;
            mainView.EditIncorrectBadges ( incorrectBadges );
        }


        internal void CorrectAlignments ( ) 
        {
            

            Controls items = A4.Children;
            ItemsControl itemsContainer = items [ 0 ] as ItemsControl;

            IEnumerable <ILogical> lines = itemsContainer.GetLogicalChildren ( );
            int count = lines.Count ();

            foreach ( ILogical line in lines )
            {
                //ItemsControl badgeItemsContainer = line.LogicalChildren;

                //IEnumerable<ILogical> badges = badgeItemsContainer.GetLogicalChildren ();

                IEnumerable<ILogical> badges = line.GetLogicalChildren ();
                count = badges.Count ();

                foreach ( ILogical badgeContainer   in   badges )
                {
                    IEnumerable <ILogical> badgesInLine = badgeContainer.GetLogicalChildren ();
                    count = badgesInLine.Count ();

                    foreach ( ILogical badge   in   badgesInLine ) 
                    {
                        IEnumerable <Border> badgeBorders = badge.GetLogicalChildren ().OfType<Border> ();
                        count = badgeBorders.Count ();
                        Border border = badgeBorders.FirstOrDefault ();
                        
                        Canvas badgeBase = border.Child as Canvas;
                        ItemsControl textLinesContainer = badgeBase.Children [2] as ItemsControl;
                        IEnumerable<Control> textLines = textLinesContainer.GetTemplateChildren ();

                        int counter = 0;

                        foreach ( var textLine in textLines )
                        {
                            if ( counter < 2 )
                            {
                                Label familyName = textLine as Label;
                                familyName.HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                            }

                            counter++;
                        }
                    }
                }

            }






            //foreach ( Control line in lines )
            //{
            //    ItemsControl badgeItems = line as ItemsControl;

            //    IEnumerable<Visual> badges = badgeItems.GetVisualChildren ( );

            //    foreach ( Control badge   in   badges )
            //    {




            //    }

            //}


        }

        internal void Focus ( object sender, TappedEventArgs args )
        {
            Label label = sender as Label;
            Border container;

            //Label lb = new Label ();
            //lb.Content = "Александровская";
            //lb.FontSize = 24;
            //Size ss = new Size ();
            //lb.Measure (ss);
            //var wwd = lb.Width;
            //Size s = lb.DesiredSize;
            //double wd = s.Width;


            string cont = label.Content as string;

            Typeface face = new Typeface (label.FontFamily, FontStyle.Normal, FontWeight.Bold);
            FormattedText formatted = new FormattedText (cont, CultureInfo.CurrentCulture
                                                                , FlowDirection.LeftToRight, face, label.FontSize, null);

            double width = formatted.WidthIncludingTrailingWhitespace;

            Avalonia.Size wdth = label.DesiredSize;

            int fdf = 0;
        }


        internal void Handle ( object sender, RoutedEventArgs args )
        {
            TextBlock tb = sender as TextBlock;

            //var badgeLines = lines.GetLogicalChildren ();

            //foreach ( var badgeLine in badgeLines )
            //{
            //    var badges = badgeLine.GetLogicalChildren ();

            //    foreach ( var badge in badges )
            //    {
            //        var badgeBorder = badge.GetLogicalChildren ();

            //        foreach ( var children in badgeBorder )
            //        {
            //            var badgeBorder = children.GetLogicalChildren ();
            //        }
            //    }
            //}

            Avalonia.Size wdth = tb.DesiredSize;
        }

    }
}
