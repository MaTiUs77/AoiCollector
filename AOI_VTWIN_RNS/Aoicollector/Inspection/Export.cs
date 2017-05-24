using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Data;

using CollectorPackage.Src.Database;
using CollectorPackage.Aoicollector.Core;

namespace CollectorPackage.Aoicollector.Inspection
{
    public class Export
    {
        //public static bool toDB(string barcode, string op, string line_id, string puesto_id, string linea)
        //{
        //    bool sp = false;

        //    if (barcode.Contains("_invalid_"))
        //    { 
        //        sp = false;
        //    }
        //    else
        //    {
        //        string query = @"
        //        INSERT INTO
        //        [sfcsplus].[dbo].[TRAZA_AOI]
        //        (
        //            [Codigo],    
        //            [OP_NRO],    
        //            [Configlinea_id],    
        //            [Puesto_id],    
        //            [Linea],    
        //            [Fecha_insercion]
        //        ) VALUES (
        //            '" + barcode + @"',
        //            '" + op + @"',
        //            '" + line_id + @"',
        //            '" + puesto_id + @"',
        //            '" + linea + @"',
        //            CURRENT_TIMESTAMP
        //        );
        //    ";
        //        SqlServerConnector sql = new SqlServerConnector();
        //        sp = sql.Ejecutar(query);
        //    }
        //    return sp;
        //}

        /// <summary>
        /// Guarda el documento XML con la informacion de la inspeccion
        /// </summary>
        public static void toXML(InspectionController ictrl, Bloque bloque, string path)
        {
            string exportPath = Path.Combine(path, ictrl.machine.line_barcode + "_" + ictrl.machine.smd);

            DirectoryInfo di = new DirectoryInfo(exportPath);
            if (!di.Exists)
            {
                Directory.CreateDirectory(di.FullName);
            }

            string new_file = bloque.barcode + "_" + ictrl.programa + ".xml";

            // Agrego el panel_barcode delante del nombre del archivo, para poder visualizar facilmente los bloques de cada panel 
            if (ictrl.pcbInfo.bloques > 1)
            {
                new_file = ictrl.barcode + "_" + new_file;
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
            if (bloque.totalErroresReales > 0)
            {
                // Save NG
                root = XMLElementRoot(ictrl, "NG", bloque);

                // Agrega detalles NG
                XElement ng = XMLElementNG(bloque);
                root.Root.Element("aoi").Add(ng);
            }
            else
            {
                if (ictrl.pendiente)
                {
                    // Save PENDIENTE
                    root = XMLElementRoot(ictrl, "PENDIENTE", bloque);
                }
                else
                {
                    // Save OK
                    root = XMLElementRoot(ictrl, "OK", bloque);
                }
            }
            root.Save(fullFile);

            ictrl.machine.LogBroadcast("debug", string.Format("+ Exportando XML: {0} en {1}", new_file, exportPath));

            #endregion
        }

        /// <summary>
        /// Genera el objeto XML OnTheFly
        /// </summary>      
        private static XDocument XMLElementRoot(InspectionController ictrl,  string resultado,  Bloque bloque)
        {
            var EtiquetatoHuman = "";
            switch (bloque.tipoBarcode)
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
                                new XElement("linea", ictrl.machine.smd),
                                new XElement("maquina", ictrl.machine.maquina),
                                new XElement("programa", ictrl.programa),
                                new XElement("total_bloques", ictrl.pcbInfo.bloques.ToString()),
                                new XElement("bloque", bloque.bloqueId),
                                new XElement("resultado", resultado),
                                new XElement("errores", bloque.totalErroresReales),
                                new XElement("panel_barcode", ictrl.barcode),
                                new XElement("barcode", bloque.barcode),
                                new XElement("tipo_barcode", EtiquetatoHuman),
                                new XElement("fecha_inspeccion", ictrl.fecha),
                                new XElement("hora_inspeccion", ictrl.hora),
                                new XElement("op", ictrl.op),
                                new XElement("config_linea_id", ictrl.machine.prodService.result.produccion.line_id),
                                new XElement("puesto_id", ictrl.machine.prodService.result.produccion.puesto_id)
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
        private static XElement XMLElementNG(Bloque bloque)
        {
            IEnumerable<Detail> dt = bloque.detailList.Where(o => o.estado == "REAL");
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
