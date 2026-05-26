using Bank.Domain.Entities;
using BankInfrastructure.Data;

using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers
{
    [ApiController]
    [Route("customer")]
    public class CustomerController : ControllerBase
    {
        private readonly BankDbContext _context;

        public CustomerController(
            BankDbContext context)
        {
            _context = context;
        }

        // =========================
        // REGISTER CUSTOMER
        // =========================

        [HttpPost("create")]
        public async Task<IActionResult> Register(
            RegisterCustomerDto dto)
        {
            // =========================
            // VALIDATION
            // =========================

            if (dto.Balance < 0)
            {
                return BadRequest(
                    "Balance cannot be negative");
            }

            // account давхцахгүй

            bool exists =
                _context.Customers.Any(
                    x => x.AccountNumber ==
                    dto.AccountNumber);

            if (exists)
            {
                return BadRequest(
                    "Account already exists");
            }

            // =========================
            // SAVE
            // =========================

            Customer customer =
                new Customer
                {
                    FullName =
                        dto.FullName,

                    PhoneNumber =
                        dto.PhoneNumber,

                    AccountNumber =
                        dto.AccountNumber,

                    Balance =
                        dto.Balance
                };

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            return Ok(customer);
        }
    }
}