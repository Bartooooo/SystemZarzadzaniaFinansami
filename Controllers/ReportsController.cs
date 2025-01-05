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
    /// <summary>
    /// Kontroler zarządzający generowaniem raportów finansowych i wykresów.
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicjalizuje nową instancję kontrolera ReportsController.
        /// </summary>
        /// <param name="context">Kontekst bazy danych.</param>
        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyświetla stronę główną raportów, umożliwiając wybór parametrów raportu.
        /// </summary>
        /// <returns>Widok strony głównej raportów.</returns>
        public IActionResult Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Pobierz kategorie użytkownika
            var categories = _context.Categories.Where(c => c.UserId == userId).ToList();
            ViewBag.Categories = categories;

            return View("~/Views/Reports1/Index.cshtml");
        }

        /// <summary>
        /// Generuje raport finansowy na podstawie podanych parametrów.
        /// </summary>
        /// <param name="startDate">Data początkowa zakresu raportu.</param>
        /// <param name="endDate">Data końcowa zakresu raportu.</param>
        /// <param name="reportType">Typ raportu: "incomes", "expenses" lub "all".</param>
        /// <param name="categoryId">Identyfikator kategorii (opcjonalnie).</param>
        /// <returns>Widok raportu finansowego.</returns>
        [HttpPost]
        public async Task<IActionResult> GenerateReport(DateTime? startDate, DateTime? endDate, string reportType, int? categoryId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Walidacja dat
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

            var incomeCategories = incomes
                .GroupBy(i => i.Category?.Name ?? "Brak kategorii")
                .Select(g => new { Category = g.Key, Total = g.Sum(i => i.Amount) })
                .ToDictionary(x => x.Category, x => x.Total);

            var expenseCategories = expenses
                .GroupBy(e => e.Category?.Name ?? "Brak kategorii")
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .ToDictionary(x => x.Category, x => x.Total);

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
                Balance = balance,
                IncomeCategories = reportType != "expenses" && incomeCategories.Any() ? string.Join(",", incomeCategories.Select(kvp => $"{kvp.Key}={kvp.Value}")) : null,
                ExpenseCategories = reportType != "incomes" && expenseCategories.Any() ? string.Join(",", expenseCategories.Select(kvp => $"{kvp.Key}={kvp.Value}")) : null
            };

            return View("~/Views/Reports1/Report.cshtml", report);
        }

        /// <summary>
        /// Generuje wykres słupkowy przedstawiający przychody i wydatki.
        /// </summary>
        /// <param name="incomeTotal">Całkowita suma przychodów.</param>
        /// <param name="expenseTotal">Całkowita suma wydatków.</param>
        /// <returns>Plik obrazu PNG z wykresem słupkowym.</returns>
        [HttpGet]
        public IActionResult GenerateBarChart(decimal incomeTotal, decimal expenseTotal)
        {
            using (var bitmap = new Bitmap(600, 450))
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

                // Oś X i Y
                graphics.DrawLine(Pens.Black, 50, 400, 550, 400);
                graphics.DrawLine(Pens.Black, 50, 50, 50, 400);

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }

        /// <summary>
        /// Generuje wykres kołowy przedstawiający podział kategorii w procentach.
        /// </summary>
        /// <param name="categories">Lista kategorii w formacie CSV (nazwa1=wartość1,nazwa2=wartość2,...).</param>
        /// <param name="title">Tytuł wykresu.</param>
        /// <returns>Plik obrazu PNG z wykresem kołowym.</returns>
        [HttpGet]
        public IActionResult GeneratePieChart(string categories, string title)
        {
            if (string.IsNullOrEmpty(categories))
            {
                return BadRequest("Nie znaleziono danych dla wykresu kołowego.");
            }

            try
            {
                // Parsowanie danych wejściowych
                var data = categories.Split(',')
                    .Select(x => x.Split('='))
                    .Where(pair => pair.Length == 2) // Upewnij się, że każda para ma dokładnie 2 elementy
                    .ToDictionary(
                        pair => pair[0],
                        pair => decimal.Parse(pair[1])
                    );

                using (var bitmap = new Bitmap(600, 450))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.White);

                    var total = data.Values.Sum();
                    if (total == 0)
                    {
                        total = 1; // Zapobieganie dzieleniu przez zero
                    }

                    var startAngle = 0f;
                    var random = new Random();
                    foreach (var kvp in data)
                    {
                        var sweepAngle = (float)(kvp.Value / total * 360);
                        var color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                        graphics.FillPie(new SolidBrush(color), 100, 50, 300, 300, startAngle, sweepAngle);
                        startAngle += sweepAngle;

                        // Dodanie legendy
                        graphics.DrawString($"{kvp.Key}: {kvp.Value / total:P1}", new Font("Arial", 10), Brushes.Black, 420, 50 + (int)(startAngle / 360 * 200));
                    }

                    graphics.DrawString(title, new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 200, 10);

                    using (var stream = new MemoryStream())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        return File(stream.ToArray(), "image/png");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd podczas generowania wykresu: {ex.Message}");
            }
        }
        /// <summary>
        /// Eksportuje raport do pliku CSV.
        /// </summary>
        /// <param name="startDate">Data początkowa zakresu raportu.</param>
        /// <param name="endDate">Data końcowa zakresu raportu.</param>
        /// <param name="reportType">Typ raportu: "incomes", "expenses", "all".</param>
        /// <returns>Plik CSV zawierający dane raportu.</returns>
        [HttpGet]
        public async Task<IActionResult> ExportToCSV(DateTime startDate, DateTime endDate, string reportType)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Walidacja dat
            if (startDate > endDate)
            {
                return BadRequest("Data początkowa nie może być późniejsza niż data końcowa.");
            }

            // Pobierz dane użytkownika
            var incomesQuery = _context.Incomes
                .Include(i => i.Category)
                .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate);
            var expensesQuery = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate);

            var incomes = reportType == "expenses" ? new List<Income>() : await incomesQuery.ToListAsync();
            var expenses = reportType == "incomes" ? new List<Expense>() : await expensesQuery.ToListAsync();

            var csvBuilder = new StringBuilder();

            // Nagłówki CSV
            csvBuilder.AppendLine("Typ,Kategoria,Kwota,Data");

            // Dodaj przychody do CSV
            foreach (var income in incomes)
            {
                csvBuilder.AppendLine($"Przychód,{income.Category?.Name ?? "Brak kategorii"},{income.Amount.ToString(CultureInfo.InvariantCulture)},{income.Date:yyyy-MM-dd}");
            }

            // Dodaj wydatki do CSV
            foreach (var expense in expenses)
            {
                csvBuilder.AppendLine($"Wydatek,{expense.Category?.Name ?? "Brak kategorii"},{expense.Amount.ToString(CultureInfo.InvariantCulture)},{expense.Date:yyyy-MM-dd}");
            }

            var fileName = $"Raport_{reportType}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", fileName);
        }
    }
}
