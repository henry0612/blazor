namespace UnifiedAccount.Domain.Enums;

/// <summary>
/// ユーザーロール (F-INF-001)
/// </summary>
public enum UserRole
{
    /// <summary>
    /// システム管理者
    /// </summary>
    SystemAdmin = 0,

    /// <summary>
    /// 運用管理者
    /// </summary>
    OperationManager = 1,

    /// <summary>
    /// 運用担当者
    /// </summary>
    OperationStaff = 2,

    /// <summary>
    /// オペレータ
    /// </summary>
    Operator = 3
}

