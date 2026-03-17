using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VideoGamesStore.Models;

[Index("Name", Name = "UQ__Platform__737584F6D58D84B8", IsUnique = true)]
public partial class Platform
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "Название платформы")]
    [Required(ErrorMessage = "Укажите название платформы.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название платформы должно содержать от 2 до 100 символов.")]
    public string Name { get; set; } = null!;

    [ForeignKey("PlatformId")]
    [InverseProperty("Platforms")]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
