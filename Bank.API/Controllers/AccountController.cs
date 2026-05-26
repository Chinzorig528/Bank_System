using Bank.Application.DTOs;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Controllers
{
    /// <summary>
    /// Данс үүсгэх, орлого хийх, зарлага гаргах, шилжүүлэг хийх болон үлдэгдэл шалгах API endpoint-ууд.
    /// </summary>
    [ApiController]

    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly BankDbContext _db;

        private readonly TransactionChannelService
            _channel;

        /// <summary>
        /// Account controller-д өгөгдлийн сан болон transaction channel-ийг онооно.
        /// </summary>
        /// <param name="db">Дансны мэдээлэл хадгалах database context.</param>
        /// <param name="channel">Орлого, зарлагын хүсэлт worker руу дамжуулах channel.</param>
        public AccountController(
            BankDbContext db,
            TransactionChannelService channel)
        {
            _db = db;

            _channel = channel;
        }



        /// <summary>
        /// Шинэ данс үүсгэнэ.
        /// </summary>
        /// <param name="accountNumber">Үүсгэх дансны дугаар.</param>
        /// <returns>Амжилттай бол баталгаажуулах мессеж, давхцвал алдаа.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> Create(
            string accountNumber)
        {
            var exists =
                await _db.BankAccounts
                    .AnyAsync(x =>
                        x.AccountNumber ==
                        accountNumber);

            if (exists)
            {
                return BadRequest(
                    "Account already exists");
            }

            var account = new BankAccount
            {
                AccountNumber =
                    accountNumber,

                Balance = 0,

                CreatedAt =
                    DateTime.Now
            };

            _db.BankAccounts.Add(account);

            await _db.SaveChangesAsync();

            return Ok(
                "Account created");
        }



        /// <summary>
        /// Дансанд орлого хийх хүсэлтийг transaction worker руу илгээнэ.
        /// </summary>
        /// <param name="dto">Дансны дугаар болон орлогын дүн.</param>
        /// <returns>Орлого амжилттай хийгдсэн эсэх boolean үр дүн.</returns>
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(
            DepositDto dto)
        {
            var request =
                new TransactionRequest
                {
                    AccountNumber =
                        dto.AccountNumber,

                    Amount =
                        dto.Amount,

                    Type =
                        TransactionType.Deposit
                };

            await _channel.Queue.Writer
                .WriteAsync(request);

            var result =
                await request
                    .CompletionSource.Task;

            return Ok(result);
        }



        /// <summary>
        /// Данснаас зарлага гаргах хүсэлтийг transaction worker руу илгээнэ.
        /// </summary>
        /// <param name="dto">Дансны дугаар болон зарлагын дүн.</param>
        /// <returns>Амжилттай бол OK, үлдэгдэл хүрэхгүй бол BadRequest.</returns>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(
            WithdrawDto dto)
        {
            var request =
                new TransactionRequest
                {
                    AccountNumber =
                        dto.AccountNumber,

                    Amount =
                        dto.Amount,

                    Type =
                        TransactionType.Withdraw
                };

            await _channel.Queue.Writer
                .WriteAsync(request);

            var result =
                await request
                    .CompletionSource.Task;

            if (!result)
            {
                return BadRequest(
                    "Insufficient balance");
            }

            return Ok();
        }



        /// <summary>
        /// Дансны дугаараар тухайн дансны үлдэгдлийг авна.
        /// </summary>
        /// <param name="accountNumber">Шалгах дансны дугаар.</param>
        /// <returns>Дансны үлдэгдэл, данс олдохгүй бол 404.</returns>
        [HttpGet("balance/{accountNumber}")]
        public async Task<IActionResult> Balance(
            string accountNumber)
        {
            var account =
                await _db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber ==
                        accountNumber);

            if (account == null)
            {
                return NotFound();
            }

            return Ok(account.Balance);
        }

        /// <summary>
        /// Нэг данснаас нөгөө данс руу мөнгө шилжүүлнэ.
        /// </summary>
        /// <param name="dto">Илгээгч данс, хүлээн авагч данс болон шилжүүлэх дүн.</param>
        /// <returns>Шилжүүлэг амжилттай эсэх үр дүн.</returns>
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer(
    TransferDto dto)
        {
            // =========================
            // VALIDATION
            // =========================

            if (dto.Amount <= 0)
            {
                return BadRequest(
                    "Amount must be greater than 0");
            }

            if (dto.FromAccount ==
                dto.ToAccount)
            {
                return BadRequest(
                    "Cannot transfer to same account");
            }

            // =========================
            // FIND ACCOUNTS
            // =========================

            var sender =
                await _db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber ==
                        dto.FromAccount);

            if (sender == null)
            {
                return BadRequest(
                    "Sender account not found");
            }

            var receiver =
                await _db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber ==
                        dto.ToAccount);

            if (receiver == null)
            {
                return BadRequest(
                    "Receiver account not found");
            }

            // =========================
            // BALANCE CHECK
            // =========================

            if (sender.Balance <
                dto.Amount)
            {
                return BadRequest(
                    "Insufficient balance");
            }

            // =========================
            // TRANSFER
            // =========================

            sender.Balance -= dto.Amount;

            receiver.Balance += dto.Amount;

            await _db.SaveChangesAsync();

            return Ok(
                "Transfer successful");
        }



        /// <summary>
        /// Үлдэгдэл нь 0 болсон дансыг устгана.
        /// </summary>
        /// <param name="accountNumber">Устгах дансны дугаар.</param>
        /// <returns>Амжилттай бол OK, данс олдохгүй эсвэл үлдэгдэлтэй бол алдаа.</returns>
        [HttpDelete("delete/{accountNumber}")]
        public async Task<IActionResult> Delete(
            string accountNumber)
        {
            var account =
                await _db.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.AccountNumber ==
                        accountNumber);

            if (account == null)
            {
                return NotFound();
            }

            if (account.Balance != 0)
            {
                return BadRequest(
                    "Balance must be 0");
            }

            _db.BankAccounts.Remove(account);

            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
