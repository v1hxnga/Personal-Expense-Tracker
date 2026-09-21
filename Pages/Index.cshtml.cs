using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Personal_Expense_Tracker.Models;
using Personal_Expense_Tracker.Services;

namespace Personal_Expense_Tracker.Pages;

public class IndexModel : PageModel
{
    private readonly ExpenseTrackerService _tracker;


    public IndexModel(
        ExpenseTrackerService tracker)
    {
        _tracker = tracker;
    }


    /*
     * ADD TRANSACTION FORM
     */

    [BindProperty]
    public TransactionType AddType { get; set; } =
        TransactionType.Expense;


    [BindProperty]
    public decimal? Amount { get; set; }


    [BindProperty]
    public DateTime? TransactionDate { get; set; } =
        DateTime.Today;


    [BindProperty]
    public string CategoryChoice { get; set; } =
        string.Empty;


    [BindProperty]
    public string CustomCategory { get; set; } =
        string.Empty;


    [BindProperty]
    public string Description { get; set; } =
        string.Empty;


    /*
     * SEARCH AND FILTERS
     */

    [BindProperty(SupportsGet = true)]
    public string SearchText { get; set; } =
        string.Empty;


    [BindProperty(SupportsGet = true)]
    public string FilterType { get; set; } =
        "All";


    [BindProperty(SupportsGet = true)]
    public string FilterCategory { get; set; } =
        "All";


    [BindProperty(SupportsGet = true)]
    public string FilterMonth { get; set; } =
        string.Empty;


    /*
     * EDIT TRANSACTION FORM
     */

    [BindProperty]
    public Guid EditId { get; set; }


    [BindProperty]
    public TransactionType EditType { get; set; }


    [BindProperty]
    public decimal? EditAmount { get; set; }


    [BindProperty]
    public DateTime? EditDate { get; set; }


    [BindProperty]
    public string EditCategoryChoice { get; set; } =
        string.Empty;


    [BindProperty]
    public string EditCustomCategory { get; set; } =
        string.Empty;


    [BindProperty]
    public string EditDescription { get; set; } =
        string.Empty;


    /*
     * DATA DISPLAYED ON THE DASHBOARD
     */

    public IReadOnlyList<Transaction> Transactions
    {
        get;
        private set;
    } =
        new List<Transaction>();


    public DashboardStatistics Statistics
    {
        get;
        private set;
    } =
        new();


    public IReadOnlyList<MonthlySummary> MonthlySummaries
    {
        get;
        private set;
    } =
        new List<MonthlySummary>();


    public IReadOnlyList<CategorySummary> ExpenseCategorySummaries
    {
        get;
        private set;
    } =
        new List<CategorySummary>();


    public IReadOnlyList<string> AvailableCategories
    {
        get;
        private set;
    } =
        new List<string>();


    public IReadOnlyList<string> IncomeCategories =>
        ExpenseTrackerService.IncomeCategories;


    public IReadOnlyList<string> ExpenseCategories =>
        ExpenseTrackerService.ExpenseCategories;


    public bool IsEditing
    {
        get;
        private set;
    }


    public string? SuccessMessage
    {
        get;
        private set;
    }


    public string? ErrorMessage
    {
        get;
        private set;
    }


    public List<string> ValidationMessages
    {
        get;
        private set;
    } =
        new();


    /*
     * Loads the dashboard.
     */
    public void OnGet(
        Guid? editId)
    {
        LoadFlashMessages();

        LoadDashboard();

        if (editId.HasValue)
        {
            LoadEditForm(
                editId.Value
            );
        }
    }


    /*
     * ADD TRANSACTION
     */
    public async Task<IActionResult>
        OnPostAddAsync()
    {
        ValidationMessages =
            new List<string>();


        ValidateCommonInput(
            Amount,
            TransactionDate,
            Description,
            ValidationMessages
        );


        string? finalCategory =
            ResolveCategory(
                AddType,
                CategoryChoice,
                CustomCategory,
                ValidationMessages
            );


        if (ValidationMessages.Count > 0)
        {
            LoadDashboard();

            return Page();
        }


        try
        {
            await _tracker
                .AddTransactionAsync(
                    Amount!.Value,
                    TransactionDate!.Value,
                    finalCategory!,
                    Description,
                    AddType
                );


            TempData["SuccessMessage"] =
                $"{AddType} transaction added successfully.";


            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage =
                ex.Message;

            LoadDashboard();

            return Page();
        }
    }


    /*
     * EDIT TRANSACTION
     */
    public async Task<IActionResult>
        OnPostEditAsync()
    {
        ValidationMessages =
            new List<string>();


        ValidateCommonInput(
            EditAmount,
            EditDate,
            EditDescription,
            ValidationMessages
        );


        string? finalCategory =
            ResolveCategory(
                EditType,
                EditCategoryChoice,
                EditCustomCategory,
                ValidationMessages
            );


        if (ValidationMessages.Count > 0)
        {
            IsEditing =
                true;

            LoadDashboard();

            return Page();
        }


        try
        {
            bool updated =
                await _tracker
                    .UpdateTransactionAsync(
                        EditId,
                        EditAmount!.Value,
                        EditDate!.Value,
                        finalCategory!,
                        EditDescription,
                        EditType
                    );


            if (!updated)
            {
                TempData["ErrorMessage"] =
                    "The transaction could not be found.";

                return RedirectToPage();
            }


            TempData["SuccessMessage"] =
                "Transaction updated successfully.";


            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage =
                ex.Message;

            IsEditing =
                true;

            LoadDashboard();

            return Page();
        }
    }


    /*
     * DELETE TRANSACTION
     */
    public async Task<IActionResult>
        OnPostDeleteAsync(
            Guid id)
    {
        bool deleted =
            await _tracker
                .DeleteTransactionAsync(
                    id
                );


        if (deleted)
        {
            TempData["SuccessMessage"] =
                "Transaction deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "Transaction could not be found.";
        }


        return RedirectToPage();
    }


    /*
     * Loads totals, filters and analytics.
     */
    private void LoadDashboard()
    {
        TransactionType? selectedType =
            null;


        if (!string.Equals(
                FilterType,
                "All",
                StringComparison.OrdinalIgnoreCase)
            &&
            Enum.TryParse(
                FilterType,
                true,
                out TransactionType parsedType))
        {
            selectedType =
                parsedType;
        }


        string? selectedCategory =
            string.Equals(
                FilterCategory,
                "All",
                StringComparison.OrdinalIgnoreCase)
                ||
                string.IsNullOrWhiteSpace(
                    FilterCategory
                )
                    ? null
                    : FilterCategory;


        DateTime? selectedMonth =
            null;


        if (!string.IsNullOrWhiteSpace(
                FilterMonth)
            &&
            DateTime.TryParseExact(
                FilterMonth,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedMonth))
        {
            selectedMonth =
                parsedMonth;
        }


        Transactions =
            _tracker
                .GetFilteredTransactions(
                    SearchText,
                    selectedType,
                    selectedCategory,
                    selectedMonth
                );


        Statistics =
            _tracker
                .GetDashboardStatistics();


        MonthlySummaries =
            _tracker
                .GetMonthlySummaries(
                    6
                );


        ExpenseCategorySummaries =
            _tracker
                .GetExpenseCategorySummaries();


        AvailableCategories =
            _tracker
                .GetAllCategories();
    }


    /*
     * Loads an existing transaction into the edit form.
     */
    private void LoadEditForm(
        Guid transactionId)
    {
        Transaction? transaction =
            _tracker
                .GetTransactionById(
                    transactionId
                );


        if (transaction == null)
        {
            ErrorMessage =
                "The selected transaction could not be found.";

            return;
        }


        IsEditing =
            true;


        EditId =
            transaction.Id;


        EditType =
            transaction.Type;


        EditAmount =
            transaction.Amount;


        EditDate =
            transaction.Date;


        EditDescription =
            transaction.Description;


        IReadOnlyList<string> builtInCategories =
            transaction.Type ==
            TransactionType.Income
                ? IncomeCategories
                : ExpenseCategories;


        string? matchingCategory =
            builtInCategories
                .FirstOrDefault(
                    category =>
                        string.Equals(
                            category,
                            transaction.Category,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


        if (matchingCategory != null)
        {
            EditCategoryChoice =
                matchingCategory;

            EditCustomCategory =
                string.Empty;
        }
        else
        {
            EditCategoryChoice =
                "Other";

            EditCustomCategory =
                transaction.Category;
        }
    }


    /*
     * Common validation for add/edit forms.
     */
    private static void ValidateCommonInput(
        decimal? amount,
        DateTime? date,
        string description,
        List<string> validationMessages)
    {
        if (!amount.HasValue ||
            amount.Value <= 0)
        {
            validationMessages.Add(
                "Amount must be greater than zero."
            );
        }


        if (!date.HasValue)
        {
            validationMessages.Add(
                "Please select a valid transaction date."
            );
        }
        else if (date.Value.Date >
                 DateTime.Today)
        {
            validationMessages.Add(
                "Transaction date cannot be in the future."
            );
        }


        if (string.IsNullOrWhiteSpace(
                description))
        {
            validationMessages.Add(
                "Description cannot be empty."
            );
        }
        else if (description.Trim().Length >
                 200)
        {
            validationMessages.Add(
                "Description must contain 200 characters or fewer."
            );
        }
    }


    /*
     * Converts "Other" into the custom category
     * and checks that the selected category matches
     * the transaction type.
     */
    private static string?
        ResolveCategory(
            TransactionType type,
            string categoryChoice,
            string customCategory,
            List<string> validationMessages)
    {
        if (string.IsNullOrWhiteSpace(
                categoryChoice))
        {
            validationMessages.Add(
                "Please select a category."
            );

            return null;
        }


        if (string.Equals(
                categoryChoice,
                "Other",
                StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(
                    customCategory))
            {
                validationMessages.Add(
                    "Please enter a custom category."
                );

                return null;
            }


            if (customCategory.Trim().Length >
                50)
            {
                validationMessages.Add(
                    "Custom category must contain 50 characters or fewer."
                );

                return null;
            }


            return customCategory.Trim();
        }


        IReadOnlyList<string> allowedCategories =
            type ==
            TransactionType.Income
                ? ExpenseTrackerService
                    .IncomeCategories
                : ExpenseTrackerService
                    .ExpenseCategories;


        string? matchingCategory =
            allowedCategories
                .FirstOrDefault(
                    category =>
                        string.Equals(
                            category,
                            categoryChoice,
                            StringComparison.OrdinalIgnoreCase
                        )
                );


        if (matchingCategory == null)
        {
            validationMessages.Add(
                "The selected category does not match the transaction type."
            );

            return null;
        }


        return matchingCategory;
    }


    private void LoadFlashMessages()
    {
        SuccessMessage =
            TempData["SuccessMessage"]
                ?.ToString();


        ErrorMessage =
            TempData["ErrorMessage"]
                ?.ToString();
    }


    /*
     * Used by the category progress bars.
     */
    public string PercentageWidth(
        decimal percentage)
    {
        decimal safePercentage =
            Math.Clamp(
                percentage,
                0,
                100
            );


        return safePercentage
                   .ToString(
                       "0.##",
                       CultureInfo.InvariantCulture
                   )
               +
               "%";
    }
}