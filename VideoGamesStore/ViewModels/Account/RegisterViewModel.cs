using System.ComponentModel.DataAnnotations;

namespace VideoGamesStore.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите логин.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 50 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Используйте только буквы, цифры и _.-")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите email.")]
    [EmailAddress(ErrorMessage = "Введите корректный email-адрес.")]
    [StringLength(100, ErrorMessage = "Email не должен быть длиннее 100 символов.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
