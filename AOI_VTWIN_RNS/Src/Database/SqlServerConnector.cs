using System;
using System.Data;
using System.Data.SqlClient;

using CollectorPackage.Src.Config;

namespace CollectorPackage.Src.Database
{
    public class SqlServerConnector : IDatabase
    {
        public bool rows = false;
        private SqlConnection connection;

        public string host { get; set; }
        public string port { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public string database { get; set; }

        public void LoadConfig(string AppConfigTag) 
        {
            host = AppConfig.Read(AppConfigTag, "db_host");
            port = AppConfig.Read(AppConfigTag, "db_port");
            user = AppConfig.Read(AppConfigTag, "db_user");
            pass = AppConfig.Read(AppConfigTag, "db_pass");
            database = AppConfig.Read(AppConfigTag, "db_database");
        }

        public void Connect()
        {
            string connectionString = "User Id=" + user + ";" +
                               "Password=" + pass + ";Server=" + host + ";" +
                               "Database=" + database + ";"; 
            connection = new SqlConnection(connectionString);
        }

        public void Disconnect()
        {
            try
            {
                connection.Close();
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("SQLSERVER: (" + ex.Number + ") " + ex.Message);
            }
        }

        private bool OpenConnection()
        {
            Connect();
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                return true;
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 1042:
                        throw new InvalidOperationException("SQLSERVER: No se pudo conectar al servidor. " + ex.Message);
                        break;
                    case 1045:
                        throw new InvalidOperationException("SQLSERVER: Usuario/Password incorrectos. " + ex.Message);
                        break;
                    case 1044:
                        throw new InvalidOperationException("SQLSERVER: Acceso denegado a la tabla. " + ex.Message);
                        break;
                    default:
                        throw new InvalidOperationException("SQLSERVER: (" + ex.Number + ") " + ex.Message);
                        break;
                }

                return false;
            }
        }

        public bool NonQuery(string query)
        {
            bool rs = false;
            Connect();

            if (OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection);

                try
                {
                    if (cmd.ExecuteNonQuery() > 0) { rs = true; }
                }
                catch (SqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1451: // Existen campos enlazados a clave referenciada
                            throw new InvalidOperationException("SQLSERVER: Existen datos enlazados a este campo, no se puede eliminar. " + ex.Message);
                            break;
                        case 1062: // Duplicados en UNIQUE
                            throw new InvalidOperationException("SQLSERVER: Elemento duplicado, ya se encuentra registrado. " + ex.Message);
                            break;
                        default:
                            throw new InvalidOperationException("SQLSERVER: (" + ex.Number + ") " + ex.Message);
                            break;
                    }
                    rs = false;
                }
                Disconnect();
            }
            return rs;
        }     

        public DataTable Query(string query)
        {
            DataTable rs = new DataTable();
            Connect();
            if (OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection);

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(rs);
                Disconnect();
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