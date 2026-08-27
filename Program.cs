ExpenseTracker tracker = new ExpenseTracker();

// Load previously saved transactions
tracker.LoadData();

bool running = true;

while (running)
{
    Console.Clear();

    ShowMainMenu(tracker);

    Console.Write("Select an option: ");
    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            Console.Clear();
            ShowSectionTitle("ADD INCOME");

            AddTransaction(
                tracker,
                TransactionType.Income
            );

            Pause();
            break;

        case "2":
            Console.Clear();
            ShowSectionTitle("ADD EXPENSE");

            AddTransaction(
                tracker,
                TransactionType.Expense
            );

            Pause();
            break;

        case "3":
            Console.Clear();

            tracker.ViewTransactions();

            Pause();
            break;

        case "4":
            Console.Clear();

            ShowFinancialSummary(tracker);

            Pause();
            break;

        case "5":
            Console.Clear();

            tracker.SaveData();

            Pause();
            break;

        case "6":
            tracker.SaveData();

            Console.WriteLine();
            Console.WriteLine("Thank you for using Personal Expense Tracker.");
            Console.WriteLine("Goodbye!");

            running = false;
            break;

        default:
            Console.WriteLine();
            Console.WriteLine(
                "Invalid selection. Please enter a number from 1 to 6."
            );

            Pause();
            break;
    }
}


static void ShowMainMenu(ExpenseTracker tracker)
{
    Console.WriteLine("===============================================");
    Console.WriteLine("           PERSONAL EXPENSE TRACKER");
    Console.WriteLine("===============================================");
    Console.WriteLine();

    Console.WriteLine(
        $"Current Balance: LKR {tracker.GetBalance():N2}"
    );

    Console.WriteLine();
    Console.WriteLine("-----------------------------------------------");
    Console.WriteLine();

    Console.WriteLine("1. Add Income");
    Console.WriteLine("2. Add Expense");
    Console.WriteLine("3. View Transactions");
    Console.WriteLine("4. View Financial Summary");
    Console.WriteLine("5. Save Data");
    Console.WriteLine("6. Exit");

    Console.WriteLine();
    Console.WriteLine("-----------------------------------------------");
    Console.WriteLine();
}


static void ShowSectionTitle(string title)
{
    Console.WriteLine("===============================================");
    Console.WriteLine($"               {title}");
    Console.WriteLine("===============================================");
    Console.WriteLine();
}


static void ShowFinancialSummary(ExpenseTracker tracker)
{
    Console.WriteLine("===============================================");
    Console.WriteLine("              FINANCIAL SUMMARY");
    Console.WriteLine("===============================================");
    Console.WriteLine();

    Console.WriteLine(
        $"Total Income    : LKR {tracker.GetTotalIncome():N2}"
    );

    Console.WriteLine(
        $"Total Expenses  : LKR {tracker.GetTotalExpenses():N2}"
    );

    Console.WriteLine("-----------------------------------------------");

    Console.WriteLine(
        $"Current Balance : LKR {tracker.GetBalance():N2}"
    );
}


static void AddTransaction(
    ExpenseTracker tracker,
    TransactionType type)
{
    decimal amount;

    while (true)
    {
        Console.Write("Enter amount: LKR ");

        string? amountInput = Console.ReadLine();

        if (decimal.TryParse(amountInput, out amount) && amount > 0)
        {
            break;
        }

        Console.WriteLine();
        Console.WriteLine(
            "Invalid amount. Please enter a number greater than 0."
        );
        Console.WriteLine();
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

    Console.WriteLine();
    Console.WriteLine(
        $"{type} transaction added successfully and saved automatically!"
    );
}


static string SelectCategory(TransactionType type)
{
    if (type == TransactionType.Income)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Select Income Category:");
            Console.WriteLine();
            Console.WriteLine("1. Salary");
            Console.WriteLine("2. Freelance");
            Console.WriteLine("3. Business");
            Console.WriteLine("4. Rental");
            Console.WriteLine("5. Gift");
            Console.WriteLine("6. Other");
            Console.WriteLine();

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
                    Console.WriteLine();
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
            Console.WriteLine();
            Console.WriteLine("Select Expense Category:");
            Console.WriteLine();
            Console.WriteLine("1. Food");
            Console.WriteLine("2. Transport");
            Console.WriteLine("3. Bills");
            Console.WriteLine("4. Shopping");
            Console.WriteLine("5. Entertainment");
            Console.WriteLine("6. Education");
            Console.WriteLine("7. Other");
            Console.WriteLine();

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
                    Console.WriteLine();
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

        Console.WriteLine();
        Console.WriteLine(
            "This field cannot be empty. Please enter a value."
        );
        Console.WriteLine();
    }
}


static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("-----------------------------------------------");
    Console.WriteLine("Press Enter to return to the main menu...");
    Console.ReadLine();
}