namespace Personal_Expense_Tracker.Models;

public class Transaction
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public Transaction()
    {
        Id = Guid.NewGuid();
        Date = DateTime.Now;
    }

    public Transaction(
        decimal amount,
        DateTime date,
        string category,
        string description,
        TransactionType type)
    {
        Id = Guid.NewGuid();
        Amount = amount;
        Date = date;
        Category = category;
        Description = description;
        Type = type;
    }
}