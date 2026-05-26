using Bank.Domain.Entities;
using BankInfrastructure.Data;

using Microsoft.AspNetCore.Mvc;

namespace Bank.API.Controllers
{
    /// <summary>
    /// Харилцагч бүртгэх API endpoint.
    /// </summary>
    [ApiController]
    [Route("customer")]
    public class CustomerController : ControllerBase
    {
        private readonly BankDbContext _context;

        /// <summary>
        /// Customer controller-д өгөгдлийн сангийн context-ийг онооно.
        /// </summary>
        /// <param name="context">Харилцагчийн мэдээлэл хадгалах database context.</param>
        public CustomerController(
            BankDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Шинэ харилцагчийг дансны дугаар, утас, эхний үлдэгдэлтэй бүртгэнэ.
        /// </summary>
        /// <param name="dto">Бүртгэх харилцагчийн мэдээлэл.</param>
        /// <returns>Амжилттай бол үүсгэсэн харилцагч, шалгалт амжилтгүй бол BadRequest.</returns>
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
