using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data;

using AOI_VTWIN_RNS.Src.Database;
using AOI_VTWIN_RNS.Aoicollector.Core;

namespace AOI_VTWIN_RNS.Aoicollector.Inspection
{
    public class Export
    {
        public static bool toDB(string barcode, string op, string line_id, string puesto_id, string linea)
        {
            bool sp = false;

            if (barcode.Contains("_invalid_"))
            { 
                sp = false;
            }
            else
            {
                string query = @"
                INSERT INTO
                [sfcsplus].[dbo].[TRAZA_AOI]
                (
                    [Codigo],    
                    [OP_NRO],    
                    [Configlinea_id],    
                    [Puesto_id],    
                    [Linea],    
                    [Fecha_insercion]
                ) VALUES (
                    '" + barcode + @"',
                    '" + op + @"',
                    '" + line_id + @"',
                    '" + puesto_id + @"',
                    '" + linea + @"',
                    CURRENT_TIMESTAMP
                );
            ";
                SqlServerConnector sql = new SqlServerConnector();
                sp = sql.Ejecutar(query);
            }
            return sp;
        }

        /// <summary>
        /// Guarda el documento XML con la informacion de la inspeccion
        /// </summary>
        public static void toXML(InspectionObject inspectionObj, BlockBarcode blockBarcode, string path)
        {
            string exportPath = Path.Combine(path, inspectionObj.machine.line_barcode + "_smd-" + inspectionObj.machine.linea);

            DirectoryInfo di = new DirectoryInfo(exportPath);
            if (!di.Exists)
            {
                Directory.CreateDirectory(di.FullName);
            }

            string new_file = blockBarcode.barcode + "_" + inspectionObj.programa + ".xml";

            // Agrego el panel_barcode delante del nombre del archivo, para poder visualizar facilmente los bloques de cada panel 
            if (inspectionObj.pcbInfo.bloques > 1)
            {
                new_file = inspectionObj.panelBarcode + "_" + new_file;
            }

            #region RUTAS DE CARPETA COMPARTIDA
            string fullFile = Path.Combine(exportPath, new_file);
            string noDeclareFile = Path.Combine(exportPath, "config", "NO_DECLARE.txt");
            string noDeclareFolder = Path.Combine(exportPath, "NO_DECLARE", new_file);

            if (File.Exists(noDeclareFile))
            {
                fullFile = noDeclareFolder;
            }
            #endregion
  
            #region GENERA Y GUARDA XML
            XDocument root;
            if (blockBarcode.total_errores_reales > 0)
            {
                // Save NG
                root = XMLElementRoot(inspectionObj, "NG", blockBarcode);

                // Agrega detalles NG
                XElement ng = XMLElementNG(blockBarcode);
                root.Root.Element("aoi").Add(ng);
            }
            else
            {
                if (inspectionObj.pendiente)
                {
                    // Save PENDIENTE
                    root = XMLElementRoot(inspectionObj, "PENDIENTE", blockBarcode);
                }
                else
                {
                    // Save OK
                    root = XMLElementRoot(inspectionObj, "OK", blockBarcode);
                }
            }
            root.Save(fullFile);
            #endregion
        }

        /// <summary>
        /// Genera el objeto XML OnTheFly
        /// </summary>      
        private static XDocument XMLElementRoot(InspectionObject inspectionObj,  string resultado,  BlockBarcode blockBarcode)
        {
            var EtiquetatoHuman = "";
            switch (blockBarcode.tipoBarcode)
            {
                case "E":
                    EtiquetatoHuman = "ETIQUETA";
                    break;
                case "V":
                    EtiquetatoHuman = "VIRTUAL";
                    break;
            }

            #region GENERA XML
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Conf",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(XNamespace.Xmlns + "xsd", xsd),
                        new XElement("aoi",
                            new XElement("info",
                                new XElement("linea", "SMD-" + inspectionObj.machine.linea),
                                new XElement("maquina", inspectionObj.machine.maquina),
                                new XElement("programa", inspectionObj.programa),
                                new XElement("total_bloques", inspectionObj.pcbInfo.bloques.ToString()),
                                new XElement("bloque", blockBarcode.bloqueId),
                                new XElement("resultado", resultado),
                                new XElement("errores", blockBarcode.total_errores_reales),
                                new XElement("panel_barcode", inspectionObj.panelBarcode),
                                new XElement("barcode", blockBarcode.barcode),
                                new XElement("tipo_barcode", EtiquetatoHuman),
                                new XElement("fecha_inspeccion", inspectionObj.fecha),
                                new XElement("hora_inspeccion", inspectionObj.hora),
                                new XElement("op", inspectionObj.op),
                                new XElement("config_linea_id", inspectionObj.lineId),
                                new XElement("puesto_id", inspectionObj.puestoId)
                            )
                        )
                )
            );
            #endregion

            return doc;
        }

        /// <summary>
        /// Elemento NG de archivo XML
        /// </summary>
        private static XElement XMLElementNG(BlockBarcode blockBarcode)
        {
            IEnumerable<Detail> dt = blockBarcode.detailList.Where(o => o.estado == "REAL");
            XElement NG = new XElement("ng");

            #region COMPLETA TAGs
            foreach (Detail ref_detail in dt)
            {
                NG.Add(
                    new XElement("item",
                        new XElement("referencia", ref_detail.referencia),
                        new XElement("faultcode", ref_detail.faultcode),
                        new XElement("descripcion", ref_detail.descripcionFaultcode)
                    )
                );
            }
            #endregion

            return NG;
        }
    }
}
