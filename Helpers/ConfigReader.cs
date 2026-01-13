using System.Text.Json;
using System.IO;

public static class ConfigReader
{
    public static DBConfig LoadConfig()
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "config.json");
        
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Archivo no encontrado: {jsonPath}");
            
        string jsonContent = File.ReadAllText(jsonPath);
        var config = JsonSerializer.Deserialize<DBConfig>(jsonContent);
        
        return config ?? throw new InvalidOperationException("Error al deserializar config.json");
    }
    public static SachConfig LoadSachConfig(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            throw new ArgumentException("El string JSON no puede estar vacío");
            
        var config = JsonSerializer.Deserialize<SachConfig>(jsonString);
        
        return config ?? throw new InvalidOperationException("Error al deserializar JSON Sach");
    }
}
