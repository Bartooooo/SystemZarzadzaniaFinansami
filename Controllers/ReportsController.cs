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
            var categories = _context.Categories.ToList();
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

            // Filtrowanie raportu na podstawie wybranego typu
            var incomes = _context.Incomes.AsQueryable();
            var expenses = _context.Expenses.AsQueryable();

            // Filtrowanie po typie raportu
            if (reportType == "all" || reportType == "incomes")
            {
                incomes = incomes
                    .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
                    .Where(i => !categoryId.HasValue || i.CategoryId == categoryId) // Filtr po kategorii
                    .Include(i => i.Category);
            }

            if (reportType == "all" || reportType == "expenses")
            {
                expenses = expenses
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                    .Where(e => !categoryId.HasValue || e.CategoryId == categoryId) // Filtr po kategorii
                    .Include(e => e.Category);
            }

            var incomeList = await incomes.ToListAsync();
            var expenseList = await expenses.ToListAsync();

            var incomeTotal = incomeList.Sum(i => i.Amount);
            var expenseTotal = expenseList.Sum(e => e.Amount);
            var balance = incomeTotal - expenseTotal;

            var report = new
            {
                StartDate = startDate.Value.ToString("yyyy-MM-dd"),
                EndDate = endDate.Value.ToString("yyyy-MM-dd"),
                ReportType = reportType,
                Incomes = incomeList,
                Expenses = expenseList,
                IncomeTotal = incomeTotal,
                ExpenseTotal = expenseTotal,
                Balance = balance,
                SelectedCategory = categoryId.HasValue ? _context.Categories.Find(categoryId).Name : "Wszystkie kategorie"
            };

            return View("~/Views/Reports1/Report.cshtml", report);
        }
    }
}
