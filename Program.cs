ExpenseTracker tracker = new ExpenseTracker();

// Load previously saved transactions
tracker.LoadData();

bool running = true;

while (running)
{
    Console.WriteLine("\n==============================");
    Console.WriteLine("   PERSONAL EXPENSE TRACKER");
    Console.WriteLine("==============================");
    Console.WriteLine("1. Add Income");
    Console.WriteLine("2. Add Expense");
    Console.WriteLine("3. View Transactions");
    Console.WriteLine("4. View Balance");
    Console.WriteLine("5. Save Data");
    Console.WriteLine("6. Exit");

    Console.Write("\nSelect an option: ");
    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            AddTransaction(tracker, TransactionType.Income);
            break;

        case "2":
            AddTransaction(tracker, TransactionType.Expense);
            break;

        case "3":
            tracker.ViewTransactions();
            break;

        case "4":
            Console.WriteLine("\n--- Financial Summary ---");
            Console.WriteLine(
                $"Total Income   : LKR {tracker.GetTotalIncome():F2}"
            );
            Console.WriteLine(
                $"Total Expenses : LKR {tracker.GetTotalExpenses():F2}"
            );
            Console.WriteLine(
                $"Current Balance: LKR {tracker.GetBalance():F2}"
            );
            break;

        case "5":
            tracker.SaveData();
            break;

        case "6":
            tracker.SaveData();
            Console.WriteLine("Goodbye!");
            running = false;
            break;

        default:
            Console.WriteLine(
                "Invalid option. Please select a number from 1 to 6."
            );
            break;
    }
}


static void AddTransaction(
    ExpenseTracker tracker,
    TransactionType type)
{
    decimal amount;

    while (true)
    {
        Console.Write("\nEnter amount: LKR ");

        string? amountInput = Console.ReadLine();

        if (decimal.TryParse(amountInput, out amount) && amount > 0)
        {
            break;
        }

        Console.WriteLine(
            "Invalid amount. Please enter a number greater than 0."
        );
    }

    string category = SelectCategory(type);

    string description = GetRequiredInput(
        "Enter description: "
    );

    Transaction transaction = new Transaction(
        amount,
        DateTime.Now,
        category,
        description,
        type
    );

    tracker.AddTransaction(transaction);

    Console.WriteLine(
        $"{type} transaction added successfully."
    );
}


static string SelectCategory(TransactionType type)
{
    if (type == TransactionType.Income)
    {
        while (true)
        {
            Console.WriteLine("\nSelect Income Category:");
            Console.WriteLine("1. Salary");
            Console.WriteLine("2. Freelance");
            Console.WriteLine("3. Business");
            Console.WriteLine("4. Rental");
            Console.WriteLine("5. Gift");
            Console.WriteLine("6. Other");

            Console.Write("Select category: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    return "Salary";

                case "2":
                    return "Freelance";

                case "3":
                    return "Business";

                case "4":
                    return "Rental";

                case "5":
                    return "Gift";

                case "6":
                    return GetRequiredInput(
                        "Enter custom category: "
                    );

                default:
                    Console.WriteLine(
                        "Invalid category. Please select 1 to 6."
                    );
                    break;
            }
        }
    }
    else
    {
        while (true)
        {
            Console.WriteLine("\nSelect Expense Category:");
            Console.WriteLine("1. Food");
            Console.WriteLine("2. Transport");
            Console.WriteLine("3. Bills");
            Console.WriteLine("4. Shopping");
            Console.WriteLine("5. Entertainment");
            Console.WriteLine("6. Education");
            Console.WriteLine("7. Other");

            Console.Write("Select category: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    return "Food";

                case "2":
                    return "Transport";

                case "3":
                    return "Bills";

                case "4":
                    return "Shopping";

                case "5":
                    return "Entertainment";

                case "6":
                    return "Education";

                case "7":
                    return GetRequiredInput(
                        "Enter custom category: "
                    );

                default:
                    Console.WriteLine(
                        "Invalid category. Please select 1 to 7."
                    );
                    break;
            }
        }
    }
}


static string GetRequiredInput(string message)
{
    while (true)
    {
        Console.Write(message);

        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine(
            "This field cannot be empty. Please enter a value."
        );
    }
}