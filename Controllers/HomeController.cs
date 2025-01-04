using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaFinansami.Data;

namespace SystemZarzadzaniaFinansami.Controllers
{
    /// <summary>
    /// Kontroler zarz¹dzaj¹cy widokiem g³ównym aplikacji.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Inicjalizuje now¹ instancjê kontrolera HomeController.
        /// </summary>
        /// <param name="context">Kontekst bazy danych.</param>
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Wyœwietla g³ówny widok aplikacji. 
        /// Dla zalogowanego u¿ytkownika wyœwietla podsumowanie finansowe bie¿¹cego miesi¹ca.
        /// </summary>
        /// <returns>Widok g³ówny aplikacji.</returns>
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // Pobranie przychodów i wydatków dla bie¿¹cego miesi¹ca
                var incomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                    .SumAsync(i => i.Amount);

                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .SumAsync(e => e.Amount);

                // Obliczenie bilansu
                var balance = incomes - expenses;

                var summary = new
                {
                    CurrentMonth = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DateTime.Now.ToString("MMMM yyyy").ToLower()),
                    Incomes = incomes,
                    Expenses = expenses,
                    Balance = balance,
                    BalanceColor = balance >= 0 ? "green" : "red"
                };

                return View(summary);
            }

            // Widok dla niezalogowanych u¿ytkowników
            return View("Guest");
        }

        /// <summary>
        /// Wyœwietla widok dla niezalogowanych u¿ytkowników.
        /// </summary>
        /// <returns>Widok strony dla goœci.</returns>
        public IActionResult Guest()
        {
            return View();
        }

        /// <summary>
        /// Generuje wykres s³upkowy przedstawiaj¹cy przychody i wydatki za bie¿¹cy miesi¹c.
        /// </summary>
        /// <returns>Plik obrazu PNG z wykresem s³upkowym.</returns>
        [Authorize]
        public IActionResult GenerateChart()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Pobranie przychodów i wydatków
            var incomes = _context.Incomes
                .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                .Sum(i => i.Amount);

            var expenses = _context.Expenses
                .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                .Sum(e => e.Amount);

            using (var bitmap = new Bitmap(600, 460))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                // Obliczenie wysokoœci s³upków i skali
                var maxValue = Math.Max(incomes, expenses);
                var step = 500;
                var adjustedMaxValue = maxValue > 0 ? Math.Ceiling((decimal)maxValue / step) * step : step;

                var barWidth = 100;
                var barSpacing = 200;

                var incomeHeight = adjustedMaxValue > 0 ? (int)((incomes / adjustedMaxValue) * 300) : 0;
                var expenseHeight = adjustedMaxValue > 0 ? (int)((expenses / adjustedMaxValue) * 300) : 0;

                // Rysowanie s³upków
                graphics.FillRectangle(Brushes.Green, 100, 400 - incomeHeight, barWidth, incomeHeight);
                graphics.FillRectangle(Brushes.Red, 100 + barSpacing, 400 - expenseHeight, barWidth, expenseHeight);

                // Dodanie wartoœci liczbowych
                graphics.DrawString($"{incomes} z³", new Font("Arial", 12), Brushes.Black, 100, 400 - incomeHeight - 20);
                graphics.DrawString($"{expenses} z³", new Font("Arial", 12), Brushes.Black, 100 + barSpacing, 400 - expenseHeight - 20);

                // Oœ X
                graphics.DrawLine(Pens.Black, 50, 400, 550, 400);
                graphics.DrawString("Przychody", new Font("Arial", 12), Brushes.Black, 100, 410);
                graphics.DrawString("Wydatki", new Font("Arial", 12), Brushes.Black, 100 + barSpacing, 410);

                // Miesi¹c pod osi¹ X
                var currentMonthName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DateTime.Now.ToString("MMMM yyyy").ToLower());
                graphics.DrawString(currentMonthName, new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 250, 435);

                // Oœ Y
                graphics.DrawLine(Pens.Black, 50, 50, 50, 400);
                for (int i = 0; i <= adjustedMaxValue; i += step)
                {
                    var y = 400 - (int)((i / (double)adjustedMaxValue) * 300);
                    if (y < 50 || y > 400) continue;

                    graphics.DrawString(i.ToString(), new Font("Arial", 10), Brushes.Black, 10, y - 5);
                    graphics.DrawLine(Pens.Gray, 50, y, 550, y);
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
