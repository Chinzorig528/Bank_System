using BankDomain.Entities;

namespace BankServices.Interfaces;

/// <summary>
/// Queue дугаар үүсгэх, дараагийн дугаарыг дуудах бизнес логикийн гэрээ.
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// Шинэ queue дугаар үүсгэнэ.
    /// </summary>
    /// <returns>Үүсгэсэн queue бичлэг.</returns>
    Task<CustomerQueue> CreateQueueAsync();

    /// <summary>
    /// Дуудагдаагүй хамгийн эхний queue дугаарыг дуудсан төлөвт шилжүүлнэ.
    /// </summary>
    /// <returns>Дуудагдсан queue, байхгүй бол null.</returns>
    Task<CustomerQueue?> CallNextAsync();

    /// <summary>
    /// Бүх queue бичлэгийг авна.
    /// </summary>
    /// <returns>Queue бичлэгүүдийн жагсаалт.</returns>
    Task<List<CustomerQueue>> GetAllAsync();
}
