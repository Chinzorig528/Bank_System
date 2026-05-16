using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankTicket
{
    public class TicketResponse
    {
        public int Id { get; set; }

        public string Number { get; set; } = "";

        public bool IsCalled { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

