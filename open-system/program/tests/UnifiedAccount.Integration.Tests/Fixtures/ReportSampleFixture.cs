using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedAccount.Infrastructure.Persistence;
using UnifiedAccount.Domain.Entities.Core;
using UnifiedAccount.Domain.Entities.Reports;
using UnifiedAccount.Integration.Tests.Helpers;

namespace UnifiedAccount.Integration.Tests.Fixtures;

/// <summary>このテストでは 対象条件を指定したとき、期待どおりの結果が得られる。</summary>
/// Test fixture that seeds AppDbContext with sample data suitable for report generation tests.
/// </summary>
public static class ReportSampleFixture
{
    public static AppDbContext CreateSeededContext(string? dbName = null)
    {
        var ctx = TestDbContextFactory.Create(dbName);
        Seed(ctx);
        return ctx;
    }

    public static void Seed(AppDbContext context)
    {
        // Prevent double-seed
        if (context.Companies.Any()) return;

        // 1) Companies (5)
        var companies = new List<Company>();
        for (int i = 1; i <= 5; i++)
        {
            companies.Add(new Company
            {
                CompanyCode = i.ToString("D6"),
                CompanyNameKana = $"ｶｲｼｬ{i}",
                CompanyNameKanji = $"会社{i}",
                ConsignorCode = $"C{i:D4}",
                BasicFee = 100m + i,
                AdminFee = 10m
            });
        }
        context.Companies.AddRange(companies);
        context.SaveChanges();

        // 2) Bank branches (3 banks, total 10 branches)
        var branches = new List<BankBranch>();
        var bankCodes = new[] { "0001", "0002", "0003" };
        int branchNo = 1;
        for (int b = 0; b < bankCodes.Length; b++)
        {
            int perBank = (b == 0) ? 4 : 3; // 4+3+3 = 10
            for (int j = 0; j < perBank; j++)
            {
                branches.Add(new BankBranch
                {
                    BankCode = bankCodes[b],
                    BranchCode = branchNo.ToString("D3"),
                    BankNameKana = $"銀行{bankCodes[b]}",
                    BranchNameKana = $"支店{branchNo}",
                    KanjiSetFlag = "0"
                });
                branchNo++;
            }
        }
        context.BankBranches.AddRange(branches);
        context.SaveChanges();

        // 3) Contracts (50)
        var contracts = new List<Contract>();
        var rng = new Random(12345);
        for (int i = 1; i <= 50; i++)
        {
            var company = companies[(i - 1) % companies.Count];
            var branch = branches[(i - 1) % branches.Count];
            var contract = new Contract
            {
                CompanyId = company.Id,
                CompanyCode = company.CompanyCode,
                PersonalCode = i.ToString("D12"),
                CheckDigit = (i % 10).ToString(),
                WithdrawalDay = (short)((i % 28) + 1),
                SuspendFlag = "0",
                NewFlag = "0",
                ZenginFlag = "1",
                DepositorNameKana = $"ﾃｽﾄ契約者{i}",
                BankCode = branch.BankCode,
                BranchCode = branch.BranchCode,
                AccountType = "1",
                AccountNo = i.ToString("D10")
            };
            contracts.Add(contract);
        }
        context.Contracts.AddRange(contracts);
        context.SaveChanges();

        // 4) TransferTransactions (30 success, 15 failure markers)
        var transactions = new List<TransferTransaction>();
        for (int i = 1; i <= 45; i++)
        {
            var c = contracts[(i - 1) % contracts.Count];
            var txn = new TransferTransaction
            {
                CompanyId = c.CompanyId,
                ConsignorCode = c.Company!.ConsignorCode,
                ConsignorName = c.Company!.CompanyNameKanji ?? c.Company!.CompanyNameKana,
                WithdrawalDate = DateOnly.FromDateTime(new DateTime(2026, 3, 19)),
                BankCode = c.BankCode,
                BranchCode = c.BranchCode,
                BankName = branches.FirstOrDefault(b => b.BankCode == c.BankCode)?.BankNameKana,
                BranchName = branches.FirstOrDefault(b => b.BranchCode == c.BranchCode)?.BranchNameKana,
                AccountType = c.AccountType,
                AccountNo = c.AccountNo,
                DepositorName = c.DepositorNameKana,
                Amount = 1000m + (i * 10),
                NewCode = "0",
                CompanyCode = c.CompanyCode,
                PersonalCode = c.PersonalCode,
                CheckDigit = c.CheckDigit,
                ResultCode = (i > 30) ? "1" : null,
                TransferRound = 1
            };
            transactions.Add(txn);
        }
        context.TransferTransactions.AddRange(transactions);
        context.SaveChanges();

        // 5) TransferFailures: create failure records for last 15 transactions
        var failures = new List<TransferFailure>();
        for (int i = 31; i <= 45; i++)
        {
            var txn = transactions[i - 1];
            var matchingContract = contracts.FirstOrDefault(x => x.PersonalCode == txn.PersonalCode && x.CompanyCode == txn.CompanyCode);
            failures.Add(new TransferFailure
            {
                ContractId = matchingContract?.Id,
                WithdrawalDay = txn.WithdrawalDate.Day.ToString("D2"),
                TransferType = "01",
                CompanyCode = txn.CompanyCode,
                PersonalCode = txn.PersonalCode,
                CheckDigit = txn.CheckDigit,
                ResultCode = "9",
                TapeType = "1",
                BankCode = txn.BankCode,
                TransferRound = txn.TransferRound
            });
        }
        context.TransferFailures.AddRange(failures);
        context.SaveChanges();

        // 6) CoverLetters (sample 送付状データ)
        var coverLetters = new List<CoverLetter>();
        for (int i = 1; i <= 3; i++)
        {
            var comp = companies[(i - 1) % companies.Count];
            coverLetters.Add(new CoverLetter
            {
                CompanyCode = comp.CompanyCode,
                CompanyName = comp.CompanyNameKanji ?? comp.CompanyNameKana,
                ProcessDate = DateOnly.FromDateTime(DateTime.Today),
                ContentText = $"振替明細のご案内 (サンプル) - {i}"
            });
        }
        context.CoverLetters.AddRange(coverLetters);
        context.SaveChanges();

        // 7) UcvBillingDetails (sample UCV 請求明細)
        var ucvList = new List<UcvBillingDetail>();
        for (int i = 1; i <= 6; i++)
        {
            var comp = companies[(i - 1) % companies.Count];
            ucvList.Add(new UcvBillingDetail
            {
                CompanyCode = comp.CompanyCode,
                BillingYearMonth = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)),
                ItemCode = (100 + i).ToString("D4"),
                UnitPrice = 1000m + i * 10,
                Quantity = i,
                TotalAmount = (1000m + i * 10) * i,
                TaxRate = 10m,
                TaxAmount = Math.Round(((1000m + i * 10) * i) * 0.1m, 0),
                InvoiceFlag = (i % 2 == 0)
            });
        }
        context.UcvBillingDetails.AddRange(ucvList);
        context.SaveChanges();
    }
}








