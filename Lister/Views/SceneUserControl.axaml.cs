using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Lister.ViewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;
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

    }
}
