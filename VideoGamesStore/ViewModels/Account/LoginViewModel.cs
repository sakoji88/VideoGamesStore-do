using System.ComponentModel.DataAnnotations;

namespace VideoGamesStore.ViewModels.Account;

public class LoginViewModel
{
    [Display(Name = "Логин или электронная почта")]
    [Required(ErrorMessage = "Введите логин или электронную почту.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Поле должно содержать от 3 до 100 символов.")]
    public string Login { get; set; } = string.Empty;

    [Display(Name = "Пароль")]
    [Required(ErrorMessage = "Введите пароль.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов.")]
    public string Password { get; set; } = string.Empty;
}
