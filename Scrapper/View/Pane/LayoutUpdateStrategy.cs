using System.Linq;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;

using AvalonDock.Layout;

using Scrapper.ViewModel;

namespace Scrapper.View.Pane
{
    class LayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout,
            LayoutAnchorable anchorableToShow,
            ILayoutContainer destinationContainer)
        {
            LayoutAnchorablePane pane = null;
            if (anchorableToShow.Content is DebugLogViewModel)
            {
                pane = layout.Descendents().OfType<LayoutAnchorablePane>()
                            .FirstOrDefault(d => d.Name == "bottom");
            }
#if false
            else if (anchorableToShow.Content is BuildInfoViewModel)
            {
                var parent = layout.Descendents().OfType<LayoutPanel>().First(
                    d => d.Orientation == Orientation.Horizontal);
                pane = new LayoutAnchorablePane() {
                    DockWidth = new System.Windows.GridLength(300),
                };
                parent.InsertChildAt(0, pane);
            }
#endif
            if (pane != null)
            {
                //anchorableToShow.CanHide = false;
                anchorableToShow.CanClose = false;
                pane.Children.Add(anchorableToShow);
                return true;
            }
            return false;
        }
#if false
        //for reference 
        static LayoutAnchorablePane CreateAnchorablePane(LayoutRoot layout, Orientation orientation,
                    PaneLocation initLocation)
        {
            var parent = layout.Descendents().OfType<LayoutPanel>().First(d => d.Orientation == orientation);
            string paneName = _paneNames[initLocation];
            var toolsPane = new LayoutAnchorablePane { Name = paneName };
            if (initLocation == PaneLocation.Left)
                parent.InsertChildAt(0, toolsPane);
            else
                parent.Children.Add(toolsPane);
            return toolsPane;
        }
#endif
        public void AfterInsertAnchorable(LayoutRoot layout,
            LayoutAnchorable anchorableShown)
        {
        }

        public bool BeforeInsertDocument(LayoutRoot layout,
            LayoutDocument anchorableToShow,
            ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout,
            LayoutDocument anchorableShown)
        {
        }
    }
}
