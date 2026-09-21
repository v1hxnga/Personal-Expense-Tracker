namespace Personal_Expense_Tracker.Models;

public sealed class DashboardStatistics
{
    public decimal TotalIncome { get; init; }

    public decimal TotalExpenses { get; init; }

    public decimal Balance { get; init; }

    public int TotalTransactions { get; init; }

    public int TotalCategories { get; init; }

    public decimal ThisMonthIncome { get; init; }

    public decimal ThisMonthExpenses { get; init; }

    public decimal LargestExpense { get; init; }

    public decimal AverageExpense { get; init; }

    public decimal ThisMonthNet =>
        ThisMonthIncome - ThisMonthExpenses;
}