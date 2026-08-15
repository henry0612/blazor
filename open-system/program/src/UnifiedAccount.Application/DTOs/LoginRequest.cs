using System.ComponentModel.DataAnnotations;

namespace UnifiedAccount.Application.DTOs;

/// <summary>
/// ログインリクエスト (ログイン画面入力値)
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "ユーザーIDは必須です。")]
    [MaxLength(20, ErrorMessage = "ユーザーIDは20文字以内で入力してください。")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "パスワードは必須です。")]
    public string Password { get; set; } = string.Empty;
}

