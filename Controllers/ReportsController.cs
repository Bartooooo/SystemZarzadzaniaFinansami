﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
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

            var incomesQuery = _context.Incomes.Include(i => i.Category).Where(i => i.UserId == userId);
            var expensesQuery = _context.Expenses.Include(e => e.Category).Where(e => e.UserId == userId);

            if (categoryId.HasValue)
            {
                incomesQuery = incomesQuery.Where(i => i.CategoryId == categoryId);
                expensesQuery = expensesQuery.Where(e => e.CategoryId == categoryId);
            }

            if (reportType == "incomes")
            {
                expensesQuery = _context.Expenses.Where(e => e.Id == 0); // Zapytanie, które zawsze zwraca pusty wynik
            }
            else if (reportType == "expenses")
            {
                incomesQuery = _context.Incomes.Where(i => i.Id == 0); // Zapytanie, które zawsze zwraca pusty wynik
            }

            var incomes = await incomesQuery.Where(i => i.Date >= startDate && i.Date <= endDate).ToListAsync();
            var expenses = await expensesQuery.Where(e => e.Date >= startDate && e.Date <= endDate).ToListAsync();

            var incomeTotal = incomes.Sum(i => i.Amount);
            var expenseTotal = expenses.Sum(e => e.Amount);
            var balance = incomeTotal - expenseTotal;

            var selectedCategory = "Wszystkie kategorie";
            if (categoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(categoryId);
                selectedCategory = category != null ? category.Name : "Nieznana kategoria";
            }

            var report = new
            {
                StartDate = startDate.Value.ToString("yyyy-MM-dd"),
                EndDate = endDate.Value.ToString("yyyy-MM-dd"),
                ReportType = reportType,
                Incomes = incomes,
                Expenses = expenses,
                IncomeTotal = incomeTotal,
                ExpenseTotal = expenseTotal,
                Balance = balance,
                SelectedCategory = selectedCategory
            };

            return View("~/Views/Reports1/Report.cshtml", report);
        }
    }
}
