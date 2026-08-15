-- ============================================================
-- サンプルデータ投入スクリプト（F-ONL-007 銀行マスタ異動）
-- 用途: 先行開発の委託先が画面の検索・ページング機能を動作確認するための正常データ
-- 注意: 本番環境へ適用してはならない
-- 注意: 冪等（MERGE）で設計する。複数回実行しても件数は増えない
-- 前提: db/migrations/V001__InitialSchema.sql と db/testdata/DevelopmentData.sql の投入が完了していること
-- ============================================================
USE [UnifiedAccount];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

-- ============================================================
-- TM_BankBranches (銀行支店マスタ) 20件
-- DevelopmentData.sql の既存 15 件に加えて、検索・ページング確認用に 5 件を追加する
-- ============================================================
MERGE [dbo].[TM_BankBranches] AS target
USING (VALUES
    -- (BankCode, BranchCode, BankNameKana, BranchNameKana, OfficeName, BankNameKanji, BranchNameKanji, KanjiSetFlag)
    (N'0009', N'101', N'ミツイスミトモ', N'ナガノ', N'三井住友銀行 長野支店', N'三井住友銀行', N'長野支店', N'1'),
    (N'0010', N'001', N'リソナ', N'ホンテン', N'りそな銀行 本店', N'りそな銀行', N'本店', N'1'),
    (N'0033', N'001', N'ジヤパンネツト', N'ホンテン', N'ジャパンネット銀行 本店', N'ジャパンネット銀行', N'本店', N'1'),
    (N'0543', N'301', N'ハチジユウニナガノ', N'ウエダ', N'八十二長野銀行 上田支店', N'八十二長野銀行', N'上田支店', N'1'),
    (N'3054', N'003', N'ナガノケンシンレン', N'マツモトシブ', N'長野県信連 松本支部', N'長野県信用農業協同組合連合会', N'松本支部', N'1')
) AS source (BankCode, BranchCode, BankNameKana, BranchNameKana, OfficeName, BankNameKanji, BranchNameKanji, KanjiSetFlag)
ON target.BankCode = source.BankCode AND target.BranchCode = source.BranchCode
WHEN NOT MATCHED THEN
    INSERT ([BankCode], [BranchCode], [BankNameKana], [BranchNameKana], [OfficeName], [BankNameKanji], [BranchNameKanji], [KanjiSetFlag])
    VALUES (source.BankCode, source.BranchCode, source.BankNameKana, source.BranchNameKana, source.OfficeName, source.BankNameKanji, source.BranchNameKanji, source.KanjiSetFlag);

COMMIT TRANSACTION;
PRINT 'Sample-F-ONL-007 投入完了';