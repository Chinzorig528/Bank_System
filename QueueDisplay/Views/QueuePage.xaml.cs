using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace QueueDisplay.Views
{
    public sealed partial class QueuePage : Page
    {
        private int number = 1;

        public QueuePage()
        {
            this.InitializeComponent();
        }

        private void CallNext_Click(
            object sender,
            RoutedEventArgs e)
        {
            number++;

            TicketText.Text =
                $"A{number:D3}";
        }
    }
}
