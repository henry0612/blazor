using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Integration.Tests.Reporting;

public class ContractMasterDeleteLogPersistenceTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public ContractMasterDeleteLogPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"contract_master_delete_log_{Guid.NewGuid():N}")
            .Options;

        _dbContext = new AppDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SaveAndLoad_ShouldPersistContractMasterDeleteLog()
    {
        var log = new ContractMasterDeleteLog
        {
            JobExecutionId = "JOB-F-REP-005-20260710-000001",
            CompanyCode = "000001",
            PersonalCode = "000000000001",
            DepositorName = "預金者名",
            ContractorName = "契約者名",
            BankCode = "0001",
            BranchCode = "001",
            AccountType = "1",
            AccountNumber = "1234567890",
            DeletionDate = new DateOnly(2026, 7, 10)
        };

        _dbContext.ContractMasterDeleteLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        var saved = await _dbContext.ContractMasterDeleteLogs
            .AsNoTracking()
            .SingleAsync(x =>
                x.JobExecutionId == log.JobExecutionId &&
                x.CompanyCode == log.CompanyCode &&
                x.PersonalCode == log.PersonalCode);

        saved.DeletionDate.Should().Be(new DateOnly(2026, 7, 10));
        saved.ContractorName.Should().Be("契約者名");
    }
}
