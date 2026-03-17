using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoGamesStore.Models;

public partial class Game
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "ID в RAWG")]
    public int? RawgId { get; set; }

    [Display(Name = "Название")]
    [Required(ErrorMessage = "Укажите название игры.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 200 символов.")]
    public string Title { get; set; } = null!;

    [Display(Name = "Описание")]
    [StringLength(4000, ErrorMessage = "Описание не должно быть длиннее 4000 символов.")]
    public string? Description { get; set; }

    [Display(Name = "Дата выхода")]
    public DateOnly? ReleaseDate { get; set; }

    [Display(Name = "Цена")]
    [Range(typeof(decimal), "0,01", "999999,99", ErrorMessage = "Цена должна быть в диапазоне от 0,01 до 999999,99.")]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Display(Name = "Остаток")]
    [Range(0, 100000, ErrorMessage = "Количество на складе должно быть от 0 до 100000.")]
    public int Stock { get; set; }

    [Display(Name = "Рейтинг")]
    [Range(typeof(decimal), "0", "10", ErrorMessage = "Рейтинг должен быть в диапазоне от 0 до 10.")]
    [Column(TypeName = "decimal(3, 1)")]
    public decimal? Rating { get; set; }

    [Display(Name = "Ссылка на обложку")]
    [StringLength(500, ErrorMessage = "Ссылка на обложку не должна быть длиннее 500 символов.")]
    public string? CoverImageUrl { get; set; }

    [Display(Name = "Возрастной рейтинг")]
    [StringLength(20, ErrorMessage = "Возрастной рейтинг не должен быть длиннее 20 символов.")]
    public string? AgeRating { get; set; }

    [Display(Name = "Издатель")]
    public int? PublisherId { get; set; }

    [Display(Name = "Жанр")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите жанр.")]
    public int GenreId { get; set; }

    [Display(Name = "Активна")]
    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("GenreId")]
    [InverseProperty("Games")]
    public virtual Genre Genre { get; set; } = null!;

    [InverseProperty("Game")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ForeignKey("PublisherId")]
    [InverseProperty("Games")]
    public virtual Publisher? Publisher { get; set; }

    [InverseProperty("Game")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("GameId")]
    [InverseProperty("Games")]
    public virtual ICollection<Platform> Platforms { get; set; } = new List<Platform>();
}
