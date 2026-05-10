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
    public sealed partial class CurrencyPage : Page
    {
        public CurrencyPage()
        {
            this.InitializeComponent();
        }

        private void Update_Click(
            object sender,
            RoutedEventArgs e)
        {
            StatusText.Text =
                "Currency updated";
        }
    }
}
