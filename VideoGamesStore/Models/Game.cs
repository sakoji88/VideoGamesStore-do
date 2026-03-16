using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoGamesStore.Models;

public partial class Game
{
    [Key]
    public int Id { get; set; }

    public int? RawgId { get; set; }

    [Required(ErrorMessage = "Укажите название игры.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Название должно содержать от 2 до 200 символов.")]
    public string Title { get; set; } = null!;

    [StringLength(4000, ErrorMessage = "Описание не должно быть длиннее 4000 символов.")]
    public string? Description { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    [Range(typeof(decimal), "0,01", "999999,99", ErrorMessage = "Цена должна быть в диапазоне от 0,01 до 999999,99.")]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Range(0, 100000, ErrorMessage = "Количество на складе должно быть от 0 до 100000.")]
    public int Stock { get; set; }

    [Range(typeof(decimal), "0", "10", ErrorMessage = "Рейтинг должен быть в диапазоне от 0 до 10.")]
    [Column(TypeName = "decimal(3, 1)")]
    public decimal? Rating { get; set; }

    [StringLength(500, ErrorMessage = "Ссылка на обложку не должна быть длиннее 500 символов.")]
    public string? CoverImageUrl { get; set; }

    [StringLength(20, ErrorMessage = "Возрастной рейтинг не должен быть длиннее 20 символов.")]
    public string? AgeRating { get; set; }

    public int? PublisherId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите жанр.")]
    public int GenreId { get; set; }

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
