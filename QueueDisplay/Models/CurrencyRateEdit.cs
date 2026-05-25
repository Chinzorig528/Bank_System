using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TellerApp.Models
{
    public class CurrencyRateEdit : INotifyPropertyChanged
    {
        private string code = string.Empty;
        private string name = string.Empty;
        private string buyRate = string.Empty;
        private string sellRate = string.Empty;

        public int Id { get; set; }

        public string Code
        {
            get => code;
            set
            {
                code = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public string BuyRate
        {
            get => buyRate;
            set
            {
                buyRate = value;
                OnPropertyChanged();
            }
        }

        public string SellRate
        {
            get => sellRate;
            set
            {
                sellRate = value;
                OnPropertyChanged();
            }
        }

        public DateTime UpdatedAt { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}