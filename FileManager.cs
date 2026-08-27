using System.Text.Json;

public class FileManager
{
    private string filePath;

    public FileManager(string filePath)
    {
        this.filePath = filePath;
    }

    public void Save(List<Transaction> transactions)
    {
        try
        {
            string json = JsonSerializer.Serialize(
                transactions,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }
            );

            File.WriteAllText(filePath, json);
        }
        catch (IOException ex)
        {
            Console.WriteLine(
                $"Error saving transaction data: {ex.Message}"
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(
                $"Access error while saving data: {ex.Message}"
            );
        }
    }

    public List<Transaction> Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new List<Transaction>();
            }

            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<List<Transaction>>(json)
                   ?? new List<Transaction>();
        }
        catch (JsonException)
        {
            Console.WriteLine(
                "The transaction data file is invalid or corrupted."
            );

            return new List<Transaction>();
        }
        catch (IOException ex)
        {
            Console.WriteLine(
                $"Error loading transaction data: {ex.Message}"
            );

            return new List<Transaction>();
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(
                $"Access error while loading data: {ex.Message}"
            );

            return new List<Transaction>();
        }
    }
}