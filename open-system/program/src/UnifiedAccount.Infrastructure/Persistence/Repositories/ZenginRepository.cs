using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Zengin;

namespace UnifiedAccount.Infrastructure.Persistence.Repositories;

/// <summary>
/// 全銀固有クエリ
/// </summary>
public class ZenginRepository : GenericRepository<ZenginBatch>
{
    public ZenginRepository(AppDbContext context) : base(context) { }

    public async Task<ZenginBatch?> GetWithTransactionsAsync(long id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(b => b.Transactions)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<ZenginBatch>> GetByDateAsync(
        DateOnly date, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(b => b.WithdrawalDate == date)
            .Include(b => b.Transactions)
            .ToListAsync(ct);
    }
}

