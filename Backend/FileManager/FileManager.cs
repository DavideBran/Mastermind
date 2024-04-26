using Newtonsoft.Json;

namespace fileManager;

public class FileManager
{
    private string _standardPath;
    private string _directoryName;
    private string _standardRestorePath;

    private string GetAppdataPath()
    {
        string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (Directory.Exists("FileManager\\LocalSavings") || appdataPath == null || appdataPath == "") return "FileManager\\LocalSavings";
        return appdataPath;
    }

    private string SetupForSave(string outFile)
    {
        string outPath = Path.Combine(_standardPath, outFile);
        if (File.Exists(outPath)) File.Delete(outFile);
        return outPath;
    }

    private string SetUpForLoad(string inFile)
    {
        return Path.Combine(_standardPath, inFile);

    }

    public FileManager(string directoryName)
    {
        // Create the generic path and add the name of the App directory
        _standardPath = Path.Combine(GetAppdataPath(), directoryName);
        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(_standardPath);
        _directoryName = directoryName;
        _standardRestorePath = Path.Combine($"FileManager\\DirectoryNotFoundRestore", _directoryName);
    }

    public void SaveAll<T>(ICollection<T> items, string outFile)
    {
        string outPath = SetupForSave(outFile);
        List<string> serializedItems = new();
        List<T> itemsList = (List<T>)items;
        for (int i = 0; i < items.Count; i++)
        {
            serializedItems.Add(JsonConvert.SerializeObject(itemsList[i], Formatting.None,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                }
            ));
        }
        try
        {
            File.WriteAllLines(outPath, serializedItems);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (DirectoryNotFoundException)
        {
            // even if the directory is not found i'll try to save on some relative path inside the FileMangager, 
            // so i'll not loose the stored data
            string restorePath = Path.Combine(_standardRestorePath, outPath);
            // checking if the path already exists, and if is not i'll create it 
            if (!File.Exists(restorePath)) Directory.CreateDirectory(restorePath);

            File.WriteAllLines(restorePath, serializedItems);

        }

    }

    public void SaveList<T>(ICollection<T> items, string outFile)
    {
        string outPath = SetupForSave(outFile);
        string[] serializedItems = new string[] {
            JsonConvert.SerializeObject(items, Formatting.None,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                }
            )
        };
        try
        {
            File.WriteAllLines(outPath, serializedItems);
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"{outPath} contains invalid character");
            throw;
        }
        catch (DirectoryNotFoundException error)
        {
            // even if the directory is not found i'll try to save on some relative path inside the FileMangager, 
            // so i'll not loose the stored data
            string restorePath = Path.Combine(_standardRestorePath, outPath);
            // checking if the path already exists, and if is not i'll create it 
            if (!File.Exists(restorePath)) Directory.CreateDirectory(restorePath);

            File.WriteAllLines(outPath, serializedItems);


            Console.WriteLine($"{outPath} not found, Restored the data on {restorePath}");
        }
    }

    public ICollection<T>? PoPAll<T>(string inFile)
    {
        try
        {
            List<T> allItems = new();
            string inPath = SetUpForLoad(inFile);
            using (StreamReader JsonFileReader = new(inPath))
            {
                string? jsonLine;
                while ((jsonLine = JsonFileReader.ReadLine()) != null)
                {
                    T? item = JsonConvert.DeserializeObject<T>(jsonLine, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                    if (item != null) allItems.Add(item);
                }
            }
            return allItems.Count == 0 ? null : allItems;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public ICollection<T>? PoPList<T>(string inPath)
    {
        try
        {
            inPath = SetUpForLoad(inPath);
            using StreamReader reader = new(inPath);
            string? jsonLine = reader.ReadLine();
            if (jsonLine != null)
            {
                List<T>? itemsList = JsonConvert.DeserializeObject<List<T>>(jsonLine, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });
                return itemsList;
            }
            else return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
