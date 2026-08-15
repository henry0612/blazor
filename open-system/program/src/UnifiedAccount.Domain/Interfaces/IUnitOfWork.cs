namespace UnifiedAccount.Domain.Interfaces;

/// <summary>
/// Unit of Work パターン — トランザクション制御
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
