using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VideoGamesStore.Models;

public partial class Review
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int GameId { get; set; }

    [Range(1, 10, ErrorMessage = "Оценка должна быть от 1 до 10.")]
    public int Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Отзыв не должен быть длиннее 1000 символов.")]
    public string? Comment { get; set; }

    public bool IsApproved { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("Reviews")]
    public virtual Game Game { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Reviews")]
    public virtual User User { get; set; } = null!;
}
