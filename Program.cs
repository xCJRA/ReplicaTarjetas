 // ⏱️ INICIO DEL CONTADOR
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

//  Cargamos la cnf json
var config = ConfigReader.LoadConfig();

// Obtiene tarjetas pendientes
var tarjetas = DBConexion.GetPanelTarjetasPendientes();
int contOK = 0;
int contER = 0;
int fila   = 1;
foreach (var tarjeta in tarjetas)
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
  string archivoXML = $"{config.pathXML}{pathXML.Trim()}";//es la direccion de la carpeta de tarjetas.
  string archivoLog = $"{config.pathXML}{pathLog.Trim()}";//es la direccion de la carpeta de tarjetas.
  string ip         = param.ip;
  string puerto     = param.puerto;

  //guardamos el xml a procesar.
          
  string rutaXml = "C:\\xampp\\htdocs\\tarjetas.xml";
  
  ProcesarXmlTarjetas.procesa(param, rutaXml);

  //ejecutamos el otro programa.
  string rutaExe = "C:\\ZORA\\PanelTarjetasAPI\\Sach1\\newExe\\eROI_Panel\\eROI_Panel.exe";
  respuesta      = Utilerias.exec(rutaExe,ip,puerto,archivoXML,archivoLog);
  
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
  //Console.WriteLine("=====================RESPUESTA:=============================");
  //Console.WriteLine($"{respuesta}");
  Console.WriteLine($"*********************** Actualizando tarjeta {tarjeta.noTarjeta}");
  DBConexion.ActualizarPanelTarjeta(idTarjeta,zupdate,respuesta);
  //Console.WriteLine("=====================TERMINO=============================");
  fila++;
}
// ⏹️ FIN DEL CONTADOR
stopwatch.Stop();
TimeSpan tiempo = stopwatch.Elapsed;
Console.WriteLine("=====================TERMINO=============================");
Console.WriteLine($"\n📊 {tarjetas.Count} tarjetas pendientes encontradas:");
Console.WriteLine($"=====================TarjetasOK: {contOK}=============================");
Console.WriteLine($"=====================TarjetasER: {contER}=============================");
Console.WriteLine($"\n⏱️ Tiempo total del foreach: {tiempo.Hours:00}:{tiempo.Minutes:00}:{tiempo.Seconds:00}.{tiempo.Milliseconds:000}");
Utilerias.GuardarLogFinal(contOK,contER,tarjetas.Count,tiempo);