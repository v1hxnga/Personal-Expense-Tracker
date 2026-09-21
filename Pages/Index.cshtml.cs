using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Personal_Expense_Tracker.Models;
using Personal_Expense_Tracker.Services;

namespace Personal_Expense_Tracker.Pages;

public class IndexModel : PageModel
{
    private readonly ExpenseTrackerService _tracker;

    public IndexModel(ExpenseTrackerService tracker)
    {
        _tracker = tracker;
    }

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string Category { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    public decimal TotalIncome { get; private set; }

    public decimal TotalExpenses { get; private set; }

    public decimal Balance { get; private set; }

    public IReadOnlyList<Transaction> Transactions { get; private set; }
        = new List<Transaction>();

    public string? SuccessMessage { get; private set; }

    public string? ErrorMessage { get; private set; }


    public void OnGet()
    {
        LoadDashboard();
    }


    public async Task<IActionResult> OnPostIncomeAsync()
    {
        try
        {
            await _tracker.AddTransactionAsync(
                Amount,
                Category,
                Description,
                TransactionType.Income
            );

            TempData["SuccessMessage"] =
                "Income transaction added successfully.";

            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;

            LoadDashboard();

            return Page();
        }
    }


    public async Task<IActionResult> OnPostExpenseAsync()
    {
        try
        {
            await _tracker.AddTransactionAsync(
                Amount,
                Category,
                Description,
                TransactionType.Expense
            );

            TempData["SuccessMessage"] =
                "Expense transaction added successfully.";

            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;

            LoadDashboard();

            return Page();
        }
    }


    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        bool deleted =
            await _tracker.DeleteTransactionAsync(id);

        TempData["SuccessMessage"] = deleted
            ? "Transaction deleted successfully."
            : "Transaction could not be found.";

        return RedirectToPage();
    }


    private void LoadDashboard()
    {
        TotalIncome = _tracker.GetTotalIncome();

        TotalExpenses = _tracker.GetTotalExpenses();

        Balance = _tracker.GetBalance();

        Transactions = _tracker.GetTransactions();

        SuccessMessage =
            TempData["SuccessMessage"]?.ToString();
    }
}