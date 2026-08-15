using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Core;

namespace UnifiedAccount.Infrastructure.Persistence.Repositories;

/// <summary>
/// 契約者マスター固有クエリ
/// </summary>
public class ContractRepository : GenericRepository<Contract>
{
    public ContractRepository(AppDbContext context) : base(context) { }

    public async Task<Contract?> GetByBusinessKeyAsync(
        string companyCode, string personalCode, string checkDigit, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.Types)
            .Include(c => c.BillingAmounts)
            .FirstOrDefaultAsync(c =>
                c.CompanyCode == companyCode &&
                c.PersonalCode == personalCode &&
                c.CheckDigit == checkDigit, ct);
    }

    public async Task<IReadOnlyList<Contract>> GetByCompanyCodeAsync(
        string companyCode, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CompanyCode == companyCode)
            .OrderBy(c => c.PersonalCode)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Contract>> SearchByBankAccountAsync(
        string bankCode, string branchCode, string accountNo, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.BankCode == bankCode && c.BranchCode == branchCode && c.AccountNo == accountNo)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Contract>> SearchByDepositorNameAsync(
        string namePart, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.DepositorNameKana.Contains(namePart))
            .OrderBy(c => c.CompanyCode)
            .ThenBy(c => c.PersonalCode)
            .ToListAsync(ct);
    }
}

