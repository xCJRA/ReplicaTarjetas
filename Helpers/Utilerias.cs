using System.Diagnostics;
public class Utilerias
{
    
   public static string exec(string rutaExe, string ip, string puerto, string archivoXml, string archivoLog)
    {
        string argumentos = $"{ip} {puerto} \"{archivoXml}\" \"{archivoLog}\"";
        
        var proceso = new ProcessStartInfo
        {
            FileName = rutaExe,
            Arguments = argumentos,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };
        
        try
        {
            using (var process = new Process { StartInfo = proceso })
            {
                bool cancelado = false;
                string outputAcumulado = "";
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null) return;
                    outputAcumulado += e.Data + "\n";
                    if (e.Data.Contains("Connect device Failed! The error id is:"))
                    {
                        GuardarLogError(e.Data.ToString());
                        cancelado = true;
                        process.Kill(); // CIERRA el proceso inmediatamente
                    }
                };
                
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null) return;
                    outputAcumulado += e.Data + "\n";
                };
                
                process.Start();
                
                // Comienza lectura asíncrona EN VIVO
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                // Espera máximo 30 segundos
                process.WaitForExit(30000);
                
                if (cancelado)
                {
                    return $"ER|{outputAcumulado}";
                }
                
                if (!process.HasExited)
                {
                    process.Kill();
                    return "ER|Timeout - Proceso no respondió";
                }
                
                //return string.IsNullOrEmpty(outputAcumulado) ? "✅ Ejecutado sin salida" : outputAcumulado.Trim();
                return string.IsNullOrEmpty(outputAcumulado) ? "OK|Ejecutado sin salida" : $"OK|{outputAcumulado.Trim()}";

            }
        }
        catch (Exception ex)
        {
            return $"ER|{ex.Message}";
        }
    }

    public static void GuardarLogFinal(int contOK, int contER, int tarjetasPendientes, TimeSpan tiempo)
    {
        // Carpeta "log" en la raíz del programa
        string carpetaLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
        
        // Crear carpeta "log" si no existe
        if (!Directory.Exists(carpetaLog))
            Directory.CreateDirectory(carpetaLog);
        
        // Nombre con fecha: 20260112_log.txt
        string fechaArchivo = DateTime.Now.ToString("yyyyMMdd");
        string rutaLog = Path.Combine(carpetaLog, $"{fechaArchivo}_log.txt");
        
        
        // Escribir/agregar al final del archivo
        string logMensaje = $"=== LOG {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n" +
                        $"=====================EMPIEZA=============================\r\n" +
                        $"📊 {tarjetasPendientes} tarjetas pendientes encontradas:\r\n" +
                        $"=====================TERMINO=============================\r\n" +
                        $"=====================TarjetasOK: {contOK}=============================\r\n" +
                        $"=====================TarjetasER: {contER}=============================\r\n" +
                        $"⏱️ Tiempo total del foreach: {tiempo.Hours:00}:{tiempo.Minutes:00}:{tiempo.Seconds:00}.{tiempo.Milliseconds:000}\r\n\r\n";
        File.AppendAllText(rutaLog, logMensaje);
    }

    public static void GuardarLogError(string logError)
    {
        // Carpeta "log" en la raíz del programa
        string carpetaLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
        
        // Crear carpeta "log" si no existe
        if (!Directory.Exists(carpetaLog))
            Directory.CreateDirectory(carpetaLog);
        
        // Nombre con fecha: 20260112_log.txt
        string fechaArchivo = DateTime.Now.ToString("yyyyMMdd");
        string rutaLog = Path.Combine(carpetaLog, $"{fechaArchivo}_logError.txt");
        
        
        // Escribir/agregar al final del archivo
        string logMensaje = $"=== LOG {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n" +
                        $"=====================EMPIEZA=============================\r\n" +
                        $"📊 {logError} tarjetas pendientes encontradas:\r\n" +
                        $"=====================TERMINO=============================\r\n";
        File.AppendAllText(rutaLog, logMensaje);
    }
}