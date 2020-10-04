using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using AvalonDock.Layout;

using Scrapper.ViewModel;
namespace Scrapper.View.Pane
{
    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        { 
        }

        public DataTemplate TextViewTemplate { get; set; }
        public DataTemplate BrowserViewTemplate { get; set; }
        public DataTemplate MediaViewTemplate { get; set; }
        public DataTemplate FileViewTemplate { get; set; }
        public DataTemplate AvDbViewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is TextViewModel)
                return TextViewTemplate;
            if (item is BrowserViewModel)
                return BrowserViewTemplate;
            if (item is MediaViewModel)
                return MediaViewTemplate;
            if (item is AvDbViewModel)
                return AvDbViewTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
