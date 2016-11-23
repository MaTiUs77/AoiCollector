using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CollectorPackage.Src.Util.Event
{
    class Event
    {
        public static string app = "AOICollector";
        public static string log = "Application";
        public static int errCode = 40101;

        public static void info(string sEvent)
        {
            add(sEvent, EventLogEntryType.Information);
        }

        public static void error(string sEvent)
        {
            add(sEvent, EventLogEntryType.Error);
        }

        public static void alerta(string sEvent)
        {
            add(sEvent, EventLogEntryType.Warning);
        }

        private static void add(string sEvent, EventLogEntryType type)
        {

            if (!EventLog.SourceExists(app))
            {
                EventLog.CreateEventSource(app, log);
            }
            EventLog.WriteEntry(app, sEvent, type, errCode);
        }
    }
}
