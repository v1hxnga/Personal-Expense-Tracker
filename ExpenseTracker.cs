public class ExpenseTracker
{
    private List<Transaction> transactions = new List<Transaction>();

    private FileManager fileManager =
        new FileManager("transactions.json");

    public void AddTransaction(Transaction transaction)
    {
        transactions.Add(transaction);
    }

    public void ViewTransactions()
    {
        if (transactions.Count == 0)
        {
            Console.WriteLine("No transactions recorded.");
            return;
        }

        Console.WriteLine("\n--- Transaction History ---");

        foreach (Transaction transaction in transactions)
        {
            Console.WriteLine(
                $"{transaction.Date:yyyy-MM-dd} | " +
                $"{transaction.Type} | " +
                $"{transaction.Category} | " +
                $"LKR {transaction.Amount:F2} | " +
                $"{transaction.Description}"
            );
        }
    }

    public decimal GetTotalIncome()
    {
        return transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);
    }

    public decimal GetTotalExpenses()
    {
        return transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);
    }

    public decimal GetBalance()
    {
        return GetTotalIncome() - GetTotalExpenses();
    }

    public void SaveData()
    {
        fileManager.Save(transactions);
        Console.WriteLine("Transaction data saved successfully.");
    }

    public void LoadData()
    {
        transactions = fileManager.Load();
    }
}