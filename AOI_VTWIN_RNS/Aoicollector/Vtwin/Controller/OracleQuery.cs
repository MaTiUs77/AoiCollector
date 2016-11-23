using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollectorPackage.Aoicollector.Inspection.Model;
using CollectorPackage.Aoicollector.Inspection;

namespace CollectorPackage.Aoicollector.Vtwin.Controller
{
    public class OracleQuery
    {
        // Obtiene lista de inspecciones realizadas
        public static string ListLastInspections(int createMachineId, string endDate, Pendiente pend = null)
        {
            string filtro = "";
            if (pend == null)
            {
                DateTime format = DateTime.Parse(endDate);
                endDate = format.ToString("yyyy-MM-dd HH:mm:ss");

                filtro = @"	
	                PCI.TEST_MACHINE_ID = " + createMachineId + @" AND 
	                PCI.END_DATE > TO_DATE('" + endDate + @"', 'YYYY-MM-DD HH24:MI:SS')  
                ";

            }
            else
            {
                filtro = @"
                (	
                    PCI.END_DATE >= TO_DATE('" + pend.fechaMaquina + @"', 'YYYY-MM-DD HH24:MI:SS')  AND
                    PCI.BARCODE = '" + pend.barcode + @"' AND 
                    PCI.TEST_MACHINE_ID = " + pend.vtwinTestMachineId + @" AND 
                    PCI.PROGRAM_NAME_ID = " + pend.vtwinProgramNameId + @" 
                ) ";
            }

            // (PCI.REVISE_END_DATE IS NOT NULL) AND 
            string query = @"
            SELECT 
 
	        PNI.PCB_NAME AS PROGRAMA,

            TO_CHAR(PCI.END_DATE, 'YYYY-MM-DD') AS AOI_FECHA, 
            TO_CHAR(PCI.END_DATE, 'HH24:MI:SS') AS AOI_HORA,  	

            TO_CHAR(PCI.REVISE_END_DATE, 'YYYY-MM-DD') AS INSP_FECHA, 
            TO_CHAR(PCI.REVISE_END_DATE, 'HH24:MI:SS') AS INSP_HORA,  	

            PCI.END_DATE,
            PCI.REVISE_END_DATE,

	        PCI.TEST_RESULT,
	        PCI.REVISE_RESULT,
	        PCI.BARCODE,
	        PCI.MACHINE_DETERMINATION,

	        PCI.PCB_NO,
            PCI.TEST_MACHINE_ID,
            PCI.SAVED_MACHINE_ID,
            PCI.PROGRAM_NAME_ID,
            PCI.REVISION_NO,
            PCI.SERIAL_NO,
            PCI.LOAD_COUNT

        FROM 
	        PROGRAM_INFO PRI,
	        PCB_INFO PCI,
	        PCB_NAME_INFO PNI  

        WHERE 	
            PRI.PCB_ID = PNI.PCB_ID  AND 
	
            (PCI.BARCODE IS NOT NULL) AND 

	        PRI.PROGRAM_NAME_ID = PCI.PROGRAM_NAME_ID  AND 
	        PRI.SAVED_MACHINE_ID = PCI.SAVED_MACHINE_ID  AND 
	        PRI.PROGRAM_NAME_ID = PCI.PROGRAM_NAME_ID  AND 
	        PRI.REVISION_NO = PCI.REVISION_NO  AND 
	        PRI.SERIAL_NO = PCI.SERIAL_NO   AND 

        " + filtro + @"

        ORDER BY 
	        PCI.END_DATE ASC
            ";

            return query;
        }

        public static string ListPanelBarcodeInfo(string barcode)
        {
            string query = @"
SELECT 
 
	        PNI.PCB_NAME AS PROGRAMA,

            TO_CHAR(PCI.END_DATE, 'YYYY-MM-DD') AS AOI_FECHA, 
            TO_CHAR(PCI.END_DATE, 'HH24:MI:SS') AS AOI_HORA,  	

            TO_CHAR(PCI.REVISE_END_DATE, 'YYYY-MM-DD') AS INSP_FECHA, 
            TO_CHAR(PCI.REVISE_END_DATE, 'HH24:MI:SS') AS INSP_HORA,  	

            PCI.END_DATE,
            PCI.REVISE_END_DATE,

	        PCI.TEST_RESULT,
	        PCI.REVISE_RESULT,
	        PCI.BARCODE,
	        PCI.MACHINE_DETERMINATION,

	        PCI.PCB_NO,
            PCI.TEST_MACHINE_ID,
            PCI.SAVED_MACHINE_ID,
            PCI.PROGRAM_NAME_ID,
            PCI.REVISION_NO,
            PCI.SERIAL_NO,
            PCI.LOAD_COUNT

        FROM 
	        PROGRAM_INFO PRI,
	        PCB_INFO PCI,
	        PCB_NAME_INFO PNI  

        WHERE 	
            PRI.PCB_ID = PNI.PCB_ID  AND 
	
            (PCI.BARCODE IS NOT NULL) AND 

	        PRI.PROGRAM_NAME_ID = PCI.PROGRAM_NAME_ID  AND 
	        PRI.SAVED_MACHINE_ID = PCI.SAVED_MACHINE_ID  AND 
	        PRI.PROGRAM_NAME_ID = PCI.PROGRAM_NAME_ID  AND 
	        PRI.REVISION_NO = PCI.REVISION_NO  AND 
	        PRI.SERIAL_NO = PCI.SERIAL_NO   AND 
                
            PCI.BARCODE = '"+barcode+ @"'

            ORDER BY 
	        PCI.END_DATE ASC";

            return query;
        }

        // Obtiene lista de defectos de inspeccion detallados por referencia
        public static string ListFaultInfo(VtwinPanel panel)
        {
            string query = @"
                SELECT 

                    (CASE WHEN FCI.REVISED_FAULT_ID IS NULL 
                        THEN (CASE WHEN ( FCI.COMPONENT_PERSON_REVISOR IS NULL OR FCI.COMPONENT_REVISE_END_DATE IS NULL )
                                    THEN 'PENDIENTE' 
                                    ELSE 'FALSO' 
                                END) 
                        ELSE 'REAL' 
                    END) AS RESULTADO,

                CI.COMPONENT_NAME,
                CI.COMPONENT_BLOCK_NO AS BLOQUE,
                FCI.FAULT_CODE

                FROM 

	                FAULT_COMPONENT_INFO FCI,
	                COMPONENT_INFO CI

                WHERE 

	                CI.SAVED_MACHINE_ID = FCI.SAVED_MACHINE_ID AND
	                CI.PROGRAM_NAME_ID = FCI.PROGRAM_NAME_ID AND
	                CI.REVISION_NO = FCI.REVISION_NO  AND
	                CI.SERIAL_NO = FCI.SERIAL_NO  AND
	                CI.COMPONENT_NO = FCI.COMPONENT_NO AND

	                FCI.TEST_MACHINE_ID = " + panel.machine.oracle_id + @" AND
                    FCI.PROGRAM_NAME_ID =  " + panel.vtwinProgramNameId + @" AND
                    FCI.SAVED_MACHINE_ID =  " + panel.vtwinSaveMachineId + @" AND
                    FCI.REVISION_NO =  " + panel.vtwinRevisionNo + @" AND
                    FCI.SERIAL_NO =  " + panel.vtwinSerialNo + @" AND
                    FCI.LOAD_COUNT =  " + panel.vtwinLoadCount + @" AND 
                    FCI.PCB_NO =  " + panel.panelNro + @"

                GROUP BY

                    (CASE WHEN FCI.REVISED_FAULT_ID IS NULL 
                        THEN (CASE WHEN ( FCI.COMPONENT_PERSON_REVISOR IS NULL OR FCI.COMPONENT_REVISE_END_DATE IS NULL )
                                    THEN 'PENDIENTE' 
                                    ELSE 'FALSO' 
                                END) 
                        ELSE 'REAL' 
                    END),

                CI.COMPONENT_NAME,
                CI.COMPONENT_BLOCK_NO,
                FCI.FAULT_CODE

                ORDER BY

                RESULTADO DESC
            ";

            return query;
        }

        public static string ListBlockBarcode(string barcode)
        {
            string query = "select bk.block_barcode, bk.block_no as bloque from BLOCK_BARCODE bk  where  bk.barcode = '" + barcode + "' ";
            return query;
        }

        // Obtiene lista de maquinas
        public static string ListMachines()
        {
            string query = "select MACHINE_ID, MACHINE_NAME from MACHINE_INFO where MACHINE_NAME like 'VT-WIN2-%'";
            return query;
        }

        // Obtiene conteo de defectos por inspeccion
        public static string CountTotalFaultInfo(string mode, int pcbNo, int savedMachineId, int programNameId, int revisionNo, int serialNo)
        {
            string query = "SELECT  COUNT (*) as total  FROM fault_component_info fci " +
                    " WHERE  FCI.PCB_NO = " + pcbNo +
                    " AND fci.saved_machine_id = " + savedMachineId + " " +
                    " AND FCI.PROGRAM_NAME_ID = " + programNameId + " " +
                    " AND FCI.REVISION_NO = " + revisionNo + " " +
                    " AND FCI.SERIAL_NO = " + serialNo;

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
    }
}
