using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace CollectorPackage.Aoicollector.Core
{
    public class RichLog
    {
        public string id { get; set; }
        public string smd { get; set; }
        public bool autoscroll = true;
        public int loglevel = 1;

        public int linelimit = 200;
        public int linecount = 0;

        protected Color colorError = Color.Red;
        protected Color colorInfo = Color.Cyan;
        protected Color colorWarning = Color.Yellow;
        protected Color colorDebug = Color.Gray;
        protected Color colorVerbose = Color.Green;
        protected Color colorStack = Color.Orange;
        protected Color colorSuccess = Color.LawnGreen;
        protected Color colorNotify= Color.DeepPink;

        public RichTextBox richTextBox { get; set; }

        public RichLog()
        {
            // Normal constructor
        }

        public RichLog(RichTextBox rbox)
        {
            richTextBox = rbox;
        }

        /// <summary>
        ///  Resetea el log, esto libera memoria
        /// </summary>
        public void reset()
        {
            richTextBox.Text = string.Empty;
            linecount = 0;
        }

        private Color getColorMode(string mode)
        {
            Color color = colorInfo;
            switch (mode)
            {
                case "error": color =  colorError; break;
                case "info": color = colorInfo; break;
                case "warning": color = colorWarning; break;
                case "debug": color = colorDebug; break;
                case "verbose": color = colorVerbose; break;
                case "stack": color = colorStack; break;
                case "success": color = colorSuccess; break;
                case "notify": color = colorNotify; break;
                default: color = colorInfo; break;
            }

            return color;
        }

        public string putLog(string mensaje, string mode, bool withDateTime = true, bool inline = false)
        {
            DateTime time = DateTime.Now;
            string format = "dd/MM HH:mm";

            StringBuilder msg = new StringBuilder();
            string finalMsg = "";

            if (withDateTime)
            {
                finalMsg = string.Format("[{0}] {1}", time.ToString(format), mensaje);
            } else
            {
                finalMsg = string.Format("{0}", mensaje);
            }

            if(inline) {
                msg.Append(finalMsg);
            } else
            {
                msg.AppendLine(finalMsg);
            }

            MethodInvoker updatListBox = new MethodInvoker(() =>
            {
                if (linecount >= linelimit)
                {
                    reset();
                }

                if (mode != "lastline")
                {
                    richTextBox.SelectionStart = richTextBox.TextLength;
                    richTextBox.SelectionLength = 0;
                    richTextBox.SelectionColor = getColorMode(mode);
                }

                richTextBox.AppendText(msg.ToString()); 
//                richTextBox.SelectionColor = richTextBox.ForeColor; // Vuelvo al color normal

                if (Log.autoscroll)
                {
                    richTextBox.SelectionStart = richTextBox.Text.Length;
                    richTextBox.ScrollToCaret();
                }

                linecount++;
            });

            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(updatListBox);
            }
            else
            {
                updatListBox();
            }

            return finalMsg;
        }

        public string responseOk()
        {
            return putLog(": OK", "success",false);
        }
        public string error(string msg)
        {
            return putLog(msg, "error");
        }
        public string stack(string msg, object objeto, Exception ex)
        {
            putLog(msg, "error");
            return putLog(stackFormatter(objeto,ex), "stack");
        }    
        public string warning(string msg)
        {
            return putLog(msg, "warning");
        }
        public string info(string msg)
        {
            return putLog(msg, "info");
        }
        public string debug(string msg)
        {            
            if(loglevel<1) { 
                msg = putLog(msg, "debug");
            } 

            return msg;
        }    
        public string put(string msg)
        {
            return putLog(msg, "log");
        }
        public string verbose(string msg, bool responseinline = false)
        {
            return putLog(msg, "verbose", true, responseinline);
        }
        public string success(string msg)
        {
            return putLog(msg, "success");
        }
        public string notify(string msg)
        {
            return putLog(msg, "notify");
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
        private string stackFormatter(object objeto, Exception ex)
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

            return message;
//            ToFile(message, DateTime.Now, "stack", "stack");
        }
    }
}
