using System.Diagnostics;
public class Utilerias
{
    public static string exec(string rutaExe, string ip, string puerto, string archivoXml, string archivoLog)
    {
        string argumentos  = $"{ip} {puerto} \"{archivoXml}\" \"{archivoLog}\"";
        string cmdCompleto = $"{rutaExe} {ip} {puerto} \"{archivoXml}\" \"{archivoLog}\"";
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
                bool termino = process.WaitForExit(30000);
                if (termino)
                {
                    process.WaitForExit(); // asegura que se lea toda la salida antes de evaluarla
                }

                if (cancelado)
                {
                    return $"ER|[{cmdCompleto}]{outputAcumulado}";
                }

                if (!process.HasExited)
                {
                    process.Kill();
                    return $"ER|Timeout - Proceso no respondió en 30 s. Salida: {Resumir(outputAcumulado)}";
                }

                // ZKTClient reporta "ER|Connect device Failed!. Error=..." y aun así la salida empieza con "OK|Inicio de Programa";
                // si nunca estableció conexión con el panel, la tarjeta no se dio de alta.
                if (outputAcumulado.Contains("Connect device Failed") && !outputAcumulado.Contains("establecida"))
                {
                    return $"ER|Sin conexión con el panel {ip}:{puerto}. Salida: {Resumir(outputAcumulado)}";
                }

                //return string.IsNullOrEmpty(outputAcumulado) ? "✅ Ejecutado sin salida" : outputAcumulado.Trim();
                return string.IsNullOrEmpty(outputAcumulado) ? "OK|Ejecutado sin salida" : $"OK|{outputAcumulado.Trim()}";

            }
        }
        catch (Exception ex)
        {
            return $"ER|No se pudo ejecutar {rutaExe}: {ex.Message}";
        }
    }

    // Deja la salida en una sola línea y corta lo muy largo para el log y observaciones.
    public static string Resumir(string texto, int maximo = 1000)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return "(sin salida)";
        string linea = texto.Replace("\r", "").Trim().Replace("\n", " | ");
        return linea.Length > maximo ? linea.Substring(0, maximo) + "..." : linea;
    }

    // Revisa que existan los archivos que usa el proceso; regresa lo que falta (vacío si todo está bien).
    public static string ValidarEntorno(DBConfig config)
    {
        var faltantes = new List<string>();
        if (string.IsNullOrWhiteSpace(config.pathTar) || !File.Exists(config.pathTar))
            faltantes.Add($"plantilla pathTar ({config.pathTar})");
        if (string.IsNullOrWhiteSpace(config.pathXML) || !Directory.Exists(config.pathXML))
            faltantes.Add($"carpeta pathXML ({config.pathXML})");
        if (string.IsNullOrWhiteSpace(config.pathExe) || !File.Exists(config.pathExe))
            faltantes.Add($"ejecutable pathExe ({config.pathExe})");
        return string.Join(", ", faltantes);
    }

    public static void GuardarLogFinal(int contOK, int contER, int tarjetasPendientes, TimeSpan tiempo, string estatus, int contED, int contEJ)
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
        string logMensaje = $"=====================EMPIEZA=============================\r\n" +
                        $"=== LOG {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n" + 
                        $"📊 {tarjetasPendientes} tarjetas pendientes encontradas (estatus {estatus}):\r\n" +
                        $"=====================TERMINO=============================\r\n" +
                        $"=====================TarjetasOK: {contOK}=============================\r\n" +
                        $"=====================TarjetasER: {contER}=============================\r\n" +
                        (contER > 0
                            ? $"   ED (sin conexión / timeout del panel): {contED} | EJ (error de datos o proceso): {contEJ} | Detalle en {fechaArchivo}_logError.txt\r\n"
                            : "") +
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
        string logMensaje =  $"=====================EMPIEZA=============================\r\n"  +
                        $"=== LOG {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n"+
                        $"📊 {logError} \r\n" +
                        $"=====================TERMINO=============================\r\n";
        File.AppendAllText(rutaLog, logMensaje);
    }
}