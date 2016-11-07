using System;
using System.Data;

using AOI_VTWIN_RNS.Src.Config;
using MySql.Data.MySqlClient;

namespace AOI_VTWIN_RNS.Src.Database
{
    class MySqlConnector
    {
        private MySqlConnection connection;
        public static string server = "";
        public static string database = "";
        public static string user = "";
        public static string password = "";

        public bool rows = false;

        public static void LoadConfig() 
        {
            database = AppConfig.Read("MYSQL", "database");
            server = AppConfig.Read("MYSQL", "server");
            user = AppConfig.Read("MYSQL", "user");
            password = AppConfig.Read("MYSQL", "pass");
        }

        public MySqlConnector()
        {
            Initialize();
        }

        private void Initialize()
        {
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + user + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        public bool OpenConnection()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1042:
                        throw new InvalidOperationException("MYSQL: No se pudo conectar al servidor. " + ex.Message);
                        break;
                    case 1045:
                        throw new InvalidOperationException("MYSQL: Usuario/Password incorrectos. " + ex.Message);
                        break;
                    case 1044:
                        throw new InvalidOperationException("MYSQL: Acceso denegado a la tabla. " + ex.Message);
                        break;
                    default:
                        throw new InvalidOperationException("MYSQL: (" + ex.Number + ") " + ex.Message);
                        break;
                }

                return false;
            }
        }
        
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("MYSQL: (" + ex.Number + ") " + ex.Message);
                return false;
            }
        }

        public bool Ejecutar(string query)
        {
            bool rs = false;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                try
                {
                    if (cmd.ExecuteNonQuery() > 0) { rs = true; }
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1451: // Existen campos enlazados a clave referenciada
                            throw new InvalidOperationException("MYSQL: Existen datos enlazados a este campo, no se puede eliminar. " + ex.Message);
                            break;
                        case 1062: // Duplicados en UNIQUE
                            throw new InvalidOperationException("MYSQL: Elemento duplicado, ya se encuentra registrado. " + ex.Message);
                            break;
                        default:
                            throw new InvalidOperationException("MYSQL: (" + ex.Number + ") " + ex.Message);
                            break;
                    }
                    rs = false;
                }
                this.CloseConnection();
            }
            return rs;
        }     

        public DataTable Select(string query)
        {
            DataTable rs = new DataTable();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                MySqlDataAdapter adapter = new MySqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(rs);
                this.CloseConnection();
            }
            Rows(rs);
            return rs;
        }

        private void Rows(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                rows = true;
            }
            else
            {
                rows = false;
            }
        }
    }
}