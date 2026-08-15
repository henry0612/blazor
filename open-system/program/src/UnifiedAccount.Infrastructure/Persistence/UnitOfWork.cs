using UnifiedAccount.Domain.Interfaces;

namespace UnifiedAccount.Infrastructure.Persistence;

/// <summary>
/// Unit of Work 実装
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    /// <summary>
    /// コンテキストを保持する。
    /// </summary>
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

