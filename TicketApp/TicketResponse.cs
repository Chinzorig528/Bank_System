using System;

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