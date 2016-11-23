using System;
using System.Data;

using CollectorPackage.Src.Config;
using MySql.Data.MySqlClient;

namespace CollectorPackage.Src.Database
{
    public class MySqlConnector : IDatabase
    {
        public bool rows = false;
        private MySqlConnection connection;

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
            string connectionString = "SERVER=" + host + ";" + "DATABASE=" + database + ";" + "UID=" + user + ";" + "PASSWORD=" + pass + ";";
            connection = new MySqlConnection(connectionString);
        }

        public void Disconnect()
        {
            try
            {
                connection.Close();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("MYSQL: (" + ex.Number + ") " + ex.Message);
            }
        }

        private bool OpenConnection()
        {
            Connect();
            bool connected = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    connected = true;
                } else
                {
                    connected = false;
                }
            }
            catch (MySqlException ex)
            {
                string errMsg = "";

                switch (ex.Number)
                {
                    case 1042:
                        errMsg = "MYSQL: No se pudo conectar al servidor. " + ex.Message;
                        break;
                    case 1045:
                        errMsg = "MYSQL: Usuario/Password incorrectos. " + ex.Message;
                        break;
                    case 1044:
                        errMsg = "MYSQL: Acceso denegado a la tabla. " + ex.Message;
                        break;
                    default:
                        errMsg = "MYSQL: (" + ex.Number + ") " + ex.Message;
                        break;
                }

                connected = false;

                throw new InvalidOperationException(errMsg);
            }

            return connected;
        }

        public bool NonQuery(string query)
        {
            bool response = false;

            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                try
                {
                    if (cmd.ExecuteNonQuery() > 0) {
                        response = true;
                    }
                }
                catch (MySqlException ex)
                {
                    string errMsg = "";
                    switch (ex.Number)
                    {
                        case 1451: // Existen campos enlazados a clave referenciada
                           errMsg = "MYSQL: Existen datos enlazados a este campo, no se puede eliminar. " + ex.Message;
                            break;
                        case 1062: // Duplicados en UNIQUE
                            errMsg = "MYSQL: Elemento duplicado, ya se encuentra registrado. " + ex.Message;
                            break;
                        default:
                            errMsg = "MYSQL: (" + ex.Number + ") " + ex.Message;
                            break;
                    }
                    response = false;

                    throw new InvalidOperationException(errMsg);
                }

                Disconnect();
            }

            return response;
        }     

        public DataTable Query(string query)
        {
            DataTable rs = new DataTable();
            Connect();

            if (OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                MySqlDataAdapter adapter = new MySqlDataAdapter();
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