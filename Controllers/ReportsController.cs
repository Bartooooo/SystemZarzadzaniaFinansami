using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;
using SystemZarzadzaniaFinansami.Data;
using SystemZarzadzaniaFinansami.Models;
using Microsoft.AspNetCore.Authorization;

namespace SystemZarzadzaniaFinansami.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports/Index
        public IActionResult Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Pobierz tylko kategorie użytkownika
            var categories = _context.Categories.Where(c => c.UserId == userId).ToList();
            ViewBag.Categories = categories;

            return View("~/Views/Reports1/Index.cshtml");
        }

        // POST: Reports/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(DateTime? startDate, DateTime? endDate, string reportType, int? categoryId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = DateTime.Now.AddMonths(-1);
                endDate = DateTime.Now;
            }

            // Pobierz tylko dane użytkownika
            var incomesQuery = _context.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId);
            var expensesQuery = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            // Filtruj na podstawie kategorii (jeśli wybrana)
            if (categoryId.HasValue)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

                if (category == null)
                {
                    return BadRequest("Nieprawidłowa kategoria.");
                }

                incomesQuery = incomesQuery.Where(i => i.CategoryId == categoryId);
                expensesQuery = expensesQuery.Where(e => e.CategoryId == categoryId);
            }

            if (reportType == "incomes")
            {
                expensesQuery = _context.Expenses.Where(e => e.Id == 0); // Pusty wynik dla wydatków
            }
            else if (reportType == "expenses")
            {
                incomesQuery = _context.Incomes.Where(i => i.Id == 0); // Pusty wynik dla przychodów
            }

            var incomes = await incomesQuery
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .ToListAsync();
            var expenses = await expensesQuery
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            var incomeTotal = incomes.Sum(i => i.Amount);
            var expenseTotal = expenses.Sum(e => e.Amount);
            var balance = incomeTotal - expenseTotal;

            string reportName = reportType switch
            {
                "incomes" => "Raport przychody",
                "expenses" => "Raport wydatki",
                _ => "Raport finansowy"
            };

            var report = new
            {
                ReportName = reportName,
                StartDate = startDate.Value.ToString("yyyy-MM-dd"),
                EndDate = endDate.Value.ToString("yyyy-MM-dd"),
                ReportType = reportType,
                Incomes = incomes,
                Expenses = expenses,
                IncomeTotal = incomeTotal,
                ExpenseTotal = expenseTotal,
                Balance = balance
            };

            return View("~/Views/Reports1/Report.cshtml", report);
        }

        // GET: Reports/ExportToCSV
        [HttpGet]
        public async Task<IActionResult> ExportToCSV(DateTime? startDate, DateTime? endDate, string reportType, int? categoryId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!startDate.HasValue || !endDate.HasValue)
            {
                return BadRequest("Nie podano zakresu dat.");
            }

            var incomesQuery = _context.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId);
            var expensesQuery = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            if (categoryId.HasValue)
            {
                incomesQuery = incomesQuery.Where(i => i.CategoryId == categoryId);
                expensesQuery = expensesQuery.Where(e => e.CategoryId == categoryId);
            }

            if (reportType == "incomes")
            {
                expensesQuery = _context.Expenses.Where(e => e.Id == 0);
            }
            else if (reportType == "expenses")
            {
                incomesQuery = _context.Incomes.Where(i => i.Id == 0);
            }

            var incomes = await incomesQuery
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .ToListAsync();
            var expenses = await expensesQuery
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

            // Tworzenie CSV
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Typ,Kategoria,Data,Kwota");

            foreach (var income in incomes)
            {
                stringBuilder.AppendLine($"Przychód,\"{income.Category?.Name ?? "Brak kategorii"}\",{income.Date.ToString("yyyy-MM-dd")},{income.Amount.ToString("F2", CultureInfo.InvariantCulture)}");
            }

            foreach (var expense in expenses)
            {
                stringBuilder.AppendLine($"Wydatek,\"{expense.Category?.Name ?? "Brak kategorii"}\",{expense.Date.ToString("yyyy-MM-dd")},{expense.Amount.ToString("F2", CultureInfo.InvariantCulture)}");
            }

            // Ustawianie nazwy pliku
            var fileName = $"Raport_{DateTime.Now:yyyyMMddHHmmss}.csv";

            // Zwracanie pliku CSV
            return File(Encoding.UTF8.GetBytes(stringBuilder.ToString()), "text/csv", fileName);
        }

    }
}
