using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VideoGamesStore.Models;

[Index("Name", Name = "UQ__Genres__737584F65AFBB661", IsUnique = true)]
public partial class Genre
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Название жанра")]
    [Required(ErrorMessage = "Укажите название жанра.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название жанра должно содержать от 2 до 100 символов.")]
    public string Name { get; set; } = null!;

    [InverseProperty("Genre")]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
