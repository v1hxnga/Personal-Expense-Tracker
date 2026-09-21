using Personal_Expense_Tracker.Models;

namespace Personal_Expense_Tracker.Services;

public class ExpenseTrackerService
{
    private readonly FileManager _fileManager;

    /*
     * A web application can receive multiple requests.
     * This lock protects the in-memory transaction collection.
     */
    private readonly object _syncRoot = new();

    /*
     * SemaphoreSlim allows asynchronous write operations
     * to be performed one at a time.
     */
    private readonly SemaphoreSlim _writeLock =
        new(1, 1);

    private List<Transaction> _transactions =
        new();


    public static IReadOnlyList<string> IncomeCategories { get; } =
        new List<string>
        {
            "Salary",
            "Freelance",
            "Business",
            "Rental",
            "Gift"
        };


    public static IReadOnlyList<string> ExpenseCategories { get; } =
        new List<string>
        {
            "Food",
            "Transport",
            "Bills",
            "Shopping",
            "Entertainment",
            "Education"
        };


    public ExpenseTrackerService(
        FileManager fileManager)
    {
        _fileManager = fileManager;
    }


    /*
     * Loads existing JSON data when the application starts.
     */
    public async Task InitializeAsync()
    {
        List<Transaction> loadedTransactions =
            await _fileManager.LoadAsync();

        bool dataChanged = false;

        HashSet<Guid> usedIds =
            new();

        /*
         * Older transactions created before IDs were added
         * may contain Guid.Empty. They are repaired here.
         */
        foreach (Transaction transaction in loadedTransactions)
        {
            if (transaction.Id == Guid.Empty ||
                !usedIds.Add(transaction.Id))
            {
                transaction.Id =
                    Guid.NewGuid();

                usedIds.Add(transaction.Id);

                dataChanged = true;
            }
        }

        lock (_syncRoot)
        {
            _transactions =
                loadedTransactions;
        }

        if (dataChanged)
        {
            await _fileManager.SaveAsync(
                CreateSnapshot()
            );
        }
    }


    /*
     * Adds a new income or expense transaction.
     */
    public async Task AddTransactionAsync(
        decimal amount,
        DateTime date,
        string category,
        string description,
        TransactionType type)
    {
        ValidateTransaction(
            amount,
            date,
            category,
            description,
            type
        );

        Transaction transaction =
            new Transaction(
                amount,
                date.Date,
                category.Trim(),
                description.Trim(),
                type
            );

        await _writeLock.WaitAsync();

        try
        {
            List<Transaction> snapshot;

            lock (_syncRoot)
            {
                _transactions.Add(
                    transaction
                );

                snapshot =
                    CreateSnapshotUnsafe();
            }

            await _fileManager.SaveAsync(
                snapshot
            );
        }
        finally
        {
            _writeLock.Release();
        }
    }


    /*
     * Updates an existing transaction.
     */
    public async Task<bool> UpdateTransactionAsync(
        Guid id,
        decimal amount,
        DateTime date,
        string category,
        string description,
        TransactionType type)
    {
        ValidateTransaction(
            amount,
            date,
            category,
            description,
            type
        );

        await _writeLock.WaitAsync();

        try
        {
            List<Transaction> snapshot;

            lock (_syncRoot)
            {
                Transaction? transaction =
                    _transactions
                        .FirstOrDefault(
                            item =>
                                item.Id == id
                        );

                if (transaction == null)
                {
                    return false;
                }

                transaction.Amount =
                    amount;

                transaction.Date =
                    date.Date;

                transaction.Category =
                    category.Trim();

                transaction.Description =
                    description.Trim();

                transaction.Type =
                    type;

                snapshot =
                    CreateSnapshotUnsafe();
            }

            await _fileManager.SaveAsync(
                snapshot
            );

            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }


    /*
     * Deletes a transaction by its unique ID.
     */
    public async Task<bool> DeleteTransactionAsync(
        Guid id)
    {
        await _writeLock.WaitAsync();

        try
        {
            List<Transaction> snapshot;

            lock (_syncRoot)
            {
                Transaction? transaction =
                    _transactions
                        .FirstOrDefault(
                            item =>
                                item.Id == id
                        );

                if (transaction == null)
                {
                    return false;
                }

                _transactions.Remove(
                    transaction
                );

                snapshot =
                    CreateSnapshotUnsafe();
            }

            await _fileManager.SaveAsync(
                snapshot
            );

            return true;
        }
        finally
        {
            _writeLock.Release();
        }
    }


    /*
     * Returns one transaction for editing.
     */
    public Transaction? GetTransactionById(
        Guid id)
    {
        lock (_syncRoot)
        {
            Transaction? transaction =
                _transactions
                    .FirstOrDefault(
                        item =>
                            item.Id == id
                    );

            return transaction == null
                ? null
                : CloneTransaction(
                    transaction
                );
        }
    }


    /*
     * Search and filter transaction history.
     */
    public IReadOnlyList<Transaction>
        GetFilteredTransactions(
            string? searchText,
            TransactionType? type,
            string? category,
            DateTime? month)
    {
        IEnumerable<Transaction> query =
            CreateSnapshot();


        if (!string.IsNullOrWhiteSpace(
                searchText))
        {
            string search =
                searchText.Trim();

            query =
                query.Where(
                    transaction =>
                        transaction.Category.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase
                        )
                        ||
                        transaction.Description.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }


        if (type.HasValue)
        {
            query =
                query.Where(
                    transaction =>
                        transaction.Type ==
                        type.Value
                );
        }


        if (!string.IsNullOrWhiteSpace(
                category))
        {
            query =
                query.Where(
                    transaction =>
                        string.Equals(
                            transaction.Category,
                            category,
                            StringComparison.OrdinalIgnoreCase
                        )
                );
        }


        if (month.HasValue)
        {
            query =
                query.Where(
                    transaction =>
                        transaction.Date.Year ==
                        month.Value.Year
                        &&
                        transaction.Date.Month ==
                        month.Value.Month
                );
        }


        return query
            .OrderByDescending(
                transaction =>
                    transaction.Date
            )
            .ThenByDescending(
                transaction =>
                    transaction.Id
            )
            .ToList()
            .AsReadOnly();
    }


    /*
     * Returns all categories for the filter dropdown.
     */
    public IReadOnlyList<string>
        GetAllCategories()
    {
        IEnumerable<string> standardCategories =
            IncomeCategories.Concat(
                ExpenseCategories
            );

        IEnumerable<string> storedCategories =
            CreateSnapshot()
                .Select(
                    transaction =>
                        transaction.Category
                )
                .Where(
                    category =>
                        !string.IsNullOrWhiteSpace(
                            category
                        )
                );

        return standardCategories
            .Concat(
                storedCategories
            )
            .Distinct(
                StringComparer.OrdinalIgnoreCase
            )
            .OrderBy(
                category =>
                    category
            )
            .ToList()
            .AsReadOnly();
    }


    /*
     * Calculates the main dashboard statistics.
     */
    public DashboardStatistics
        GetDashboardStatistics()
    {
        List<Transaction> transactions =
            CreateSnapshot();

        DateTime today =
            DateTime.Today;

        List<Transaction> expenses =
            transactions
                .Where(
                    transaction =>
                        transaction.Type ==
                        TransactionType.Expense
                )
                .ToList();


        decimal totalIncome =
            transactions
                .Where(
                    transaction =>
                        transaction.Type ==
                        TransactionType.Income
                )
                .Sum(
                    transaction =>
                        transaction.Amount
                );


        decimal totalExpenses =
            expenses.Sum(
                transaction =>
                    transaction.Amount
            );


        decimal thisMonthIncome =
            transactions
                .Where(
                    transaction =>
                        transaction.Type ==
                        TransactionType.Income
                        &&
                        transaction.Date.Year ==
                        today.Year
                        &&
                        transaction.Date.Month ==
                        today.Month
                )
                .Sum(
                    transaction =>
                        transaction.Amount
                );


        decimal thisMonthExpenses =
            expenses
                .Where(
                    transaction =>
                        transaction.Date.Year ==
                        today.Year
                        &&
                        transaction.Date.Month ==
                        today.Month
                )
                .Sum(
                    transaction =>
                        transaction.Amount
                );


        decimal largestExpense =
            expenses.Count == 0
                ? 0
                : expenses.Max(
                    transaction =>
                        transaction.Amount
                );


        decimal averageExpense =
            expenses.Count == 0
                ? 0
                : expenses.Average(
                    transaction =>
                        transaction.Amount
                );


        int totalCategories =
            transactions
                .Select(
                    transaction =>
                        transaction.Category
                )
                .Where(
                    category =>
                        !string.IsNullOrWhiteSpace(
                            category
                        )
                )
                .Distinct(
                    StringComparer.OrdinalIgnoreCase
                )
                .Count();


        return new DashboardStatistics
        {
            TotalIncome =
                totalIncome,

            TotalExpenses =
                totalExpenses,

            Balance =
                totalIncome -
                totalExpenses,

            TotalTransactions =
                transactions.Count,

            TotalCategories =
                totalCategories,

            ThisMonthIncome =
                thisMonthIncome,

            ThisMonthExpenses =
                thisMonthExpenses,

            LargestExpense =
                largestExpense,

            AverageExpense =
                averageExpense
        };
    }


    /*
     * Produces a summary for the most recent months.
     */
    public IReadOnlyList<MonthlySummary>
        GetMonthlySummaries(
            int numberOfMonths = 6)
    {
        if (numberOfMonths < 1)
        {
            numberOfMonths = 6;
        }

        List<Transaction> transactions =
            CreateSnapshot();

        DateTime currentMonth =
            new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1
            );

        List<MonthlySummary> summaries =
            new();


        for (int offset =
                 numberOfMonths - 1;
             offset >= 0;
             offset--)
        {
            DateTime month =
                currentMonth.AddMonths(
                    -offset
                );


            List<Transaction> monthTransactions =
                transactions
                    .Where(
                        transaction =>
                            transaction.Date.Year ==
                            month.Year
                            &&
                            transaction.Date.Month ==
                            month.Month
                    )
                    .ToList();


            decimal income =
                monthTransactions
                    .Where(
                        transaction =>
                            transaction.Type ==
                            TransactionType.Income
                    )
                    .Sum(
                        transaction =>
                            transaction.Amount
                    );


            decimal expenses =
                monthTransactions
                    .Where(
                        transaction =>
                            transaction.Type ==
                            TransactionType.Expense
                    )
                    .Sum(
                        transaction =>
                            transaction.Amount
                    );


            summaries.Add(
                new MonthlySummary
                {
                    MonthLabel =
                        month.ToString(
                            "MMM yyyy"
                        ),

                    Income =
                        income,

                    Expenses =
                        expenses
                }
            );
        }


        return summaries
            .AsReadOnly();
    }


    /*
     * Calculates expense totals grouped by category.
     */
    public IReadOnlyList<CategorySummary>
        GetExpenseCategorySummaries()
    {
        List<Transaction> expenses =
            CreateSnapshot()
                .Where(
                    transaction =>
                        transaction.Type ==
                        TransactionType.Expense
                )
                .ToList();


        decimal totalExpenses =
            expenses.Sum(
                transaction =>
                    transaction.Amount
            );


        if (totalExpenses == 0)
        {
            return new List<CategorySummary>()
                .AsReadOnly();
        }


        return expenses
            .GroupBy(
                transaction =>
                    transaction.Category,
                StringComparer.OrdinalIgnoreCase
            )
            .Select(
                group =>
                    new CategorySummary
                    {
                        Category =
                            group.First().Category,

                        Total =
                            group.Sum(
                                transaction =>
                                    transaction.Amount
                            ),

                        TransactionCount =
                            group.Count(),

                        Percentage =
                            group.Sum(
                                transaction =>
                                    transaction.Amount
                            )
                            /
                            totalExpenses
                            *
                            100
                    }
            )
            .OrderByDescending(
                summary =>
                    summary.Total
            )
            .ToList()
            .AsReadOnly();
    }


    /*
     * Validates data before it is added or edited.
     */
    private static void ValidateTransaction(
        decimal amount,
        DateTime date,
        string category,
        string description,
        TransactionType type)
    {
        if (amount <= 0)
        {
            throw new ArgumentException(
                "Amount must be greater than zero."
            );
        }


        if (date == default)
        {
            throw new ArgumentException(
                "A valid transaction date is required."
            );
        }


        if (date.Date >
            DateTime.Today)
        {
            throw new ArgumentException(
                "Transaction date cannot be in the future."
            );
        }


        if (string.IsNullOrWhiteSpace(
                category))
        {
            throw new ArgumentException(
                "A category is required."
            );
        }


        if (category.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Category must contain 50 characters or fewer."
            );
        }


        if (string.Equals(
                category.Trim(),
                "Other",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Please enter a custom category when Other is selected."
            );
        }


        if (string.IsNullOrWhiteSpace(
                description))
        {
            throw new ArgumentException(
                "Description cannot be empty."
            );
        }


        if (description.Trim().Length > 200)
        {
            throw new ArgumentException(
                "Description must contain 200 characters or fewer."
            );
        }


        /*
         * Prevent an expense-only built-in category from being
         * accidentally stored as income, and vice versa.
         */
        if (type ==
                TransactionType.Income
            &&
            ExpenseCategories.Contains(
                category.Trim(),
                StringComparer.OrdinalIgnoreCase
            ))
        {
            throw new ArgumentException(
                "The selected category belongs to an expense transaction."
            );
        }


        if (type ==
                TransactionType.Expense
            &&
            IncomeCategories.Contains(
                category.Trim(),
                StringComparer.OrdinalIgnoreCase
            ))
        {
            throw new ArgumentException(
                "The selected category belongs to an income transaction."
            );
        }
    }


    /*
     * Creates a safe copy of the transaction collection.
     */
    private List<Transaction>
        CreateSnapshot()
    {
        lock (_syncRoot)
        {
            return CreateSnapshotUnsafe();
        }
    }


    private List<Transaction>
        CreateSnapshotUnsafe()
    {
        return _transactions
            .Select(
                CloneTransaction
            )
            .ToList();
    }


    private static Transaction
        CloneTransaction(
            Transaction transaction)
    {
        return new Transaction
        {
            Id =
                transaction.Id,

            Amount =
                transaction.Amount,

            Date =
                transaction.Date,

            Category =
                transaction.Category,

            Description =
                transaction.Description,

            Type =
                transaction.Type
        };
    }
}