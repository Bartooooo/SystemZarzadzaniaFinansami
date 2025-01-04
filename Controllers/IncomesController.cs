using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaFinansami.Data;
using SystemZarzadzaniaFinansami.Models;

namespace SystemZarzadzaniaFinansami.Controllers
{
    /// <summary>
    /// Kontroler zarządzający przychodami użytkownika.
    /// </summary>
    public class IncomesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera IncomesController.
        /// </summary>
        /// <param name="context">Kontekst bazy danych.</param>
        public IncomesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyświetla listę przychodów dla zalogowanego użytkownika.
        /// </summary>
        /// <returns>Widok z listą przychodów.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var incomes = _context.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId);

            return View(await incomes.ToListAsync());
        }

        /// <summary>
        /// Wyświetla szczegóły wybranego przychodu.
        /// </summary>
        /// <param name="id">Identyfikator przychodu.</param>
        /// <returns>Widok szczegółów przychodu.</returns>
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

            var income = await _context.Incomes
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        /// <summary>
        /// Wyświetla formularz do utworzenia nowego przychodu.
        /// </summary>
        /// <returns>Widok formularza tworzenia przychodu.</returns>
        public IActionResult Create()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            ViewBag.CategoryId = _context.Categories
                .Where(c => c.UserId == userId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

            return View();
        }

        /// <summary>
        /// Przetwarza dane przesłane przez formularz tworzenia przychodu.
        /// </summary>
        /// <param name="income">Model przychodu przesłany przez formularz.</param>
        /// <returns>Widok z listą przychodów lub formularz w przypadku błędów.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Date,CategoryId,UserId")] Income income)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            income.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.UserId == userId),
                "Id",
                "Name",
                income.CategoryId
            );
            return View(income);
        }

        /// <summary>
        /// Wyświetla formularz edycji istniejącego przychodu.
        /// </summary>
        /// <param name="id">Identyfikator przychodu.</param>
        /// <returns>Widok formularza edycji przychodu.</returns>
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

            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (income == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.UserId == userId),
                "Id",
                "Name",
                income.CategoryId
            );
            return View(income);
        }

        /// <summary>
        /// Przetwarza dane przesłane przez formularz edycji przychodu.
        /// </summary>
        /// <param name="id">Identyfikator przychodu.</param>
        /// <param name="income">Model przychodu przesłany przez formularz.</param>
        /// <returns>Widok z listą przychodów lub formularz w przypadku błędów.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Date,CategoryId,UserId")] Income income)
        {
            if (id != income.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            income.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.UserId == userId),
                "Id",
                "Name",
                income.CategoryId
            );
            return View(income);
        }

        /// <summary>
        /// Wyświetla formularz usunięcia przychodu.
        /// </summary>
        /// <param name="id">Identyfikator przychodu.</param>
        /// <returns>Widok formularza usunięcia przychodu.</returns>
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

            var income = await _context.Incomes
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        /// <summary>
        /// Przetwarza potwierdzenie usunięcia przychodu.
        /// </summary>
        /// <param name="id">Identyfikator przychodu.</param>
        /// <returns>Przekierowanie do listy przychodów.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var income = await _context.Incomes.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Sprawdza, czy przychód istnieje w bazie danych.
        /// </summary>
        /// <param name="id">Identyfikator przychodu.</param>
        /// <returns>True, jeśli przychód istnieje; w przeciwnym razie false.</returns>
        private bool IncomeExists(int id)
        {
            return _context.Incomes.Any(i => i.Id == id);
        }
    }
}
