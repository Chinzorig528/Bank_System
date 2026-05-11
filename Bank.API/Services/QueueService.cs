using BankApi.Models;

namespace BankApi.Services;

public class QueueService
{
    private readonly Queue<Ticket> _queue = new();

    private int _depositCounter = 1;
    private int _withdrawCounter = 1;
    private int _exchangeCounter = 1;

    private Ticket? _currentTicket;

    public Ticket CreateTicket(string serviceType)
    {
        string prefix = serviceType switch
        {
            "Deposit" => "D",
            "Withdraw" => "W",
            "Exchange" => "E",
            _ => "X"
        };

        int number = serviceType switch
        {
            "Deposit" => _depositCounter++,
            "Withdraw" => _withdrawCounter++,
            "Exchange" => _exchangeCounter++,
            _ => 0
        };

        var ticket = new Ticket
        {
            Number = $"{prefix}{number:000}",
            ServiceType = serviceType
        };

        _queue.Enqueue(ticket);

        return ticket;
    }

    public Ticket? GetNext()
    {
        if (_queue.Count == 0)
            return null;

        _currentTicket = _queue.Dequeue();

        return _currentTicket;
    }

    public Ticket? GetCurrent()
    {
        return _currentTicket;
    }
}