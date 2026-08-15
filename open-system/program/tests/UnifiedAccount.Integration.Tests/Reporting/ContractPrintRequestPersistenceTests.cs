using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Infrastructure.Persistence;

namespace UnifiedAccount.Integration.Tests.Reporting;

public class ContractPrintRequestPersistenceTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public ContractPrintRequestPersistenceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"contract_print_request_{Guid.NewGuid():N}")
            .Options;

        _dbContext = new AppDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    /// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
    [Fact]
    public async Task SaveAndLoad_ShouldPersistContractPrintRequest()
    {
        // Arrange
        var request = new ContractPrintRequest
        {
            JobExecutionId = "JOB-KOZ120N-20260630-000001",
            RequestOrder = 1,
            CompanyCode = "000001",
            PersonCodeFrom = "000000000001",
            PersonCodeTo = "000000000010",
            AllCompanyFlag = false,
            Status = "0",
            PrintedCount = 0,
            AuditCount = 0
        };

        // Act
        _dbContext.ContractPrintRequests.Add(request);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.ContractPrintRequests
            .AsNoTracking()
            .SingleAsync(x => x.JobExecutionId == request.JobExecutionId && x.RequestOrder == request.RequestOrder);

        saved.Should().NotBeNull();
        saved.CompanyCode.Should().Be("000001");
        saved.PersonCodeFrom.Should().Be("000000000001");
        saved.PersonCodeTo.Should().Be("000000000010");
        saved.Status.Should().Be("0");
    }
}










