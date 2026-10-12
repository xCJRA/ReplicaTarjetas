using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

public static class ConfigReader
{
    // Opciones para el contenido de la tarjeta: acepta números o texto en cualquier campo (PHP 7 y PHP 8).
    private static readonly JsonSerializerOptions SachOptions = CreateSachOptions();

    private static JsonSerializerOptions CreateSachOptions()
    {
        var options = new JsonSerializerOptions { NumberHandling = JsonNumberHandling.AllowReadingFromString };
        options.Converters.Add(new StringFlexibleConverter());
        return options;
    }

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
            
        var config = JsonSerializer.Deserialize<SachConfig>(jsonString, SachOptions);
        
        return config ?? throw new InvalidOperationException("Error al deserializar JSON Sach");
    }
}
