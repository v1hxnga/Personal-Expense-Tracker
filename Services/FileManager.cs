using System.Text.Json;
using Personal_Expense_Tracker.Models;

namespace Personal_Expense_Tracker.Services;

public class FileManager
{
    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new JsonSerializerOptions
        {
            WriteIndented = true
        };

    public FileManager(IWebHostEnvironment environment)
    {
        string dataDirectory = Path.Combine(
            environment.ContentRootPath,
            "Data"
        );

        Directory.CreateDirectory(dataDirectory);

        _filePath = Path.Combine(
            dataDirectory,
            "transactions.json"
        );
    }

    public async Task SaveAsync(List<Transaction> transactions)
    {
        try
        {
            string json = JsonSerializer.Serialize(
                transactions,
                _jsonOptions
            );

            await File.WriteAllTextAsync(
                _filePath,
                json
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error saving transactions: {ex.Message}"
            );
        }
    }

    public async Task<List<Transaction>> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<Transaction>();
            }

            string json = await File.ReadAllTextAsync(
                _filePath
            );

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Transaction>();
            }

            return JsonSerializer.Deserialize<List<Transaction>>(
                       json,
                       _jsonOptions
                   )
                   ?? new List<Transaction>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error loading transactions: {ex.Message}"
            );

            return new List<Transaction>();
        }
    }
}