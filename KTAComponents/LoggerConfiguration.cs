using System;
using System.Diagnostics;


namespace KTAComponents
{

    // gestion des logs
    // utilise un singleton
    public class LoggerConfiguration
    {

        private static LoggerConfiguration _instance;


        // préparation logger dans les events logs windows
        private LoggerConfiguration() {

            Trace.Listeners.Add(new EventLogTraceListener("SIRIUS - Composants"));
            Trace.AutoFlush = true;
        }


        // permet de logger une erreur dans les event logs
        public void TraceError(string msg)
        {
            Trace.TraceError(msg);
        }

        // getter singleton
        public static LoggerConfiguration GetLogger()
        {
            if (_instance == null)
            {

                _instance = new LoggerConfiguration();
            }
           
           return _instance;
        }
    }
}
