// ⏱️ INICIO DEL CONTADOR
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

//  Cargamos la cnf json
var config = ConfigReader.LoadConfig();

// Obtiene tarjetas pendientes
var tarjetas = DBConexion.GetPanelTarjetasPendientes();
int contOK = 0;
int contER = 0;
int fila   = 1;
Console.WriteLine($"*********************************************************");
Console.WriteLine($"\n📊 {tarjetas.Count} tarjetas pendientes encontradas:");
foreach (var tarjeta in tarjetas)
{
  try
  {
    Console.WriteLine($"==============================================================================");
    Console.WriteLine($"{fila}.-ID: {tarjeta.id}, Tarjeta: {tarjeta.noTarjeta}, IP: {tarjeta.ip} , Puerto: {tarjeta.puerto}, Servidor: {tarjeta.servidor}");
    int idTarjeta  = tarjeta.id;
    var param = ConfigReader.LoadSachConfig(tarjeta.contenido);
    var servidor   = tarjeta.servidor;
    string respuesta  = "";
    string zupdate    = "PE";//pendiente envio a zora
    string pathXML    = param.archivoXml;
    string pathLog    = param.archivoLog;
    string pathBaseXML= config.pathXML;
    string archivoXML = $"{pathBaseXML}{pathXML.Trim()}";//es la direccion de la carpeta de tarjetas.
    string archivoLog = $"{pathBaseXML}{pathLog.Trim()}";//es la direccion de la carpeta de tarjetas.
    string ip         = param.ip;
    string puerto     = param.puerto;

    //guardamos el xml a procesar.

    string rutaXml = config.pathTar;//"C:\\xampp\\htdocs\\tarjetas.xml";
  
    ProcesarXmlTarjetas.procesa(param, rutaXml, archivoXML);

    //ejecutamos el otro programa.
  
    string rutaExe = config.pathExe;//"C:\\ZORA\\PanelTarjetasAPI\\Sach1\\newExe\\eROI_Panel\\eROI_Panel.exe";

    respuesta      = Utilerias.exec(rutaExe,ip,puerto,archivoXML,archivoLog);
    Console.WriteLine(respuesta);
    string[] partes = respuesta.Split('|', StringSplitOptions.RemoveEmptyEntries);
    if (partes.Length > 0 && partes[0] == "ER")
    {
      zupdate = "ED";//ERROR DEVICE
      contER++;
    }
    else
    {
      contOK++;
    }
    Console.WriteLine($"*********************** Actualizando tarjeta {tarjeta.noTarjeta}");
    DBConexion.ActualizarPanelTarjeta(idTarjeta,zupdate,respuesta);
  }
  catch (Exception ex)
  {
    // Una tarjeta con error no debe detener a las demás: se marca EJ y se continúa con la siguiente.
    contER++;
    Console.WriteLine($"❌ Error en tarjeta {tarjeta.noTarjeta} (ID {tarjeta.id}): {ex.Message}");
    Utilerias.GuardarLogError($"ID {tarjeta.id}, Tarjeta {tarjeta.noTarjeta}: {ex.Message}");
    DBConexion.ActualizarPanelTarjeta(tarjeta.id, "EJ", ex.Message);//ERROR JSON / PROCESO
  }
  fila++;
}
// ⏹️ FIN DEL CONTADOR
stopwatch.Stop();
TimeSpan tiempo = stopwatch.Elapsed;
// MENSAJE FINAL
Console.WriteLine("=====================TERMINO=============================");
Console.WriteLine($"=====================TarjetasOK: {contOK}=============================");
Console.WriteLine($"=====================TarjetasER: {contER}=============================");
Console.WriteLine($"\n⏱️ Tiempo total del foreach: {tiempo.Hours:00}:{tiempo.Minutes:00}:{tiempo.Seconds:00}.{tiempo.Milliseconds:000}");
Utilerias.GuardarLogFinal(contOK,contER,tarjetas.Count,tiempo);