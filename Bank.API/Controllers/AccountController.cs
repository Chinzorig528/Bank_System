using Bank.Application.DTOs;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Controllers
{
    [ApiController]

    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly BankDbContext _db;

        private readonly TransactionChannelService
            _channel;

        public AccountController(
            BankDbContext db,
            TransactionChannelService channel)
        {
            _db = db;

            _channel = channel;
        }



        // ======================
        // CREATE ACCOUNT
        // ======================

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



        // ======================
        // DEPOSIT
        // ======================

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



        // ======================
        // WITHDRAW
        // ======================

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



        // ======================
        // GET BALANCE
        // ======================

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



        // ======================
        // DELETE ACCOUNT
        // ======================

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