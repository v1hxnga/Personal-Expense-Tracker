public class ExpenseTracker
{
    private List<Transaction> transactions = new List<Transaction>();

    private FileManager fileManager =
        new FileManager("transactions.json");

   public void AddTransaction(Transaction transaction)
{
    transactions.Add(transaction);

    fileManager.Save(transactions);
}

    public void ViewTransactions()
    {
        Console.WriteLine("===============================================");
        Console.WriteLine("             TRANSACTION HISTORY");
        Console.WriteLine("===============================================");
        Console.WriteLine();

        if (transactions.Count == 0)
        {
            Console.WriteLine("No transactions recorded.");
            return;
        }

        int number = 1;

        foreach (Transaction transaction in transactions)
        {
            Console.WriteLine($"Transaction #{number}");
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine($"Type        : {transaction.Type}");
            Console.WriteLine($"Amount      : LKR {transaction.Amount:N2}");
            Console.WriteLine($"Category    : {transaction.Category}");
            Console.WriteLine($"Description : {transaction.Description}");
            Console.WriteLine(
                $"Date        : {transaction.Date:dd MMM yyyy - h:mm tt}"
            );
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine();

            number++;
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

        Console.WriteLine(
            "Transaction data saved successfully."
        );
    }

    public void LoadData()
    {
        transactions = fileManager.Load();
    }
}