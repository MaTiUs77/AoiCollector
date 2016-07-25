using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using AOI_VTWIN_RNS.Src.Config;

namespace AOI_VTWIN_RNS.Aoicollector.Core
{
    public class Log
    {
        public static bool autoscroll = true;

        public static ListBox logSystem { get; set; }
        public ListBox logArea { get; set; }

        /// <summary>
        /// Loguea informacion de la Maquina
        /// </summary>
        /// <param name="mensaje"></param>
        public void Area(string mensaje, string mode = "info")
        {
            StartLog(mensaje, logArea, mode);
        }
        
        /// <summary>
        /// Resetea el area de log, esto libera memoria
        /// </summary>
        public void AreaReset()
        {
            logArea.Items.Clear();
        }

        /// <summary>
        /// Log en area de Sistema
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="mode"></param>
        public static void Sys(string mensaje, string mode = "info")
        {
            StartLog(mensaje, logSystem, mode);
        }

        /// <summary>
        /// Guarda archivo log 
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="time"></param>
        /// <param name="control"></param>
        /// <param name="mode"></param>
        public static void ToFile(string mensaje, DateTime time, string control, string mode)
        {
            string diaMes = time.ToString("dd-MM");
            string diaMesHora = time.ToString("HH:mm:ss");
            string pathLog = @"c:\AOILog\";
            string file = control + "_" + mode + ".txt";

            string fullPath = "";
            fullPath = Path.Combine(pathLog, diaMes);

            DirectoryInfo di = new DirectoryInfo(fullPath);
            if (!di.Exists)
            {
                Directory.CreateDirectory(fullPath);
            }

            string fullFilePath = Path.Combine(fullPath, file);

            using (StreamWriter w = File.AppendText(fullFilePath))
            {
                w.WriteLine(diaMesHora + " | " + mensaje);
            }
        }

        /// <summary>
        /// Guarda la excepcion en un archivo llamado stack_stack
        /// </summary>
        /// <param name="objeto"></param>
        /// <param name="ex"></param>
        public static void Stack(object objeto, Exception ex)
        {
            string message = "";
            message += objeto.GetType().FullName;
            message += "\n";
            message += "- TargetSite: " + ex.TargetSite;
            message += "\n";
            message += "- Message: " + ex.Message;
            message += "\n";
            message += "- Source: " + ex.Source;
            message += "\n";
            message += "- StackTrace: " + ex.StackTrace;
            message += "\n";
            message += "\n";

            ToFile(message, DateTime.Now, "stack", "stack");
        }

        /// <summary>
        /// Log, adjunta fecha y hora del log, a su vez administra Invokes
        /// </summary>
        /// <param name="mensaje"></param>
        /// <param name="listBoxLog"></param>
        /// <param name="mode"></param>
        private static void StartLog(string mensaje, ListBox listBoxLog, string mode)
        {
            DateTime time = DateTime.Now;
            string format = "dd/MM HH:mm | ";
            string sendMsg = time.ToString(format) + "[" + mode + "] " + mensaje;

            MethodInvoker updatListBox = new MethodInvoker(() =>
            {
                listBoxLog.Items.Add(sendMsg);

                if (autoscroll)
                {
                    listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                    listBoxLog.SelectedIndex = -1;
                }

                #region SAVE_TO_FILE
                if (mode.Equals("info"))
                {
                    // Si el modo es info, y logueo info... genero archivo.
                    if (AppConfig.Read("SERVICE", "loginfo").ToString().Equals("true"))
                    {
                        ToFile(mensaje, time, listBoxLog.Name.Replace("list", ""), mode);
                    }
                }
                else
                {
                    // Logueo todo lo demas. 
                    ToFile(mensaje, time, listBoxLog.Name.Replace("list", ""), mode);
                }
                #endregion
            });

            if (listBoxLog.InvokeRequired)
            {
                listBoxLog.Invoke(updatListBox);
            }
            else
            {
                updatListBox();
            }
        }
    }
}
