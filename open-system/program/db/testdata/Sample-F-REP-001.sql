-- ============================================================
-- サンプルデータ投入スクリプト（F-REP-001 会社マスター異動リスト）
-- 用途: 先行開発の委託先が帳票（REP-KOZ025）の出力を動作確認するための正常データ
-- 注意: 本番環境へ適用してはならない
-- 注意: 冪等（IF NOT EXISTS）で設計する。複数回実行しても件数は増えない
-- 前提: db/migrations/V001__InitialSchema.sql と db/testdata/DevelopmentData.sql の投入が完了していること
-- 補足: JobExecutionId は BAT-KOZ024 が実行時に採番する値とは別の固定値とし、
--       REP-KOZ025 を単独でも BAT-KOZ024 の後段としても実行できるようにする。
-- ============================================================
USE [UnifiedAccount];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- ============================================================
-- TR_CompanyMasterChangeLogs (会社マスター異動ログ) 20件
-- RequestType（DENK 11〜19）ごとに、DDL コメントが定める列群だけを設定し、
-- 対象外の列は NULL とする。ChangeAction は 000001=新規(2)、000002=修正(3)、
-- 000003=削除(1) の組み合わせで一通り網羅する。
-- RequestType='14' の新規行（#4）は Transfer1/2UnitPrice・ReceiptUnitPrice を
-- 含め DENK14 の7項目すべてを設定し、修正行（#11）は ChangeCode/BasicFee/AdminFee
-- のみに留める（Task 6 の TD_CompanyChangeRequests #8/#9 と対応する配分）。
-- Transfer2BankCode〜PassbookNo（DENK15 の後半）は本データセットでは
-- どの行も使用しないため、常に NULL とする。
-- JobExecutionId は Task 6（Sample-F-KOZ-001.sql）の JobExecutionId とは異なる固定値。
-- ============================================================
INSERT INTO [dbo].[TR_CompanyMasterChangeLogs] (
    [JobExecutionId], [ProcessingDate], [IsError],
    [CompanyCode], [RequestType], [ChangeAction], [MessageType], [SequenceNo],
    [CompanyNameKana], [PhoneNumber], [PostalCode], [Prefecture],
    [City], [Town1], [Town2],
    [WithdrawalDay1], [WithdrawalDay2], [WithdrawalDay3], [WithdrawalDay4], [ProcessingFlag], [SheetFlag],
    [ChangeCode], [BasicFee], [AdminFee], [NewUnitPrice], [ModifyUnitPrice], [Transfer1UnitPrice], [Transfer2UnitPrice], [ReceiptUnitPrice],
    [Transfer1BankCode], [Transfer1BranchCode], [Transfer1AccountType], [Transfer1AccountNo], [ConsignorCode], [Transfer2BankCode], [Transfer2BranchCode], [Transfer2AccountType], [Transfer2AccountNo], [PassbookNo],
    [CollectionCode], [Cycle], [OperatingYear], [OperatingMonth], [ItemName], [Amount]
)
SELECT N'KOZ024-20260827-9001' AS JobExecutionId, CAST('2026-08-27' AS date) AS ProcessingDate, CAST(0 AS bit) AS IsError, src.*
FROM (VALUES
    -- #1: DENK11 新規 000001 (会社名カナ・電話番号・郵便番号・都道府県)
    (N'000001', N'11', N'2', 1, 1,
     'TEST KAISHA 000001', N'0260000001', N'3800000', N'長野県',
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #2: DENK12 新規 000001 (住所)
    (N'000001', N'12', N'2', 2, 2,
     NULL, NULL, NULL, NULL,
     N'長野市', N'大字南長野', N'字幅下0002',
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #3: DENK13 新規 000001 (引落日1〜4・処理フラグ・シートフラグ)
    (N'000001', N'13', N'2', 3, 3,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     N'05', N'15', N'25', N'27', N'111111111111', N'000000000000',
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #4: DENK14 新規 000001 (料金・単価。Transfer1/2UnitPrice・ReceiptUnitPrice も含め DENK14 の7項目を全設定)
    (N'000001', N'14', N'2', 4, 4,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     N'01', CAST(100000 AS decimal(10,0)), CAST(5000 AS decimal(10,0)), CAST(200 AS decimal(5,0)), CAST(150 AS decimal(5,0)), CAST(100 AS decimal(5,0)), CAST(100 AS decimal(5,0)), CAST(50 AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #5: DENK15 新規 000001 (振替口座・委託者)
    (N'000001', N'15', N'2', 5, 5,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     N'0543', N'001', N'1', N'0100000005', N'1234567890', CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #6: DENK16 新規 000001 (収納区分・サイクル・処理年月・種目名・金額)
    (N'000001', N'16', N'2', 6, 6,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', N'01', N'2026', N'04', N'テストシユモク06', CAST(10000 AS decimal(10,0))),
    -- #7: DENK17 新規 000001 (収納区分・サイクル・種目名・金額)
    (N'000001', N'17', N'2', 7, 7,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', N'01', NULL, NULL, N'テストシユモク07', CAST(10000 AS decimal(10,0))),
    -- #8: DENK11 修正 000002 (会社名カナ・電話番号のみ)
    (N'000002', N'11', N'3', 8, 8,
     'TEST KAISHA 000002', N'0260000008', NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #9: DENK12 修正 000002 (市・町名1のみ)
    (N'000002', N'12', N'3', 9, 9,
     NULL, NULL, NULL, NULL,
     N'長野市', N'大字南長野', NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #10: DENK13 修正 000002 (引落日1・処理フラグのみ)
    (N'000002', N'13', N'3', 10, 10,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     N'05', NULL, NULL, NULL, N'111111111111', NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #11: DENK14 修正 000002 (料金の一部のみ。ChangeCode/BasicFee/AdminFeeに留める)
    (N'000002', N'14', N'3', 1, 11,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     N'01', CAST(100000 AS decimal(10,0)), CAST(5000 AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #12: DENK15 修正 000002 (振替銀行コード・支店コードのみ)
    (N'000002', N'15', N'3', 2, 12,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     N'0543', N'001', NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #13: DENK18 修正 000002 (収納区分・種目名・金額)
    (N'000002', N'18', N'3', 3, 13,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', NULL, NULL, NULL, N'テストシユモク13', CAST(10000 AS decimal(10,0))),
    -- #14: DENK19 修正 000002 (収納区分・種目名・金額)
    (N'000002', N'19', N'3', 4, 14,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', NULL, NULL, NULL, N'テストシユモク14', CAST(10000 AS decimal(10,0))),
    -- #15: DENK11 削除 000003 (会社名カナのみ)
    (N'000003', N'11', N'1', 5, 15,
     'TEST KAISHA 000003', NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #16: DENK12 削除 000003 (市のみ)
    (N'000003', N'12', N'1', 6, 16,
     NULL, NULL, NULL, NULL,
     N'長野市', NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #17: DENK13 削除 000003 (引落日1のみ)
    (N'000003', N'13', N'1', 7, 17,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     N'05', NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS decimal(10,0))),
    -- #18: DENK16 削除 000003 (収納区分・種目名のみ)
    (N'000003', N'16', N'1', 8, 18,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', NULL, NULL, NULL, N'テストシユモク18', CAST(NULL AS decimal(10,0))),
    -- #19: DENK17 削除 000003 (収納区分・種目名のみ)
    (N'000003', N'17', N'1', 9, 19,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', NULL, NULL, NULL, N'テストシユモク19', CAST(NULL AS decimal(10,0))),
    -- #20: DENK19 削除 000003 (収納区分・種目名のみ)
    (N'000003', N'19', N'1', 10, 20,
     NULL, NULL, NULL, NULL,
     NULL, NULL, NULL,
     NULL, NULL, NULL, NULL, NULL, NULL,
     NULL, CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)), CAST(NULL AS decimal(5,0)),
     NULL, NULL, NULL, NULL, NULL, CAST(NULL AS char(4)), CAST(NULL AS char(3)), CAST(NULL AS char(1)), CAST(NULL AS char(10)), CAST(NULL AS char(8)),
     N'1', NULL, NULL, NULL, N'テストシユモク20', CAST(NULL AS decimal(10,0)))
) AS src (
    CompanyCode, RequestType, ChangeAction, MessageType, SequenceNo,
    CompanyNameKana, PhoneNumber, PostalCode, Prefecture,
    City, Town1, Town2,
    WithdrawalDay1, WithdrawalDay2, WithdrawalDay3, WithdrawalDay4, ProcessingFlag, SheetFlag,
    ChangeCode, BasicFee, AdminFee, NewUnitPrice, ModifyUnitPrice, Transfer1UnitPrice, Transfer2UnitPrice, ReceiptUnitPrice,
    Transfer1BankCode, Transfer1BranchCode, Transfer1AccountType, Transfer1AccountNo, ConsignorCode, Transfer2BankCode, Transfer2BranchCode, Transfer2AccountType, Transfer2AccountNo, PassbookNo,
    CollectionCode, Cycle, OperatingYear, OperatingMonth, ItemName, Amount
)
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[TR_CompanyMasterChangeLogs] t
    WHERE t.JobExecutionId = N'KOZ024-20260827-9001'
      AND t.CompanyCode = src.CompanyCode
      AND t.RequestType = src.RequestType
      AND t.SequenceNo = src.SequenceNo
);

COMMIT TRANSACTION;
PRINT 'Sample-F-REP-001 投入完了';
