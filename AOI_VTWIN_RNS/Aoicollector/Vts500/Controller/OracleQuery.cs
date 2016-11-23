using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Inspection;

namespace CollectorPackage.Aoicollector.Vts500.Controller
{
    public class OracleQuery
    {
        public static string ListBlocks(InspectionController ictrl)
        {
            string query = @"
            SELECT 
                SEG_ID, SEG_NO

            FROM 
            seg_info 

            WHERE 
              pos_x <> 0 and 
              pos_y <> 0 and
              size_x <> 0 and
              size_y <> 0 and
              PG_ITEM_ID = " + ictrl.vtsOraclePgItemId;
            return query;
        }

        // Obtiene lista de inspecciones realizadas
        public static string ListLastInspections(Machine machine, Pendiente pend = null)
        {
            string filtro = "";
            if (pend == null)
            {
                string endDate = machine.ultima_inspeccion;
                DateTime format = DateTime.Parse(endDate);
                endDate = format.ToString("yyyy-MM-dd HH:mm:ss");

                filtro = @"	
                    SUM.INSP_BEGIN_DATE  > TO_DATE('" + endDate + @"', 'YYYY-MM-DD HH24:MI:SS')  
                ";

            } else
            {
                filtro = @"
                (	
                    SUM.INSP_BEGIN_DATE >= TO_DATE('" + pend.fechaMaquina + @"', 'YYYY-MM-DD HH24:MI:SS')  AND
                    SUM.BOARD_BARCODE = '" + pend.barcode + @"' 
                ) ";
            }

            string query = @"
                SELECT

                PG.PG_NAME as PROGRAMA,
                PG.PG_ITEM_ID as PROGRAMA_ID,
                SUM.INSP_ID,
                SUM.BOARD_BARCODE as BARCODE,

                TO_CHAR(SUM.INSP_BEGIN_DATE, 'YYYY-MM-DD') AS AOI_FECHA, 
                TO_CHAR(SUM.INSP_BEGIN_DATE, 'HH24:MI:SS') AS AOI_HORA,  	
                TO_CHAR(SUM.INSP_END_DATE, 'YYYY-MM-DD') AS INSP_FECHA, 
                TO_CHAR(SUM.INSP_END_DATE, 'HH24:MI:SS') AS INSP_HORA,  

                SUM.INSP_BEGIN_DATE,
                SUM.INSP_END_DATE,

                SUM.INSP_RESULT_CODE,

                CASE WHEN SUM.INSP_RESULT_CODE = 0 THEN 'OK' ELSE 'NG' END AS TEST_RESULT,
                SUM.VC_COMPLETE_FLAG as REVISED

                FROM 

                INSP_RESULT_SUMMARY_INFO SUM,
                PG_INFO PG

                WHERE 
                PG.PG_ITEM_ID = SUM.PG_ITEM_ID AND
                SUM.SYS_MACHINE_NAME = '" + machine.maquina +@"' AND
                " + filtro + @"

                ORDER BY
                INSP_END_DATE ASC
            ";

            return query;
        }
     
        // Obtiene lista de defectos de inspeccion detallados por referencia
        public static string ListFaultInfo(InspectionController ictrl)
        {
            //                    CI.VC_LAST_RESULT_CODE IS NULL OR

            string query = @"           
                SELECT
                CI.SEG_ID,

                REF.CIR_NAME as referencia,

                CI.INSP_RESULT_CODE as faultcode,
                CI.VC_LAST_RESULT_CODE,

                CASE WHEN 
                    CI.VC_LAST_RESULT_CODE = 0
                       THEN 'FALSO' ELSE 'REAL' END AS resultado

                FROM
                COMP_RESULT_INFO CI,
                CIR_INFO REF

                WHERE

                REF.CIR_ID = CI.COMP_ID AND
                CI.INSP_RESULT_CODE != 0 AND

                REF.PG_ITEM_ID = " + ictrl.vtsOraclePgItemId + @" AND
                CI.INSP_ID = " + ictrl.vtsOracleInspId;

            //  CASE WHEN CI.VC_LAST_RESULT_CODE = 0 THEN 'FALSO' ELSE 'REAL' END AS resultado
            //  CI.USR_CONFIRM_FLAG  = 1 AND
    
            return query;
        }
        
        // Obtiene lista de maquinas
        public static string ListMachines()
        {
            string query = "select sys_machine_name from machine_name";
            return query;
        }

        // Obtiene lista de maquinas
        public static string ListBlockBarcode(InspectionController ictrl)
        {
            string query = @"
                SELECT
                SEG_ID,
                SEG_BARCODE,
                COMP_NG_COUNT,
                VC_COMPLETE_FLAG
                FROM 

                SEG_RESULT_INFO

                WHERE

                INSP_ID = " + ictrl.vtsOracleInspId;
            return query;
        }

        /*
        // Obtiene conteo de defectos por inspeccion
        public string VERIFY_____CountTotalFaultInfo(string mode, int pcb_no, int saved_machine_id, int program_name_id, int revision_no, int serial_no)
        {
            string query = "";

            switch (mode)
            {
                case "real":
                    query = query + " AND FCI.REVISED_FAULT_ID is not null";
                break;
                case "falso":
                    query = query + " AND FCI.REVISED_FAULT_ID is null";
                break;
            }

            return query;
        }
         */
    }
}
