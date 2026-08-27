public class Transaction
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public TransactionType Type { get; set; }

    public Transaction(
        decimal amount,
        DateTime date,
        string category,
        string description,
        TransactionType type)
    {
        Amount = amount;
        Date = date;
        Category = category;
        Description = description;
        Type = type;
    }
}