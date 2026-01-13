using System.Xml.Linq;
public class ProcesarXmlTarjetas
{
    public static void procesa(SachConfig param, string rutaXml)
    {
        // Cargar XML equivalente a simplexml_load_file
        XDocument xmlDoc = XDocument.Load(rutaXml);
        XElement root = xmlDoc.Root ?? throw new InvalidOperationException("XML sin raíz");
        
        // xpath('//Clientes') equivalente
        var clientes = xmlDoc.Descendants("Clientes").ToList();
        
        foreach (XElement cfdiComprobante in clientes)
        {
            foreach (var kvp in param.datos)
            {
                string key = kvp.Key;
                dynamic value = kvp.Value; // Para acceder como en PHP
                
                // if ($key == 0) continue;
                if (key == "0") continue;
                
                // if(isset($value->CardNo) && !empty($value->CardNo))
                if (value.CardNo != null && !string.IsNullOrEmpty(value.CardNo.Trim()))
                {
                    string refnr = value.CardNo;
                    Console.WriteLine($"RefNr: {refnr}");
                }
                
                // $CfdiRelacionado = $cfdiComprobante->addChild('Cliente');
                XElement cfdiRelacionado = new XElement("Cliente");
                
                // foreach ($value as $key2 => $value2)
                foreach (var prop in value.GetType().GetProperties())
                {
                    string key2 = prop.Name;
                    object value2 = prop.GetValue(value);
                    
                    if (value2 != null)
                    {
                        // $CfdiRelacionado->AddAttribute($key2, $value2);
                        cfdiRelacionado.Add(new XAttribute(key2, value2.ToString()));
                    }
                }
                
                cfdiComprobante.Add(cfdiRelacionado);
            }
        }
        
        // Guardar XML modificado
        xmlDoc.Save(rutaXml);
        //Console.WriteLine("✅ XML actualizado");
    }
}