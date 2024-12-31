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

        // GET: Reports1/Index
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;
            return View("~/Views/Reports1/Index.cshtml");
        }

        // POST: Reports1/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(DateTime? startDate, DateTime? endDate, string reportType)
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

            var incomes = Enumerable.Empty<Income>();
            var expenses = Enumerable.Empty<Expense>();

            if (reportType == "all" || reportType == "incomes")
            {
                incomes = await _context.Incomes
                    .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
                    .Include(i => i.Category)
                    .ToListAsync();
            }

            if (reportType == "all" || reportType == "expenses")
            {
                expenses = await _context.Expenses
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                    .Include(e => e.Category)
                    .ToListAsync();
            }

            var incomeTotal = incomes.Sum(i => i.Amount);
            var expenseTotal = expenses.Sum(e => e.Amount);
            var balance = incomeTotal - expenseTotal;

            var report = new
            {
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
    }
}
