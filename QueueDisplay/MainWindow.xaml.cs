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
using QueueDisplay.Views;

namespace QueueDisplay
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            ContentFrame.Navigate(typeof(QueuePage));
        }

        private void MainNav_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null)
                return;

            string tag =
                args.SelectedItemContainer.Tag.ToString();

            switch (tag)
            {
                case "queue":
                    ContentFrame.Navigate(typeof(QueuePage));
                    break;

                case "transfer":
                    ContentFrame.Navigate(typeof(TransferPage));
                    break;

                case "currency":
                    ContentFrame.Navigate(typeof(CurrencyPage));
                    break;
            }
        }
    }
}