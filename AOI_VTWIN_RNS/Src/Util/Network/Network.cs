using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using CollectorPackage.Src.Config;


namespace CollectorPackage.Src.Util.Network
{
    class Network
    {
        /// <summary>
        /// Conecta a la carpeta compartida C: en una pc de red
        /// </summary>
        /// <param name="ip_host"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static bool NetUse(string ip_host, string user, string pass)
        {
            bool executed = false;
            string cmd = @"/C net use " + ip_host + " " + pass + " /user:" + user;

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd", cmd);
            Process process = new Process();

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            startInfo.CreateNoWindow = false; //nowindow
            startInfo.UseShellExecute = true; //use shell
            process = Process.Start(startInfo);
            process.WaitForExit(5000); //give it some time to finish
            process.Close();
            return executed;
        }

        /// <summary>
        /// Ejecuta NetUse usando el App.config con el TAG solicitado 
        /// </summary>
        /// <param name="tag"></param>
        public static void ConnectCredential(string tag)
        {
            Network.NetUse(
                AppConfig.Read(tag, "server"),
                AppConfig.Read(tag, "user"),
                AppConfig.Read(tag, "pass")
            );
        }
    }
}
