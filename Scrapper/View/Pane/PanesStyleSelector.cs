using System.Windows;
using System.Windows.Controls;

using Scrapper.ViewModel;
namespace Scrapper.View.Pane
{
    class PanesStyleSelector : StyleSelector
    {
        public Style DocStyle { get; set; }
        public Style AnchorStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is DebugLogViewModel)
                return AnchorStyle;

            if (item is ViewModel.Base.Pane)
                return DocStyle;

            return base.SelectStyle(item, container);
        }
    }
}
