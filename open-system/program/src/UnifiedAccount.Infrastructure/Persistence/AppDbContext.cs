using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UnifiedAccount.Domain.Entities.Apply;
using UnifiedAccount.Domain.Entities.Changes;
using UnifiedAccount.Domain.Entities.Cho;
using UnifiedAccount.Domain.Common;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Domain.Entities.Zengin;

namespace UnifiedAccount.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Authentication
    public DbSet<User> Users => Set<User>();

    // Tier 1: Core (9 + 5子テーブル)
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyWithdrawalDay> CompanyWithdrawalDays => Set<CompanyWithdrawalDay>();
    public DbSet<CompanyType> CompanyTypes => Set<CompanyType>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractType> ContractTypes => Set<ContractType>();
    public DbSet<ContractBillingAmount> ContractBillingAmounts => Set<ContractBillingAmount>();
    public DbSet<BankBranch> BankBranches => Set<BankBranch>();
    public DbSet<ProcessingCalendar> ProcessingCalendars => Set<ProcessingCalendar>();
    public DbSet<ProcessingSlot> ProcessingSlots => Set<ProcessingSlot>();
    public DbSet<TransferTransaction> TransferTransactions => Set<TransferTransaction>();
    public DbSet<TransferAmount> TransferAmounts => Set<TransferAmount>();
    public DbSet<TransferFailure> TransferFailures => Set<TransferFailure>();
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<JapaneseEra> JapaneseEras => Set<JapaneseEra>();
    public DbSet<CodeSetting> CodeSettings => Set<CodeSetting>();
    public DbSet<AmountSetting> AmountSettings => Set<AmountSetting>();
    public DbSet<BatchExecutionHistory> BatchExecutionHistories => Set<BatchExecutionHistory>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    // Tier 2: Changes
    public DbSet<AccountNumberChangeRequest> AccountNumberChangeRequests => Set<AccountNumberChangeRequest>();
    public DbSet<BankChangeRequest> BankChangeRequests => Set<BankChangeRequest>();
    public DbSet<CompanyChangeRequest> CompanyChangeRequests => Set<CompanyChangeRequest>();
    public DbSet<MessageChangeRequest> MessageChangeRequests => Set<MessageChangeRequest>();
    public DbSet<ContractChangeRequest> ContractChangeRequests => Set<ContractChangeRequest>();
    public DbSet<ContractChangeCard> ContractChangeCards => Set<ContractChangeCard>();
    public DbSet<AmountModification> AmountModifications => Set<AmountModification>();
    public DbSet<ContractorCodeConversion> ContractorCodeConversions => Set<ContractorCodeConversion>();
    public DbSet<BankBranchChange> BankBranchChanges => Set<BankBranchChange>();
    public DbSet<BatchErrorCheck> BatchErrorChecks => Set<BatchErrorCheck>();
    public DbSet<CollectionAmountRecord> CollectionAmountRecords => Set<CollectionAmountRecord>();

    // 確定前保持（Pending）方式
    public DbSet<ApplyRun> ApplyRuns => Set<ApplyRun>();
    public DbSet<ContractApplyPending> ContractApplyPendings => Set<ContractApplyPending>();
    public DbSet<AmountApplyPending> AmountApplyPendings => Set<AmountApplyPending>();

    // Tier 3: Zengin (5) — schema: zengin
    public DbSet<ZenginBatch> ZenginBatches => Set<ZenginBatch>();
    public DbSet<ZenginTransaction> ZenginTransactions => Set<ZenginTransaction>();
    public DbSet<ZenginTransmissionLog> ZenginTransmissionLogs => Set<ZenginTransmissionLog>();
    public DbSet<AccumulatedZenginRecord> AccumulatedZenginRecords => Set<AccumulatedZenginRecord>();
    public DbSet<ZenginVerification> ZenginVerifications => Set<ZenginVerification>();

    // Tier 4: Cho (5) — schema: cho
    public DbSet<CooperativeTransfer> CooperativeTransfers => Set<CooperativeTransfer>();
    public DbSet<BatchControlParameter> BatchControlParameters => Set<BatchControlParameter>();
    public DbSet<ChoZenginBatch> ChoZenginBatches => Set<ChoZenginBatch>();
    public DbSet<ChoZenginTransaction> ChoZenginTransactions => Set<ChoZenginTransaction>();
    public DbSet<DailyAccountingEntry> DailyAccountingEntries => Set<DailyAccountingEntry>();
    public DbSet<BillingRecord> BillingRecords => Set<BillingRecord>();
    public DbSet<ProcessingInstruction> ProcessingInstructions => Set<ProcessingInstruction>();

    // Tier 5: Reports (13 + 3子テーブル + 実行履歴)
    public DbSet<CompanyMonthlySummary> CompanyMonthlySummaries => Set<CompanyMonthlySummary>();
    public DbSet<CompanyMonthlySummaryDetail> CompanyMonthlySummaryDetails => Set<CompanyMonthlySummaryDetail>();
    public DbSet<FeeAggregation> FeeAggregations => Set<FeeAggregation>();
    public DbSet<FeeAggregationDetail> FeeAggregationDetails => Set<FeeAggregationDetail>();
    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();
    public DbSet<InstitutionSummary> InstitutionSummaries => Set<InstitutionSummary>();
    public DbSet<ManagementAccounting> ManagementAccountings => Set<ManagementAccounting>();
    public DbSet<BatchParameter> BatchParameters => Set<BatchParameter>();
    public DbSet<BatchParameterCompany> BatchParameterCompanies => Set<BatchParameterCompany>();
    public DbSet<TransmissionParameter> TransmissionParameters => Set<TransmissionParameter>();
    public DbSet<BankHeadOffice> BankHeadOffices => Set<BankHeadOffice>();
    public DbSet<OutputLog> OutputLogs => Set<OutputLog>();
    public DbSet<AccountLink> AccountLinks => Set<AccountLink>();
    public DbSet<UcvBillingDetail> UcvBillingDetails => Set<UcvBillingDetail>();
    public DbSet<AmountErrorRecord> AmountErrorRecords => Set<AmountErrorRecord>();
    public DbSet<ContractMasterDeleteLog> ContractMasterDeleteLogs => Set<ContractMasterDeleteLog>();
    public DbSet<ErrorSuppressParameter> ErrorSuppressParameters => Set<ErrorSuppressParameter>();
    public DbSet<ContractPrintRequest> ContractPrintRequests => Set<ContractPrintRequest>();
    public DbSet<CompanyMasterChangeLog> CompanyMasterChangeLogs => Set<CompanyMasterChangeLog>();
    public DbSet<KanjiCompanyMasterChangeLog> KanjiCompanyMasterChangeLogs => Set<KanjiCompanyMasterChangeLog>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableAttribute = entityType.ClrType.GetCustomAttribute<TableAttribute>();
            if (tableAttribute is null)
            {
                continue;
            }

            entityType.SetTableName(tableAttribute.Name);

            if (!string.IsNullOrWhiteSpace(tableAttribute.Schema))
            {
                entityType.SetSchema(tableAttribute.Schema);
            }
        }

        // Schema setup
        modelBuilder.HasDefaultSchema("dbo");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = LocalDateTimeProvider.Now;
                    entry.Entity.UpdatedAt = LocalDateTimeProvider.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = LocalDateTimeProvider.Now;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
