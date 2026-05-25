using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TellerApp.Models
{
    public class CurrencyRate
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public decimal BuyRate { get; set; }
        public decimal SellRate { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}