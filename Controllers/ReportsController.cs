using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;
using SystemZarzadzaniaFinansami.Data;
using SystemZarzadzaniaFinansami.Models;
using Microsoft.AspNetCore.Authorization;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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

            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.AddDays(-30); // Ostatnie 30 dni
            }

            if (!endDate.HasValue)
            {
                endDate = DateTime.Now;
            }

            if (startDate > endDate)
            {
                return BadRequest("Data początkowa nie może być późniejsza niż data końcowa.");
            }
            else
            {
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1); // Ustaw na koniec dnia
            }

            // Pobierz dane użytkownika
            var incomesQuery = _context.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId);
            var expensesQuery = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            // Filtruj według kategorii
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

        // GET: Reports/GenerateBarChart
        [HttpGet]
        public IActionResult GenerateBarChart(decimal incomeTotal, decimal expenseTotal)
        {
            using (var bitmap = new Bitmap(600, 450)) // Zwiększenie wysokości obrazu
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                var maxValue = Math.Max(incomeTotal, expenseTotal);
                if (maxValue == 0)
                {
                    maxValue = 1; // Zapobieganie dzieleniu przez zero
                }

                var incomeHeight = (int)((incomeTotal / maxValue) * 300);
                var expenseHeight = (int)((expenseTotal / maxValue) * 300);

                // Rysowanie słupków
                var barWidth = 100;
                var barSpacing = 200;

                graphics.FillRectangle(Brushes.Green, 100, 400 - incomeHeight, barWidth, incomeHeight);
                graphics.FillRectangle(Brushes.Red, 100 + barSpacing, 400 - expenseHeight, barWidth, expenseHeight);

                // Dodanie wartości liczbowych
                graphics.DrawString($"Przychody: {incomeTotal} zł", new Font("Arial", 12), Brushes.Black, 100, 400 - incomeHeight - 20);
                graphics.DrawString($"Wydatki: {expenseTotal} zł", new Font("Arial", 12), Brushes.Black, 100 + barSpacing, 400 - expenseHeight - 20);

                // Oś X
                graphics.DrawLine(Pens.Black, 50, 400, 550, 400); // Linia osi X
                graphics.DrawString("Przychody", new Font("Arial", 12), Brushes.Black, 100 + (barWidth / 2) - 30, 410); // Przesunięcie etykiety w dół
                graphics.DrawString("Wydatki", new Font("Arial", 12), Brushes.Black, 100 + barSpacing + (barWidth / 2) - 30, 410); // Przesunięcie etykiety w dół

                // Oś Y
                graphics.DrawLine(Pens.Black, 50, 50, 50, 400); // Linia osi Y
                for (int i = 0; i <= maxValue; i += (int)Math.Ceiling(maxValue / 5))
                {
                    var y = 400 - (int)((i / maxValue) * 300);
                    graphics.DrawString(i.ToString(), new Font("Arial", 10), Brushes.Black, 10, y - 5);
                    graphics.DrawLine(Pens.Gray, 50, y, 550, y); // Siatka osi Y
                }

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }

    }
}
