using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TellerApp.Models
{
    /// <summary>
    /// Валютын ханшийг UI дээр string хэлбэрээр засварлахад ашиглах model.
    /// </summary>
    public class CurrencyRateEdit : INotifyPropertyChanged
    {
        private string code = string.Empty;
        private string name = string.Empty;
        private string buyRate = string.Empty;
        private string sellRate = string.Empty;

        /// <summary>
        /// Засварлаж буй валютын ханшийн ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// UI дээр засварлагдах валютын код.
        /// </summary>
        public string Code
        {
            get => code;
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// UI дээр засварлагдах валютын нэр.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// UI дээр текстээр оруулах авах ханш.
        /// </summary>
        public string BuyRate
        {
            get => buyRate;
            set
            {
                buyRate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// UI дээр текстээр оруулах зарах ханш.
        /// </summary>
        public string SellRate
        {
            get => sellRate;
            set
            {
                sellRate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ханш шинэчлэгдсэн огноо.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Property өөрчлөгдөх үед UI-д мэдэгдэх event.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Property өөрчлөгдсөнийг binding системд мэдэгдэнэ.
        /// </summary>
        /// <param name="propertyName">Өөрчлөгдсөн property-ийн нэр.</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
