﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
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
    /// Interaction logic for AvEditorDialog.xaml
    /// </summary>
    public partial class AvEditorDialog : Window
    {
        public AvEditorDialog()
        {
            InitializeComponent();
        }

        void Close(object sender, EventArgs e)
        {
            Close();
        }
    }
}
