using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.EntityFrameworkCore;
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

            var incomes = _context.Incomes.AsQueryable();
            var expenses = _context.Expenses.AsQueryable();

            if (reportType == "all" || reportType == "incomes")
            {
                incomes = incomes
                    .Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
                    .Where(i => !categoryId.HasValue || i.CategoryId == categoryId);
            }

            if (reportType == "all" || reportType == "expenses")
            {
                expenses = expenses
                    .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                    .Where(e => !categoryId.HasValue || e.CategoryId == categoryId);
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

        // Metoda do generowania CSV
        [HttpGet]
        public async Task<IActionResult> ExportToCSV(DateTime startDate, DateTime endDate, string reportType, int? categoryId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var incomes = _context.Incomes.AsQueryable();
            var expenses = _context.Expenses.AsQueryable();

            if (reportType == "all" || reportType == "incomes")
            {
                incomes = incomes.Where(i => i.UserId == userId && i.Date >= startDate && i.Date <= endDate)
                                 .Where(i => !categoryId.HasValue || i.CategoryId == categoryId);
            }

            if (reportType == "all" || reportType == "expenses")
            {
                expenses = expenses.Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                                   .Where(e => !categoryId.HasValue || e.CategoryId == categoryId);
            }

            var incomeList = await incomes.ToListAsync();
            var expenseList = await expenses.ToListAsync();

            // Generowanie pliku CSV
            var csv = new StringBuilder();
            csv.AppendLine("Kategoria, Kwota, Data");

            foreach (var income in incomeList)
            {
                csv.AppendLine($"{income.Category.Name}, {income.Amount}, {income.Date:yyyy-MM-dd}");
            }

            foreach (var expense in expenseList)
            {
                csv.AppendLine($"{expense.Category.Name}, {expense.Amount}, {expense.Date:yyyy-MM-dd}");
            }

            var fileName = "raport_finansowy.csv";
            var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());

            // Zwracanie pliku CSV do pobrania
            return File(fileBytes, "text/csv", fileName);
        }
    }
}
