-- このファイルは Generate-InitialSchema.ps1 が 209_テーブル定義書 から生成した。
-- 直接編集しない。定義を変えるときは 209_テーブル定義書 を直して再生成する。

SET NOCOUNT ON;

-- フィルター付きインデックスの作成には QUOTED_IDENTIFIER ON と ANSI_NULLS ON が必須。
-- sqlcmd の既定は QUOTED_IDENTIFIER OFF のため、実行クライアントに依存せず宣言する。
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'UnifiedAccount')
BEGIN
    CREATE DATABASE [UnifiedAccount];
END
GO
USE [UnifiedAccount];
GO

-- USE の後も同一セッションで設定は維持されるが、部分実行に備えて再宣言する。
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

BEGIN TRY
BEGIN TRANSACTION;

IF SCHEMA_ID(N'cho') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [cho];');
END;

IF SCHEMA_ID(N'zengin') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA [zengin];');
END;

-- テーブル作成（外部キー依存の順）

IF OBJECT_ID(N'[dbo].[TD_AccountNumberChangeRequests]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_AccountNumberChangeRequests] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [RequestType] CHAR(2) NOT NULL DEFAULT '20',
    [ChangeAction] CHAR(1) NOT NULL,
    [OldBankCode] CHAR(4) NULL,
    [OldBranchCode] CHAR(3) NULL,
    [OldAccountType] CHAR(1) NULL,
    [OldAccountNumber] CHAR(10) NULL,
    [NewBranchCode] CHAR(3) NULL,
    [NewAccountType] CHAR(1) NULL,
    [NewAccountNumber] CHAR(10) NULL,
    [ErrorFlags] CHAR(15) NOT NULL DEFAULT '000000000000000',
    [JobExecutionId] NVARCHAR(50) NOT NULL,
    [BatchStatus] VARCHAR(16) NOT NULL DEFAULT 'SUCCESS',
    [FailedByProgram] VARCHAR(20) NULL,
    [FailedReasonCode] CHAR(4) NULL,
    [FailedAt] DATETIME2(3) NULL,
    [Version] INT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_AccountNumberChangeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AccountNumberChangeRequests_Type] CHECK ([RequestType] = '20'),
    CONSTRAINT [CK_AccountNumberChangeRequests_Status]
        CHECK ([BatchStatus] IN ('PROCESSING', 'SUCCESS', 'FAILED'))
);
END;

IF OBJECT_ID(N'[zengin].[TD_AccumulatedZenginRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [zengin].[TD_AccumulatedZenginRecords] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [BankCode]        CHAR(4)         NOT NULL,
    [BranchCode]      CHAR(3)         NOT NULL,
    [AccountType]     CHAR(1)         NOT NULL,
    [AccountNo]       CHAR(7)         NOT NULL,
    [DepositorName]   NVARCHAR(30)    NOT NULL,
    [Amount]          DECIMAL(10,0)   NOT NULL,
    [ResultCode]      CHAR(1)         NULL,
    [ConsignorCode]   CHAR(10)        NULL,
    [WithdrawalDate]  DATE            NULL,
    [BankName]        NVARCHAR(15)    NULL,
    [BranchName]      NVARCHAR(15)    NULL,
    [PassbookComment] NVARCHAR(8)     NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_AccumulatedZenginRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_ApplyRuns]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_ApplyRuns] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId]        NVARCHAR(50)    NOT NULL,
    [RunJob]                NVARCHAR(8)     NOT NULL,
    [TransferDate]          DATE            NOT NULL,
    [AttemptNo]             SMALLINT        NOT NULL,
    [Status]                NVARCHAR(16)    NOT NULL DEFAULT 'RUNNING',
    [StartedAt]             DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    [CompletedAt]           DATETIME2(3)    NULL,
    [ExecutedBy]            NVARCHAR(50)    NOT NULL,
    [ContractApplyCount]    INT             NOT NULL DEFAULT 0,
    [ContractErrorCount]    INT             NOT NULL DEFAULT 0,
    [AmountApplyCount]      INT             NOT NULL DEFAULT 0,
    [AmountErrorCount]      INT             NOT NULL DEFAULT 0,
    [ConfirmedBy]           NVARCHAR(50)    NULL,
    [ConfirmedAt]           DATETIME2(3)    NULL,
    [CreatedAt]             DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]             DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TD_ApplyRuns] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_TD_ApplyRuns_Status] CHECK ([Status] IN
        (N'RUNNING', N'REVIEWING', N'CONFIRMED', N'ABANDONED', N'FAILED'))
);
END;

IF OBJECT_ID(N'[dbo].[TD_AmountApplyPending]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_AmountApplyPending] (
    [Id]                     BIGINT IDENTITY(1,1) NOT NULL,
    [ApplyRunId]             BIGINT          NOT NULL,
    [Operation]              NVARCHAR(8)     NOT NULL,
    [TransferAmountId]       BIGINT          NULL,
    [SourceModificationId]   BIGINT          NULL,
    [CompanyCode]            CHAR(6)     NOT NULL,
    [BatchNo]                CHAR(3)     NOT NULL,
    [PersonalCode]           CHAR(12)    NOT NULL,
    [ValidationFlags]        CHAR(15)    NOT NULL DEFAULT '000000000000000',
    [IsApplicable]           BIT             NOT NULL DEFAULT 1,
    [ContractId]             BIGINT          NULL,
    [RecordType]             CHAR(2)     NOT NULL,
    [SequenceNo]             INT             NOT NULL,
    [CheckDigit]             CHAR(1)     NOT NULL,
    [Amount1]                DECIMAL(10,0)   NULL,
    [Amount2]                DECIMAL(10,0)   NULL,
    [Amount3]                DECIMAL(10,0)   NULL,
    [Amount4]                DECIMAL(10,0)   NULL,
    [Amount5]                DECIMAL(10,0)   NULL,
    [ErrorFlag1]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag2]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag3]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag4]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag5]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag6]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag7]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag8]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag9]             CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag10]            CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag11]            CHAR(1)     NOT NULL DEFAULT '0',
    [ErrorFlag12]            CHAR(1)     NOT NULL DEFAULT '0',
    [CreatedAt]              DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]              DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TD_AmountApplyPending] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TD_AmountApplyPending_Run] FOREIGN KEY ([ApplyRunId])
        REFERENCES [dbo].[TD_ApplyRuns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_TD_AmountApplyPending_Operation] CHECK ([Operation] IN
        (N'INSERT', N'UPDATE', N'DELETE'))
);
END;

IF OBJECT_ID(N'[dbo].[TM_Companies]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_Companies] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]           CHAR(6)         NOT NULL,
    [CompanyNameKana]       NVARCHAR(36)    NOT NULL,
    [CompanyNameKanji]      NVARCHAR(30)    NULL,
    [PostalCode]            CHAR(7)         NULL,
    [Prefecture]            NVARCHAR(9)     NULL,
    [City]                  NVARCHAR(20)    NULL,
    [Town1]                 NVARCHAR(24)    NULL,
    [Town2]                 NVARCHAR(24)    NULL,
    [PrefectureKanji]       NVARCHAR(4)     NULL,
    [CityKanji]             NVARCHAR(10)    NULL,
    [TownKanji1]            NVARCHAR(20)    NULL,
    [TownKanji2]            NVARCHAR(20)    NULL,
    [DepartmentKanji]       NVARCHAR(20)    NULL,
    [PersonInChargeKanji]   NVARCHAR(20)    NULL,
    [PhoneNumber]           VARCHAR(13)     NULL,
    [FaxNumber]             VARCHAR(13)     NULL,
    [ZenginLinkFlag]        CHAR(1)         NOT NULL DEFAULT '0',
    [PageChangeKey]         SMALLINT        NOT NULL DEFAULT 0,
    [BasicFee]              DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [AdminFee]              DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [NewUnitPrice]          DECIMAL(5,0)    NOT NULL DEFAULT 0,
    [ModifyUnitPrice]       DECIMAL(5,0)    NOT NULL DEFAULT 0,
    [Transfer1UnitPrice]    DECIMAL(5,0)    NOT NULL DEFAULT 0,
    [Transfer2UnitPrice]    DECIMAL(5,0)    NOT NULL DEFAULT 0,
    [ReceiptUnitPrice]      DECIMAL(5,0)    NOT NULL DEFAULT 0,
    [Transfer1BankCode]     CHAR(4)         NULL,
    [Transfer1BranchCode]   CHAR(3)         NULL,
    [Transfer1AccountType]  CHAR(1)         NULL,
    [Transfer1AccountNo]    CHAR(10)        NULL,
    [ConsignorCode]         CHAR(10)        NOT NULL,
    [Transfer2BankCode]     CHAR(4)         NULL,
    [Transfer2BranchCode]   CHAR(3)         NULL,
    [Transfer2AccountType]  CHAR(1)         NULL,
    [Transfer2AccountNo]    CHAR(10)        NULL,
    [PassbookComment]       NVARCHAR(8)     NULL,
    [TransferCode]          CHAR(6)         NULL,
    [LastTransferDate]      DATE            NULL,
    [ContractSeqNo]         CHAR(10)       NULL,
    [CustomerChargeNo]      CHAR(4)         NULL,
    [CustomerChargeSubType] CHAR(1)         NULL,
    [AccountHolderName]     NVARCHAR(30)    NULL,
    [NewCount]              DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [ModifyCount]           DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Transfer1Count]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Transfer2Count]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [FailureCount]          DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [ConvenienceParams]     NVARCHAR(100)   NULL,
    [PostalTransferParams]  NVARCHAR(100)   NULL,
    [CoopRemitParams]       NVARCHAR(200)   NULL,
    [ResultDeliveryParams]  NVARCHAR(100)   NULL,
    [ProcessingFlag1]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag2]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag3]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag4]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag5]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag6]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag7]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag8]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag9]       CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag10]      CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag11]      CHAR(1)       NOT NULL DEFAULT '0',
    [ProcessingFlag12]      CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag1]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag2]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag3]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag4]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag5]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag6]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag7]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag8]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag9]           CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag10]          CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag11]          CHAR(1)       NOT NULL DEFAULT '0',
    [SheetFlag12]          CHAR(1)       NOT NULL DEFAULT '0',
    [PreviousTransferRound] CHAR(1)       NOT NULL DEFAULT '0',
    [CurrentTransferRound]  CHAR(1)       NOT NULL DEFAULT '0',
    [SuspendFlag]           CHAR(1)         NOT NULL DEFAULT '0',   -- 利用停止フラグ (0=有効, 1=停止),
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Companies_CompanyCode] UNIQUE ([CompanyCode]),
    CONSTRAINT [CK_Companies_ZenginLinkFlag] CHECK ([ZenginLinkFlag] IN ('0', '1', '2'))
);
END;

IF OBJECT_ID(N'[dbo].[TD_AmountModifications]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_AmountModifications] (
    [Id]                      BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]               BIGINT          NULL,
    [CompanyCode]             CHAR(6)         NOT NULL,
    [BatchNo]                 CHAR(3)         NOT NULL,
    [SequenceNo]              CHAR(7)         NOT NULL,
    [TransferType]            CHAR(2)         NOT NULL,
    [KdBatchNo]               CHAR(3)         NOT NULL,
    [ContractCompanyCode]     CHAR(6)         NOT NULL,
    [ContractPersonalCode]    CHAR(12)        NOT NULL,
    [ContractCheckDigit]      CHAR(1)         NOT NULL,
    [Amount1]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Amount2]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Amount3]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Amount4]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Amount5]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [ErrorFlags]              CHAR(12)        NOT NULL DEFAULT '000000000000',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_AmountModifications] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_AmountModifications_Key] UNIQUE ([CompanyCode], [BatchNo], [SequenceNo]),
    CONSTRAINT [FK_AmountModifications_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_AuditRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_AuditRecords] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [OccurredAt]        DATETIMEOFFSET(3) NOT NULL,
    [ActorId]           NVARCHAR(100)       NOT NULL,
    [FeatureId]         NVARCHAR(20)        NOT NULL,
    [Action]            NVARCHAR(20)        NOT NULL,
    [TargetType]        NVARCHAR(100)       NOT NULL,
    [TargetKey]         NVARCHAR(1000)      NOT NULL,
    [Result]            NVARCHAR(32)        NOT NULL,
    [CorrelationId]     NVARCHAR(100)       NOT NULL,
    [BeforeValuesJson]  NVARCHAR(MAX)       NOT NULL,
    [AfterValuesJson]   NVARCHAR(MAX)       NOT NULL,
    [ErrorCode]         NVARCHAR(50)        NULL,
    [CreatedAt]         DATETIME2(3)        NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]         DATETIME2(3)        NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TD_AuditRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_BankBranches]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_BankBranches] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [BankCode]        CHAR(4)       NOT NULL,
    [BranchCode]      CHAR(3)       NOT NULL,
    [BankNameKana]    NVARCHAR(15)  NOT NULL,
    [BranchNameKana]  NVARCHAR(15)  NOT NULL,
    [OfficeName]      NVARCHAR(30)  NULL,
    [BankNameKanji]   NVARCHAR(15)  NULL,
    [BranchNameKanji] NVARCHAR(15)  NULL,
    [KanjiSetFlag]    CHAR(1)       NOT NULL DEFAULT '0',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BankBranches] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_BankBranches_BankBranch] UNIQUE ([BankCode], [BranchCode])
);
END;

IF OBJECT_ID(N'[dbo].[TD_BankBranchChanges]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_BankBranchChanges] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [BankBranchId]    BIGINT          NULL,
    [BankCode]        CHAR(4)         NOT NULL,
    [BranchCode]      CHAR(3)         NOT NULL,
    [ChangeType]      CHAR(1)         NOT NULL,
    [BankNameKana]    NVARCHAR(15)    NULL,
    [BranchNameKana]  NVARCHAR(15)    NULL,
    [BankNameKanji]   NVARCHAR(15)    NULL,
    [BranchNameKanji] NVARCHAR(15)    NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BankBranchChanges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BankBranchChanges_BankBranch] FOREIGN KEY ([BankBranchId])
        REFERENCES [dbo].[TM_BankBranches] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_BankChangeRequests]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_BankChangeRequests] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [RequestType]           CHAR(2)         NOT NULL,
    [ChangeAction]          CHAR(1)         NOT NULL,
    [BankCode]              CHAR(4)         NULL,
    [BranchCode]            CHAR(3)         NULL,
    [BankName]              NVARCHAR(20)    NULL,
    [BranchName]            NVARCHAR(20)    NULL,
    [ErrorFlags]            CHAR(15)        NOT NULL DEFAULT '000000000000000',
    [JobExecutionId]        NVARCHAR(50)    NOT NULL,
    [BatchStatus]           VARCHAR(16)     NOT NULL DEFAULT 'SUCCESS',
    [FailedByProgram]       VARCHAR(20)     NULL,
    [FailedReasonCode]      CHAR(4)         NULL,
    [FailedAt]              DATETIME2(3)    NULL,
    [Version]               INT             NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BankChangeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_BankChangeRequests_Type] CHECK ([RequestType] IN ('01')),
    CONSTRAINT [CK_BankChangeRequests_BatchStatus] CHECK ([BatchStatus] IN ('PROCESSING', 'SUCCESS', 'FAILED'))
);
END;

IF OBJECT_ID(N'[dbo].[TD_BatchErrorChecks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_BatchErrorChecks] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]   CHAR(6)         NOT NULL,
    [BatchNo]       CHAR(3)         NOT NULL,
    [ErrorFlag1]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag2]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag3]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag4]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag5]    CHAR(1)         NOT NULL DEFAULT '0',
    [Total1]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Total2]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Total3]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Total4]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Total5]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BatchErrorChecks] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_BatchErrorChecks_Key] UNIQUE ([CompanyCode], [BatchNo])
);
END;

IF OBJECT_ID(N'[dbo].[TD_BatchExecutionHistories]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_BatchExecutionHistories] (
    [Id]                  BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId]       NVARCHAR(50)         NOT NULL,
    [FunctionId]           NVARCHAR(20)         NOT NULL,
    [JobId]                NVARCHAR(40)         NOT NULL,
    [ProcessDate]          DATE                 NOT NULL,
    [StartedAt]            DATETIME2(3)         NOT NULL,
    [CompletedAt]          DATETIME2(3)         NULL,
    [Status]               NVARCHAR(16)         NOT NULL,
    [ReadCount]            INT                  NOT NULL DEFAULT (0),
    [WriteCount]           INT                  NOT NULL DEFAULT (0),
    [ErrorCount]           INT                  NOT NULL DEFAULT (0),
    [Message]              NVARCHAR(4000)       NULL,
    [CreatedAt]            DATETIME2(3)         NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]            DATETIME2(3)         NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TD_BatchExecutionHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_TD_BatchExecutionHistories_JobExecutionId_StartedAt] UNIQUE ([JobExecutionId], [StartedAt])
);
END;

IF OBJECT_ID(N'[cho].[TD_BillingRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_BillingRecords] (
    [Id]                      BIGINT IDENTITY(1,1) NOT NULL,
    [SettlementType]          CHAR(2)         NOT NULL,
    [BusinessCode]            CHAR(10)        NOT NULL,
    [ItemCode]                CHAR(4)         NOT NULL,
    [SalesYearMonth]          DATE            NOT NULL,
    [UnitPrice]               DECIMAL(10,2)   NOT NULL,
    [Quantity]                DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [TotalAmount]             DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [BusinessProcessType]     CHAR(1)         NULL,
    [BusinessProcessDate]     DATE            NULL,
    [AccountingResultType]    CHAR(1)         NULL,
    [AccountingProcessDate]   DATE            NULL,
    [CreatedAt]               DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]               DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BillingRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_BillingRecords_Key] UNIQUE
        ([SettlementType], [BusinessCode], [ItemCode], [SalesYearMonth])
);
END;

IF OBJECT_ID(N'[cho].[TD_ChoZenginBatches]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_ChoZenginBatches] (
    [Id]                  BIGINT IDENTITY(1,1) NOT NULL,
    [TypeCode]            NVARCHAR(2)     NOT NULL,
    [ConsignorCode]       NVARCHAR(10)    NOT NULL,
    [ConsignorName]       NVARCHAR(40)    NOT NULL,
    [WithdrawalDate]      DATE            NOT NULL,
    [TransferBankCode]    NVARCHAR(4)     NULL,
    [TransferBranchCode]  NVARCHAR(3)     NULL,
    [TransferAccountNo]   NVARCHAR(10)    NULL,
    [TotalCount]          INT             NULL,
    [TotalAmount]         DECIMAL(12,0)   NULL,
    [SettledCount]        INT             NULL,
    [SettledAmount]       DECIMAL(12,0)   NULL,
    [FailedCount]         INT             NULL,
    [FailedAmount]        DECIMAL(12,0)   NULL,
    [CreatedAt]           DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]           DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ChoZenginBatches] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[cho].[TD_ChoZenginTransactions]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_ChoZenginTransactions] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [ZenginBatchId]     BIGINT          NOT NULL,
    [BankCode]          NVARCHAR(4)    NOT NULL,
    [BranchCode]        NVARCHAR(3)    NOT NULL,
    [AccountType]       NVARCHAR(1)    NOT NULL,
    [AccountNo]         NVARCHAR(7)    NOT NULL,
    [DepositorName]     NVARCHAR(30)   NOT NULL,
    [Amount]            DECIMAL(10,0)  NOT NULL,
    [NewCode]           NVARCHAR(1)    NOT NULL,
    [ContractorCode]    NVARCHAR(19)   NULL,
    [ResultCode]        NVARCHAR(1)    NULL,
    [CreatedAt]         DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]         DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ChoZenginTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChoZenginTransactions_ChoZenginBatches]
        FOREIGN KEY ([ZenginBatchId])
        REFERENCES [cho].[TD_ChoZenginBatches]([Id])
        ON DELETE CASCADE
);
END;

IF OBJECT_ID(N'[dbo].[TD_CollectionAmountRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_CollectionAmountRecords] (
    [Id]           BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]  CHAR(6)       NOT NULL,
    [PersonalCode] CHAR(12)      NOT NULL,
    [ChangeType]   CHAR(2)       NOT NULL DEFAULT '23',
    [ChangeAction] CHAR(1)       NULL DEFAULT '2',
    [TypeNo]       SMALLINT      NOT NULL,
    [TypeCode]     CHAR(1)       NOT NULL,
    [Amount]       DECIMAL(10,0) NOT NULL,
    [CreatedAt]    DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]    DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CollectionAmountRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_CompanyChangeRequests]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_CompanyChangeRequests] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [RequestType]           CHAR(2)         NOT NULL,   -- 11-19 または 71(CBAN 1～3)
    [Cban]                  VARCHAR(1)      NULL,       -- 71 のときのみ 1～3
    [ChangeAction]          CHAR(1)         NOT NULL,
    [CompanyCode]           CHAR(6)         NULL,
    [CompanyName]           NVARCHAR(40)    NULL,
    [PhoneNumber]           VARCHAR(13)     NULL,
    [PostalCode]            CHAR(7)         NULL,
    [Prefecture]            NVARCHAR(9)     NULL,
    [City]                  NVARCHAR(20)    NULL,
    [Town1]                 NVARCHAR(24)    NULL,
    [Town2]                 NVARCHAR(24)    NULL,
    [WithdrawalDay1]        NVARCHAR(2)     NULL,
    [WithdrawalDay2]        NVARCHAR(2)     NULL,
    [WithdrawalDay3]        NVARCHAR(2)     NULL,
    [WithdrawalDay4]        NVARCHAR(2)     NULL,
    [ProcessingFlag1]       NVARCHAR(1)     NULL,
    [ProcessingFlag2]       NVARCHAR(1)     NULL,
    [ProcessingFlag3]       NVARCHAR(1)     NULL,
    [ProcessingFlag4]       NVARCHAR(1)     NULL,
    [ProcessingFlag5]       NVARCHAR(1)     NULL,
    [ProcessingFlag6]       NVARCHAR(1)     NULL,
    [ProcessingFlag7]       NVARCHAR(1)     NULL,
    [ProcessingFlag8]       NVARCHAR(1)     NULL,
    [ProcessingFlag9]       NVARCHAR(1)     NULL,
    [ProcessingFlag10]      NVARCHAR(1)     NULL,
    [ProcessingFlag11]      NVARCHAR(1)     NULL,
    [ProcessingFlag12]      NVARCHAR(1)     NULL,
    [SheetFlag1]            NVARCHAR(1)     NULL,
    [SheetFlag2]            NVARCHAR(1)     NULL,
    [SheetFlag3]            NVARCHAR(1)     NULL,
    [SheetFlag4]            NVARCHAR(1)     NULL,
    [SheetFlag5]            NVARCHAR(1)     NULL,
    [SheetFlag6]            NVARCHAR(1)     NULL,
    [SheetFlag7]            NVARCHAR(1)     NULL,
    [SheetFlag8]            NVARCHAR(1)     NULL,
    [SheetFlag9]            NVARCHAR(1)     NULL,
    [SheetFlag10]           NVARCHAR(1)     NULL,
    [SheetFlag11]           NVARCHAR(1)     NULL,
    [SheetFlag12]           NVARCHAR(1)     NULL,
    [ProcessingFlagsJson]   NVARCHAR(100)   NULL,
    [ReportFlagsJson]       NVARCHAR(100)   NULL,
    [BasicFee]              DECIMAL(6,0)   NULL,
    [AdminFee]              DECIMAL(6,0)   NULL,
    [PageChangeKey]         DECIMAL(2,0)  NOT NULL DEFAULT 0,
    [NewUnitPrice]          DECIMAL(4,0)  NULL,
    [ModifyUnitPrice]       DECIMAL(4,0)  NULL,
    [Transfer1UnitPrice]    DECIMAL(4,0)  NULL,
    [Transfer2UnitPrice]    DECIMAL(4,0)  NULL,
    [ReceiptUnitPrice]      DECIMAL(4,0)  NULL,
    [Transfer1BankCode]     CHAR(4)         NULL,
    [Transfer1BranchCode]   CHAR(3)         NULL,
    [Transfer1AccountType]  CHAR(1)         NULL,
    [Transfer1AccountNo]    CHAR(10)        NULL,
    [Transfer2BankCode]     NVARCHAR(4)     NULL,
    [Transfer2BranchCode]   NVARCHAR(3)     NULL,
    [Transfer2AccountType]  NVARCHAR(1)     NULL,
    [Transfer2AccountNo]    NVARCHAR(10)    NULL,
    [PassbookComment]       NVARCHAR(8)     NULL,
    [ConsignorCode]         CHAR(10)        NULL,
    [TypeCategory]          NVARCHAR(1)     NULL,
    [Cycle]                 NVARCHAR(2)     NULL,
    [OperatingYear]         NVARCHAR(4)     NULL,
    [OperatingMonth]        NVARCHAR(2)     NULL,
    [TypeName]              NVARCHAR(12)    NULL,
    [Amount]                DECIMAL(8,0)    NULL,
    [CompanyNameKanji]      NVARCHAR(30)    NULL,
    [PrefectureKanji]       NVARCHAR(4)     NULL,
    [CityKanji]             NVARCHAR(10)    NULL,
    [TownKanji1]            NVARCHAR(20)    NULL,
    [TownKanji2]            NVARCHAR(20)    NULL,
    [DepartmentKanji]       NVARCHAR(20)    NULL,
    [PersonInChargeKanji]   NVARCHAR(20)    NULL,
    [IsRequiredOrFormatError] BIT          NOT NULL DEFAULT 0,
    [IsApplied]            BIT             NOT NULL DEFAULT 0,
    [IsUnitPriceOrOperatingYearMonthError] BIT NOT NULL DEFAULT 0,
    [IsModifyUnitPriceOrAmountError] BIT  NOT NULL DEFAULT 0,
    [IsTransfer1UnitPriceError] BIT       NOT NULL DEFAULT 0,
    [IsTransfer2UnitPriceError] BIT       NOT NULL DEFAULT 0,
    [IsReceiptUnitPriceError] BIT         NOT NULL DEFAULT 0,
    [IsChangeActionError]   BIT           NOT NULL DEFAULT 0,
    [IsCombinationOrNotApplicableError] BIT NOT NULL DEFAULT 0,
    [IsCountExceededError]  BIT           NOT NULL DEFAULT 0,
    [JobExecutionId]        NVARCHAR(50)    NOT NULL,
    [BatchStatus]           VARCHAR(16)     NOT NULL DEFAULT 'SUCCESS',
    [FailedByProgram]       VARCHAR(20)     NULL,
    [FailedReasonCode]      CHAR(4)         NULL,
    [FailedAt]              DATETIME2(3)    NULL,
    [Version]               INT             NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CompanyChangeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CompanyChangeRequests_BatchStatus] CHECK ([BatchStatus] IN ('PROCESSING', 'SUCCESS', 'FAILED')),
    CONSTRAINT [CK_TD_CompanyChangeRequests_RequestType]
        CHECK ([RequestType] IN ('11', '12', '13', '14', '15', '16', '17', '18', '19', '71')),
    CONSTRAINT [CK_TD_CompanyChangeRequests_Cban]
        CHECK (
            ([RequestType] = '71' AND [Cban] IS NOT NULL AND [Cban] IN ('1', '2', '3'))
            OR ([RequestType] <> '71' AND [Cban] IS NULL)
        )
);
END;

IF OBJECT_ID(N'[dbo].[TD_ContractApplyPending]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_ContractApplyPending] (
    [Id]                     BIGINT IDENTITY(1,1) NOT NULL,
    [ApplyRunId]             BIGINT          NOT NULL,
    [Source]                 NVARCHAR(16)    NOT NULL,
    [Operation]              NVARCHAR(8)     NOT NULL,
    [ContractId]             BIGINT          NULL,
    [SourceRequestId]        BIGINT          NULL,
    [CompanyCode]            CHAR(6)     NOT NULL,
    [PersonalCode]           CHAR(12)    NOT NULL,
    [CheckDigit]             CHAR(1)     NOT NULL,
    [ValidationFlags]        CHAR(15)    NOT NULL DEFAULT '000000000000000',
    [IsApplicable]           BIT             NOT NULL DEFAULT 1,
    [CompanyId]              BIGINT          NOT NULL,
    [BankBranchId]           BIGINT          NULL,
    [CodeSave]               CHAR(1)     NOT NULL DEFAULT ' ',
    [WithdrawalDay]          SMALLINT        NOT NULL DEFAULT 0,
    [StartYearMonth]         CHAR(6)     NULL,
    [SuspendFlag]            CHAR(1)     NOT NULL DEFAULT '0',
    [NewFlag]                CHAR(1)     NOT NULL DEFAULT '0',
    [ZenginFlag]             CHAR(1)     NOT NULL DEFAULT '0',
    [NotifiedFlag]           CHAR(1)     NOT NULL DEFAULT '0',
    [ResultFlag]             CHAR(1)     NOT NULL DEFAULT '0',
    [ProcessType]            CHAR(1)     NOT NULL DEFAULT '0',
    [CreditCompleteFlag]     CHAR(1)     NOT NULL DEFAULT '0',
    [TransferMethod]         CHAR(1)     NOT NULL DEFAULT '0',
    [AutoDeleteFlag]         CHAR(1)     NOT NULL DEFAULT '0',
    [FailureCount]           SMALLINT        NOT NULL DEFAULT 0,
    [DepositorNameKana]      NVARCHAR(32)    NOT NULL,
    [ContractorNameKana]     NVARCHAR(32)    NULL,
    [DepositorNameKanji]     NVARCHAR(20)    NULL,
    [ContractorNameKanji]    NVARCHAR(20)    NULL,
    [BankCode]               CHAR(4)     NOT NULL,
    [BranchCode]             CHAR(3)     NOT NULL,
    [AccountType]            CHAR(1)     NOT NULL,
    [AccountNo]              CHAR(10)    NOT NULL,
    [PostalCode]             CHAR(7)     NULL,
    [Prefecture]             NVARCHAR(9)     NULL,
    [City]                   NVARCHAR(20)    NULL,
    [Town1]                  NVARCHAR(24)    NULL,
    [Town2]                  NVARCHAR(24)    NULL,
    [PhoneNumber]            VARCHAR(7)     NULL,
    [Type1StartYearMonth]    CHAR(6)     NULL,
    [Type1Cycle]             CHAR(2)     NULL,
    [Type1Amount]            DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Type2StartYearMonth]    CHAR(6)     NULL,
    [Type2Cycle]             CHAR(2)     NULL,
    [Type2Amount]            DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Type3StartYearMonth]    CHAR(6)     NULL,
    [Type3Cycle]             CHAR(2)     NULL,
    [Type3Amount]            DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Type4StartYearMonth]    CHAR(6)     NULL,
    [Type4Cycle]             CHAR(2)     NULL,
    [Type4Amount]            DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Billing1Amount]         DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Billing2Amount]         DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Billing3Amount]         DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Billing4Amount]         DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [CreditProductName]      NVARCHAR(10)    NULL,
    [CreditTotalAmount]      DECIMAL(10,0)   NULL,
    [CreditTotalCount]       SMALLINT        NULL,
    [CreditCompletedCount]   SMALLINT        NULL,
    [CreditPayment1]         DECIMAL(10,0)   NULL,
    [CreditPayment2]         DECIMAL(10,0)   NULL,
    [CreditSpecialAddition]  DECIMAL(10,0)   NULL,
    [CreditBillingAmount]    DECIMAL(10,0)   NULL,
    [PreviousBalance]        DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [CurrentBillingAmount]   DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [WithdrawalDate]         DATE            NULL,
    [BankProcessDate]        DATE            NULL,
    [ExemptionDate]          DATE            NULL,
    [ExemptionType]          CHAR(1)     NULL,
    [ChangeDate]             DATE            NULL,
    [ChangeAction]           CHAR(1)         NULL,
    [InvoiceTaxRate1]        DECIMAL(5,2)    NULL,
    [InvoiceTaxRate2]        DECIMAL(5,2)    NULL,
    [InvoiceBasePrice]       DECIMAL(10,0)   NULL,
    [InvoiceConsumptionTax]  DECIMAL(10,0)   NULL,
    [CreatedAt]              DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]              DATETIME2(3)    NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TD_ContractApplyPending] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TD_ContractApplyPending_Run] FOREIGN KEY ([ApplyRunId])
        REFERENCES [dbo].[TD_ApplyRuns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [CK_TD_ContractApplyPending_Source] CHECK ([Source] IN
        (N'CHANGE_REQUEST', N'ZENGIN_IMPORT')),
    CONSTRAINT [CK_TD_ContractApplyPending_Operation] CHECK ([Operation] IN
        (N'INSERT', N'UPDATE'))
);
END;

IF OBJECT_ID(N'[dbo].[TD_ContractChangeCards]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_ContractChangeCards] (
    [Id]                        BIGINT IDENTITY(1,1) NOT NULL,
    [CardType]                  CHAR(2)         NOT NULL,
    [CardSubType]               CHAR(1)         NOT NULL,
    [ChangeAction]              CHAR(1)         NOT NULL,
    [ContractCode]              CHAR(18)        NOT NULL,
    [DepositorName]             NVARCHAR(32)    NULL,
    [AccountInfo]               CHAR(18)        NULL,
    [WithdrawalDay]             CHAR(2)         NULL,
    [StartYearMonth]            CHAR(4)         NULL,
    [SuspendFlag]               CHAR(1)         NULL,
    [TypeNo]                    CHAR(1)         NULL,
    [AmountText]                CHAR(9)         NULL,
    [NextYearMonth]             CHAR(4)         NULL,
    [CreditProductName]         NVARCHAR(10)    NULL,
    [CreditTotalAmountText]     CHAR(9)         NULL,
    [CreditTotalCountText]      CHAR(2)         NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractChangeCards] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_Contracts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_Contracts] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]             BIGINT          NOT NULL,
    [BankBranchId]          BIGINT          NULL,
    [CompanyCode]           CHAR(6)         NOT NULL,
    [PersonalCode]          CHAR(12)        NOT NULL,
    [CheckDigit]            CHAR(1)         NOT NULL,
    [ContractSeq]           SMALLINT        NOT NULL CONSTRAINT [DF_TM_Contracts_ContractSeq] DEFAULT 0,
    [CodeSave]              CHAR(1)         NOT NULL DEFAULT ' ',
    [WithdrawalDay]         SMALLINT        NOT NULL DEFAULT 0,
    [StartYearMonth]        CHAR(6)         NULL,
    [SuspendFlag]           CHAR(1)         NOT NULL DEFAULT '0',
    [NewFlag]               CHAR(1)         NOT NULL DEFAULT '0',
    [ZenginFlag]            CHAR(1)         NOT NULL DEFAULT '0',
    [NotifiedFlag]          CHAR(1)         NOT NULL DEFAULT '0',
    [ResultFlag]            CHAR(1)         NOT NULL DEFAULT '0',
    [ProcessType]           CHAR(1)         NOT NULL DEFAULT '0',
    [CreditCompleteFlag]    CHAR(1)         NOT NULL DEFAULT '0',
    [TransferMethod]        CHAR(1)         NOT NULL DEFAULT '0',
    [AutoDeleteFlag]        CHAR(1)         NOT NULL DEFAULT '0',
    [FailureCount]          SMALLINT        NOT NULL DEFAULT 0,
    [DepositorNameKana]     NVARCHAR(32)    NOT NULL,
    [ContractorNameKana]    NVARCHAR(32)    NULL,
    [DepositorNameKanji]    NVARCHAR(20)    NULL,
    [ContractorNameKanji]   NVARCHAR(20)    NULL,
    [PostalCode]            CHAR(7)         NULL,
    [Prefecture]            NVARCHAR(9)     NULL,
    [City]                  NVARCHAR(20)    NULL,
    [Town1]                 NVARCHAR(24)    NULL,
    [Town2]                 NVARCHAR(24)    NULL,
    [PhoneNumber]           VARCHAR(7)      NULL,
    [BankCode]              CHAR(4)         NOT NULL,
    [BranchCode]            CHAR(3)         NOT NULL,
    [AccountType]           CHAR(1)         NOT NULL,
    [AccountNo]             CHAR(10)        NOT NULL,
    [CreditProductName]     NVARCHAR(10)    NULL,
    [CreditTotalAmount]     DECIMAL(10,0)   NULL,
    [CreditTotalCount]      SMALLINT        NULL,
    [CreditCompletedCount]  SMALLINT        NULL,
    [CreditPayment1]        DECIMAL(10,0)   NULL,
    [CreditPayment2]        DECIMAL(10,0)   NULL,
    [CreditSpecialAddition] DECIMAL(10,0)   NULL,
    [CreditBillingAmount]   DECIMAL(10,0)   NULL,
    [PreviousBalance]       DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [CurrentBillingAmount]  DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [WithdrawalDate]        DATE            NULL,
    [BankProcessDate]       DATE            NULL,
    [ExemptionDate]         DATE            NULL,
    [ExemptionType]         CHAR(1)         NULL,
    [ChangeDate]            DATE            NULL,
    [ChangeAction]          CHAR(1)         NULL,
    [InvoiceTaxRate1]       DECIMAL(5,2)    NULL,
    [InvoiceTaxRate2]       DECIMAL(5,2)    NULL,
    [InvoiceBasePrice]      DECIMAL(10,0)   NULL,
    [InvoiceConsumptionTax] DECIMAL(10,0)   NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contracts_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_ContractChangeRequests]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_ContractChangeRequests] (
    [Id]                      BIGINT IDENTITY(1,1) NOT NULL,
    [RequestType]             CHAR(2)         NOT NULL,
    [ChangeAction]            CHAR(1)         NOT NULL,
    [CompanyCode]             CHAR(6)         NOT NULL,
    [PersonalCode]            CHAR(12)        NOT NULL,
    [ContractId]              BIGINT          NULL,
    [TransferDate]            DATE            NOT NULL CONSTRAINT [DF_TD_ContractChangeRequests_TransferDate] DEFAULT '1900-01-01',
    [SequenceNo]              SMALLINT        NOT NULL,
    [CompanyName]             NVARCHAR(36)    NULL,
    [MessageNo]               SMALLINT        NULL,
    [ContractorName]          NVARCHAR(32)    NULL,
    [DepositorName]           NVARCHAR(32)    NULL,
    [BankCode]                CHAR(4)         NULL,
    [BranchCode]              CHAR(3)         NULL,
    [AccountType]             CHAR(1)         NULL,
    [AccountNo]               CHAR(10)        NULL,
    [WithdrawalDay]           SMALLINT        NULL,
    [StartYearMonth]          CHAR(6)         NULL,
    [SuspendFlag]             CHAR(1)         NULL,
    [PostalCode]              CHAR(7)         NULL,
    [Prefecture]              NVARCHAR(9)     NULL,
    [City]                    NVARCHAR(20)    NULL,
    [Town1]                   NVARCHAR(24)    NULL,
    [Town2]                   NVARCHAR(24)    NULL,
    [PhoneNumber]             VARCHAR(7)      NULL,
    [DischargeType]           CHAR(1)         NULL,
    [TypeNo]                  SMALLINT        NULL,
    [Amount]                  DECIMAL(10,0)   NULL,
    [NextTransferYearMonth]   CHAR(6)         NULL,
    [CreditProductName]       NVARCHAR(10)    NULL,
    [CreditTotalAmount]       DECIMAL(10,0)   NULL,
    [CreditTotalCount]        SMALLINT        NULL,
    [CreditPayment1]          DECIMAL(10,0)   NULL,
    [CreditPayment2]          DECIMAL(10,0)   NULL,
    [CreditSpecialAddition]   DECIMAL(10,0)   NULL,
    [ErrorFlags]              CHAR(15)        NOT NULL DEFAULT '000000000000000',
    [JobExecutionId]          NVARCHAR(50)    NOT NULL,
    [BatchStatus]             VARCHAR(16)     NOT NULL DEFAULT 'SUCCESS',
    [FailedByProgram]         VARCHAR(20)     NULL,
    [FailedReasonCode]        CHAR(4)         NULL,
    [FailedAt]                DATETIME2(3)    NULL,
    [Version]                 INT             NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractChangeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ContractChangeRequests_RequestType] CHECK ([RequestType] IN ('21','22','23','24')),
    CONSTRAINT [CK_ContractChangeRequests_BatchStatus] CHECK ([BatchStatus] IN ('PROCESSING', 'SUCCESS', 'FAILED')),
    CONSTRAINT [FK_TD_ContractChangeRequests_Contract] FOREIGN KEY ([ContractId])
        REFERENCES [dbo].[TM_Contracts] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_ContractorCodeConversions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_ContractorCodeConversions] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [RequestType]       CHAR(2)         NOT NULL,
    [OldCompanyCode]    CHAR(6)         NOT NULL,
    [OldPersonalCode]   CHAR(12)        NOT NULL,
    [NewPersonalCode]   CHAR(12)        NOT NULL,
    [ErrorFlag]         CHAR(1)         NOT NULL DEFAULT '0',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractorCodeConversions] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[cho].[TD_CooperativeTransferReceipts]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_CooperativeTransferReceipts] (
    [JobExecutionId]      NVARCHAR(50)     NOT NULL,
    [Status]              NVARCHAR(20)     NOT NULL,
    [StagingIdentifier]   NVARCHAR(200)    NOT NULL,
    [ErrorCode]           NVARCHAR(20)     NULL,
    [ErrorDetail]         NVARCHAR(1000)   NULL,
    [ProcessingYearMonth] CHAR(6)          NOT NULL,
    [TransferMonth]       CHAR(2)          NOT NULL,
    [RecordCount]         INT              NOT NULL DEFAULT 0,
    [CreatedAt]           DATETIME2(7)     NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]           DATETIME2(7)     NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CooperativeTransferReceipts] PRIMARY KEY ([JobExecutionId]),
    CONSTRAINT [CK_CooperativeTransferReceipts_Status]
        CHECK ([Status] IN ('Received', 'Processing', 'Confirmed', 'Rejected', 'Invalidated')),
    CONSTRAINT [CK_CooperativeTransferReceipts_RecordCount]
        CHECK ([RecordCount] >= 0)
);
END;

IF OBJECT_ID(N'[cho].[TD_CooperativeTransfers]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_CooperativeTransfers] (
    [Id]                  BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId]      NVARCHAR(50)    NOT NULL,
    [ReceiptSequence]     INT             NOT NULL,
    [Status]              NVARCHAR(10)    NOT NULL,
    [RecordType]          CHAR(1)         NOT NULL,
    [ConsignorCode]       CHAR(10)        NULL,
    [ConsignorNameKana]   NVARCHAR(40)    NULL,
    [WithdrawalMonth]     CHAR(2)         NULL,
    [WithdrawalDay]       CHAR(2)         NULL,
    [BankCode]            CHAR(4)         NULL,
    [BranchCode]          CHAR(3)         NULL,
    [ContractNo]          CHAR(4)         NULL,
    [MemberCode]          CHAR(6)         NULL,
    [PlanCode]            CHAR(4)         NULL,
    [CooperativeNo]       CHAR(3)         NULL,
    [BranchOfficeNo]      CHAR(3)         NULL,
    [BillingBranchOfficeNo] CHAR(3)       NULL,
    [InsuranceType]       CHAR(2)         NULL,
    [PaymentMethod]       CHAR(1)         NULL,
    [ContractDate]        CHAR(6)         NULL,
    [AnnualMonthlyType]   CHAR(1)         NULL,
    [AccountType]         CHAR(1)         NULL,
    [AccountNo]           CHAR(7)         NULL,
    [DepositorName]       NVARCHAR(30)    NULL,
    [Amount]              DECIMAL(10,0)   NULL,
    [NewCode]             CHAR(1)         NULL,
    [InvariantNo]         CHAR(14)        NULL,
    [PremiumYearMonth]    CHAR(6)         NULL,
    [ResultCode]          CHAR(1)         NULL,
    [PostalBankCode]      CHAR(4)         NULL,
    [PostalBranchCode]    CHAR(3)         NULL,
    [PostalAccountType]   CHAR(1)         NULL,
    [PostalAccountNo]     CHAR(10)        NULL,
    [KozConsignorCode]    CHAR(6)         NULL,
    [MatchYearMonth]      CHAR(6)         NULL,
    [MatchSeqNo]          INT             NULL,
    [TotalCount]          INT             NULL,
    [TotalAmount]         DECIMAL(12,0)   NULL,
    [SettledCount]        INT             NULL,
    [SettledAmount]       DECIMAL(12,0)   NULL,
    [FailedCount]         INT             NULL,
    [FailedAmount]        DECIMAL(12,0)   NULL,
    [CreatedAt]           DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]           DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CooperativeTransfers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CooperativeTransfers_CooperativeTransferReceipts]
        FOREIGN KEY ([JobExecutionId])
        REFERENCES [cho].[TD_CooperativeTransferReceipts] ([JobExecutionId]),
    CONSTRAINT [UQ_CooperativeTransfers_JobExecutionId_ReceiptSequence]
        UNIQUE ([JobExecutionId], [ReceiptSequence]),
    CONSTRAINT [CK_CooperativeTransfers_Type] CHECK ([RecordType] IN ('1','2','8','9')),
    CONSTRAINT [CK_CooperativeTransfers_ReceiptSequence] CHECK ([ReceiptSequence] >= 1),
    CONSTRAINT [CK_CooperativeTransfers_Status] CHECK ([Status] IN ('Processing', 'Confirmed'))
);
END;

IF OBJECT_ID(N'[cho].[TD_DailyAccountingEntries]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_DailyAccountingEntries] (
    [Id]                  BIGINT IDENTITY(1,1) NOT NULL,
    [DataType]            CHAR(2)         NOT NULL,
    [CooperativeNo]       CHAR(3)         NOT NULL,
    [BranchOfficeNo]      CHAR(2)         NOT NULL,
    [AccountCode]         CHAR(4)         NOT NULL,
    [DebitCreditType]     CHAR(1)         NOT NULL,
    [TransactionAmount]   DECIMAL(13,0)   NOT NULL,
    [SettlementDate]      DATE            NOT NULL,
    [TransferCount]       INT             NOT NULL DEFAULT 0,
    [ProcessingDate]      DATE            NULL,
    [TransactionCode]     CHAR(5)         NULL,
    [SubtotalType]        CHAR(1)         NULL,
    [CreatedAt]           DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]           DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_DailyAccountingEntries] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_MessageChangeRequests]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_MessageChangeRequests] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]             BIGINT          NULL,
    [RequestType]           CHAR(2)         NOT NULL,
    [Cban]                  VARCHAR(2)      NOT NULL,
    [ChangeAction]          CHAR(1)         NOT NULL,
    [CompanyCode]           CHAR(6)         NULL,
    [TekiyoKubun]           CHAR(1)         NULL,
    [StartYear]             INT             NULL,
    [StartMonth]            INT             NULL,
    [StartDay]              INT             NULL,
    [EndYear]               INT             NULL,
    [EndMonth]              INT             NULL,
    [EndDay]                INT             NULL,
    [Midashi]               NVARCHAR(17)    NULL,
    [Midashi1]              NVARCHAR(24)    NULL,
    [Midashi2]              NVARCHAR(24)    NULL,
    [AvisCode]              CHAR(2)         NULL,
    [ErrorFlags]            CHAR(15)        NOT NULL DEFAULT '000000000000000',
    [JobExecutionId]        NVARCHAR(50)    NOT NULL,
    [BatchStatus]           VARCHAR(16)     NOT NULL DEFAULT 'SUCCESS',
    [FailedByProgram]       VARCHAR(20)     NULL,
    [FailedReasonCode]      CHAR(4)         NULL,
    [FailedAt]              DATETIME2(3)    NULL,
    [Version]               INT             NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_MessageChangeRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MessageChangeRequests_Company] FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[TM_Companies] ([Id]),
    CONSTRAINT [CK_MessageChangeRequests_BatchStatus] CHECK ([BatchStatus] IN ('PROCESSING', 'SUCCESS', 'FAILED'))
);
END;

IF OBJECT_ID(N'[dbo].[TD_NotificationMessages]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_NotificationMessages] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]       BIGINT          NOT NULL,
    [CompanyCode]     CHAR(6)         NOT NULL,
    [AvisCode]        CHAR(2)         NOT NULL,
    [ApplicableType]  CHAR(1)         NOT NULL,
    [ValidFrom]       DATE            NULL,
    [ValidTo]         DATE            NULL,
    [Heading]         NVARCHAR(MAX)   NULL,
    [Body]            NVARCHAR(MAX)   NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_NotificationMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_NotificationMessages_Key] UNIQUE ([CompanyCode], [AvisCode]),
    CONSTRAINT [FK_NotificationMessages_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_OutputLogs]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_OutputLogs] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [LogDate]       DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [Source]        NVARCHAR(20)    NOT NULL,
    [ProgramName]   CHAR(8)         NOT NULL,
    [MessageCode]   CHAR(4)         NULL,
    [MessageText]   NVARCHAR(200)   NULL,
    [RecordCount]   INT             NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_OutputLogs] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[cho].[TD_ProcessingInstructions]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TD_ProcessingInstructions] (
    [Id]                      BIGINT IDENTITY(1,1) NOT NULL,
    [SettlementType]          CHAR(2)         NOT NULL,
    [TargetYearMonth]         DATE            NOT NULL,
    [AccountingResultType]    CHAR(1)         NULL,
    [BusinessUpdateDate]      DATE            NULL,
    [AccountingUpdateDate]    DATE            NULL,
    [CreatedAt]               DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]               DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ProcessingInstructions] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_ProcessingInstructions_Key] UNIQUE ([SettlementType], [TargetYearMonth])
);
END;

IF OBJECT_ID(N'[dbo].[TD_TransferAmounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_TransferAmounts] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [ContractId]    BIGINT          NULL,
    [RecordType]    CHAR(2)         NOT NULL,
    [BatchNo]       CHAR(3)         NOT NULL,
    [SequenceNo]    INT             NOT NULL,
    [CompanyCode]   CHAR(6)         NOT NULL,
    [PersonalCode]  CHAR(12)        NOT NULL,
    [CheckDigit]    CHAR(1)         NOT NULL,
    [Amount1]       DECIMAL(10,0)   NULL,
    [Amount2]       DECIMAL(10,0)   NULL,
    [Amount3]       DECIMAL(10,0)   NULL,
    [Amount4]       DECIMAL(10,0)   NULL,
    [Amount5]       DECIMAL(10,0)   NULL,
    [ErrorFlag1]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag2]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag3]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag4]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag5]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag6]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag7]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag8]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag9]    CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag10]   CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag11]   CHAR(1)         NOT NULL DEFAULT '0',
    [ErrorFlag12]   CHAR(1)         NOT NULL DEFAULT '0',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TransferAmounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TransferAmounts_Contract] FOREIGN KEY ([ContractId])
        REFERENCES [dbo].[TM_Contracts] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_TransferFailures]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_TransferFailures] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [ContractId]    BIGINT          NULL,
    [WithdrawalDay] CHAR(2)         NOT NULL,
    [TransferType]  CHAR(2)         NOT NULL,
    [CompanyCode]   CHAR(6)         NOT NULL,
    [PersonalCode]  CHAR(12)        NOT NULL,
    [CheckDigit]    CHAR(1)         NOT NULL,
    [ResultCode]    CHAR(1)         NOT NULL,
    [TapeType]      CHAR(1)         NOT NULL DEFAULT '0',
    [BankCode]      CHAR(4)         NOT NULL,
    [TransferRound] INT             NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TransferFailures] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_TransferFailures_Key] UNIQUE
        ([WithdrawalDay], [TransferType], [CompanyCode], [PersonalCode], [CheckDigit]),
    CONSTRAINT [FK_TransferFailures_Contract] FOREIGN KEY ([ContractId])
        REFERENCES [dbo].[TM_Contracts] ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TD_TransferTransactions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TD_TransferTransactions] (
    [Id]                          BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]                   BIGINT          NULL,
    [ConsignorCode]               CHAR(10)        NOT NULL,
    [ConsignorName]               NVARCHAR(40)    NULL,
    [WithdrawalDate]              DATE            NOT NULL,
    [BankCode]                    CHAR(4)         NOT NULL,
    [BranchCode]                  CHAR(3)         NOT NULL,
    [BankName]                    NVARCHAR(15)    NULL,
    [BranchName]                  NVARCHAR(15)    NULL,
    [AccountType]                 CHAR(1)         NOT NULL,
    [AccountNo]                   CHAR(10)        NOT NULL,
    [DepositorName]               NVARCHAR(30)    NOT NULL,
    [Amount]                      DECIMAL(10,0)   NOT NULL,
    [NewCode]                     CHAR(1)         NOT NULL DEFAULT '0',
    [CompanyCode]                 CHAR(6)         NOT NULL,
    [PersonalCode]                CHAR(12)        NOT NULL,
    [CheckDigit]                  CHAR(1)         NOT NULL,
    [ResultCode]                  CHAR(1)         NULL,
    [PassbookComment]             NVARCHAR(8)     NULL,
    [Type1Amount]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Type2Amount]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Type3Amount]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [Type4Amount]                 DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [CreditAmount]                DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [PreviousBalance]             DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [CurrentBilling]              DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [ContractorName]              NVARCHAR(30)    NULL,
    [TransferAccountBankCode]     CHAR(4)         NULL,
    [TransferAccountBranchCode]   CHAR(3)         NULL,
    [TransferAccountNo]           CHAR(10)        NULL,
    [TransferRound]               INT             NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TransferTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TransferTransactions_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id])
);
END;

IF OBJECT_ID(N'[zengin].[TD_ZenginBatches]', N'U') IS NULL
BEGIN
CREATE TABLE [zengin].[TD_ZenginBatches] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [TypeCode]              CHAR(2)         NOT NULL DEFAULT '91',
    [ConsignorCode]         CHAR(10)        NOT NULL,
    [ConsignorName]         NVARCHAR(40)    NOT NULL,
    [WithdrawalDate]        DATE            NOT NULL,
    [TransferBankCode]      CHAR(4)         NULL,
    [TransferBranchCode]    CHAR(3)         NULL,
    [TransferAccountNo]     CHAR(10)        NULL,
    [TotalCount]            INT             NULL,
    [TotalAmount]           DECIMAL(12,0)   NULL,
    [SettledCount]          INT             NULL,
    [SettledAmount]         DECIMAL(12,0)   NULL,
    [FailedCount]           INT             NULL,
    [FailedAmount]          DECIMAL(12,0)   NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ZenginBatches] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[zengin].[TD_ZenginTransactions]', N'U') IS NULL
BEGIN
CREATE TABLE [zengin].[TD_ZenginTransactions] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ZenginBatchId]   BIGINT          NOT NULL,
    [BankCode]        CHAR(4)         NOT NULL,
    [BankName]        NVARCHAR(15)    NULL,
    [BranchCode]      CHAR(3)         NOT NULL,
    [BranchName]      NVARCHAR(15)    NULL,
    [AccountType]     CHAR(1)         NOT NULL,
    [AccountNo]       CHAR(7)         NOT NULL,
    [DepositorName]   NVARCHAR(30)    NOT NULL,
    [Amount]          DECIMAL(10,0)   NOT NULL,
    [NewCode]         CHAR(1)         NOT NULL DEFAULT '0',
    [ContractorCode]  CHAR(19)        NULL,
    [ResultCode]      CHAR(1)         NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ZenginTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ZenginTransactions_Batch] FOREIGN KEY ([ZenginBatchId])
        REFERENCES [zengin].[TD_ZenginBatches] ([Id]) ON DELETE CASCADE
);
END;

IF OBJECT_ID(N'[zengin].[TD_ZenginTransmissionLogs]', N'U') IS NULL
BEGIN
CREATE TABLE [zengin].[TD_ZenginTransmissionLogs] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ZenginBatchId]   BIGINT          NULL,
    [ManagementDate]  DATE            NOT NULL,
    [ManagementTime]  TIME            NOT NULL,
    [WithdrawalMonth] CHAR(2)         NOT NULL,
    [WithdrawalDay]   CHAR(2)         NOT NULL,
    [RecordCount]     INT             NOT NULL DEFAULT 0,
    [DeliveryDate]    DATE            NULL,
    [DeliveryTime]    TIME            NULL,
    [DeliveryFlag]    CHAR(1)         NOT NULL DEFAULT '0',
    [ResultReceiveFlag] CHAR(1)       NOT NULL DEFAULT '0',
    [ResultReceiveDate] DATE          NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ZenginTransmissionLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ZenginTransmissionLogs_Batch] FOREIGN KEY ([ZenginBatchId])
        REFERENCES [zengin].[TD_ZenginBatches] ([Id])
);
END;

IF OBJECT_ID(N'[zengin].[TD_ZenginVerifications]', N'U') IS NULL
BEGIN
CREATE TABLE [zengin].[TD_ZenginVerifications] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [TransferDate]      DATE            NOT NULL,
    [CycleCode]         CHAR(2)         NOT NULL,
    [VerificationCode]  CHAR(2)         NOT NULL,
    [CancelFlag]        CHAR(1)         NOT NULL DEFAULT '0',
    [RequestorCode]     CHAR(10)        NULL,
    [TotalCount]        INT             NOT NULL DEFAULT 0,
    [TotalAmount]       DECIMAL(12,0)   NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ZenginVerifications] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_AmountSettings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_AmountSettings] (
    [Id]          BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode] CHAR(6)       NOT NULL,
    [TypeNo]      SMALLINT      NOT NULL,
    [TypeCode]    CHAR(1)       NOT NULL,
    [Amount]      DECIMAL(10,0) NOT NULL,
    [CreatedAt]   DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]   DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_AmountSettings] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_BankHeadOffices]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_BankHeadOffices] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [BankCode]      CHAR(4)         NOT NULL,
    [HeadBranchCode] CHAR(3)        NOT NULL,
    [BankName]      NVARCHAR(15)    NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BankHeadOffices] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_BankHeadOffices_Bank] UNIQUE ([BankCode])
);
END;

IF OBJECT_ID(N'[cho].[TM_BatchControlParameters]', N'U') IS NULL
BEGIN
CREATE TABLE [cho].[TM_BatchControlParameters] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ParameterKey]    NVARCHAR(50)    NOT NULL,
    [ProcessingDate]  DATE            NOT NULL,
    [TransferDate]    DATE            NOT NULL,
    [CreatedAt]       DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]       DATETIME2       NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BatchControlParameters] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_BatchControlParameters_Key] UNIQUE ([ParameterKey])
);
END;

IF OBJECT_ID(N'[dbo].[TM_BatchParameters]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_BatchParameters] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ParameterType]   CHAR(1)         NOT NULL,
    [WithdrawalDate]  DATE            NOT NULL,
    [JobExecutionStatus] CHAR(1)      NOT NULL DEFAULT '0',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BatchParameters] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_BatchParameterCompanies]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_BatchParameterCompanies] (
    [Id]               BIGINT IDENTITY(1,1) NOT NULL,
    [BatchParameterId] BIGINT        NOT NULL,
    [SlotNo]           SMALLINT      NOT NULL,
    [CompanyCode]      CHAR(6)       NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_BatchParameterCompanies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BatchParameterCompanies_Parent] FOREIGN KEY ([BatchParameterId])
        REFERENCES [dbo].[TM_BatchParameters] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_BatchParameterCompanies_Slot] UNIQUE ([BatchParameterId], [SlotNo])
);
END;

IF OBJECT_ID(N'[dbo].[TM_CodeSettings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_CodeSettings] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [CodeCategory]  NVARCHAR(20)         NOT NULL,
    [CodeValue]     NVARCHAR(50)         NOT NULL,
    [DisplayText]   NVARCHAR(200)        NOT NULL,
    [ChangeValue]   NVARCHAR(50)         NULL,
    [DisplayOrder]  INT                  NOT NULL,
    [CreatedAt]     DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]     DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TM_CodeSettings] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_CompanyTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_CompanyTypes] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]      BIGINT        NOT NULL,
    [TypeNo]         SMALLINT      NOT NULL,
    [TypeCode]       CHAR(1)       NOT NULL,
    [Cycle]          CHAR(2)       NULL,
    [StartYearMonth] CHAR(6)       NULL,
    [TypeName]       NVARCHAR(20)  NULL,
    [Amount]         DECIMAL(10,0) NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CompanyTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompanyTypes_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_CompanyTypes_Slot] UNIQUE ([CompanyId], [TypeNo])
);
END;

IF OBJECT_ID(N'[dbo].[TM_CompanyWithdrawalDays]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_CompanyWithdrawalDays] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]     BIGINT        NOT NULL,
    [SlotNo]        SMALLINT      NOT NULL,
    [WithdrawalDay] CHAR(2)       NOT NULL,
    [PreviousProcessingType] CHAR(1)       NOT NULL DEFAULT '0',
    [CurrentProcessingType]  CHAR(1)       NOT NULL DEFAULT '0',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CompanyWithdrawalDays] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompanyWithdrawalDays_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_CompanyWithdrawalDays_Slot] UNIQUE ([CompanyId], [SlotNo])
);
END;

IF OBJECT_ID(N'[dbo].[TM_ContractBillingAmounts]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_ContractBillingAmounts] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ContractId]      BIGINT        NOT NULL,
    [TypeNo]          SMALLINT      NOT NULL,
    [BillingAmount]   DECIMAL(10,0) NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractBillingAmounts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractBillingAmounts_Contract] FOREIGN KEY ([ContractId])
        REFERENCES [dbo].[TM_Contracts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ContractBillingAmounts_Slot] UNIQUE ([ContractId], [TypeNo])
);
END;

IF OBJECT_ID(N'[dbo].[TM_ContractTypes]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_ContractTypes] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ContractId]      BIGINT        NOT NULL,
    [TypeNo]          SMALLINT      NOT NULL,
    [StartYearMonth]  CHAR(6)       NULL,
    [Cycle]           CHAR(2)       NULL,
    [Amount]          DECIMAL(10,0) NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContractTypes_Contract] FOREIGN KEY ([ContractId])
        REFERENCES [dbo].[TM_Contracts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ContractTypes_Slot] UNIQUE ([ContractId], [TypeNo])
);
END;

IF OBJECT_ID(N'[dbo].[TM_ErrorSuppressParameters]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_ErrorSuppressParameters] (
    [Id]            BIGINT IDENTITY(1,1)  NOT NULL,
    [CompanyCode]   CHAR(6)               NOT NULL,
    [SuppressFlag]  BIT                   NOT NULL DEFAULT 1,
    [Description]   NVARCHAR(200)         NULL,
    [CreatedAt]     DATETIME2             NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]     DATETIME2             NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ErrorSuppressParameters] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_ErrorSuppressParameters_Company] UNIQUE ([CompanyCode])
);
END;

IF OBJECT_ID(N'[dbo].[TM_JapaneseEras]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_JapaneseEras] (
    [Id]           BIGINT IDENTITY(1,1) NOT NULL,
    [Code]         INT          NOT NULL,   -- 1=明治, 2=大正, 3=昭和, 4=平成, 5=令和
    [Name]         NVARCHAR(4)  NOT NULL,
    [Abbreviation] NVARCHAR(1)  NOT NULL,
    [StartDate]    DATE         NOT NULL,
    [EndDate]      DATE         NULL,
    [BaseYear]     INT          NOT NULL,
    [CreatedAt]    DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]    DATETIME2    NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_JapaneseEras] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_JapaneseEras_Code] UNIQUE ([Code])
);
END;

IF OBJECT_ID(N'[dbo].[TM_LedgerParameters]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_LedgerParameters] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [OutputMode]     CHAR(1)   NULL,     -- 0=全社, 1=パラメータ指定
    [CompanyCode]    CHAR(6)   NOT NULL,
    [PersonCodeFrom] CHAR(12)  NOT NULL,
    [PersonCodeTo]   CHAR(12)  NOT NULL,
    [CreatedAt]      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_LedgerParameters] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TM_ProcessingCalendars]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_ProcessingCalendars] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [ProcessingDate]  DATE          NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ProcessingCalendars] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_ProcessingCalendars_Date] UNIQUE ([ProcessingDate])
);
END;

IF OBJECT_ID(N'[dbo].[TM_ProcessingSlots]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_ProcessingSlots] (
    [Id]                     BIGINT IDENTITY(1,1) NOT NULL,
    [ProcessingCalendarId]   BIGINT        NOT NULL,
    [SlotNo]                 SMALLINT      NOT NULL,
    [ProcessingType]         CHAR(1)       NOT NULL DEFAULT '0',
    [WithdrawalDate1]        DATE          NULL,
    [WithdrawalDate2]        DATE          NULL,
    [WithdrawalDate3]        DATE          NULL,
    [CompletedFlag]          BIT           NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ProcessingSlots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProcessingSlots_Calendar] FOREIGN KEY ([ProcessingCalendarId])
        REFERENCES [dbo].[TM_ProcessingCalendars] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_ProcessingSlots_Slot] UNIQUE ([ProcessingCalendarId], [SlotNo])
);
END;

IF OBJECT_ID(N'[dbo].[TM_TransmissionParameters]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_TransmissionParameters] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [ParameterKey]      NVARCHAR(50)    NOT NULL,
    [ParameterValue]    NVARCHAR(200)   NULL,
    [Description]       NVARCHAR(100)   NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TransmissionParameters] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_TransmissionParameters_Key] UNIQUE ([ParameterKey])
);
END;

IF OBJECT_ID(N'[dbo].[TM_Users]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TM_Users] (
    [Id]           BIGINT IDENTITY(1,1) NOT NULL,
    [UserId]       NVARCHAR(20)  NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [UserName]     NVARCHAR(50)  NOT NULL,
    [Role]         NVARCHAR(30)  NOT NULL,
    [IsActive]     BIT           NOT NULL DEFAULT 1,
    [CreatedAt]    DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]    DATETIME2     NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AccountFilePrintRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AccountFilePrintRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AccountFilePrintRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AccountLinks]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AccountLinks] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]       CHAR(6)         NOT NULL,
    [PersonalCode]      CHAR(12)        NOT NULL,
    [LinkedBankCode]    CHAR(4)         NOT NULL,
    [LinkedBranchCode]  CHAR(3)         NOT NULL,
    [LinkedAccountNo]   CHAR(10)        NOT NULL,
    [LinkType]          CHAR(1)         NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_AccountLinks] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AccountNumberChangeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AccountNumberChangeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AccountNumberChangeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AccountProcedureCompletionNotices]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AccountProcedureCompletionNotices] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [ReportLdCode]   CHAR(1)              NOT NULL,
    [ZipCode]        CHAR(8)              NOT NULL,
    [Prefecture]     NVARCHAR(12)         NOT NULL,
    [City]           NVARCHAR(12)         NOT NULL,
    [Town1]          NVARCHAR(12)         NOT NULL,
    [Town2]          NVARCHAR(12)         NOT NULL,
    [RecipientName]  NVARCHAR(16)         NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [CompanyName]    NVARCHAR(28)         NOT NULL,
    [ContactPersonName]   NVARCHAR(15)    NULL,
    [CompanyZipCode] CHAR(8)              NOT NULL,
    [CompanyAddress] NVARCHAR(48)         NOT NULL,
    [CompanyTel]     CHAR(17)             NULL,
    [CompanyFax]     CHAR(17)             NULL,
    [PersonalCode]   CHAR(12)             NOT NULL,
    [DepositorName]  NVARCHAR(25)             NOT NULL,
    [ContractorName] NVARCHAR(25)             NOT NULL,
    [StartYearMonth] DATE                 NOT NULL,
    [TransferDay]    CHAR(2)              NOT NULL,
    [BankbookNo]     CHAR(9)              NOT NULL,
    [BankCode]       CHAR(4)              NOT NULL,
    [BankName]       NVARCHAR(20)         NOT NULL,
    [AccountType]    CHAR(1)              NOT NULL,
    [AccountNo]      CHAR(10)             NOT NULL,
    [MessageHeader]  NVARCHAR(51)         NULL,
    [MessageBody]    NVARCHAR(240)        NULL,
    [MaskingLevel]   BIT                  NOT NULL DEFAULT 0,    
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AccountProcedureCompletionNotices] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AccountProcedureSampleRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AccountProcedureSampleRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AccountProcedureSampleRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AllocationCoefficientRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AllocationCoefficientRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AllocationCoefficientRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AmountDataChangeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AmountDataChangeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AmountDataChangeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AmountDataCountConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AmountDataCountConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AmountDataCountConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AmountDataDuplicateWarningRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AmountDataDuplicateWarningRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_AmountDataDuplicateWarningRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_AmountErrorRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_AmountErrorRecords] (
    [Id]            BIGINT IDENTITY(1,1)  NOT NULL,
    [JobExecutionId]    NVARCHAR(50)          NOT NULL,
    [CompanyCode]   CHAR(6)               NOT NULL,
    [BatchNo]       CHAR(3)               NOT NULL,
    [PersonalCode]  CHAR(12)              NOT NULL,
    [CheckDigit]    CHAR(1)               NOT NULL,
    [ErrorType]     VARCHAR(20)           NOT NULL,  -- 'Item', 'Duplicate', 'Total'
    [IsErrorCompanyCode] BIT               NOT NULL DEFAULT 0,
    [IsErrorPersonalCode] BIT              NOT NULL DEFAULT 0,
    [IsErrorCheckDigit]   BIT              NOT NULL DEFAULT 0,
    [IsErrorAmount1]      BIT              NOT NULL DEFAULT 0,
    [IsErrorAmount2]      BIT              NOT NULL DEFAULT 0,
    [IsErrorAmount3]      BIT              NOT NULL DEFAULT 0,
    [IsErrorAmount4]      BIT              NOT NULL DEFAULT 0,
    [IsErrorAmount5]      BIT              NOT NULL DEFAULT 0,
    [Amount1]       DECIMAL(10,0)         NOT NULL DEFAULT 0,
    [Amount2]       DECIMAL(10,0)         NOT NULL DEFAULT 0,
    [Amount3]       DECIMAL(10,0)         NOT NULL DEFAULT 0,
    [Amount4]       DECIMAL(10,0)         NOT NULL DEFAULT 0,
    [Amount5]       DECIMAL(10,0)         NOT NULL DEFAULT 0,
    [SequenceNo]    INT                   NOT NULL,  -- グループ内連番
    [CreatedAt]     DATETIME2             NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_AmountErrorRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_AmountErrorRecords_Key]
        UNIQUE ([JobExecutionId], [CompanyCode], [BatchNo], [SequenceNo])
);
END;

IF OBJECT_ID(N'[dbo].[TR_BankArrivalConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_BankArrivalConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_BankArrivalConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_BankSummaryRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_BankSummaryRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_BankSummaryRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_BankWithdrawalDataManagementRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_BankWithdrawalDataManagementRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_BankWithdrawalDataManagementRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_BillingDataPresenceConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_BillingDataPresenceConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_BillingDataPresenceConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CmtFormInputConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CmtFormInputConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CmtFormInputConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CmtTransferResultConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CmtTransferResultConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CmtTransferResultConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyBankSummaryRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyBankSummaryRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CompanyBankSummaryRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyBankSummaryRoutineRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyBankSummaryRoutineRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CompanyBankSummaryRoutineRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyBillingSummaryRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyBillingSummaryRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CompanyBillingSummaryRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyMasterChangeLogs]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyMasterChangeLogs] (
    -- 共通
    [Id]                BIGINT IDENTITY(1,1)  NOT NULL,
    [JobExecutionId]        NVARCHAR(50)         NOT NULL,
    [ProcessingDate]    DATE                  NOT NULL,
    [CompanyCode]       CHAR(6)               NOT NULL,
    [RequestType]       CHAR(2)               NOT NULL,  -- '11'〜'19'
    [ChangeAction]      CHAR(1)               NOT NULL,  -- IDOK/IDOKU: '1'=削除 / '2'=新規 / '3'=修正
    [MessageType]       TINYINT               NOT NULL,  -- メッセージ区分コード（1〜10）
    [IsError]           BIT                   NOT NULL DEFAULT 0,
    [SequenceNo]        INT                   NOT NULL,
    -- DENK 11: 会社名・連絡先
    [CompanyNameKana]   VARCHAR(40)           NULL,
    [PhoneNumber]       NVARCHAR(13)          NULL,
    [PostalCode]        CHAR(7)               NULL,
    [Prefecture]        NVARCHAR(9)           NULL,
    -- DENK 12: 住所
    [City]              NVARCHAR(20)          NULL,
    [Town1]             NVARCHAR(24)          NULL,
    [Town2]             NVARCHAR(24)          NULL,
    -- DENK 13: 引落日・フラグ
    [WithdrawalDay1]    CHAR(2)               NULL,
    [WithdrawalDay2]    CHAR(2)               NULL,
    [WithdrawalDay3]    CHAR(2)               NULL,
    [WithdrawalDay4]    CHAR(2)               NULL,
    [ProcessingFlag]    CHAR(12)              NULL,
    [SheetFlag]         CHAR(12)              NULL,
    -- DENK 14: 料金・単価
    [ChangeCode]        CHAR(2)               NULL,
    [BasicFee]          DECIMAL(10,0)         NULL,
    [AdminFee]          DECIMAL(10,0)         NULL,
    [NewUnitPrice]      DECIMAL(5,0)          NULL,
    [ModifyUnitPrice]   DECIMAL(5,0)          NULL,
    [Transfer1UnitPrice] DECIMAL(5,0)         NULL,
    [Transfer2UnitPrice] DECIMAL(5,0)         NULL,
    [ReceiptUnitPrice]  DECIMAL(5,0)          NULL,
    -- DENK 15: 振替口座・委託者
    [Transfer1BankCode] CHAR(4)               NULL,
    [Transfer1BranchCode] CHAR(3)             NULL,
    [Transfer1AccountType] CHAR(1)            NULL,
    [Transfer1AccountNo] CHAR(10)              NULL,
    [ConsignorCode]     CHAR(10)              NULL,
    [Transfer2BankCode] CHAR(4)               NULL,
    [Transfer2BranchCode] CHAR(3)             NULL,
    [Transfer2AccountType] CHAR(1)            NULL,
    [Transfer2AccountNo] CHAR(10)              NULL,
    [PassbookNo]        CHAR(8)               NULL,
    -- DENK 16〜19: 収納・種目・金額（X2 = DENK - 15 の配列アクセスを単一行で表現）
    [CollectionCode]    CHAR(1)               NULL,
    [Cycle]             CHAR(2)               NULL,
    [OperatingYear]     CHAR(4)               NULL,
    [OperatingMonth]    CHAR(2)               NULL,
    [ItemName]          NVARCHAR(12)          NULL,
    [Amount]            DECIMAL(10,0)         NULL,
    -- 監査
    [CreatedAt]         DATETIME2             NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]         DATETIME2             NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CompanyMasterChangeLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_CompanyMasterChangeLogs_Key]
        UNIQUE ([JobExecutionId], [CompanyCode], [RequestType], [SequenceNo])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyMonthlySummaries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyMonthlySummaries] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyId]     BIGINT          NOT NULL,
    [CompanyCode]   CHAR(6)         NOT NULL,
    [FiscalYear]    SMALLINT        NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CompanyMonthlySummaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompanyMonthlySummaries_Company] FOREIGN KEY ([CompanyId])
        REFERENCES [dbo].[TM_Companies] ([Id]),
    CONSTRAINT [UQ_CompanyMonthlySummaries_Key] UNIQUE ([CompanyCode], [FiscalYear])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyMonthlySummaryDetails]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyMonthlySummaryDetails] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyMonthlySummaryId] BIGINT        NOT NULL,
    [MonthNo]               SMALLINT      NOT NULL,
    [TransferAmount]        DECIMAL(12,0) NOT NULL DEFAULT 0,
    [CollectionAmount]      DECIMAL(12,0) NOT NULL DEFAULT 0,
    [TransferCount]         INT           NOT NULL DEFAULT 0,
    [CollectionCount]       INT           NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CompanyMonthlySummaryDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompanyMonthlySummaryDetails_Parent] FOREIGN KEY ([CompanyMonthlySummaryId])
        REFERENCES [dbo].[TR_CompanyMonthlySummaries] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_CompanyMonthlySummaryDetails_Month] UNIQUE ([CompanyMonthlySummaryId], [MonthNo])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CompanyTransferScheduleRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CompanyTransferScheduleRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CompanyTransferScheduleRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractChangeErrorRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractChangeErrorRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [MessageNo]      CHAR(2)              NULL,
    [ContractorCode] CHAR(12)             NOT NULL,
    [ChangeAction]   CHAR(1)              NULL,
    [RequestType]    CHAR(2)              NULL,
    [ErrorFlags]     BIT                  NOT NULL DEFAULT 0,
    [DepositorName]  NVARCHAR(32)         NULL,
    [ContractorName] NVARCHAR(32)         NULL,
    [BankCode]       CHAR(4)              NULL,
    [BranchCode]     CHAR(3)              NULL,
    [AccountType]    CHAR(1)              NULL,
    [AccountNo]      CHAR(10)             NULL,
    [DebitDay]       CHAR(2)              NULL,
    [SuspendFlag]    CHAR(1)              NULL,
    [PostalCode]     CHAR(7)              NULL,
    [Prefecture]     NVARCHAR(9)          NULL,
    [City]           NVARCHAR(20)         NULL,
    [Town1]          NVARCHAR(24)         NULL,
    [Town2]          NVARCHAR(24)         NULL,
    [PhoneNo]        CHAR(11)             NULL,
    [BillingType]    CHAR(1)              NULL,
    [BillingAmount]  DECIMAL(9,0)         NULL,
    [OperatingYearMonth]    CHAR(4)       NULL,
    [ItemName]       NVARCHAR(10)         NULL,
    [TotalAmount]    DECIMAL(9,0)         NULL,
    [TotalCount]     CHAR(2)              NULL,
    [FirstAmount]    DECIMAL(9,0)         NULL,
    [SecondAmount]   DECIMAL(9,0)         NULL,
    [ExtraAmount]    DECIMAL(9,0)         NULL,
    [Month1]         CHAR(2)              NULL,
    [Month2]         CHAR(2)              NULL,
    [BillingCycle]   CHAR(2)              NULL,
    [StartYearMonth] CHAR(5)              NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ContractChangeErrorRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractListDiskRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractListDiskRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ContractListDiskRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractMasterDeleteLogs]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractMasterDeleteLogs] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [PersonalCode]   CHAR(12)             NOT NULL,
    [DepositorName]  NVARCHAR(32)         NOT NULL,
    [ContractorName] NVARCHAR(32)         NOT NULL,
    [BankCode]       CHAR(4)              NOT NULL,
    [BranchCode]     CHAR(3)              NOT NULL,
    [AccountType]    CHAR(1)              NOT NULL,
    [AccountNumber]  CHAR(10)             NOT NULL,
    [DeletionDate]   DATE                 NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractMasterDeleteLogs] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractMasterShortListRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractMasterShortListRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ContractMasterShortListRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractorDuplicateChangeCheckRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractorDuplicateChangeCheckRecords] (
    [Id]                  BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId]      NVARCHAR(50)         NOT NULL,
    [ProcessingDate]      DATE                 NOT NULL,
    [CompanyCode]         CHAR(6)              NOT NULL,
    [ErrorMessage]        NVARCHAR(27)         NOT NULL,
    [ContractorNumber]    CHAR(12)             NOT NULL,
    [ChangeAction]        CHAR(1)              NOT NULL,
    [TransactionType]     CHAR(2)              NOT NULL,
    [TypeCode]            CHAR(1)              NOT NULL,
    [SequenceNo]          INT                  NOT NULL,
    [CreatedAt]           DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]           DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ContractorDuplicateChangeCheckRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractorMasterDeleteRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractorMasterDeleteRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [ContractorNumber]      CHAR(12)      NOT NULL,
    [TypeCode]       CHAR(1)              NOT NULL,
    [DepositorKanjiName]    NVARCHAR(40)  NULL,
    [ContractorKanjiName]   NVARCHAR(40)  NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ContractorMasterDeleteRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ContractPrintRequests]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ContractPrintRequests] (
    [Id]             BIGINT IDENTITY(1,1)  NOT NULL,
    [JobExecutionId]     NVARCHAR(50)          NOT NULL,
    [RequestOrder]   SMALLINT              NOT NULL,
    [CompanyCode]    CHAR(6)               NOT NULL,
    [PersonCodeFrom] CHAR(12)              NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_PersonCodeFrom] DEFAULT '',
    [PersonCodeTo]   CHAR(12)              NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_PersonCodeTo]   DEFAULT '',
    [AllCompanyFlag] BIT                   NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_AllCompanyFlag] DEFAULT 0,
    [Status]         CHAR(1)               NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_Status]         DEFAULT '0',
    [PrintedCount]   INT                   NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_PrintedCount]   DEFAULT 0,
    [AuditCount]     INT                   NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_AuditCount]     DEFAULT 0,
    [CreatedAt]      DATETIME2             NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_CreatedAt]      DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2             NOT NULL
        CONSTRAINT [DF_ContractPrintRequests_UpdatedAt]      DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ContractPrintRequests]
        PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_ContractPrintRequests_Run_Order]
        UNIQUE ([JobExecutionId], [RequestOrder]),
    CONSTRAINT [CK_ContractPrintRequests_Status]
        CHECK ([Status] IN ('0', '1', '2', '9')),
    CONSTRAINT [CK_ContractPrintRequests_Order]
        CHECK ([RequestOrder] BETWEEN 1 AND 20)
);
END;

IF OBJECT_ID(N'[dbo].[TR_CooperativeTransferTapeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CooperativeTransferTapeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CooperativeTransferTapeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CoverLetterPrintRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CoverLetterPrintRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_CoverLetterPrintRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_CoverLetters]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_CoverLetters] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]   CHAR(6)         NOT NULL,
    [CompanyName]   NVARCHAR(36)    NULL,
    [ProcessDate]   DATE            NOT NULL,
    [ContentText]   NVARCHAR(MAX)   NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_CoverLetters] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_DiskFormInputConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_DiskFormInputConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_DiskFormInputConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_DiskTransferResultConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_DiskTransferResultConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_DiskTransferResultConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_EnrollmentFormRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_EnrollmentFormRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [CompanyName]    NVARCHAR(27)             NOT NULL,
    [TypeName1]      NVARCHAR(9)              NOT NULL,
    [TypeName2]      NVARCHAR(9)              NOT NULL,
    [TypeName3]      NVARCHAR(9)              NOT NULL,
    [TypeName4]      NVARCHAR(9)              NOT NULL,
    [ContractorName] NVARCHAR(25)             NOT NULL,
    [DepositorName]  NVARCHAR(25)             NOT NULL,
    [BankCode]       CHAR(4)              NOT NULL,
    [BranchCode]     CHAR(3)              NOT NULL,
    [AccountType]    CHAR(1)              NOT NULL,
    [AccountNo]      CHAR(10)             NOT NULL,
    [PersonalCode]   CHAR(12)             NOT NULL,
    [CheckDigit]     CHAR(1)              NOT NULL,
    [Amount1]        DECIMAL(9,0)         NOT NULL,
    [Amount2]        DECIMAL(9,0)         NOT NULL,
    [Amount3]        DECIMAL(9,0)         NOT NULL,
    [Amount4]        DECIMAL(9,0)         NOT NULL,
    [Amount5]        DECIMAL(9,0)         NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_EnrollmentFormRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_FeeAggregations]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_FeeAggregations] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]     CHAR(6)         NOT NULL,
    [ProcessingDate]  DATE            NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_FeeAggregations] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_FeeAggregationDetails]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_FeeAggregationDetails] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [FeeAggregationId]  BIGINT        NOT NULL,
    [LineNo]            SMALLINT      NOT NULL,
    [BankCode]          CHAR(4)       NULL,
    [TransferCount]     INT           NOT NULL DEFAULT 0,
    [TransferAmount]    DECIMAL(12,0) NOT NULL DEFAULT 0,
    [FeeAmount]         DECIMAL(10,0) NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_FeeAggregationDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FeeAggregationDetails_Parent] FOREIGN KEY ([FeeAggregationId])
        REFERENCES [dbo].[TR_FeeAggregations] ([Id]) ON DELETE CASCADE
);
END;

IF OBJECT_ID(N'[dbo].[TR_FinancialInstitutionExchangeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_FinancialInstitutionExchangeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_FinancialInstitutionExchangeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_FinancialInstitutionRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_FinancialInstitutionRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [BankCode]       CHAR(4)              NOT NULL,
    [BankName]       NVARCHAR(16)         NOT NULL,
    [BranchCode]     CHAR(3)              NOT NULL,
    [BranchName]     NVARCHAR(19)         NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_FinancialInstitutionRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_GeneralAffairsDepositListRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_GeneralAffairsDepositListRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [HachijuniAmount]    DECIMAL(10,0)    NOT NULL,
    [KenshinrenAmount]   DECIMAL(10,0)    NOT NULL,
    [OtherBankAmount]    DECIMAL(10,0)    NOT NULL,
    [TotalAmount]        DECIMAL(10,0)    NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_GeneralAffairsDepositListRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_InstitutionSummaries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_InstitutionSummaries] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [BankCode]        CHAR(4)         NOT NULL,
    [BranchCode]      CHAR(3)         NOT NULL,
    [BankName]        NVARCHAR(15)    NULL,
    [BranchName]      NVARCHAR(15)    NULL,
    [TotalCount]      INT             NOT NULL DEFAULT 0,
    [TotalAmount]     DECIMAL(12,0)   NOT NULL DEFAULT 0,
    [SettledCount]    INT             NOT NULL DEFAULT 0,
    [SettledAmount]   DECIMAL(12,0)   NOT NULL DEFAULT 0,
    [FailedCount]     INT             NOT NULL DEFAULT 0,
    [FailedAmount]    DECIMAL(12,0)   NOT NULL DEFAULT 0,
    [ProcessDate]     DATE            NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_InstitutionSummaries] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_InstitutionSummaryPrintRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_InstitutionSummaryPrintRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_InstitutionSummaryPrintRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_InstitutionSummarySampleBRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_InstitutionSummarySampleBRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_InstitutionSummarySampleBRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_InstitutionSummarySampleRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_InstitutionSummarySampleRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_InstitutionSummarySampleRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_KanjiCompanyMasterChangeLogs]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_KanjiCompanyMasterChangeLogs] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId]        NVARCHAR(50)         NOT NULL,
    [ProcessingDate]        DATE                 NOT NULL,
    [CompanyCode]           CHAR(6)              NOT NULL,
    [SectionNo]             CHAR(1)              NOT NULL,
    [MessageType]           TINYINT              NULL,
    [ChangeAction]          CHAR(1)              NOT NULL,
    [CompanyNameKanji]      NVARCHAR(30)         NULL,
    [PrefectureKanji]       NVARCHAR(4)         NULL,
    [CityKanji]             NVARCHAR(10)         NULL,
    [TownKanji1]            NVARCHAR(20)         NULL,
    [TownKanji2]            NVARCHAR(20)         NULL,
    [DepartmentKanji]       NVARCHAR(20)         NULL,
    [PersonInChargeKanji]   NVARCHAR(20)         NULL,
    [SequenceNo]            INT                  NOT NULL,
    [CreatedAt]             DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_KanjiCompanyMasterChangeLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_KanjiCompanyMasterChangeLogs_Key]
        UNIQUE ([JobExecutionId], [CompanyCode], [SectionNo], [SequenceNo]),
    CONSTRAINT [CK_KanjiCompanyMasterChangeLogs_SectionNo]
        CHECK ([SectionNo] BETWEEN '1' AND '4')
);
END;

IF OBJECT_ID(N'[dbo].[TR_ManagementAccountings]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ManagementAccountings] (
    [Id]              BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]     CHAR(6)         NOT NULL,
    [AccountingDate]  DATE            NOT NULL,
    [AccountCode]     CHAR(4)         NOT NULL,
    [Amount]          DECIMAL(12,0)   NOT NULL DEFAULT 0,
    [Count]           INT             NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_ManagementAccountings] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_MessageMasterChangeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_MessageMasterChangeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [Idok]           CHAR(1)              NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [AvisCode]       CHAR(2)              NULL,
    [ApplyType]      CHAR(1)              NULL,
    [StartDate]      DATE                 NULL,
    [EndDate]        DATE                 NULL,
    [SectionNo]      CHAR(2)              NULL,
    [HeadingKanji]   NVARCHAR(48)         NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_MessageMasterChangeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_OtherBankTransferDiskRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_OtherBankTransferDiskRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_OtherBankTransferDiskRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_OtherBankTransferTapeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_OtherBankTransferTapeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_OtherBankTransferTapeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_OutputCountRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_OutputCountRecords] (
    [Id]               BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId]   NVARCHAR(50)         NOT NULL,
    [ProcessingDate]   DATE                 NOT NULL,
    [JobKubun1]        CHAR(1)              NULL,
    [JobKubun2]        CHAR(1)              NULL,
    [JobKubun3]        CHAR(1)              NULL,
    [WithdrawalMonth1] CHAR(2)              NULL,
    [WithdrawalDay1]   CHAR(2)              NULL,
    [WithdrawalMonth2] CHAR(2)              NULL,
    [WithdrawalDay2]   CHAR(2)              NULL,
    [WithdrawalMonth3] CHAR(2)              NULL,
    [WithdrawalDay3]   CHAR(2)              NULL,
    [CompanyCode]      CHAR(6)              NOT NULL,
    [CompanyName]      NVARCHAR(27)             NOT NULL,
    [FormTitle]        CHAR(4)              NULL,
    [PrintCounts1]     CHAR(6)              NOT NULL,
    [PrintCounts2]     CHAR(6)              NOT NULL,
    [PrintCounts3]     CHAR(6)              NOT NULL,
    [PrintCounts4]     CHAR(6)              NOT NULL,
    [PrintCounts5]     CHAR(6)              NOT NULL,
    [PrintCounts6]     CHAR(6)              NOT NULL,
    [PrintCounts7]     CHAR(6)              NOT NULL,
    [PrintCounts8]     CHAR(6)              NOT NULL,
    [PrintCounts9]     CHAR(6)              NOT NULL,
    [PrintCounts10]    CHAR(6)              NOT NULL,
    [PrintCounts11]    CHAR(6)              NOT NULL,
    [PrintCounts12]    CHAR(6)              NOT NULL,
    [PrintCounts13]    CHAR(6)              NOT NULL,
    [PrintCounts14]    CHAR(6)              NOT NULL,
    [SequenceNo]       INT                  NOT NULL,
    [CreatedAt]        DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]        DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_OutputCountRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_PostOfficeBusinessOwnerNumberSummaries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_PostOfficeBusinessOwnerNumberSummaries] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_PostOfficeBusinessOwnerNumberSummaries] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_PostOfficeTransferTapeRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_PostOfficeTransferTapeRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_PostOfficeTransferTapeRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ProcessParameterCheckRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ProcessParameterCheckRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [Message]        NVARCHAR(10)         NOT NULL,
    [CompanyCode]    CHAR(6)              NOT NULL,
    [CompanyName]    NVARCHAR(40)         NOT NULL,
    [ProcessType]    CHAR(1)              NOT NULL,
    [PreviousType]   CHAR(1)              NOT NULL,
    [TransferYearWareki]    CHAR(2)       NOT NULL,
    [TransferMonth]  CHAR(2)              NOT NULL,
    [TransferDay]    CHAR(2)              NOT NULL,
    [Category]       CHAR(1)              NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ProcessParameterCheckRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_RemittanceRequestRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_RemittanceRequestRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_RemittanceRequestRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ReportExecutions]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ReportExecutions] (
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [ExecutionId] NVARCHAR(50) NOT NULL,
    [JobExecutionId] NVARCHAR(100) NULL,
    [TemplateId] NVARCHAR(50) NOT NULL,
    [Status] NVARCHAR(20) NOT NULL,
    [OutputMode] NVARCHAR(20) NOT NULL,
    [IsPartialOutput] BIT NOT NULL DEFAULT 0,
    [OutputFilePath] NVARCHAR(500) NULL,
    [IncompleteFilePath] NVARCHAR(500) NULL,
    [RetryCount] INT NOT NULL DEFAULT 0,
    [MaxRetries] INT NOT NULL DEFAULT 3,
    [StartTime] DATETIME2 NULL,
    [EndTime] DATETIME2 NULL,
    [ElapsedMilliseconds] BIGINT NULL,
    [ErrorMessage] NVARCHAR(2000) NULL,
    [ErrorStackTrace] NVARCHAR(4000) NULL,
    [UserId] NVARCHAR(50) NULL,
    [Notes] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ReportExecutions] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ResultDetailKanjiSealRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ResultDetailKanjiSealRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ResultDetailKanjiSealRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ResultDetailSequenceRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ResultDetailSequenceRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ResultDetailSequenceRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_SealReceiptKanaRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_SealReceiptKanaRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_SealReceiptKanaRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_SealReceiptSampleRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_SealReceiptSampleRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_SealReceiptSampleRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferCountRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferCountRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferCountRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferDataCountConfirmationRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferDataCountConfirmationRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferDataCountConfirmationRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferFailureLists]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferFailureLists] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferFailureLists] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferNotifications]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferNotifications] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferNotifications] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferResultSets]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferResultSets] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferResultSets] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferResultSummaries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferResultSummaries] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferResultSummaries] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransferResultTransmissionNotices]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransferResultTransmissionNotices] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransferResultTransmissionNotices] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_TransmissionInstitutionResultSummaries]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_TransmissionInstitutionResultSummaries] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_TransmissionInstitutionResultSummaries] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_UcvBillingDetails]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_UcvBillingDetails] (
    [Id]                BIGINT IDENTITY(1,1) NOT NULL,
    [CompanyCode]       CHAR(6)         NOT NULL,
    [BillingYearMonth]  DATE            NOT NULL,
    [ItemCode]          CHAR(4)         NOT NULL,
    [UnitPrice]         DECIMAL(10,2)   NULL,
    [Quantity]          INT             NOT NULL DEFAULT 0,
    [TotalAmount]       DECIMAL(10,0)   NOT NULL DEFAULT 0,
    [TaxRate]           DECIMAL(5,2)    NULL,
    [TaxAmount]         DECIMAL(10,0)   NULL,
    [InvoiceFlag]       BIT             NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_UcvBillingDetails] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_UedaCableReceiptRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_UedaCableReceiptRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_UedaCableReceiptRecords] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_UedaCableResultDetails]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_UedaCableResultDetails] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_UedaCableResultDetails] PRIMARY KEY ([Id])
);
END;

IF OBJECT_ID(N'[dbo].[TR_ZenginImportErrorRecords]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[TR_ZenginImportErrorRecords] (
    [Id]             BIGINT IDENTITY(1,1) NOT NULL,
    [JobExecutionId] NVARCHAR(50)         NOT NULL,
    [ProcessingDate] DATE                 NOT NULL,
    [SourceProgram]  NVARCHAR(10)         NOT NULL,
    [ErrorMessage]   NVARCHAR(30)         NOT NULL,
    [DetailText]     NVARCHAR(80)         NOT NULL,
    [SequenceNo]     INT                  NOT NULL,
    [CreatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    [UpdatedAt]      DATETIME2            NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT [PK_TR_ZenginImportErrorRecords] PRIMARY KEY ([Id])
);
END;

-- インデックス作成（全テーブル作成後）

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_AccumulatedZenginRecords_Account'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_AccumulatedZenginRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_AccumulatedZenginRecords_Account] ON [zengin].[TD_AccumulatedZenginRecords]
    ([BankCode], [BranchCode], [AccountNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UQ_TD_ApplyRuns_Attempt'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_ApplyRuns]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UQ_TD_ApplyRuns_Attempt]
    ON [dbo].[TD_ApplyRuns] ([RunJob], [TransferDate], [AttemptNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_TD_ApplyRuns_Confirmed'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_ApplyRuns]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_TD_ApplyRuns_Confirmed]
    ON [dbo].[TD_ApplyRuns] ([RunJob], [TransferDate])
    WHERE [Status] = N''CONFIRMED'';');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_AmountApplyPending_Run'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_AmountApplyPending]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_AmountApplyPending_Run]
    ON [dbo].[TD_AmountApplyPending] ([ApplyRunId], [CompanyCode], [BatchNo], [PersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_AuditRecords_ActorId_OccurredAt'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_AuditRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_AuditRecords_ActorId_OccurredAt]
    ON [dbo].[TD_AuditRecords] ([ActorId], [OccurredAt]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_AuditRecords_FeatureId_OccurredAt'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_AuditRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_AuditRecords_FeatureId_OccurredAt]
    ON [dbo].[TD_AuditRecords] ([FeatureId], [OccurredAt]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_AuditRecords_TargetType_TargetKey_OccurredAt'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_AuditRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_AuditRecords_TargetType_TargetKey_OccurredAt]
    ON [dbo].[TD_AuditRecords] ([TargetType], [OccurredAt]) INCLUDE ([TargetKey]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_BankBranchChanges_Bank'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_BankBranchChanges]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_BankBranchChanges_Bank] ON [dbo].[TD_BankBranchChanges] ([BankCode], [BranchCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_BankChangeRequests_Type'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_BankChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_BankChangeRequests_Type] ON [dbo].[TD_BankChangeRequests] ([RequestType]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_BankChangeRequests_Code'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_BankChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_BankChangeRequests_Code] ON [dbo].[TD_BankChangeRequests] ([BankCode], [BranchCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_BankChangeRequests_Job'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_BankChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_BankChangeRequests_Job] ON [dbo].[TD_BankChangeRequests] ([JobExecutionId], [BatchStatus]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_BatchExecutionHistories_Status'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_BatchExecutionHistories]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_BatchExecutionHistories_Status]
ON [dbo].[TD_BatchExecutionHistories] ([Status], [CompletedAt], [FunctionId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ChoZenginBatches_WithdrawalDate'
                 AND object_id = OBJECT_ID(N'[cho].[TD_ChoZenginBatches]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ChoZenginBatches_WithdrawalDate]
    ON [cho].[TD_ChoZenginBatches] ([WithdrawalDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ChoZenginBatches_ConsignorCode'
                 AND object_id = OBJECT_ID(N'[cho].[TD_ChoZenginBatches]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ChoZenginBatches_ConsignorCode]
    ON [cho].[TD_ChoZenginBatches] ([ConsignorCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ChoZenginTransactions_ZenginBatchId'
                 AND object_id = OBJECT_ID(N'[cho].[TD_ChoZenginTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ChoZenginTransactions_ZenginBatchId]
    ON [cho].[TD_ChoZenginTransactions] ([ZenginBatchId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_CollectionAmountRecords_Contract'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_CollectionAmountRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_CollectionAmountRecords_Contract]
    ON [dbo].[TD_CollectionAmountRecords] ([CompanyCode], [PersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CompanyChangeRequests_RequestType'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_CompanyChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CompanyChangeRequests_RequestType] ON [dbo].[TD_CompanyChangeRequests] ([RequestType]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CompanyChangeRequests_Cban'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_CompanyChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CompanyChangeRequests_Cban] ON [dbo].[TD_CompanyChangeRequests] ([Cban]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CompanyChangeRequests_CompanyCode'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_CompanyChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CompanyChangeRequests_CompanyCode] ON [dbo].[TD_CompanyChangeRequests] ([CompanyCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CompanyChangeRequests_Job'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_CompanyChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CompanyChangeRequests_Job] ON [dbo].[TD_CompanyChangeRequests] ([JobExecutionId], [BatchStatus]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_ContractApplyPending_Run'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_ContractApplyPending]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TD_ContractApplyPending_Run]
    ON [dbo].[TD_ContractApplyPending] ([ApplyRunId], [CompanyCode], [PersonalCode], [CheckDigit]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ContractChangeCards_Contract'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_ContractChangeCards]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ContractChangeCards_Contract] ON [dbo].[TD_ContractChangeCards] ([ContractCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TM_Contracts_CompanyCode_PersonalCode_CheckDigit'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_Contracts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TM_Contracts_CompanyCode_PersonalCode_CheckDigit]
    ON [dbo].[TM_Contracts] ([CompanyCode], [PersonalCode], [CheckDigit]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TM_Contracts_CompanyCode_PersonalCode_ContractSeq'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_Contracts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TM_Contracts_CompanyCode_PersonalCode_ContractSeq]
    ON [dbo].[TM_Contracts] ([CompanyCode], [PersonalCode], [ContractSeq]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Contracts_BankAccount'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_Contracts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_Contracts_BankAccount] ON [dbo].[TM_Contracts]
    ([BankCode], [BranchCode], [AccountType], [AccountNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Contracts_DepositorName'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_Contracts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_Contracts_DepositorName] ON [dbo].[TM_Contracts]
    ([DepositorNameKana]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Contracts_PersonalCode'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_Contracts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_Contracts_PersonalCode] ON [dbo].[TM_Contracts]
    ([PersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TD_ContractChangeRequests_CompanyCode_PersonalCode_TransferDate_SequenceNo'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_ContractChangeRequests]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TD_ContractChangeRequests_CompanyCode_PersonalCode_TransferDate_SequenceNo]
    ON [dbo].[TD_ContractChangeRequests] ([CompanyCode], [PersonalCode], [TransferDate], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ContractorCodeConversions_Old'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_ContractorCodeConversions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ContractorCodeConversions_Old] ON [dbo].[TD_ContractorCodeConversions]
    ([OldCompanyCode], [OldPersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CooperativeTransferReceipts_Status_CreatedAt_JobExecutionId'
                 AND object_id = OBJECT_ID(N'[cho].[TD_CooperativeTransferReceipts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CooperativeTransferReceipts_Status_CreatedAt_JobExecutionId]
    ON [cho].[TD_CooperativeTransferReceipts] ([Status], [CreatedAt], [JobExecutionId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CooperativeTransfers_Member'
                 AND object_id = OBJECT_ID(N'[cho].[TD_CooperativeTransfers]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CooperativeTransfers_Member] ON [cho].[TD_CooperativeTransfers] ([MemberCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CooperativeTransfers_Type'
                 AND object_id = OBJECT_ID(N'[cho].[TD_CooperativeTransfers]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CooperativeTransfers_Type] ON [cho].[TD_CooperativeTransfers] ([RecordType]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CooperativeTransfers_Match'
                 AND object_id = OBJECT_ID(N'[cho].[TD_CooperativeTransfers]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CooperativeTransfers_Match] ON [cho].[TD_CooperativeTransfers]
    ([KozConsignorCode], [MatchYearMonth], [MatchSeqNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CooperativeTransfers_JobExecutionId_Status_Sequence'
                 AND object_id = OBJECT_ID(N'[cho].[TD_CooperativeTransfers]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CooperativeTransfers_JobExecutionId_Status_Sequence]
    ON [cho].[TD_CooperativeTransfers] ([JobExecutionId], [Status], [ReceiptSequence]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_DailyAccountingEntries_Date'
                 AND object_id = OBJECT_ID(N'[cho].[TD_DailyAccountingEntries]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_DailyAccountingEntries_Date] ON [cho].[TD_DailyAccountingEntries]
    ([SettlementDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_DailyAccountingEntries_Coop'
                 AND object_id = OBJECT_ID(N'[cho].[TD_DailyAccountingEntries]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_DailyAccountingEntries_Coop] ON [cho].[TD_DailyAccountingEntries]
    ([CooperativeNo], [BranchOfficeNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_MessageChangeRequests_RequestType_Cban'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_MessageChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_MessageChangeRequests_RequestType_Cban] ON [dbo].[TD_MessageChangeRequests] ([RequestType], [Cban]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_MessageChangeRequests_CompanyCode'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_MessageChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_MessageChangeRequests_CompanyCode] ON [dbo].[TD_MessageChangeRequests] ([CompanyCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_MessageChangeRequests_Job'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_MessageChangeRequests]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_MessageChangeRequests_Job] ON [dbo].[TD_MessageChangeRequests] ([JobExecutionId], [BatchStatus]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_OutputLogs_Date'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_OutputLogs]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_OutputLogs_Date] ON [dbo].[TD_OutputLogs] ([LogDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TransferAmounts_Contract'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_TransferAmounts]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TransferAmounts_Contract] ON [dbo].[TD_TransferAmounts]
    ([CompanyCode], [PersonalCode], [CheckDigit]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TransferAmounts_CompanyCode_BatchNo_SequenceNo'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_TransferAmounts]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TransferAmounts_CompanyCode_BatchNo_SequenceNo] ON [dbo].[TD_TransferAmounts]
    ([CompanyCode], [BatchNo], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TransferTransactions_Date'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_TransferTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TransferTransactions_Date] ON [dbo].[TD_TransferTransactions] ([WithdrawalDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TransferTransactions_Company'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_TransferTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TransferTransactions_Company] ON [dbo].[TD_TransferTransactions] ([CompanyId], [WithdrawalDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TransferTransactions_Contract'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_TransferTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TransferTransactions_Contract] ON [dbo].[TD_TransferTransactions] ([CompanyCode], [PersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TransferTransactions_DateRound'
                 AND object_id = OBJECT_ID(N'[dbo].[TD_TransferTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TransferTransactions_DateRound] ON [dbo].[TD_TransferTransactions] ([WithdrawalDate], [TransferRound]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginBatches_Date'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginBatches]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginBatches_Date] ON [zengin].[TD_ZenginBatches] ([WithdrawalDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginBatches_Consignor'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginBatches]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginBatches_Consignor] ON [zengin].[TD_ZenginBatches] ([ConsignorCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginTransactions_Account'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginTransactions_Account] ON [zengin].[TD_ZenginTransactions]
    ([BankCode], [BranchCode], [AccountNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginTransactions_Batch'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginTransactions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginTransactions_Batch] ON [zengin].[TD_ZenginTransactions]
    ([ZenginBatchId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginTransmissionLogs_Date'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginTransmissionLogs]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginTransmissionLogs_Date] ON [zengin].[TD_ZenginTransmissionLogs]
    ([ManagementDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginTransmissionLogs_Receive'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginTransmissionLogs]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginTransmissionLogs_Receive] ON [zengin].[TD_ZenginTransmissionLogs]
    ([ManagementDate], [ResultReceiveFlag]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ZenginVerifications_Date'
                 AND object_id = OBJECT_ID(N'[zengin].[TD_ZenginVerifications]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ZenginVerifications_Date] ON [zengin].[TD_ZenginVerifications]
    ([TransferDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TM_AmountSettings_CompanyCode'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_AmountSettings]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TM_AmountSettings_CompanyCode] ON [dbo].[TM_AmountSettings] ([CompanyCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TM_CodeSettings_CodeCategory_CodeValue'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_CodeSettings]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TM_CodeSettings_CodeCategory_CodeValue]
    ON [dbo].[TM_CodeSettings] ([CodeCategory], [CodeValue]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TM_CodeSettings_CodeCategory_DisplayOrder_CodeValue'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_CodeSettings]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TM_CodeSettings_CodeCategory_DisplayOrder_CodeValue]
    ON [dbo].[TM_CodeSettings] ([CodeCategory], [DisplayOrder], [CodeValue]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ErrorSuppressParameters_Flag'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_ErrorSuppressParameters]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ErrorSuppressParameters_Flag] ON [dbo].[TM_ErrorSuppressParameters]
    ([CompanyCode], [SuppressFlag]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_JapaneseEras_StartDate'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_JapaneseEras]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_JapaneseEras_StartDate] ON [dbo].[TM_JapaneseEras] ([StartDate]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UQ_TM_Users_UserId'
                 AND object_id = OBJECT_ID(N'[dbo].[TM_Users]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UQ_TM_Users_UserId] ON [dbo].[TM_Users] ([UserId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AccountFilePrintRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AccountFilePrintRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AccountFilePrintRecords_JobExecution]
ON [dbo].[TR_AccountFilePrintRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_AccountLinks_Contract'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AccountLinks]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_AccountLinks_Contract] ON [dbo].[TR_AccountLinks]
    ([CompanyCode], [PersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AccountNumberChangeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AccountNumberChangeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AccountNumberChangeRecords_JobExecution]
ON [dbo].[TR_AccountNumberChangeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AccountProcedureCompletionNotices_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AccountProcedureCompletionNotices]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AccountProcedureCompletionNotices_JobExecution]
ON [dbo].[TR_AccountProcedureCompletionNotices] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AccountProcedureSampleRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AccountProcedureSampleRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AccountProcedureSampleRecords_JobExecution]
ON [dbo].[TR_AccountProcedureSampleRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AllocationCoefficientRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AllocationCoefficientRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AllocationCoefficientRecords_JobExecution]
ON [dbo].[TR_AllocationCoefficientRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AmountDataChangeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AmountDataChangeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AmountDataChangeRecords_JobExecution]
ON [dbo].[TR_AmountDataChangeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AmountDataCountConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AmountDataCountConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AmountDataCountConfirmationRecords_JobExecution]
ON [dbo].[TR_AmountDataCountConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_AmountDataDuplicateWarningRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AmountDataDuplicateWarningRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_AmountDataDuplicateWarningRecords_JobExecution]
ON [dbo].[TR_AmountDataDuplicateWarningRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_AmountErrorRecords_BatchRun'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_AmountErrorRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_AmountErrorRecords_BatchRun] ON [dbo].[TR_AmountErrorRecords]
    ([JobExecutionId], [CompanyCode], [BatchNo], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_BankArrivalConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_BankArrivalConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_BankArrivalConfirmationRecords_JobExecution]
ON [dbo].[TR_BankArrivalConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_BankSummaryRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_BankSummaryRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_BankSummaryRecords_JobExecution]
ON [dbo].[TR_BankSummaryRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_BankWithdrawalDataManagementRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_BankWithdrawalDataManagementRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_BankWithdrawalDataManagementRecords_JobExecution]
ON [dbo].[TR_BankWithdrawalDataManagementRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_BillingDataPresenceConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_BillingDataPresenceConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_BillingDataPresenceConfirmationRecords_JobExecution]
ON [dbo].[TR_BillingDataPresenceConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CmtFormInputConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CmtFormInputConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CmtFormInputConfirmationRecords_JobExecution]
ON [dbo].[TR_CmtFormInputConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CmtTransferResultConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CmtTransferResultConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CmtTransferResultConfirmationRecords_JobExecution]
ON [dbo].[TR_CmtTransferResultConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CompanyBankSummaryRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CompanyBankSummaryRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CompanyBankSummaryRecords_JobExecution]
ON [dbo].[TR_CompanyBankSummaryRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CompanyBankSummaryRoutineRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CompanyBankSummaryRoutineRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CompanyBankSummaryRoutineRecords_JobExecution]
ON [dbo].[TR_CompanyBankSummaryRoutineRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CompanyBillingSummaryRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CompanyBillingSummaryRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CompanyBillingSummaryRecords_JobExecution]
ON [dbo].[TR_CompanyBillingSummaryRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_CompanyMasterChangeLogs_BatchRun'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CompanyMasterChangeLogs]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_CompanyMasterChangeLogs_BatchRun] ON [dbo].[TR_CompanyMasterChangeLogs]
    ([JobExecutionId], [CompanyCode], [RequestType]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CompanyTransferScheduleRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CompanyTransferScheduleRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CompanyTransferScheduleRecords_JobExecution]
ON [dbo].[TR_CompanyTransferScheduleRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ContractChangeErrorRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractChangeErrorRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ContractChangeErrorRecords_JobExecution]
ON [dbo].[TR_ContractChangeErrorRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ContractListDiskRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractListDiskRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ContractListDiskRecords_JobExecution]
ON [dbo].[TR_ContractListDiskRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ContractMasterDeleteLogs_JobExecutionId_CompanyCode_PersonalCode'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractMasterDeleteLogs]'))
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ContractMasterDeleteLogs_JobExecutionId_CompanyCode_PersonalCode]
    ON [dbo].[TR_ContractMasterDeleteLogs] ([JobExecutionId], [CompanyCode], [PersonalCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_ContractMasterDeleteLogs_JobExecutionId'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractMasterDeleteLogs]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_ContractMasterDeleteLogs_JobExecutionId]
    ON [dbo].[TR_ContractMasterDeleteLogs] ([JobExecutionId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ContractMasterShortListRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractMasterShortListRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ContractMasterShortListRecords_JobExecution]
ON [dbo].[TR_ContractMasterShortListRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ContractorDuplicateChangeCheckRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractorDuplicateChangeCheckRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ContractorDuplicateChangeCheckRecords_JobExecution]
ON [dbo].[TR_ContractorDuplicateChangeCheckRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ContractorMasterDeleteRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ContractorMasterDeleteRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ContractorMasterDeleteRecords_JobExecution]
ON [dbo].[TR_ContractorMasterDeleteRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CooperativeTransferTapeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CooperativeTransferTapeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CooperativeTransferTapeRecords_JobExecution]
ON [dbo].[TR_CooperativeTransferTapeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_CoverLetterPrintRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_CoverLetterPrintRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_CoverLetterPrintRecords_JobExecution]
ON [dbo].[TR_CoverLetterPrintRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_DiskFormInputConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_DiskFormInputConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_DiskFormInputConfirmationRecords_JobExecution]
ON [dbo].[TR_DiskFormInputConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_DiskTransferResultConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_DiskTransferResultConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_DiskTransferResultConfirmationRecords_JobExecution]
ON [dbo].[TR_DiskTransferResultConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_EnrollmentFormRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_EnrollmentFormRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_EnrollmentFormRecords_JobExecution]
ON [dbo].[TR_EnrollmentFormRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_FinancialInstitutionExchangeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_FinancialInstitutionExchangeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_FinancialInstitutionExchangeRecords_JobExecution]
ON [dbo].[TR_FinancialInstitutionExchangeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_FinancialInstitutionRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_FinancialInstitutionRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_FinancialInstitutionRecords_JobExecution]
ON [dbo].[TR_FinancialInstitutionRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_GeneralAffairsDepositListRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_GeneralAffairsDepositListRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_GeneralAffairsDepositListRecords_JobExecution]
ON [dbo].[TR_GeneralAffairsDepositListRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_InstitutionSummaries_Bank'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_InstitutionSummaries]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_InstitutionSummaries_Bank] ON [dbo].[TR_InstitutionSummaries]
    ([BankCode], [BranchCode]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_InstitutionSummaryPrintRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_InstitutionSummaryPrintRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_InstitutionSummaryPrintRecords_JobExecution]
ON [dbo].[TR_InstitutionSummaryPrintRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_InstitutionSummarySampleBRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_InstitutionSummarySampleBRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_InstitutionSummarySampleBRecords_JobExecution]
ON [dbo].[TR_InstitutionSummarySampleBRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_InstitutionSummarySampleRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_InstitutionSummarySampleRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_InstitutionSummarySampleRecords_JobExecution]
ON [dbo].[TR_InstitutionSummarySampleRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_KanjiCompanyMasterChangeLogs_BatchRun'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_KanjiCompanyMasterChangeLogs]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_KanjiCompanyMasterChangeLogs_BatchRun] ON [dbo].[TR_KanjiCompanyMasterChangeLogs]
    ([JobExecutionId], [CompanyCode], [SectionNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_MessageMasterChangeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_MessageMasterChangeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_MessageMasterChangeRecords_JobExecution]
ON [dbo].[TR_MessageMasterChangeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_OtherBankTransferDiskRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_OtherBankTransferDiskRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_OtherBankTransferDiskRecords_JobExecution]
ON [dbo].[TR_OtherBankTransferDiskRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_OtherBankTransferTapeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_OtherBankTransferTapeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_OtherBankTransferTapeRecords_JobExecution]
ON [dbo].[TR_OtherBankTransferTapeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_OutputCountRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_OutputCountRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_OutputCountRecords_JobExecution]
ON [dbo].[TR_OutputCountRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_PostOfficeBusinessOwnerNumberSummaries_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_PostOfficeBusinessOwnerNumberSummaries]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_PostOfficeBusinessOwnerNumberSummaries_JobExecution]
ON [dbo].[TR_PostOfficeBusinessOwnerNumberSummaries] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_PostOfficeTransferTapeRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_PostOfficeTransferTapeRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_PostOfficeTransferTapeRecords_JobExecution]
ON [dbo].[TR_PostOfficeTransferTapeRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ProcessParameterCheckRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ProcessParameterCheckRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ProcessParameterCheckRecords_JobExecution]
ON [dbo].[TR_ProcessParameterCheckRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_RemittanceRequestRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_RemittanceRequestRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_RemittanceRequestRecords_JobExecution]
ON [dbo].[TR_RemittanceRequestRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ReportExecutions_ExecutionId'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ReportExecutions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ReportExecutions_ExecutionId]
ON [dbo].[TR_ReportExecutions] ([ExecutionId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ReportExecutions_JobExecutionId'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ReportExecutions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ReportExecutions_JobExecutionId]
ON [dbo].[TR_ReportExecutions] ([JobExecutionId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ReportExecutions_TemplateId'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ReportExecutions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ReportExecutions_TemplateId]
ON [dbo].[TR_ReportExecutions] ([TemplateId]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ReportExecutions_Status'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ReportExecutions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ReportExecutions_Status]
ON [dbo].[TR_ReportExecutions] ([Status]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ReportExecutions_CreatedAt'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ReportExecutions]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ReportExecutions_CreatedAt]
ON [dbo].[TR_ReportExecutions] ([CreatedAt]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ResultDetailKanjiSealRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ResultDetailKanjiSealRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ResultDetailKanjiSealRecords_JobExecution]
ON [dbo].[TR_ResultDetailKanjiSealRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ResultDetailSequenceRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ResultDetailSequenceRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ResultDetailSequenceRecords_JobExecution]
ON [dbo].[TR_ResultDetailSequenceRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_SealReceiptKanaRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_SealReceiptKanaRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_SealReceiptKanaRecords_JobExecution]
ON [dbo].[TR_SealReceiptKanaRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_SealReceiptSampleRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_SealReceiptSampleRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_SealReceiptSampleRecords_JobExecution]
ON [dbo].[TR_SealReceiptSampleRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferCountRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferCountRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferCountRecords_JobExecution]
ON [dbo].[TR_TransferCountRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferDataCountConfirmationRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferDataCountConfirmationRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferDataCountConfirmationRecords_JobExecution]
ON [dbo].[TR_TransferDataCountConfirmationRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferFailureLists_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferFailureLists]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferFailureLists_JobExecution]
ON [dbo].[TR_TransferFailureLists] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferNotifications_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferNotifications]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferNotifications_JobExecution]
ON [dbo].[TR_TransferNotifications] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferResultSets_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferResultSets]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferResultSets_JobExecution]
ON [dbo].[TR_TransferResultSets] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferResultSummaries_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferResultSummaries]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferResultSummaries_JobExecution]
ON [dbo].[TR_TransferResultSummaries] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransferResultTransmissionNotices_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransferResultTransmissionNotices]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransferResultTransmissionNotices_JobExecution]
ON [dbo].[TR_TransferResultTransmissionNotices] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_TransmissionInstitutionResultSummaries_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_TransmissionInstitutionResultSummaries]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_TransmissionInstitutionResultSummaries_JobExecution]
ON [dbo].[TR_TransmissionInstitutionResultSummaries] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_UcvBillingDetails_Company'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_UcvBillingDetails]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_UcvBillingDetails_Company] ON [dbo].[TR_UcvBillingDetails]
    ([CompanyCode], [BillingYearMonth]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_UedaCableReceiptRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_UedaCableReceiptRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_UedaCableReceiptRecords_JobExecution]
ON [dbo].[TR_UedaCableReceiptRecords] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_UedaCableResultDetails_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_UedaCableResultDetails]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_UedaCableResultDetails_JobExecution]
ON [dbo].[TR_UedaCableResultDetails] ([JobExecutionId], [SequenceNo]);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_TR_ZenginImportErrorRecords_JobExecution'
                 AND object_id = OBJECT_ID(N'[dbo].[TR_ZenginImportErrorRecords]'))
BEGIN
    EXEC(N'CREATE INDEX [IX_TR_ZenginImportErrorRecords_JobExecution]
ON [dbo].[TR_ZenginImportErrorRecords] ([JobExecutionId], [SequenceNo]);');
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
