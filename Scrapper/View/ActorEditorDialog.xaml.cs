using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Scrapper.View
{
    /// <summary>
    /// Interaction logic for ActorEditorDialog.xaml
    /// </summary>
    public partial class ActorEditorDialog : Window
    {
        public ActorEditorDialog()
        {
            InitializeComponent();
            var buttons = Enumerable.Range('A', 'Z' - 'A' + 1).
                      Select(c => ((char)c).ToString()).ToList();
            buttons.Insert(0, "All");
            numberButtonItems.ItemsSource = buttons;
        }
    }
}
