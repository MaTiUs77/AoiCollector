using CollectorPackage.Src.Util.Files;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CollectorPackage.Aoicollector.Rns.Controller
{
    class InspectionTextFileHandler
    {
        // Salto de linea en archivo
        private char FILAS = '\n';

        public List<InspectionTextFileHeader> ContentFileToObject(DirectoryInfo currentInspectionDirectory, string fileName = "InspectionResult")
        {
            string fileUrl = currentInspectionDirectory.FullName + @"\" + fileName + @".txt";
            if (File.Exists(fileUrl))
            {
                return GetFileHeaders(fileUrl);
            }
            else
            {
                return new List<InspectionTextFileHeader>();
            }
        }

        private List<InspectionTextFileHeader> GetFileHeaders(string inspectionResultTextFile)
        {
            string contenido = FilesHandler.ReadFile(inspectionResultTextFile);
            contenido += "\n----DUMMY";
            string[] lineas = contenido.Split(FILAS);

            List<InspectionTextFileHeader> headers = new List<InspectionTextFileHeader>();
            InspectionTextFileHeader obj = new InspectionTextFileHeader();

            // Guardo datos de headers, variables y valores en un diccionario
            foreach (string txt in lineas)
            {
                if (txt.Contains("----"))
                {
                    if (!obj.header.Equals(""))
                    {
                        headers.Add(obj);
                    }
                    obj = new InspectionTextFileHeader();
                    obj.header = txt.ToString().Replace('\r', ' ').Replace('-', ' ').Trim();
                }
                else
                {
                    string[] sp = txt.Split(':');

                    if (sp.Length > 1)
                    {
                        InspectionTextFileAtrribute inspectionAttribute = new InspectionTextFileAtrribute();

                        inspectionAttribute.variable = sp[0].ToString().Replace('\r', ' ').Trim();
                        inspectionAttribute.valor = sp[1].ToString().Replace('\r', ' ').Trim();

                        obj.attributes.Add(inspectionAttribute);
                    }
                }
            }
            return headers;
        }     
    }
}
