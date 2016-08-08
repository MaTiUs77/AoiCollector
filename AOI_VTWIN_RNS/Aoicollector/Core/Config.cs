using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOI_VTWIN_RNS.Aoicollector.Inspection.Model;
using AOI_VTWIN_RNS.Src.Config;

namespace AOI_VTWIN_RNS.Aoicollector.Core
{
    public class Config
    {
        public static bool debugMode = false;

        public string machineType       { get; set; }
        public string machineNameKey    { get; set; }

        public int intervalo            { get; set; }
        public string xmlExportPath     { get; set; }
        public string dataProgPath      { get; set; }
        public string inspectionCsvPath { get; set; }

        public static bool dbDownloadComplete { get; set; }
        public static List<int> byPassLine = new List<int>();
        public static List<int> toEndInspect = new List<int>();
        public static bool isByPassMode(string linea)
        {
            int exist = Config.byPassLine.Where(o => o.ToString() == linea.ToString()).Count();
            if (exist > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isEndInspect(string linea)
        {
            int exist = Config.toEndInspect.Where(o => o.ToString() == linea.ToString()).Count();
            if (exist > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isAutoStart()
        {
            return AppConfig.Read("SERVICE", "autostart").ToString().Equals("true");
        }

        public static bool dbDownload()
        {
            #region DESCARGA INFORMACION DE MYSQL
            Log.system.info("Iniciando descarga de datos MySql");
            try
            {            
                Faultcode.Download();
                Machine.Download();
                PcbInfo.Download();

                Log.system.info("Faultcodes: " + Faultcode.Total());
                Log.system.info("Maquinas: " + Machine.Total());
                Log.system.info("PcbInfo: " + PcbInfo.Total());

                Config.dbDownloadComplete = true;
            }
            catch (Exception ex)
            {
                Log.system.error(ex.Message);
            }
            #endregion

            return Config.dbDownloadComplete;
        }
    }
}
