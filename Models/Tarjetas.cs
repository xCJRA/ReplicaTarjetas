public class SachDataItem
{
    public string CardNo { get; set; }
    public string EndTime { get; set; }
    public string Group { get; set; }
    public string Password { get; set; }
    public string Pin { get; set; }
    public string StartTime { get; set; }
    public int AutZone { get; set; }
    public string Door { get; set; }
}

public class SachDatosItem
{
    public string archivoXml { get; set; }
    public string archivoLog { get; set; }
    public string ip { get; set; }
    public string puerto { get; set; }
    public string idCentro { get; set; }
    public string fecha { get; set; }
    public string ultimo { get; set; }
}

public class SachConfig
{
    public string username { get; set; }
    public string password { get; set; }
    public string action { get; set; }
    public string tipo { get; set; }
    public string socnr { get; set; }
    public string idCentro { get; set; }
    public Dictionary<string, SachDataItem> datos { get; set; }
    public string archivoXml { get; set; }
    public string archivoLog { get; set; }
    public string ip { get; set; }
    public string puerto { get; set; }
    public string espera { get; set; }
}
