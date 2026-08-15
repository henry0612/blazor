using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Infrastructure.Persistence.Repositories;

/// <summary>
/// 会社マスター固有クエリ
/// </summary>
public class CompanyRepository : GenericRepository<Company>
{
    public CompanyRepository(AppDbContext context) : base(context) { }

    public async Task<Company?> GetByCompanyCodeAsync(string companyCode, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.WithdrawalDays)
            .Include(c => c.Types)
            .FirstOrDefaultAsync(c => c.CompanyCode == companyCode, ct);
    }

    public async Task<IReadOnlyList<Company>> SearchByNameAsync(string namePart, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CompanyNameKana.Contains(namePart))
            .OrderBy(c => c.CompanyCode)
            .ToListAsync(ct);
    }
}

