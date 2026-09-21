using Personal_Expense_Tracker.Models;

namespace Personal_Expense_Tracker.Services;

public class ExpenseTrackerService
{
    private readonly FileManager _fileManager;

    private List<Transaction> _transactions = new();

    public ExpenseTrackerService(FileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public async Task InitializeAsync()
    {
        _transactions = await _fileManager.LoadAsync();
    }

    public IReadOnlyList<Transaction> GetTransactions()
    {
        return _transactions
            .OrderByDescending(transaction => transaction.Date)
            .ToList()
            .AsReadOnly();
    }

    public async Task AddTransactionAsync(
        decimal amount,
        string category,
        string description,
        TransactionType type)
    {
        if (amount <= 0)
        {
            throw new ArgumentException(
                "Transaction amount must be greater than zero."
            );
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException(
                "A category is required."
            );
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A description is required."
            );
        }

        Transaction transaction = new Transaction(
            amount,
            DateTime.Now,
            category.Trim(),
            description.Trim(),
            type
        );

        _transactions.Add(transaction);

        await _fileManager.SaveAsync(_transactions);
    }

    public async Task<bool> DeleteTransactionAsync(Guid id)
    {
        Transaction? transaction =
            _transactions.FirstOrDefault(
                transaction => transaction.Id == id
            );

        if (transaction == null)
        {
            return false;
        }

        _transactions.Remove(transaction);

        await _fileManager.SaveAsync(_transactions);

        return true;
    }

    public decimal GetTotalIncome()
    {
        return _transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount);
    }

    public decimal GetTotalExpenses()
    {
        return _transactions
            .Where(transaction =>
                transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);
    }

    public decimal GetBalance()
    {
        return GetTotalIncome() - GetTotalExpenses();
    }

    public List<Transaction> GetTransactionsByType(
        TransactionType type)
    {
        return _transactions
            .Where(transaction =>
                transaction.Type == type)
            .OrderByDescending(transaction =>
                transaction.Date)
            .ToList();
    }

    public List<Transaction> SearchTransactions(
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return _transactions
                .OrderByDescending(transaction =>
                    transaction.Date)
                .ToList();
        }

        return _transactions
            .Where(transaction =>
                transaction.Category.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase)
                ||
                transaction.Description.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(transaction =>
                transaction.Date)
            .ToList();
    }
}