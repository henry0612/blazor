-- ============================================================
-- サンプルデータ投入スクリプト（F-CHO-004 結果ファイル出力）
-- 用途: 先行開発の委託先が BAT-CHO230（結果ファイル出力）を自環境で動作確認するための正常データ
-- 注意: 本番環境へ適用してはならない
-- 注意: 冪等（IF NOT EXISTS / WHERE NOT EXISTS）で設計する。複数回実行しても件数は増えない
-- 前提: db/migrations/V001__InitialSchema.sql と db/testdata/DevelopmentData.sql の投入が完了していること
-- 対象テーブルは cho スキーマの 3 テーブルのみ（dbo は変更しない）
-- レコード構成: ReceiptSequence 1=ヘッダ(1), 2-18=データ17件(2), 19=トレーラ(8), 20=エンド(9)
-- トレーラの集計値（TotalCount/TotalAmount/SettledCount/SettledAmount/FailedCount/FailedAmount）は
-- データ17件（ReceiptSequence 2-18）の実値から算出した値と一致させている（本ファイル末尾の検算コメント参照）。
-- ============================================================
USE [UnifiedAccount];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- ============================================================
-- TM_BatchControlParameters (制御パラメータ) 1件
-- UQ_BatchControlParameters_Key により ParameterKey が一意である。
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [cho].[TM_BatchControlParameters] WHERE [ParameterKey] = N'CHO_CTL')
BEGIN
    INSERT INTO [cho].[TM_BatchControlParameters] ([ParameterKey], [ProcessingDate], [TransferDate])
    VALUES (N'CHO_CTL', CAST('2026-08-27' AS date), CAST('2026-08-27' AS date));
END;

-- ============================================================
-- TD_CooperativeTransferReceipts (受領ヘッダ) 1件
-- RecordCount はヘッダ、明細、トレーラ、エンドを含む全レコード件数（1 + 17 + 1 + 1 = 20）。
-- TD_CooperativeTransfers は JobExecutionId で本テーブルへ FK 参照するため、先に投入する。
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [cho].[TD_CooperativeTransferReceipts] WHERE [JobExecutionId] = N'CHO100-20260827-0001')
BEGIN
    INSERT INTO [cho].[TD_CooperativeTransferReceipts]
        ([JobExecutionId], [Status], [StagingIdentifier], [ProcessingYearMonth], [TransferMonth], [RecordCount])
    VALUES
        (N'CHO100-20260827-0001', N'Confirmed', N'F-IF-013_CHO100-20260827-0001', N'202608', N'08', 20);
END;

-- ============================================================
-- TD_CooperativeTransfers (共済振替データ) 20件
-- 1=ヘッダ, 2-18=データ17件, 19=トレーラ, 20=エンド。
-- 存在判定は UQ_CooperativeTransfers_JobExecutionId_ReceiptSequence に合わせ (JobExecutionId, ReceiptSequence) で行う。
-- ============================================================
DECLARE @Job NVARCHAR(50) = N'CHO100-20260827-0001';
DECLARE @Kana NVARCHAR(40) = N'ｾﾞﾝｷｮｳﾚﾝ ﾅｶﾞﾉｹﾝ ｷｮｳｻｲﾚﾝ';
DECLARE @Koz CHAR(6) = N'500660';
DECLARE @MYM CHAR(6) = N'202608';
DECLARE @WM CHAR(2) = N'08';
DECLARE @WD CHAR(2) = N'27';
DECLARE @Consignor CHAR(10) = N'0500660000';

INSERT INTO [cho].[TD_CooperativeTransfers]
    ([JobExecutionId], [ReceiptSequence], [Status], [RecordType], [ConsignorCode], [ConsignorNameKana], [WithdrawalMonth], [WithdrawalDay],
     [BankCode], [BranchCode], [ContractNo], [MemberCode], [PlanCode], [CooperativeNo], [BranchOfficeNo], [BillingBranchOfficeNo],
     [InsuranceType], [PaymentMethod], [ContractDate], [AnnualMonthlyType], [AccountType], [AccountNo], [DepositorName], [Amount],
     [NewCode], [InvariantNo], [PremiumYearMonth], [ResultCode], [PostalBankCode], [PostalBranchCode], [PostalAccountType], [PostalAccountNo],
     [KozConsignorCode], [MatchYearMonth], [MatchSeqNo],
     [TotalCount], [TotalAmount], [SettledCount], [SettledAmount], [FailedCount], [FailedAmount])
SELECT
    @Job, src.ReceiptSequence, N'Confirmed', src.RecordType, src.ConsignorCode, src.ConsignorNameKana, src.WithdrawalMonth, src.WithdrawalDay,
    src.BankCode, src.BranchCode, src.ContractNo, src.MemberCode, src.PlanCode, src.CooperativeNo, src.BranchOfficeNo, src.BillingBranchOfficeNo,
    src.InsuranceType, src.PaymentMethod, src.ContractDate, src.AnnualMonthlyType, src.AccountType, src.AccountNo, src.DepositorName, src.Amount,
    src.NewCode, src.InvariantNo, src.PremiumYearMonth, src.ResultCode, src.PostalBankCode, src.PostalBranchCode, src.PostalAccountType, src.PostalAccountNo,
    src.KozConsignorCode, src.MatchYearMonth, src.MatchSeqNo,
    src.TotalCount, src.TotalAmount, src.SettledCount, src.SettledAmount, src.FailedCount, src.FailedAmount
FROM (VALUES
    (1, N'1', @Consignor, @Kana, @WM, @WD, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @Koz, @MYM, NULL, NULL, NULL, NULL, NULL, NULL, NULL), -- ヘッダ
    (2, N'2', @Consignor, @Kana, @WM, @WD, N'0543', N'001', N'0002', N'000002', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000001', N'ﾃｽﾄ ﾀﾛｳ02', CAST(12000 AS DECIMAL(10,0)), N'0', N'00000000000002', N'202608', N'0', N'9900', N'001', N'1', N'0000000002', @Koz, @MYM, 2, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=2
    (3, N'2', @Consignor, @Kana, @WM, @WD, N'0543', N'101', N'0003', N'000003', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000002', N'ﾃｽﾄ ﾀﾛｳ03', CAST(8500 AS DECIMAL(10,0)), N'0', N'00000000000003', N'202608', N'0', N'9900', N'001', N'1', N'0000000003', @Koz, @MYM, 3, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=3
    (4, N'2', @Consignor, @Kana, @WM, @WD, N'0543', N'201', N'0004', N'000004', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000003', N'ﾃｽﾄ ﾀﾛｳ04', CAST(23000 AS DECIMAL(10,0)), N'0', N'00000000000004', N'202608', N'0', N'9900', N'001', N'1', N'0000000004', @Koz, @MYM, 4, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=4
    (5, N'2', @Consignor, @Kana, @WM, @WD, N'3054', N'001', N'0005', N'000005', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000004', N'ﾃｽﾄ ﾀﾛｳ05', CAST(5000 AS DECIMAL(10,0)), N'0', N'00000000000005', N'202608', N'0', N'9900', N'001', N'1', N'0000000005', @Koz, @MYM, 5, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=5
    (6, N'2', @Consignor, @Kana, @WM, @WD, N'3054', N'002', N'0006', N'000006', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'2', N'1000005', N'ﾃｽﾄ ﾀﾛｳ06', CAST(31000 AS DECIMAL(10,0)), N'0', N'00000000000006', N'202608', N'0', N'9900', N'001', N'1', N'0000000006', @Koz, @MYM, 6, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=6
    (7, N'2', @Consignor, @Kana, @WM, @WD, N'1025', N'001', N'0007', N'000007', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000006', N'ﾃｽﾄ ﾀﾛｳ07', CAST(7200 AS DECIMAL(10,0)), N'0', N'00000000000007', N'202608', N'1', N'9900', N'001', N'1', N'0000000007', @Koz, @MYM, 7, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=7
    (8, N'2', @Consignor, @Kana, @WM, @WD, N'1025', N'002', N'0008', N'000008', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000007', N'ﾃｽﾄ ﾀﾛｳ08', CAST(15800 AS DECIMAL(10,0)), N'0', N'00000000000008', N'202608', N'0', N'9900', N'001', N'1', N'0000000008', @Koz, @MYM, 8, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=8
    (9, N'2', @Consignor, @Kana, @WM, @WD, N'0001', N'001', N'0009', N'000009', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000008', N'ﾃｽﾄ ﾀﾛｳ09', CAST(9900 AS DECIMAL(10,0)), N'0', N'00000000000009', N'202608', N'0', N'9900', N'001', N'1', N'0000000009', @Koz, @MYM, 9, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=9
    (10, N'2', @Consignor, @Kana, @WM, @WD, N'0001', N'101', N'0010', N'000010', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000009', N'ﾃｽﾄ ﾀﾛｳ10', CAST(45000 AS DECIMAL(10,0)), N'0', N'00000000000010', N'202608', N'0', N'9900', N'001', N'1', N'0000000010', @Koz, @MYM, 10, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=10
    (11, N'2', @Consignor, @Kana, @WM, @WD, N'0005', N'001', N'0011', N'000011', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000010', N'ﾃｽﾄ ﾀﾛｳ11', CAST(3000 AS DECIMAL(10,0)), N'0', N'00000000000011', N'202608', N'0', N'9900', N'001', N'1', N'0000000011', @Koz, @MYM, 11, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=11
    (12, N'2', @Consignor, @Kana, @WM, @WD, N'0005', N'201', N'0012', N'000012', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'2', N'1000011', N'ﾃｽﾄ ﾀﾛｳ12', CAST(62000 AS DECIMAL(10,0)), N'0', N'00000000000012', N'202608', N'0', N'9900', N'001', N'1', N'0000000012', @Koz, @MYM, 12, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=12
    (13, N'2', @Consignor, @Kana, @WM, @WD, N'0009', N'001', N'0013', N'000013', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000012', N'ﾃｽﾄ ﾀﾛｳ13', CAST(11000 AS DECIMAL(10,0)), N'0', N'00000000000013', N'202608', N'0', N'9900', N'001', N'1', N'0000000013', @Koz, @MYM, 13, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=13
    (14, N'2', @Consignor, @Kana, @WM, @WD, N'9900', N'001', N'0014', N'000014', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000013', N'ﾃｽﾄ ﾀﾛｳ14', CAST(4400 AS DECIMAL(10,0)), N'0', N'00000000000014', N'202608', N'1', N'9900', N'001', N'1', N'0000000014', @Koz, @MYM, 14, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=14
    (15, N'2', @Consignor, @Kana, @WM, @WD, N'1234', N'001', N'0015', N'000015', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000014', N'ﾃｽﾄ ﾀﾛｳ15', CAST(27500 AS DECIMAL(10,0)), N'0', N'00000000000015', N'202608', N'0', N'9900', N'001', N'1', N'0000000015', @Koz, @MYM, 15, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=15
    (16, N'2', @Consignor, @Kana, @WM, @WD, N'1234', N'002', N'0016', N'000016', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000015', N'ﾃｽﾄ ﾀﾛｳ16', CAST(18000 AS DECIMAL(10,0)), N'0', N'00000000000016', N'202608', N'0', N'9900', N'001', N'1', N'0000000016', @Koz, @MYM, 16, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=16
    (17, N'2', @Consignor, @Kana, @WM, @WD, N'0543', N'001', N'0017', N'000017', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000016', N'ﾃｽﾄ ﾀﾛｳ17', CAST(300000 AS DECIMAL(10,0)), N'0', N'00000000000017', N'202608', N'0', N'9900', N'001', N'1', N'0000000017', @Koz, @MYM, 17, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=17
    (18, N'2', @Consignor, @Kana, @WM, @WD, N'0543', N'101', N'0018', N'000018', N'0101', N'001', N'010', N'010', N'01', N'1', N'250401', N'2', N'1', N'1000017', N'ﾃｽﾄ ﾀﾛｳ18', CAST(6300 AS DECIMAL(10,0)), N'0', N'00000000000018', N'202608', N'0', N'9900', N'001', N'1', N'0000000018', @Koz, @MYM, 18, NULL, NULL, NULL, NULL, NULL, NULL), -- データ Seq=18
    (19, N'8', NULL, @Kana, @WM, @WD, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @Koz, @MYM, NULL, 17, CAST(589600 AS DECIMAL(12,0)), 15, CAST(578000 AS DECIMAL(12,0)), 2, CAST(11600 AS DECIMAL(12,0))), -- トレーラ
    (20, N'9', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, @Koz, @MYM, 20, NULL, NULL, NULL, NULL, NULL, NULL) -- エンド
) AS src(ReceiptSequence, RecordType, ConsignorCode, ConsignorNameKana, WithdrawalMonth, WithdrawalDay,
         BankCode, BranchCode, ContractNo, MemberCode, PlanCode, CooperativeNo, BranchOfficeNo, BillingBranchOfficeNo,
         InsuranceType, PaymentMethod, ContractDate, AnnualMonthlyType, AccountType, AccountNo, DepositorName, Amount,
         NewCode, InvariantNo, PremiumYearMonth, ResultCode, PostalBankCode, PostalBranchCode, PostalAccountType, PostalAccountNo,
         KozConsignorCode, MatchYearMonth, MatchSeqNo,
         TotalCount, TotalAmount, SettledCount, SettledAmount, FailedCount, FailedAmount)
WHERE NOT EXISTS (
    SELECT 1 FROM [cho].[TD_CooperativeTransfers] t
    WHERE t.JobExecutionId = @Job AND t.ReceiptSequence = src.ReceiptSequence
);

COMMIT TRANSACTION;
PRINT 'Sample-F-CHO-004 投入完了';

