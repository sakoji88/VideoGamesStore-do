using System.ComponentModel.DataAnnotations;

namespace VideoGamesStore.ViewModels.Account;

public class RegisterViewModel
{
    [Display(Name = "Логин")]
    [Required(ErrorMessage = "Укажите логин.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 50 символов.")]
    [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Логин может содержать только латинские буквы, цифры и символы _.-")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Электронная почта")]
    [Required(ErrorMessage = "Укажите электронную почту.")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    [StringLength(100, ErrorMessage = "Адрес электронной почты не должен быть длиннее 100 символов.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Пароль")]
    [Required(ErrorMessage = "Введите пароль.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов.")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Подтверждение пароля")]
    [Required(ErrorMessage = "Повторите пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
