using CollectorPackage.Aoicollector.Inspection.Model;
using System;

namespace CollectorPackage.Aoicollector.Zenith.Controller
{
    public class SqlServerQuery
    {
        // Obtiene lista de inspecciones realizadas
        public static string ListLastInspections(string machineId, string endDate)
        {
            DateTime format = DateTime.Parse(endDate);
            endDate = format.ToString("yyyy-MM-dd HH:mm:ss");

            string query = string.Format(@"
            SELECT
            [PCBID]
              ,CAST([StartDateTime] as DATE) as aoi_fecha
			  ,CAST([EndDateTime] as TIME) as aoi_hora
			  ,CAST([ReviewStartDateTime] as DATE) as insp_fecha      
			  ,CAST([ReviewEndDateTime] as TIME) as insp_hora
              ,(	
				CASE 
					WHEN [PCBResultBefore] = 11000000 THEN 'OK'
					WHEN [PCBResultBefore] = 12000000 THEN 'OK'
					WHEN [PCBResultBefore] = 13000000 THEN 'NG'
					WHEN [PCBResultBefore] = 14000000 THEN 'Scrap'
					WHEN [PCBResultBefore] = 15000000 THEN 'ByPass'
				End
 			  ) as revision_aoi
 			   ,(	
				CASE 
					WHEN [PCBResultAfter] = 11000000 THEN 'OK'
					WHEN [PCBResultAfter] = 12000000 THEN 'OK'
					WHEN [PCBResultAfter] = 13000000 THEN 'NG'
					WHEN [PCBResultAfter] = 14000000 THEN 'Scrap'
					WHEN [PCBResultAfter] = 15000000 THEN 'ByPass'
				End
 			  ) as revision_ins
               			
              ,[PCBRepair]
              ,[BarCode]
              ,[ArrayCnt] as bloques
              ,[ResultDBName]
              ,[ImageDBName]
              ,[PCBModel] as programa
              ,[PCBGUID]


              FROM [KY_AOI].[dbo].[TB_AOIPCB]
                
              WHERE
	            machineId = '{0}' AND 
	            EndDateTime > '{1}'  and
	            PCBModel is not null
              ORDER BY 
	                EndDateTime asc
            ",
                machineId,
                endDate
            );

            return query;
        }

        public static string ListPanelBarcodeInfo(Pendiente pend)
        {
            string query = string.Format(@"
            SELECT
            [PCBID]
              ,CAST([StartDateTime] as DATE) as aoi_fecha
			  ,CAST([EndDateTime] as TIME) as aoi_hora
			  ,CAST([ReviewStartDateTime] as DATE) as insp_fecha      
			  ,CAST([ReviewEndDateTime] as TIME) as insp_hora
              ,(	
				CASE 
					WHEN [PCBResultBefore] = 11000000 THEN 'OK'
					WHEN [PCBResultBefore] = 12000000 THEN 'OK'
					WHEN [PCBResultBefore] = 13000000 THEN 'NG'
					WHEN [PCBResultBefore] = 14000000 THEN 'Scrap'
					WHEN [PCBResultBefore] = 15000000 THEN 'ByPass'
				End
 			  ) as revision_aoi
 			   ,(	
				CASE 
					WHEN [PCBResultAfter] = 11000000 THEN 'OK'
					WHEN [PCBResultAfter] = 12000000 THEN 'OK'
					WHEN [PCBResultAfter] = 13000000 THEN 'NG'
					WHEN [PCBResultAfter] = 14000000 THEN 'Scrap'
					WHEN [PCBResultAfter] = 15000000 THEN 'ByPass'
				End
 			  ) as revision_ins
               			
              ,[PCBRepair]
              ,[BarCode]
              ,[ArrayCnt] as bloques
              ,[ResultDBName]
              ,[ImageDBName]
              ,[PCBModel] as programa
              ,[PCBGUID]

              FROM [KY_AOI].[dbo].[TB_AOIPCB]
                
              WHERE
	            EndDateTime = '{0}'  and
                BarCode = '{1}'  and
                PCBModel = '{2}' and
	            PCBModel is not null 
              ORDER BY 
	                EndDateTime desc
            
            ",
                   pend.fechaMaquina,
                   pend.barcode,
                   pend.programa
            );
            return query;
        }

        // Obtiene lista de defectos de inspeccion detallados por referencia
        public static string ListFaultInfo(ZenithPanel panel)
        {
            string query = string.Format(@"
               SELECT 
		        det.[InspType] as faultcode
              ,(SELECT InspectionName FROM [KY_AOI].[dbo].[TB_InspectionType]  WHERE InspectionType = det.[InspType] ) as descripcion
              ,pos.[uname]
              ,pos.[ArrayIndex] as bloque
 			   ,CAST(	
				CASE 
					WHEN pos.ResultRepair = 11000000 THEN 'FALSO'
					WHEN pos.ResultRepair = 12000000 THEN 'FALSO'
					WHEN pos.ResultRepair = 13000000 THEN 'REAL'
					WHEN pos.ResultRepair = 14000000 THEN 'REAL'
					WHEN pos.ResultRepair = 15000000 THEN 'REAL'
					WHEN pos.ResultRepair = 0 THEN 'REAL'
				End
 			   as VARCHAR) as estado
 			  
              FROM [{0}].[dbo].[TB_AOIDefectDetail] as det
              inner join [{0}].[dbo].[TB_AOIDefect] pos on pos.PCBGUID = det.PCBGUID and pos.ComponentGUID = det.ComponentGUID

              where
  
              pos.PCBGUID = '{1}'
            ",
                panel.zenithResultDb,
                panel.zenithPcbguid
            );

            return query;
        }

        public static string ListBlockBarcode(string barcode)
        {
            string query = @"
            ";
            return query;
        }
    }
}
