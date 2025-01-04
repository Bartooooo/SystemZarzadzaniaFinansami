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
    /// Kontroler do zarządzania wydatkami użytkownika.
    /// </summary>
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera ExpensesController.
        /// </summary>
        /// <param name="context">Kontekst bazy danych.</param>
        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyświetla listę wydatków dla zalogowanego użytkownika.
        /// </summary>
        /// <returns>Widok z listą wydatków.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var expenses = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            return View(await expenses.ToListAsync());
        }

        /// <summary>
        /// Wyświetla szczegóły wybranego wydatku.
        /// </summary>
        /// <param name="id">Identyfikator wydatku.</param>
        /// <returns>Widok szczegółów wydatku.</returns>
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

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        /// <summary>
        /// Wyświetla formularz do tworzenia nowego wydatku.
        /// </summary>
        /// <returns>Widok formularza tworzenia wydatku.</returns>
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
        /// Przetwarza dane przesłane przez formularz tworzenia wydatku.
        /// </summary>
        /// <param name="expense">Model wydatku przesłany przez formularz.</param>
        /// <returns>Widok z listą wydatków lub formularz w przypadku błędów.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Date,CategoryId,UserId")] Expense expense)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            expense.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.UserId == userId),
                "Id",
                "Name",
                expense.CategoryId
            );
            return View(expense);
        }

        /// <summary>
        /// Wyświetla formularz edycji istniejącego wydatku.
        /// </summary>
        /// <param name="id">Identyfikator wydatku.</param>
        /// <returns>Widok formularza edycji wydatku.</returns>
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

            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (expense == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Where(c => c.UserId == userId),
                "Id",
                "Name",
                expense.CategoryId
            );
            return View(expense);
        }

        /// <summary>
        /// Przetwarza dane przesłane przez formularz edycji wydatku.
        /// </summary>
        /// <param name="id">Identyfikator wydatku.</param>
        /// <param name="expense">Model wydatku przesłany przez formularz.</param>
        /// <returns>Widok z listą wydatków lub formularz w przypadku błędów.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Date,CategoryId,UserId")] Expense expense)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            expense.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
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
                expense.CategoryId
            );
            return View(expense);
        }

        /// <summary>
        /// Wyświetla formularz usunięcia wydatku.
        /// </summary>
        /// <param name="id">Identyfikator wydatku.</param>
        /// <returns>Widok formularza usunięcia wydatku.</returns>
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

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        /// <summary>
        /// Przetwarza potwierdzenie usunięcia wydatku.
        /// </summary>
        /// <param name="id">Identyfikator wydatku.</param>
        /// <returns>Przekierowanie do listy wydatków.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Sprawdza, czy dany wydatek istnieje w bazie danych.
        /// </summary>
        /// <param name="id">Identyfikator wydatku.</param>
        /// <returns>True, jeśli wydatek istnieje; w przeciwnym razie false.</returns>
        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}
