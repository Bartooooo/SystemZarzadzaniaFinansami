using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemZarzadzaniaFinansami.Data;
using SystemZarzadzaniaFinansami.Models;

namespace SystemZarzadzaniaFinansami.Controllers
{
    [Authorize]
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
    }
}
