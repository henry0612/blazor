using UnifiedAccount.Domain.Enums;

namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// ログイン結果
/// </summary>
public record LoginResult(
    bool IsSuccess,
    string? UserId = null,
    string? UserName = null,
    UserRole? Role = null,
    string? ErrorMessage = null);



