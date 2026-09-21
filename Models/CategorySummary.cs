namespace Personal_Expense_Tracker.Models;

public sealed class CategorySummary
{
    public string Category { get; init; } = string.Empty;

    public decimal Total { get; init; }

    public int TransactionCount { get; init; }

    public decimal Percentage { get; init; }
}