using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VideoGamesStore.Models;
using VideoGamesStore.ViewModels.Games;

namespace VideoGamesStore.Controllers;

public class GamesController : Controller
{
    private readonly VideoGamesStoreContext _context;
    private const int PageSize = 9;
    private const int MetaNameMaxLength = 100;
    public GamesController(VideoGamesStoreContext context) => _context = context;

    public async Task<IActionResult> Index(string? searchString, int? genreId, int? publisherId, int[]? selectedPlatformIds, string? sortOrder, int page = 1)
    {
        var gamesQuery = _context.Games.Include(g => g.Genre).Include(g => g.Publisher).Include(g => g.Platforms).AsQueryable();
        selectedPlatformIds ??= [];

        if (!User.IsInRole("Admin")) gamesQuery = gamesQuery.Where(g => g.IsActive);
        if (!string.IsNullOrWhiteSpace(searchString)) gamesQuery = gamesQuery.Where(g => g.Title.Contains(searchString));
        if (genreId.HasValue) gamesQuery = gamesQuery.Where(g => g.GenreId == genreId.Value);
        if (publisherId.HasValue) gamesQuery = gamesQuery.Where(g => g.PublisherId == publisherId.Value);
        if (selectedPlatformIds.Length > 0) gamesQuery = gamesQuery.Where(g => g.Platforms.Any(p => selectedPlatformIds.Contains(p.Id)));

        gamesQuery = sortOrder switch
        {
            "title_desc" => gamesQuery.OrderByDescending(g => g.Title),
            "price_asc" => gamesQuery.OrderBy(g => g.Price),
            "price_desc" => gamesQuery.OrderByDescending(g => g.Price),
            _ => gamesQuery.OrderBy(g => g.Title)
        };

        var total = await gamesQuery.CountAsync();
        var vm = new GamesIndexViewModel
        {
            Items = await gamesQuery.Skip((Math.Max(page, 1) - 1) * PageSize).Take(PageSize).ToListAsync(),
            Genres = new SelectList(await _context.Genres.OrderBy(g => g.Name).ToListAsync(), "Id", "Name", genreId),
            Publishers = new SelectList(await _context.Publishers.OrderBy(p => p.Name).ToListAsync(), "Id", "Name", publisherId),
            Platforms = new SelectList(await _context.Platforms.OrderBy(p => p.Name).ToListAsync(), "Id", "Name"),
            SearchString = searchString,
            GenreId = genreId,
            PublisherId = publisherId,
            SelectedPlatformIds = selectedPlatformIds,
            SortOrder = sortOrder,
            Page = Math.Max(page, 1),
            TotalPages = (int)Math.Ceiling(total / (double)PageSize)
        };
        return View(vm);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var game = await _context.Games
            .Include(g => g.Genre)
            .Include(g => g.Publisher)
            .Include(g => g.Platforms)
            .Include(g => g.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null || (!game.IsActive && !User.IsInRole("Admin"))) return NotFound();
        return View(game);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int gameId, int rating, string? comment)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId && g.IsActive);
        if (game is null) return NotFound();

        if (rating < 1 || rating > 10)
        {
            TempData["Error"] = "Оценка должна быть от 1 до 10.";
            return RedirectToAction(nameof(Details), new { id = gameId });
        }

        var normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (normalizedComment?.Length > 1000)
        {
            TempData["Error"] = "Комментарий не должен быть длиннее 1000 символов.";
            return RedirectToAction(nameof(Details), new { id = gameId });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId)) return Forbid();

        _context.Reviews.Add(new Review
        {
            GameId = gameId,
            UserId = userId,
            Rating = rating,
            Comment = normalizedComment,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Спасибо! Отзыв отправлен на модерацию.";
        return RedirectToAction(nameof(Details), new { id = gameId });
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        FillMeta();
        return View(new Game { IsActive = true });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGenre(string genreName, string? returnAction = null, int? returnId = null)
    {
        var normalizedName = NormalizeMetaName(genreName);
        if (normalizedName is null)
        {
            TempData["Error"] = $"Название жанра должно содержать от 2 до {MetaNameMaxLength} символов.";
            return RedirectToSafeAction(returnAction, returnId);
        }

        if (!await _context.Genres.AnyAsync(g => g.Name == normalizedName))
        {
            _context.Genres.Add(new Genre { Name = normalizedName });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Жанр добавлен.";
        }
        else
        {
            TempData["Error"] = "Такой жанр уже существует.";
        }

        return RedirectToSafeAction(returnAction, returnId);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPublisher(string publisherName, string? returnAction = null, int? returnId = null)
    {
        var normalizedName = NormalizeMetaName(publisherName, 150);
        if (normalizedName is null)
        {
            TempData["Error"] = "Название издателя должно содержать от 2 до 150 символов.";
            return RedirectToSafeAction(returnAction, returnId);
        }

        if (!await _context.Publishers.AnyAsync(p => p.Name == normalizedName))
        {
            _context.Publishers.Add(new Publisher { Name = normalizedName });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Издатель добавлен.";
        }
        else
        {
            TempData["Error"] = "Такой издатель уже существует.";
        }

        return RedirectToSafeAction(returnAction, returnId);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPlatform(string platformName, string? returnAction = null, int? returnId = null)
    {
        var normalizedName = NormalizeMetaName(platformName);
        if (normalizedName is null)
        {
            TempData["Error"] = $"Название платформы должно содержать от 2 до {MetaNameMaxLength} символов.";
            return RedirectToSafeAction(returnAction, returnId);
        }

        if (!await _context.Platforms.AnyAsync(p => p.Name == normalizedName))
        {
            _context.Platforms.Add(new Platform { Name = normalizedName });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Платформа добавлена.";
        }
        else
        {
            TempData["Error"] = "Такая платформа уже существует.";
        }

        return RedirectToSafeAction(returnAction, returnId);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Game game, int[] selectedPlatforms)
    {
        if (!ModelState.IsValid)
        {
            FillMeta(game.GenreId, game.PublisherId, selectedPlatforms);
            return View(game);
        }
        game.Platforms = await _context.Platforms.Where(p => selectedPlatforms.Contains(p.Id)).ToListAsync();
        game.CreatedAt = DateTime.UtcNow;
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Игра добавлена.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var game = await _context.Games.Include(g => g.Platforms).FirstOrDefaultAsync(g => g.Id == id);
        if (game is null) return NotFound();
        FillMeta(game.GenreId, game.PublisherId, game.Platforms.Select(p => p.Id));
        return View(game);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Game model, int[] selectedPlatforms)
    {
        var game = await _context.Games.Include(g => g.Platforms).FirstOrDefaultAsync(g => g.Id == id);
        if (game is null) return NotFound();
        if (!ModelState.IsValid)
        {
            FillMeta(model.GenreId, model.PublisherId, selectedPlatforms);
            return View(model);
        }

        game.Title = model.Title;
        game.Description = model.Description;
        game.Price = model.Price;
        game.Stock = model.Stock;
        game.GenreId = model.GenreId;
        game.PublisherId = model.PublisherId;
        game.CoverImageUrl = model.CoverImageUrl;
        game.ReleaseDate = model.ReleaseDate;
        game.Rating = model.Rating;
        game.AgeRating = model.AgeRating;
        game.IsActive = model.IsActive;
        game.Platforms = await _context.Platforms.Where(p => selectedPlatforms.Contains(p.Id)).ToListAsync();
        await _context.SaveChangesAsync();

        TempData["Success"] = "Игра обновлена.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _context.Games.Include(g => g.Genre).FirstOrDefaultAsync(g => g.Id == id);
        return game is null ? NotFound() : View(game);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var game = await _context.Games
            .Include(g => g.Platforms)
            .Include(g => g.Reviews)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (game is not null)
        {
            if (game.Reviews.Count > 0)
            {
                _context.Reviews.RemoveRange(game.Reviews);
            }

            game.Platforms.Clear();
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private void FillMeta(int? genreId = null, int? publisherId = null, IEnumerable<int>? selectedPlatforms = null)
    {
        ViewBag.Genres = new SelectList(_context.Genres.OrderBy(g => g.Name), "Id", "Name", genreId);
        ViewBag.Publishers = new SelectList(_context.Publishers.OrderBy(p => p.Name), "Id", "Name", publisherId);
        ViewBag.Platforms = new MultiSelectList(_context.Platforms.OrderBy(p => p.Name), "Id", "Name", selectedPlatforms);
    }

    private static string? NormalizeMetaName(string? input, int maxLength = MetaNameMaxLength)
    {
        var value = input?.Trim();
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Length is < 2 or > maxLength ? null : value;
    }

    private IActionResult RedirectToSafeAction(string? returnAction, int? returnId)
    {
        if (string.Equals(returnAction, nameof(Edit), StringComparison.OrdinalIgnoreCase) && returnId.HasValue)
            return RedirectToAction(nameof(Edit), new { id = returnId.Value });

        return RedirectToAction(nameof(Create));
    }
}
