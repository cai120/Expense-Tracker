using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Views.Category
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions.Include(a => a.Category).Where(y => y.Date >= StartDate && y.Date <= EndDate).ToListAsync();


            int TotalIncome = SelectedTransactions.Where(i => i.Category.Type == "Income").Sum(a => a.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            int TotalExpense = SelectedTransactions.Where(i => i.Category.Type == "Expense").Sum(a => a.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = Balance.ToString("C0");

            ViewBag.DoughnutChartData = SelectedTransactions.Where(i => i.Category.Type == "Expense").GroupBy(a => a.Category.CategoryId).Select(b => new
            {
                categoryTitleWithIcon = b.First().Category.Icon + " " + b.First().Category.Title,
                amount = b.Sum(c => c.Amount),
                formattedAmount = b.Sum(c => c.Amount).ToString("C0")
            }).OrderByDescending(a => a.amount).ToList();

            List<SplineChartData> IncomeSummary = SelectedTransactions.Where(a => a.Category.Type == "Income")
                .GroupBy(a => a.Date)
                .Select(a => new SplineChartData()
                {
                    day = a.First().Date.ToString("dd-MMM"),
                    income = a.Sum(b => b.Amount)
                }).ToList();

            List<SplineChartData> ExpenseSummary = SelectedTransactions.Where(a => a.Category.Type == "Expense")
                .GroupBy(a => a.Date)
                .Select(a => new SplineChartData()
                {
                    day = a.First().Date.ToString("dd-MMM"),
                    income = a.Sum(b => b.Amount)
                }).ToList();


            string[] last7Days = Enumerable.Range(0, 7).Select(a => StartDate.AddDays(a).ToString("dd-MMM")).ToArray();

            ViewBag.SplineChartData = from day in last7Days
                                      join income in IncomeSummary on day equals income.day
                                      into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };

            ViewBag.RecentTransactions = await _context.Transactions.Include(a => a.Category).OrderByDescending(a => a.Date).Take(5).ToListAsync();

            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
