using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

using AOI_VTWIN_RNS.Src.Config;

namespace AOI_VTWIN_RNS.Src.Database
{
    class SqlServerConnector
    {
        private SqlConnection connection;
        public static string server = "";
        public static string database = "";
        public static string user = "";
        public static string password = "";

        public bool rows = false;

        public static void LoadConfig() 
        {
            SqlServerConnector.database = AppConfig.Read("SQLSERVER", "database");
            SqlServerConnector.server = AppConfig.Read("SQLSERVER", "server");
            SqlServerConnector.user = AppConfig.Read("SQLSERVER", "user");
            SqlServerConnector.password = AppConfig.Read("SQLSERVER", "pass");
        }

        public SqlServerConnector()
        {
            Initialize();
        }

        private void Initialize()
        {
            string connectionString;
            connectionString = "User Id=" + SqlServerConnector.user + ";" +
                               "Password=" + SqlServerConnector.password + ";Server=" + SqlServerConnector.server + ";" +
                               "Database=" + SqlServerConnector.database + ";"; 
            connection = new SqlConnection(connectionString);
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

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException("SQLSERVER: (" + ex.Number + ") " + ex.Message);
                return false;
            }
        }

        public bool Ejecutar(string query)
        {
            bool rs = false;
            if (this.OpenConnection() == true)
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
                this.CloseConnection();
            }
            return rs;
        }     

        public DataTable Select(string query)
        {
            DataTable rs = new DataTable();

            if (this.OpenConnection() == true)
            {
                SqlCommand cmd = new SqlCommand(query, connection);

                SqlDataAdapter adapter = new SqlDataAdapter();
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