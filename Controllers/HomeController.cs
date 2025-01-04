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
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index
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

                // Przychody za aktualny miesi¹c
                var incomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date.Month == currentMonth && i.Date.Year == currentYear)
                    .SumAsync(i => i.Amount);

                // Wydatki za aktualny miesi¹c
                var expenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date.Month == currentMonth && e.Date.Year == currentYear)
                    .SumAsync(e => e.Amount);

                // Bilans
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

        // GET: Home/Guest
        public IActionResult Guest()
        {
            return View();
        }

        // GET: Home/GenerateChart
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
