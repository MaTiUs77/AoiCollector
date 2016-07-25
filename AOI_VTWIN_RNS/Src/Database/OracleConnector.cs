using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using AOI_VTWIN_RNS.Src.Config;
using Oracle.DataAccess.Client;

namespace AOI_VTWIN_RNS.Src.Database
{
    public class OracleConnector
    {
        public string user = "";
        public string pass = "";
        public string server = "";
        public string service = "";
        public string port = "";

        public OracleConnection oConnection;

        public void LoadConfig(string AppConfigTag) 
        {
            user = AppConfig.Read(AppConfigTag, "oracle_user");
            pass = AppConfig.Read(AppConfigTag, "oracle_pass");
            server = AppConfig.Read(AppConfigTag, "oracle_server");
            service = AppConfig.Read(AppConfigTag, "oracle_service");
            port = AppConfig.Read(AppConfigTag, "oracle_port");
        }

        public void Connect()
        {
            /* 
                Conexion con TNS
                string connectionString = "Data Source=" + service + ";Persist Security Info=True;User ID=" + user + ";Password=" + pass + ";";
            */

            // Sin TNS
            string connectionString = "Data Source=(DESCRIPTION= (ADDRESS= (PROTOCOL=TCP) (HOST="+server+") (PORT="+port+ ")) (CONNECT_DATA= (SERVICE_NAME=" + service + ")));User ID=" + user + ";Password=" + pass+ ";";

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
