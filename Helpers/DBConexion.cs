using MySql.Data.MySqlClient;

public static class DBConexion
{
    public static string TestConnection()
    {
        var config = ConfigReader.LoadConfig();
        string connectionString = $"Server={config.host};" +
                                 $"Database={config.db};" +
                                 $"Uid={config.user};" +
                                 $"Pwd={config.pass};";
        Console.WriteLine(config);
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                return $"✅ Conexión exitosa a {config.db} en {config.host}";
            }
        }
        catch (Exception ex)
        {
            return $"❌ Error de conexión: {ex.Message}";
        }
    }

    public static List<PanelTarjeta> GetPanelTarjetasPendientes()
    {
        var config = ConfigReader.LoadConfig();
        //alimentando de la config
        string connectionString = $"Server={config.host};Database={config.db};Uid={config.user};Pwd={config.pass};";
        int noTarjetas = config.noTarjetas;
        string zupdate = config.zupdate;
        
        var resultados = new List<PanelTarjeta>();
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                
                string query = @" SELECT id,contenido,noTarjeta,servidor,ip,puerto,zupdate,idCentro,socnr,fecha 
                                FROM `zora_windows`.`paneltarjetas` 
                                WHERE `zupdate` = @zupdate LIMIT @noTarjetas";
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@zupdate", zupdate);
                    command.Parameters.AddWithValue("@noTarjetas", noTarjetas);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resultados.Add(new PanelTarjeta
                            {
                                id       = reader.GetInt32("id"),
                                contenido = reader["contenido"].ToString(),
                                noTarjeta = reader["noTarjeta"].ToString(),
                                servidor = reader["servidor"].ToString(),
                                ip = reader["ip"].ToString(),
                                puerto = reader["puerto"].ToString(),
                                zupdate = reader["zupdate"].ToString(),
                                idCentro = int.TryParse(reader["idCentro"]?.ToString(), out int idCentro) ? idCentro : 0,//es varchar, puede venir vacío
                                socnr = reader["socnr"].ToString(),
                                fecha = reader.GetDateTime("fecha")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en consulta: {ex.Message}");
        }

        return resultados;
    }

    public static int ActualizarPanelTarjeta(int idTarjeta,string zupdate,string observaciones)
    {
        var config = ConfigReader.LoadConfig();
        string connectionString = $"Server={config.host};Database={config.db};Uid={config.user};Pwd={config.pass};";
        
        string query = @"UPDATE `zora_windows`.`paneltarjetas` 
                    SET `zupdate` = @zupdate, 
                    `intentos` = CASE WHEN `intentos` = 0 THEN 1 ELSE `intentos` + 1 END,
                    `observaciones` = @obs
                    WHERE `id` = @id";
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(query, connection))
                {
                    // Parámetro para evitar SQL injection
                    command.Parameters.AddWithValue("@id", idTarjeta);
                    command.Parameters.AddWithValue("@zupdate", zupdate);
                    command.Parameters.AddWithValue("@obs", observaciones);
                    int filasAfectadas = command.ExecuteNonQuery();
                    return filasAfectadas; // 1 = éxito, 0 = no encontró registro
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error UPDATE: {ex.Message}");
            return -1; // Error
        }
    }

}