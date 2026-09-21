namespace Personal_Expense_Tracker.Models;

public sealed class MonthlySummary
{
    public string MonthLabel { get; init; } = string.Empty;

    public decimal Income { get; init; }

    public decimal Expenses { get; init; }

    public decimal Net =>
        Income - Expenses;
}