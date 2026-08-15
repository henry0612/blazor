-- ============================================================
-- テストデータ投入スクリプト
-- 対象: 統一口座システム マスタテーブル
-- 用途: 開発・単体テスト・結合テスト環境への初期データ投入
-- 注意: 本番環境へ適用してはならない
-- 注意: 本スクリプトは冪等（MERGE または IF NOT EXISTS）で設計する
-- 注意: マイグレーションではない。db/migrations とは分離されており、Flyway の適用対象ではない
-- 前提: db/migrations/V001__InitialSchema.sql の投入が完了していること
-- ============================================================

-- 接続時の既定データベースに依存しないよう、対象データベースをスクリプト側で明示する。
USE [UnifiedAccount];
GO

SET NOCOUNT ON;

-- QUOTED_IDENTIFIER / ANSI_NULLS は実行クライアントの既定に依存させない（sqlcmd の既定は OFF）。
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- ============================================================
-- 1. TM_BankHeadOffices (銀行本店マスタ)
-- ============================================================
MERGE [dbo].[TM_BankHeadOffices] AS target
USING (VALUES
    -- (BankCode, HeadBranchCode, BankName)
    (N'0001', N'001', N'みずほ銀行'),
    (N'0005', N'001', N'三菱UFJ銀行'),
    (N'0009', N'001', N'三井住友銀行'),
    (N'0010', N'001', N'りそな銀行'),
    (N'0033', N'001', N'ジャパンネット銀行'),
    (N'9900', N'001', N'ゆうちょ銀行'),
    -- 長野県内主要金融機関
    (N'0543', N'001', N'八十二長野銀行'),
    (N'3054', N'001', N'長野県信用農業協同組合連合会'),  -- 県信連（JAバンク長野）
    (N'1025', N'001', N'長野信用金庫'),
    (N'1234', N'001', N'テスト銀行')
) AS source (BankCode, HeadBranchCode, BankName)
ON target.BankCode = source.BankCode AND target.HeadBranchCode = source.HeadBranchCode
WHEN NOT MATCHED THEN
    INSERT ([BankCode], [HeadBranchCode], [BankName])
    VALUES (source.BankCode, source.HeadBranchCode, source.BankName);


-- ============================================================
-- 2. TM_BankBranches (銀行支店マスタ)
-- ============================================================
MERGE [dbo].[TM_BankBranches] AS target
USING (VALUES
    -- (BankCode, BranchCode, BankNameKana, BranchNameKana, OfficeName, BankNameKanji, BranchNameKanji, KanjiSetFlag)
    (N'0001', N'001', N'ﾐｽﾞﾎ', N'ﾎﾝﾃﾝ', N'みずほ銀行 本店', N'みずほ銀行', N'本店', N'1'),
    (N'0001', N'101', N'ﾐｽﾞﾎ', N'ﾄｳｷｮｳ', N'みずほ銀行 東京支店', N'みずほ銀行', N'東京支店', N'1'),
    (N'0005', N'001', N'ﾐﾂﾋﾞｼUFJ', N'ﾎﾝﾃﾝ', N'三菱UFJ銀行 本店', N'三菱UFJ銀行', N'本店', N'1'),
    (N'0005', N'201', N'ﾐﾂﾋﾞｼUFJ', N'ｵｵｻｶ', N'三菱UFJ銀行 大阪支店', N'三菱UFJ銀行', N'大阪支店', N'1'),
    (N'0009', N'001', N'ﾐﾂｲｽﾐﾄﾓ', N'ﾎﾝﾃﾝ', N'三井住友銀行 本店', N'三井住友銀行', N'本店', N'1'),
    (N'9900', N'001', N'ﾕｳﾁｮ', N'ﾎﾝﾃﾝ', N'ゆうちょ銀行 本店', N'ゆうちょ銀行', N'本店', N'1'),
    -- 長野県内主要金融機関
    (N'0543', N'001', N'ﾊﾁｼﾞﾕｳﾆﾅｶﾞﾉ', N'ﾎﾝﾃﾝ', N'八十二長野銀行 本店', N'八十二長野銀行', N'本店', N'1'),
    (N'0543', N'101', N'ﾊﾁｼﾞﾕｳﾆﾅｶﾞﾉ', N'ﾅｶﾞﾉ', N'八十二長野銀行 長野支店', N'八十二長野銀行', N'長野支店', N'1'),
    (N'0543', N'201', N'ﾊﾁｼﾞﾕｳﾆﾅｶﾞﾉ', N'ﾏﾂﾓﾄ', N'八十二長野銀行 松本支店', N'八十二長野銀行', N'松本支店', N'1'),
    (N'3054', N'001', N'ﾅｶﾞﾉｹﾝｼﾝﾚﾝ', N'ﾎﾝｷﾖｸ', N'長野県信連 本局', N'長野県信用農業協同組合連合会', N'本局', N'1'),
    (N'3054', N'002', N'ﾅｶﾞﾉｹﾝｼﾝﾚﾝ', N'ﾅｶﾞﾉｼﾌﾞ', N'長野県信連 長野支部', N'長野県信用農業協同組合連合会', N'長野支部', N'1'),
    (N'1025', N'001', N'ﾅｶﾞﾉｼﾝｷﾝ', N'ﾎﾝﾃﾝ', N'長野信用金庫 本店', N'長野信用金庫', N'本店', N'1'),
    (N'1025', N'002', N'ﾅｶﾞﾉｼﾝｷﾝ', N'ﾅｶﾞﾉｴｷﾏｴ', N'長野信用金庫 長野駅前支店', N'長野信用金庫', N'長野駅前支店', N'1'),
    (N'1234', N'001', N'ﾃｽﾄｷﾝｺｳ', N'ﾎﾝﾃﾝ', N'テスト銀行 本店', N'テスト銀行', N'本店', N'1'),
    (N'1234', N'002', N'ﾃｽﾄｷﾝｺｳ', N'ﾀﾞｲﾆｼﾃﾝ', N'テスト銀行 第二支店', N'テスト銀行', N'第二支店', N'1')
) AS source (BankCode, BranchCode, BankNameKana, BranchNameKana, OfficeName, BankNameKanji, BranchNameKanji, KanjiSetFlag)
ON target.BankCode = source.BankCode AND target.BranchCode = source.BranchCode
WHEN NOT MATCHED THEN
    INSERT ([BankCode], [BranchCode], [BankNameKana], [BranchNameKana], [OfficeName], [BankNameKanji], [BranchNameKanji], [KanjiSetFlag])
    VALUES (source.BankCode, source.BranchCode, source.BankNameKana, source.BranchNameKana, source.OfficeName, source.BankNameKanji, source.BranchNameKanji, source.KanjiSetFlag);


-- ============================================================
-- 3. TM_Companies (会社マスタ)
-- ============================================================
MERGE [dbo].[TM_Companies] AS target
USING (VALUES
    -- (CompanyCode, CompanyNameKana, CompanyNameKanji, ConsignorCode, ZenginLinkFlag, SuspendFlag, PreviousTransferRound, CurrentTransferRound,
    --  BasicFee, AdminFee, NewUnitPrice, ModifyUnitPrice, Transfer1UnitPrice, Transfer2UnitPrice, ReceiptUnitPrice,
    --  Transfer1BankCode, Transfer1BranchCode, Transfer1AccountType, Transfer1AccountNo,
    --  NewCount, ModifyCount, Transfer1Count, Transfer2Count, FailureCount,
    --  PageChangeKey, PostalCode, PhoneNumber)
    (N'000001', N'テストカイシャ　アルファ', N'テスト会社 アルファ', N'1234567890', N'1', N'0', N'1', N'1',
     100000, 5000, 200, 150, 100, 100, 50,
     N'1234', N'001', N'1', N'0012345678',
     0, 0, 0, 0, 0,
     1, N'1000001', N'0312345678'),
    (N'000002', N'テストカイシャ　ベータ', N'テスト会社 ベータ', N'1234567891', N'1', N'0', N'1', N'1',
     200000, 10000, 300, 200, 150, 150, 80,
     N'0001', N'001', N'1', N'0000111222',
     0, 0, 0, 0, 0,
     2, N'5300001', N'0612345678'),
    (N'000003', N'テストカイシャ　ガンマ', N'テスト会社 ガンマ', N'1234567892', N'0', N'0', N'1', N'1',
     50000, 3000, 150, 100, 80, 80, 40,
     N'0005', N'001', N'2', N'9876543210',
     0, 0, 0, 0, 0,
     1, NULL, NULL)
) AS source (
    CompanyCode, CompanyNameKana, CompanyNameKanji, ConsignorCode, ZenginLinkFlag, SuspendFlag, PreviousTransferRound, CurrentTransferRound,
    BasicFee, AdminFee, NewUnitPrice, ModifyUnitPrice, Transfer1UnitPrice, Transfer2UnitPrice, ReceiptUnitPrice,
    Transfer1BankCode, Transfer1BranchCode, Transfer1AccountType, Transfer1AccountNo,
    NewCount, ModifyCount, Transfer1Count, Transfer2Count, FailureCount,
    PageChangeKey, PostalCode, PhoneNumber
)
ON target.CompanyCode = source.CompanyCode
WHEN NOT MATCHED THEN
    INSERT (
        [CompanyCode], [CompanyNameKana], [CompanyNameKanji], [ConsignorCode], [ZenginLinkFlag],
        [SuspendFlag], [PreviousTransferRound], [CurrentTransferRound],
        [BasicFee], [AdminFee], [NewUnitPrice], [ModifyUnitPrice], [Transfer1UnitPrice], [Transfer2UnitPrice], [ReceiptUnitPrice],
        [Transfer1BankCode], [Transfer1BranchCode], [Transfer1AccountType], [Transfer1AccountNo],
        [NewCount], [ModifyCount], [Transfer1Count], [Transfer2Count], [FailureCount],
        [PageChangeKey], [PostalCode], [PhoneNumber]
    )
    VALUES (
        source.CompanyCode, source.CompanyNameKana, source.CompanyNameKanji, source.ConsignorCode, source.ZenginLinkFlag,
        source.SuspendFlag, source.PreviousTransferRound, source.CurrentTransferRound,
        source.BasicFee, source.AdminFee, source.NewUnitPrice, source.ModifyUnitPrice, source.Transfer1UnitPrice, source.Transfer2UnitPrice, source.ReceiptUnitPrice,
        source.Transfer1BankCode, source.Transfer1BranchCode, source.Transfer1AccountType, source.Transfer1AccountNo,
        source.NewCount, source.ModifyCount, source.Transfer1Count, source.Transfer2Count, source.FailureCount,
        source.PageChangeKey, source.PostalCode, source.PhoneNumber
    );


-- ============================================================
-- 4. TM_CompanyTypes (会社種別マスタ) ※ TM_Companies に依存
-- ============================================================
MERGE [dbo].[TM_CompanyTypes] AS target
USING (
    SELECT
        c.Id AS CompanyId,
        src.TypeNo,
        src.TypeCode,
        src.Cycle,
        src.StartYearMonth,
        src.TypeName,
        src.Amount
    FROM (VALUES
        -- (CompanyCode, TypeNo, TypeCode, Cycle, StartYearMonth, TypeName, Amount)
        (N'000001', CAST(1 AS smallint), N'A', N'01', N'202601', N'月払い', CAST(10000 AS decimal(10,0))),
        (N'000001', CAST(2 AS smallint), N'B', N'12', N'202601', N'年払い', CAST(120000 AS decimal(10,0))),
        (N'000002', CAST(1 AS smallint), N'A', N'01', N'202601', N'月払い', CAST(15000 AS decimal(10,0))),
        (N'000003', CAST(1 AS smallint), N'A', N'01', N'202601', N'月払い', CAST(8000 AS decimal(10,0)))
    ) AS src (CompanyCode, TypeNo, TypeCode, Cycle, StartYearMonth, TypeName, Amount)
    INNER JOIN [dbo].[TM_Companies] c ON c.CompanyCode = src.CompanyCode
) AS source (CompanyId, TypeNo, TypeCode, Cycle, StartYearMonth, TypeName, Amount)
ON target.CompanyId = source.CompanyId AND target.TypeNo = source.TypeNo
WHEN NOT MATCHED THEN
    INSERT ([CompanyId], [TypeNo], [TypeCode], [Cycle], [StartYearMonth], [TypeName], [Amount])
    VALUES (source.CompanyId, source.TypeNo, source.TypeCode, source.Cycle, source.StartYearMonth, source.TypeName, source.Amount);


-- ============================================================
-- 5. TM_CompanyWithdrawalDays (会社引落日マスタ) ※ TM_Companies に依存
-- ============================================================
MERGE [dbo].[TM_CompanyWithdrawalDays] AS target
USING (
    SELECT
        c.Id AS CompanyId,
        src.SlotNo,
        src.WithdrawalDay,
        src.PreviousProcessingType,
        src.CurrentProcessingType
    FROM (VALUES
        -- (CompanyCode, SlotNo, WithdrawalDay, PreviousProcessingType, CurrentProcessingType)
        (N'000001', CAST(1 AS smallint), N'27', N'1', N'1'),
        (N'000002', CAST(1 AS smallint), N'27', N'1', N'1'),
        (N'000002', CAST(2 AS smallint), N'05', N'2', N'2'),
        (N'000003', CAST(1 AS smallint), N'15', N'1', N'1')
    ) AS src (CompanyCode, SlotNo, WithdrawalDay, PreviousProcessingType, CurrentProcessingType)
    INNER JOIN [dbo].[TM_Companies] c ON c.CompanyCode = src.CompanyCode
) AS source (CompanyId, SlotNo, WithdrawalDay, PreviousProcessingType, CurrentProcessingType)
ON target.CompanyId = source.CompanyId AND target.SlotNo = source.SlotNo
WHEN NOT MATCHED THEN
    INSERT ([CompanyId], [SlotNo], [WithdrawalDay], [PreviousProcessingType], [CurrentProcessingType])
    VALUES (source.CompanyId, source.SlotNo, source.WithdrawalDay, source.PreviousProcessingType, source.CurrentProcessingType);


-- ============================================================
-- 6. TM_Contracts (契約マスタ) ※ TM_Companies, TM_BankBranches に依存
-- ============================================================
-- CheckDigit は会社コードと個人コードを連結した18桁へMod10を適用した値である。
-- 算出規則の正本は現行COBOL（KOZ035N / KOZ830 の CHECK-DIGIT セクション）とする。
MERGE [dbo].[TM_Contracts] AS target
USING (
    SELECT
        c.Id          AS CompanyId,
        bb.Id         AS BankBranchId,
        src.CompanyCode,
        src.PersonalCode,
        src.CheckDigit,
        src.CodeSave,
        src.WithdrawalDay,
        src.StartYearMonth,
        src.SuspendFlag,
        src.NewFlag,
        src.ZenginFlag,
        src.NotifiedFlag,
        src.ResultFlag,
        src.ProcessType,
        src.CreditCompleteFlag,
        src.TransferMethod,
        src.AutoDeleteFlag,
        src.FailureCount,
        src.DepositorNameKana,
        src.ContractorNameKana,
        src.DepositorNameKanji,
        src.BankCode,
        src.BranchCode,
        src.AccountType,
        src.AccountNo,
        src.PreviousBalance,
        src.CurrentBillingAmount
    FROM (VALUES
        -- (CompanyCode, PersonalCode, CheckDigit, CodeSave, WithdrawalDay, StartYearMonth,
        --  SuspendFlag, NewFlag, ZenginFlag, NotifiedFlag, ResultFlag, ProcessType, CreditCompleteFlag, TransferMethod, AutoDeleteFlag,
        --  FailureCount, DepositorNameKana, ContractorNameKana, DepositorNameKanji,
        --  BankCode, BranchCode, AccountType, AccountNo, PreviousBalance, CurrentBillingAmount)
        (N'000001', N'000001000001', N'4', N'0', CAST(27 AS smallint), N'202601',
         N'0', N'0', N'1', N'0', N'0', N'1', N'0', N'1', N'0',
         CAST(0 AS smallint), N'テスト　タロウ', N'テスト　タロウ', N'テスト 太郎',
         N'1234', N'001', N'1', N'0000001111', CAST(0 AS decimal(10,0)), CAST(10000 AS decimal(10,0))),
        (N'000001', N'000001000002', N'2', N'0', CAST(27 AS smallint), N'202601',
         N'0', N'0', N'1', N'0', N'0', N'1', N'0', N'1', N'0',
         CAST(0 AS smallint), N'テスト　ハナコ', N'テスト　ハナコ', N'テスト 花子',
         N'0001', N'001', N'1', N'0000002222', CAST(10000 AS decimal(10,0)), CAST(10000 AS decimal(10,0))),
        (N'000002', N'000002000001', N'0', N'0', CAST(27 AS smallint), N'202601',
         N'0', N'0', N'1', N'0', N'0', N'1', N'0', N'1', N'0',
         CAST(0 AS smallint), N'サンプル　ジロウ', N'サンプル　ジロウ', N'サンプル 次郎',
         N'0005', N'001', N'2', N'9999888877', CAST(0 AS decimal(10,0)), CAST(15000 AS decimal(10,0))),
        (N'000003', N'000003000001', N'6', N'0', CAST(15 AS smallint), N'202601',
         N'0', N'0', N'0', N'0', N'0', N'1', N'0', N'1', N'0',
         CAST(1 AS smallint), N'エラー　サブロウ', NULL, NULL,
         N'1234', N'002', N'1', N'1111222233', CAST(0 AS decimal(10,0)), CAST(8000 AS decimal(10,0)))
    ) AS src (
        CompanyCode, PersonalCode, CheckDigit, CodeSave, WithdrawalDay, StartYearMonth,
        SuspendFlag, NewFlag, ZenginFlag, NotifiedFlag, ResultFlag, ProcessType, CreditCompleteFlag, TransferMethod, AutoDeleteFlag,
        FailureCount, DepositorNameKana, ContractorNameKana, DepositorNameKanji,
        BankCode, BranchCode, AccountType, AccountNo, PreviousBalance, CurrentBillingAmount
    )
    INNER JOIN [dbo].[TM_Companies] c ON c.CompanyCode = src.CompanyCode
    LEFT  JOIN [dbo].[TM_BankBranches] bb ON bb.BankCode = src.BankCode AND bb.BranchCode = src.BranchCode
) AS source (
    CompanyId, BankBranchId, CompanyCode, PersonalCode, CheckDigit, CodeSave, WithdrawalDay, StartYearMonth,
    SuspendFlag, NewFlag, ZenginFlag, NotifiedFlag, ResultFlag, ProcessType, CreditCompleteFlag, TransferMethod, AutoDeleteFlag,
    FailureCount, DepositorNameKana, ContractorNameKana, DepositorNameKanji,
    BankCode, BranchCode, AccountType, AccountNo, PreviousBalance, CurrentBillingAmount
)
ON target.CompanyCode = source.CompanyCode AND target.PersonalCode = source.PersonalCode
WHEN NOT MATCHED THEN
    INSERT (
        [CompanyId], [BankBranchId], [CompanyCode], [PersonalCode], [CheckDigit], [CodeSave], [WithdrawalDay], [StartYearMonth],
        [SuspendFlag], [NewFlag], [ZenginFlag], [NotifiedFlag], [ResultFlag], [ProcessType], [CreditCompleteFlag], [TransferMethod], [AutoDeleteFlag],
        [FailureCount], [DepositorNameKana], [ContractorNameKana], [DepositorNameKanji],
        [BankCode], [BranchCode], [AccountType], [AccountNo], [PreviousBalance], [CurrentBillingAmount]
    )
    VALUES (
        source.CompanyId, source.BankBranchId, source.CompanyCode, source.PersonalCode, source.CheckDigit, source.CodeSave, source.WithdrawalDay, source.StartYearMonth,
        source.SuspendFlag, source.NewFlag, source.ZenginFlag, source.NotifiedFlag, source.ResultFlag, source.ProcessType, source.CreditCompleteFlag, source.TransferMethod, source.AutoDeleteFlag,
        source.FailureCount, source.DepositorNameKana, source.ContractorNameKana, source.DepositorNameKanji,
        source.BankCode, source.BranchCode, source.AccountType, source.AccountNo, source.PreviousBalance, source.CurrentBillingAmount
    );


-- ============================================================
-- 7. TM_ContractTypes (契約種別マスタ) ※ TM_Contracts に依存
-- ============================================================
MERGE [dbo].[TM_ContractTypes] AS target
USING (
    SELECT
        ct.Id AS ContractId,
        src.TypeNo,
        src.StartYearMonth,
        src.Cycle,
        src.Amount
    FROM (VALUES
        -- (CompanyCode, PersonalCode, TypeNo, StartYearMonth, Cycle, Amount)
        (N'000001', N'000001000001', CAST(1 AS smallint), N'202601', N'01', CAST(10000 AS decimal(10,0))),
        (N'000001', N'000001000002', CAST(1 AS smallint), N'202601', N'01', CAST(10000 AS decimal(10,0))),
        (N'000002', N'000002000001', CAST(1 AS smallint), N'202601', N'01', CAST(15000 AS decimal(10,0))),
        (N'000003', N'000003000001', CAST(1 AS smallint), N'202601', N'01', CAST(8000 AS decimal(10,0)))
    ) AS src (CompanyCode, PersonalCode, TypeNo, StartYearMonth, Cycle, Amount)
    INNER JOIN [dbo].[TM_Contracts] ct ON ct.CompanyCode = src.CompanyCode AND ct.PersonalCode = src.PersonalCode
) AS source (ContractId, TypeNo, StartYearMonth, Cycle, Amount)
ON target.ContractId = source.ContractId AND target.TypeNo = source.TypeNo
WHEN NOT MATCHED THEN
    INSERT ([ContractId], [TypeNo], [StartYearMonth], [Cycle], [Amount])
    VALUES (source.ContractId, source.TypeNo, source.StartYearMonth, source.Cycle, source.Amount);


-- ============================================================
-- 8. TM_ContractBillingAmounts (契約請求金額マスタ) ※ TM_Contracts に依存
-- ============================================================
MERGE [dbo].[TM_ContractBillingAmounts] AS target
USING (
    SELECT
        ct.Id AS ContractId,
        src.TypeNo,
        src.BillingAmount
    FROM (VALUES
        -- (CompanyCode, PersonalCode, TypeNo, BillingAmount)
        (N'000001', N'000001000001', CAST(1 AS smallint), CAST(10000 AS decimal(10,0))),
        (N'000001', N'000001000002', CAST(1 AS smallint), CAST(10000 AS decimal(10,0))),
        (N'000002', N'000002000001', CAST(1 AS smallint), CAST(15000 AS decimal(10,0))),
        (N'000003', N'000003000001', CAST(1 AS smallint), CAST(8000 AS decimal(10,0)))
    ) AS src (CompanyCode, PersonalCode, TypeNo, BillingAmount)
    INNER JOIN [dbo].[TM_Contracts] ct ON ct.CompanyCode = src.CompanyCode AND ct.PersonalCode = src.PersonalCode
) AS source (ContractId, TypeNo, BillingAmount)
ON target.ContractId = source.ContractId AND target.TypeNo = source.TypeNo
WHEN NOT MATCHED THEN
    INSERT ([ContractId], [TypeNo], [BillingAmount])
    VALUES (source.ContractId, source.TypeNo, source.BillingAmount);


-- ============================================================
-- 9. TM_BatchParameters (バッチパラメータ)
-- ============================================================
MERGE [dbo].[TM_BatchParameters] AS target
USING (VALUES
    -- (ParameterType, WithdrawalDate, JobExecutionStatus)
    (N'1', CAST('2026-08-27' AS date), N'0'),
    (N'2', CAST('2026-09-05' AS date), N'0')
) AS source (ParameterType, WithdrawalDate, JobExecutionStatus)
ON target.ParameterType = source.ParameterType
WHEN NOT MATCHED THEN
    INSERT ([ParameterType], [WithdrawalDate], [JobExecutionStatus])
    VALUES (source.ParameterType, source.WithdrawalDate, source.JobExecutionStatus);


-- ============================================================
-- 10. TM_BatchParameterCompanies (バッチパラメータ会社) ※ TM_BatchParameters に依存
-- ============================================================
MERGE [dbo].[TM_BatchParameterCompanies] AS target
USING (
    SELECT
        bp.Id AS BatchParameterId,
        src.SlotNo,
        src.CompanyCode
    FROM (VALUES
        -- (ParameterType, SlotNo, CompanyCode)
        (N'1', CAST(1 AS smallint), N'000001'),
        (N'1', CAST(2 AS smallint), N'000002'),
        (N'2', CAST(1 AS smallint), N'000002'),
        (N'2', CAST(2 AS smallint), N'000003')
    ) AS src (ParameterType, SlotNo, CompanyCode)
    INNER JOIN [dbo].[TM_BatchParameters] bp ON bp.ParameterType = src.ParameterType
) AS source (BatchParameterId, SlotNo, CompanyCode)
ON target.BatchParameterId = source.BatchParameterId AND target.SlotNo = source.SlotNo
WHEN NOT MATCHED THEN
    INSERT ([BatchParameterId], [SlotNo], [CompanyCode])
    VALUES (source.BatchParameterId, source.SlotNo, source.CompanyCode);


-- ============================================================
-- 11. TM_ProcessingCalendars (処理カレンダー)
-- ============================================================
MERGE [dbo].[TM_ProcessingCalendars] AS target
USING (VALUES
    (CAST('2026-08-01' AS date)),
    (CAST('2026-09-01' AS date)),
    (CAST('2026-10-01' AS date))
) AS source (ProcessingDate)
ON target.ProcessingDate = source.ProcessingDate
WHEN NOT MATCHED THEN
    INSERT ([ProcessingDate]) VALUES (source.ProcessingDate);


-- ============================================================
-- 12. TM_ProcessingSlots (処理スロット) ※ TM_ProcessingCalendars に依存
-- ============================================================
MERGE [dbo].[TM_ProcessingSlots] AS target
USING (
    SELECT
        pc.Id AS ProcessingCalendarId,
        src.SlotNo,
        src.ProcessingType,
        src.WithdrawalDate1,
        src.WithdrawalDate2,
        src.WithdrawalDate3,
        src.CompletedFlag
    FROM (VALUES
        -- (ProcessingDate, SlotNo, ProcessingType, WithdrawalDate1, WithdrawalDate2, WithdrawalDate3, CompletedFlag)
        (CAST('2026-08-01' AS date), CAST(1 AS smallint), N'1', CAST('2026-08-27' AS date), NULL, NULL, CAST(0 AS bit)),
        (CAST('2026-08-01' AS date), CAST(2 AS smallint), N'2', CAST('2026-09-05' AS date), NULL, NULL, CAST(0 AS bit)),
        (CAST('2026-09-01' AS date), CAST(1 AS smallint), N'1', CAST('2026-09-26' AS date), NULL, NULL, CAST(0 AS bit))
    ) AS src (ProcessingDate, SlotNo, ProcessingType, WithdrawalDate1, WithdrawalDate2, WithdrawalDate3, CompletedFlag)
    INNER JOIN [dbo].[TM_ProcessingCalendars] pc ON pc.ProcessingDate = src.ProcessingDate
) AS source (ProcessingCalendarId, SlotNo, ProcessingType, WithdrawalDate1, WithdrawalDate2, WithdrawalDate3, CompletedFlag)
ON target.ProcessingCalendarId = source.ProcessingCalendarId AND target.SlotNo = source.SlotNo
WHEN NOT MATCHED THEN
    INSERT ([ProcessingCalendarId], [SlotNo], [ProcessingType], [WithdrawalDate1], [WithdrawalDate2], [WithdrawalDate3], [CompletedFlag])
    VALUES (source.ProcessingCalendarId, source.SlotNo, source.ProcessingType, source.WithdrawalDate1, source.WithdrawalDate2, source.WithdrawalDate3, source.CompletedFlag);


-- ============================================================
-- 13. TM_AmountSettings (金額設定マスタ)
-- ============================================================
MERGE [dbo].[TM_AmountSettings] AS target
USING (VALUES
    -- (CompanyCode, TypeNo, TypeCode, Amount)
    (N'000001', CAST(1 AS smallint), N'A', CAST(10000 AS decimal(10,0))),
    (N'000001', CAST(2 AS smallint), N'B', CAST(120000 AS decimal(10,0))),
    (N'000002', CAST(1 AS smallint), N'A', CAST(15000 AS decimal(10,0))),
    (N'000003', CAST(1 AS smallint), N'A', CAST(8000 AS decimal(10,0)))
) AS source (CompanyCode, TypeNo, TypeCode, Amount)
ON target.CompanyCode = source.CompanyCode AND target.TypeNo = source.TypeNo AND target.TypeCode = source.TypeCode
WHEN NOT MATCHED THEN
    INSERT ([CompanyCode], [TypeNo], [TypeCode], [Amount])
    VALUES (source.CompanyCode, source.TypeNo, source.TypeCode, source.Amount);


-- ============================================================
-- 14. TM_TransmissionParameters (送信パラメータ)
-- ============================================================
MERGE [dbo].[TM_TransmissionParameters] AS target
USING (VALUES
    -- (ParameterKey, ParameterValue, Description)
    (N'ZENGIN_HOST',       N'test-zengin.example.local', N'全銀接続ホスト（テスト環境）'),
    (N'ZENGIN_PORT',       N'9090',                      N'全銀接続ポート（テスト環境）'),
    (N'OUTPUT_BASE_PATH',  N'C:\Work\UnifiedAccount\Out', N'出力ベースパス（テスト環境）'),
    (N'CONSIGNOR_CODE',    N'9999999999',                 N'委託者コード（テスト用）')
) AS source (ParameterKey, ParameterValue, Description)
ON target.ParameterKey = source.ParameterKey
WHEN NOT MATCHED THEN
    INSERT ([ParameterKey], [ParameterValue], [Description])
    VALUES (source.ParameterKey, source.ParameterValue, source.Description);


-- ============================================================
-- 15. TM_ErrorSuppressParameters (エラー抑止パラメータ)
-- ============================================================
MERGE [dbo].[TM_ErrorSuppressParameters] AS target
USING (VALUES
    -- (CompanyCode, SuppressFlag, Description)
    (N'000001', CAST(0 AS bit), N'テスト会社アルファ：エラー抑止なし'),
    (N'000003', CAST(1 AS bit), N'テスト会社ガンマ：エラー抑止あり（振替失敗テスト用）')
) AS source (CompanyCode, SuppressFlag, Description)
ON target.CompanyCode = source.CompanyCode
WHEN NOT MATCHED THEN
    INSERT ([CompanyCode], [SuppressFlag], [Description])
    VALUES (source.CompanyCode, source.SuppressFlag, source.Description);


-- ============================================================
-- 17. TM_Users (ユーザーマスタ)
-- ============================================================
-- パスワードハッシュ: PBKDF2(SHA-256, 600000) hash of 'admin'
-- PasswordHasher と同一形式 (Base64(salt[16] + hash[32]))
MERGE [dbo].[TM_Users] AS target
USING (VALUES
    -- (UserId, PasswordHash, UserName, Role, IsActive)
    (N'admin',   N'bjkI1P8BbcW20TXsvbYyhjmiNeLgdiNJY40ZUbX8eks6AZQ4ddDboa6Ij4gWI6kX', N'システム管理者', N'SystemAdmin',    CAST(1 AS bit)),
    (N'operator',N'3vqHEkYHPeAeCkUaoSg4KIacotcMMFxQqMcTY7lxF7L+Zy7OGOUoCazwFxSaEJcT', N'オペレーター',   N'Operator',       CAST(1 AS bit)),
    (N'viewer',  N'A3fClAO+JJqC5CsDftwlJXDfG1dd4xzFo4uL3cbYuAe7OIfNsDWKuVtc9yBlINFW', N'参照ユーザー',   N'OperationStaff', CAST(1 AS bit))
) AS source (UserId, PasswordHash, UserName, Role, IsActive)
ON target.UserId = source.UserId
WHEN MATCHED THEN
    UPDATE SET
        [PasswordHash] = source.PasswordHash,
        [UserName] = source.UserName,
        [Role] = source.Role,
        [IsActive] = source.IsActive
WHEN NOT MATCHED THEN
    INSERT ([UserId], [PasswordHash], [UserName], [Role], [IsActive])
    VALUES (source.UserId, source.PasswordHash, source.UserName, source.Role, source.IsActive);


COMMIT TRANSACTION;
PRINT 'テストデータ投入完了';
