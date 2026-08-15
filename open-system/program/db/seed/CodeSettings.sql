SET NOCOUNT ON;

-- コード設定マスタの業務初期データ。
-- 内容の正本は open-system/docs/12C-基本設計/データベース定義書/184-コード定義書.md 2.13。
-- マイグレーションではない。db/migrations とは分離されており、Flyway の適用対象ではない。
-- テスト環境データ (db/testdata/DevelopmentData.sql) とは分離して管理する。
--
-- 【適用範囲】本スクリプトは開発・テスト環境専用とする。本番適用は現行資産との突合完了後とする。
-- 理由: 投入する20行のうち、根拠が確定しているのは AccountType の2行 (184-コード定義書 2.1。
--       現行COBOLの口座種目との突合済。根拠は main-frame/docs/12C-基本設計/184-コード体系調査.md 3.1)
--       と区分マスタ親行5行のみである。
--       残る13行 (ProcessType / TransferMethod / SuspendFlag / JobExecutionStatus) は
--       現行COBOL資産との突合が未実施であり、実装側の消費者も0件である。
--       突合の結果によって値・意味が変わりうるため、本番データとして確定していない。
-- 追跡: 184-コード定義書 5.1.4 No.13。突合完了後に本コメントと db/README.md の記載を更新する。
-- 一意索引 IX_TM_CodeSettings_CodeCategory_CodeValue に合わせ、再実行可能とする。
MERGE [dbo].[TM_CodeSettings] AS target
USING (VALUES
    -- (CodeCategory, CodeValue, DisplayText, ChangeValue, DisplayOrder)
    -- 区分マスタ（親行）。CodeCategory='00' は区分マスタ自体を表す予約値。
    (N'00', N'AccountType',        N'口座種目',       NULL, 10),
    (N'00', N'ProcessType',        N'処理種別',       NULL, 20),
    (N'00', N'TransferMethod',     N'振替方法',       NULL, 30),
    (N'00', N'SuspendFlag',        N'停止フラグ',     NULL, 40),
    (N'00', N'JobExecutionStatus', N'バッチ実行状態', NULL, 50),
    -- 口座種目（コード値の正本は 184-コード定義書 2.1）
    -- 現行COBOLの値域は '1'/'2' の2値のみ。'4' 貯蓄預金 / '9' その他は現行に存在しないため削除した。
    (N'AccountType', N'1', N'普通預金', NULL, 1),
    (N'AccountType', N'2', N'当座預金', NULL, 2),
    -- 処理種別
    (N'ProcessType', N'1', N'通常', NULL, 1),
    (N'ProcessType', N'2', N'追加', NULL, 2),
    (N'ProcessType', N'3', N'変更', NULL, 3),
    (N'ProcessType', N'4', N'削除', NULL, 4),
    -- 振替方法
    (N'TransferMethod', N'1', N'全銀',     NULL, 1),
    (N'TransferMethod', N'2', N'ゆうちょ', NULL, 2),
    (N'TransferMethod', N'3', N'コンビニ', NULL, 3),
    -- 停止フラグ
    (N'SuspendFlag', N'0', N'有効', NULL, 1),
    (N'SuspendFlag', N'1', N'停止', NULL, 2),
    -- バッチ実行状態
    (N'JobExecutionStatus', N'0', N'未実行', NULL, 1),
    (N'JobExecutionStatus', N'1', N'実行中', NULL, 2),
    (N'JobExecutionStatus', N'2', N'完了',   NULL, 3),
    (N'JobExecutionStatus', N'9', N'エラー', NULL, 4)
) AS source (CodeCategory, CodeValue, DisplayText, ChangeValue, DisplayOrder)
ON target.CodeCategory = source.CodeCategory AND target.CodeValue = source.CodeValue
WHEN NOT MATCHED THEN
    INSERT ([CodeCategory], [CodeValue], [DisplayText], [ChangeValue], [DisplayOrder], [CreatedAt], [UpdatedAt])
    VALUES (source.CodeCategory, source.CodeValue, source.DisplayText, source.ChangeValue, source.DisplayOrder, SYSDATETIME(), SYSDATETIME());
