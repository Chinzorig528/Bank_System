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
using TellerApp.Views;
namespace QueueDisplay
{
    /// <summary>
    /// Teller app-ийн үндсэн цонх бөгөөд navigation menu-ээр хуудсууд хооронд шилжүүлнэ.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Үндсэн цонхыг үүсгэж эхний хуудсаар queue page-ийг нээнэ.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            ContentFrame.Navigate(typeof(QueuePage));
        }

        /// <summary>
        /// Navigation menu дээр сонгосон item-ийн tag-аар тохирох хуудсыг ContentFrame-д нээнэ.
        /// </summary>
        /// <param name="sender">NavigationView control.</param>
        /// <param name="args">Сонголт өөрчлөгдсөн event-ийн мэдээлэл.</param>
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
                case "createAccount":
                    ContentFrame.Navigate(typeof(CreateAccountPage));
                    break;
            }
        }
    }
}
