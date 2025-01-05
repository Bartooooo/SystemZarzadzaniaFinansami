using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaFinansami.Data;
using SystemZarzadzaniaFinansami.Models;

namespace SystemZarzadzaniaFinansami.Controllers
{
    /// <summary>
    /// Kontroler do zarządzania kategoriami użytkownika.
    /// </summary>
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicjalizuje instancję kontrolera CategoriesController.
        /// </summary>
        /// <param name="context">Kontekst bazy danych.</param>
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyświetla listę kategorii dla zalogowanego użytkownika.
        /// </summary>
        /// <returns>Widok z listą kategorii.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var categories = _context.Categories.Where(c => c.UserId == userId);
            return View(await categories.ToListAsync());
        }

        /// <summary>
        /// Wyświetla szczegóły wybranej kategorii.
        /// </summary>
        /// <param name="id">Identyfikator kategorii.</param>
        /// <returns>Widok szczegółów kategorii.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// Wyświetla formularz do utworzenia nowej kategorii.
        /// </summary>
        /// <returns>Widok formularza tworzenia kategorii.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Przetwarza dane przesłane przez formularz tworzenia kategorii.
        /// </summary>
        /// <param name="category">Model kategorii z formularza.</param>
        /// <returns>Widok z listą kategorii lub formularz w przypadku błędów.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            category.UserId = userId; // Przypisanie wartości UserId
            ModelState.Remove("UserId"); // Usunięcie walidacji pola UserId

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas zapisu do bazy danych: {ex.Message}");
                    ModelState.AddModelError("", "Nie można dodać kategorii. Spróbuj ponownie.");
                }
            }
            return View(category);
        }

        /// <summary>
        /// Wyświetla formularz edycji istniejącej kategorii.
        /// </summary>
        /// <param name="id">Identyfikator kategorii.</param>
        /// <returns>Widok formularza edycji kategorii.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        /// <summary>
        /// Przetwarza dane przesłane przez formularz edycji kategorii.
        /// </summary>
        /// <param name="id">Identyfikator kategorii.</param>
        /// <param name="category">Model kategorii z formularza.</param>
        /// <returns>Widok z listą kategorii lub formularz w przypadku błędów.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id)
            {
                Console.WriteLine($"Błąd: Id z URL ({id}) różni się od Id z modelu ({category.Id}).");
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Błąd: Brak UserId.");
                return Unauthorized();
            }

            // Pobierz istniejącą kategorię
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (existingCategory == null)
            {
                Console.WriteLine($"Błąd: Nie znaleziono kategorii o Id={id} dla UserId={userId}.");
                return NotFound();
            }

            Console.WriteLine($"Przed aktualizacją: Id={existingCategory.Id}, Name={existingCategory.Name}, UserId={existingCategory.UserId}");

            // Zaktualizuj tylko nazwę kategorii
            existingCategory.Name = category.Name;

            // Usuń UserId z ModelState
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Zapisano zmiany w kategorii.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas zapisywania zmian: {ex.Message}");
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("ModelState jest nieprawidłowy.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Błąd walidacji: {error.ErrorMessage}");
            }

            return View(category);
        }




        /// <summary>
        /// Wyświetla formularz usunięcia kategorii.
        /// </summary>
        /// <param name="id">Identyfikator kategorii.</param>
        /// <returns>Widok formularza usunięcia kategorii.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        /// <summary>
        /// Przetwarza potwierdzenie usunięcia kategorii.
        /// </summary>
        /// <param name="id">Identyfikator kategorii.</param>
        /// <returns>Przekierowanie do listy kategorii.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Sprawdza, czy kategoria istnieje w bazie danych.
        /// </summary>
        /// <param name="id">Identyfikator kategorii.</param>
        /// <returns>True, jeśli kategoria istnieje; w przeciwnym razie false.</returns>
        private bool CategoryExists(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return _context.Categories.Any(e => e.Id == id && e.UserId == userId);
        }
    }
}
