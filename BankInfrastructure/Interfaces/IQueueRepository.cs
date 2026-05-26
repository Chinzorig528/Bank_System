using BankDomain.Entities;

namespace BankInfrastructure.Interfaces;

/// <summary>
/// Харилцагчийн queue дугаарын өгөгдөлтэй ажиллах repository-ийн гэрээ.
/// </summary>
public interface IQueueRepository
{
    /// <summary>
    /// Шинээр үүсгэсэн queue дугаарыг өгөгдлийн санд нэмнэ.
    /// </summary>
    /// <param name="queue">Хадгалах queue бичлэг.</param>
    Task AddAsync(CustomerQueue queue);

    /// <summary>
    /// Бүх queue бичлэгийг жагсаалтаар авна.
    /// </summary>
    /// <returns>Queue бичлэгүүдийн жагсаалт.</returns>
    Task<List<CustomerQueue>> GetAllAsync();

    /// <summary>
    /// Дуудагдаагүй хамгийн эхний queue дугаарыг авна.
    /// </summary>
    /// <returns>Дараагийн дуудах queue, байхгүй бол null.</returns>
    Task<CustomerQueue?> GetNextAsync();

    /// <summary>
    /// Repository дээр хийгдсэн өөрчлөлтүүдийг өгөгдлийн санд хадгална.
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// Хамгийн сүүлд үүссэн queue бичлэгийг авна.
    /// </summary>
    /// <returns>Сүүлчийн queue бичлэг, байхгүй бол null.</returns>
    Task<CustomerQueue?> GetLastQueueAsync();
}
