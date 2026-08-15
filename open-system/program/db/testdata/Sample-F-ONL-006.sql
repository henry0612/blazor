-- ============================================================
-- サンプルデータ投入スクリプト（F-ONL-006 金額データ修正）
-- 用途: 先行開発の委託先が画面の検索・編集・保存を動作確認するための正常データ
-- 注意: 本番環境へ適用してはならない
-- 注意: 冪等（IF NOT EXISTS）で設計する。複数回実行しても件数は増えない
-- 前提: db/migrations/V001__InitialSchema.sql と db/testdata/DevelopmentData.sql の投入が完了していること
-- ============================================================
USE [UnifiedAccount];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- ============================================================
-- TD_TransferAmounts (金額データ) 20件
-- ContractId は TM_Contracts との左外部結合で解決する。一致しない行は NULL とする。
-- CheckDigit はブリーフ（task-3-brief.md）記載の値をそのまま使用する。
-- ============================================================
INSERT INTO [dbo].[TD_TransferAmounts]
    ([ContractId], [RecordType], [BatchNo], [SequenceNo], [CompanyCode], [PersonalCode], [CheckDigit],
     [Amount1], [Amount2], [Amount3], [Amount4], [Amount5],
     [ErrorFlag1], [ErrorFlag2], [ErrorFlag3], [ErrorFlag4], [ErrorFlag5], [ErrorFlag6],
     [ErrorFlag7], [ErrorFlag8], [ErrorFlag9], [ErrorFlag10], [ErrorFlag11], [ErrorFlag12])
SELECT
    c.Id, N'30', src.BatchNo, src.SequenceNo, src.CompanyCode, src.PersonalCode, src.CheckDigit,
    src.Amount1, src.Amount2, src.Amount3, src.Amount4, src.Amount5,
    N'0', N'0', N'0', N'0', N'0', N'0', N'0', N'0', N'0', N'0', N'0', N'0'
FROM (VALUES
    -- (CompanyCode, BatchNo, SequenceNo, PersonalCode, CheckDigit, Amount1, Amount2, Amount3, Amount4, Amount5)
    (N'000001', N'001', 1, N'000001000001', N'4',
     CAST(10000 AS decimal(10,0)), CAST(5000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 2, N'000001000002', N'2',
     CAST(12000 AS decimal(10,0)), CAST(3000 AS decimal(10,0)), CAST(2000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 3, N'000001000003', N'0',
     CAST(8000 AS decimal(10,0)), CAST(0 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(1500 AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 4, N'000001000004', N'8',
     CAST(15000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 5, N'000001000005', N'5',
     CAST(20000 AS decimal(10,0)), CAST(10000 AS decimal(10,0)), CAST(5000 AS decimal(10,0)), CAST(2500 AS decimal(10,0)), CAST(1000 AS decimal(10,0))),
    (N'000001', N'001', 6, N'000001000006', N'3',
     CAST(3000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 7, N'000001000007', N'1',
     CAST(45000 AS decimal(10,0)), CAST(0 AS decimal(10,0)), CAST(0 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 8, N'000001000008', N'9',
     CAST(7500 AS decimal(10,0)), CAST(2500 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 9, N'000001000009', N'7',
     CAST(60000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(30000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 10, N'000001000010', N'5',
     CAST(1000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 11, N'000001000011', N'3',
     CAST(250000 AS decimal(10,0)), CAST(100000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 12, N'000001000012', N'1',
     CAST(9800 AS decimal(10,0)), CAST(0 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(400 AS decimal(10,0))),
    (N'000001', N'001', 13, N'000001000013', N'9',
     CAST(33000 AS decimal(10,0)), CAST(11000 AS decimal(10,0)), CAST(11000 AS decimal(10,0)), CAST(11000 AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 14, N'000001000014', N'7',
     CAST(4200 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 15, N'000001000015', N'4',
     CAST(18000 AS decimal(10,0)), CAST(6000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000001', N'001', 16, N'000001000016', N'2',
     CAST(500000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000002', N'002', 1, N'000002000001', N'0',
     CAST(15000 AS decimal(10,0)), CAST(5000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000002', N'002', 2, N'000002000002', N'8',
     CAST(22000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(8000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000002', N'002', 3, N'000002000003', N'6',
     CAST(3500 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0))),
    (N'000003', N'010', 1, N'000003000001', N'6',
     CAST(8000 AS decimal(10,0)), CAST(2000 AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)), CAST(NULL AS decimal(10,0)))
) AS src (CompanyCode, BatchNo, SequenceNo, PersonalCode, CheckDigit, Amount1, Amount2, Amount3, Amount4, Amount5)
OUTER APPLY (
    -- TM_Contracts は (CompanyCode, PersonalCode) に UNIQUE 制約を持たない。
    -- ContractSeq は同一会社・個人コードで複数契約を持てる前提の列であるため、
    -- 単純な LEFT JOIN では同キーに複数行がある DB で行が増殖する。
    -- 引当は 1 件に限定する。
    SELECT TOP 1 c2.Id
    FROM [dbo].[TM_Contracts] c2
    WHERE c2.CompanyCode = src.CompanyCode AND c2.PersonalCode = src.PersonalCode
    ORDER BY c2.Id
) c
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[TD_TransferAmounts] t
    WHERE t.CompanyCode = src.CompanyCode
      AND t.BatchNo = src.BatchNo
      AND t.SequenceNo = src.SequenceNo
);

COMMIT TRANSACTION;
PRINT 'Sample-F-ONL-006 投入完了';
