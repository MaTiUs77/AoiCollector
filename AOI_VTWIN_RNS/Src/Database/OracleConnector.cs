using System;
using System.Data;

using CollectorPackage.Src.Config;
using Oracle.DataAccess.Client;

namespace CollectorPackage.Src.Database
{
    public class OracleConnector : IDatabase
    {
        public string host { get; set; }
        public string port { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public string service { get; set; }

        public OracleConnection connection;

        public void LoadConfig(string AppConfigTag) 
        {
            host = AppConfig.Read(AppConfigTag, "db_host");
            port = AppConfig.Read(AppConfigTag, "db_port");
            user = AppConfig.Read(AppConfigTag, "db_user");
            pass = AppConfig.Read(AppConfigTag, "db_pass");
            service = AppConfig.Read(AppConfigTag, "db_service");
        }

        public void Connect()
        {
            /* 
                Conexion con TNS
                string connectionString = "Data Source=" + service + ";Persist Security Info=True;User ID=" + user + ";Password=" + pass + ";";
            */

            // Sin TNS
            string connectionString = "Data Source=(DESCRIPTION= (ADDRESS= (PROTOCOL=TCP) (HOST="+host+") (PORT="+port+ ")) (CONNECT_DATA= (SERVICE_NAME=" + service + ")));User ID=" + user + ";Password=" + pass+ ";";

            connection = new OracleConnection(connectionString);
            connection.Open();
        }

        public void Disconnect()
        {
            connection.Close();
            connection.Dispose();
        }

        public DataTable Query(string query) {
            DataTable table = new DataTable();

            OracleDataAdapter oAdapter;
            OracleCommand oCommand;
            OracleCommandBuilder oCommandBuilder;

            Connect();

            oCommand = new OracleCommand(query, connection);
            oAdapter = new OracleDataAdapter(oCommand);
            oCommandBuilder = new OracleCommandBuilder(oAdapter);
            oAdapter.Fill(table);

            Disconnect();

            return table;
        }

        public DateTime GetSysDate() {
            DateTime time = new DateTime();
            DataTable dt = Query("select sysdate from dual");
            string sysdate = dt.Rows[0]["sysdate"].ToString();

            time = DateTime.Parse(sysdate);
            
            return time;
        }
    }
}
