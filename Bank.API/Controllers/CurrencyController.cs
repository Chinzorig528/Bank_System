using BankApi.Hubs;
using BankDomain.Entities;
using BankInfrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Controllers
{
    /// <summary>
    /// Валютын ханшийг авах, эхлүүлэх, шинэчлэх API endpoint-ууд.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly BankDbContext _context;
        private readonly IHubContext<CurrencyHub> _hubContext;

        /// <summary>
        /// Валютын controller-д өгөгдлийн сан болон SignalR hub context-ийг онооно.
        /// </summary>
        /// <param name="context">Валютын ханш хадгалах database context.</param>
        /// <param name="hubContext">Ханшийн өөрчлөлтийг realtime илгээх hub context.</param>
        public CurrencyController(
            BankDbContext context,
            IHubContext<CurrencyHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Бүх валютын ханшийг кодоор эрэмбэлж авна.
        /// </summary>
        /// <returns>Валютын ханшийн жагсаалт.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rates = await _context.CurrencyRates
                .OrderBy(x => x.Code)
                .ToListAsync();

            return Ok(rates);
        }

        /// <summary>
        /// Валютын ханш хоосон үед анхны жишээ ханшуудыг үүсгэнэ.
        /// </summary>
        /// <returns>Одоогийн бүх валютын ханш.</returns>
        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            if (await _context.CurrencyRates.AnyAsync())
            {
                var existingRates = await _context.CurrencyRates
                    .OrderBy(x => x.Code)
                    .ToListAsync();

                return Ok(existingRates);
            }

            var rates = new List<CurrencyRate>
            {
                new CurrencyRate
                {
                    Code = "USD",
                    Name = "Америк доллар",
                    BuyRate = 3450,
                    SellRate = 3470,
                    UpdatedAt = DateTime.Now
                },
                new CurrencyRate
                {
                    Code = "EUR",
                    Name = "Евро",
                    BuyRate = 3700,
                    SellRate = 3740,
                    UpdatedAt = DateTime.Now
                },
                new CurrencyRate
                {
                    Code = "CNY",
                    Name = "Юань",
                    BuyRate = 475,
                    SellRate = 482,
                    UpdatedAt = DateTime.Now
                },
                new CurrencyRate
                {
                    Code = "JPY",
                    Name = "Иен",
                    BuyRate = 22,
                    SellRate = 24,
                    UpdatedAt = DateTime.Now
                },
                new CurrencyRate
                {
                    Code = "KRW",
                    Name = "Вон",
                    BuyRate = 2.5m,
                    SellRate = 2.8m,
                    UpdatedAt = DateTime.Now
                }
            };

            _context.CurrencyRates.AddRange(rates);
            await _context.SaveChangesAsync();

            var allRates = await _context.CurrencyRates
                .OrderBy(x => x.Code)
                .ToListAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveCurrencyRates", allRates);

            return Ok(allRates);
        }

        /// <summary>
        /// Нэг валютын ханшийг шинэчилж бүх teller app руу realtime мэдэгдэнэ.
        /// </summary>
        /// <param name="id">Шинэчлэх валютын ID.</param>
        /// <param name="updatedRate">Шинэ ханшийн мэдээлэл.</param>
        /// <returns>Шинэчлэгдсэн валютын ханш.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CurrencyRate updatedRate)
        {
            var rate = await _context.CurrencyRates.FindAsync(id);

            if (rate == null)
            {
                return NotFound(new
                {
                    message = "Ийм ID-тай валютын ханш олдсонгүй.",
                    id
                });
            }

            rate.Code = updatedRate.Code;
            rate.Name = updatedRate.Name;
            rate.BuyRate = updatedRate.BuyRate;
            rate.SellRate = updatedRate.SellRate;
            rate.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var allRates = await _context.CurrencyRates
                .OrderBy(x => x.Code)
                .ToListAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveCurrencyRates", allRates);

            return Ok(rate);
        }

        /// <summary>
        /// Олон валютын ханшийг нэг дор шинэчилж realtime мэдэгдэл илгээнэ.
        /// </summary>
        /// <param name="updatedRates">Шинэчлэх валютын ханшууд.</param>
        /// <returns>Шинэчлэгдсэний дараах бүх ханш.</returns>
        [HttpPost("update-all")]
        public async Task<IActionResult> UpdateAll([FromBody] List<CurrencyRate> updatedRates)
        {
            if (updatedRates == null || updatedRates.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Шинэчлэх ханшийн жагсаалт хоосон байна."
                });
            }

            foreach (var updatedRate in updatedRates)
            {
                var existingRate = await _context.CurrencyRates
                    .FirstOrDefaultAsync(x => x.Id == updatedRate.Id);

                if (existingRate != null)
                {
                    existingRate.Code = updatedRate.Code;
                    existingRate.Name = updatedRate.Name;
                    existingRate.BuyRate = updatedRate.BuyRate;
                    existingRate.SellRate = updatedRate.SellRate;
                    existingRate.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            var allRates = await _context.CurrencyRates
                .OrderBy(x => x.Code)
                .ToListAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveCurrencyRates", allRates);

            return Ok(allRates);
        }
    }
}
