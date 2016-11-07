using System;
using System.Data;

using AOI_VTWIN_RNS.Src.Config;
using Oracle.DataAccess.Client;

namespace AOI_VTWIN_RNS.Src.Database
{
    public class OracleConnector
    {
        public string user = "";
        public string pass = "";
        public string host = "";
        public string service = "";
        public string port = "";

        public OracleConnection oConnection;

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

            oConnection = new OracleConnection(connectionString);
            oConnection.Open();
        }

        public void Disconnect()
        {
            oConnection.Close();
            oConnection.Dispose();
        }

        public DataTable Query(string query) {
            DataTable table = new DataTable();

            OracleDataAdapter oAdapter;
            OracleCommand oCommand;
            OracleCommandBuilder oCommandBuilder;

            Connect();

            oCommand = new OracleCommand(query,oConnection);
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
