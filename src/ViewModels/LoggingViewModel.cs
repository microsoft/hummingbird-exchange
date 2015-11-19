using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace Hummingbird.ViewModels
{
    public class LoggingViewModel
    {
        private static LoggingViewModel _instance;
        private static readonly object Padlock = new object();

        public LoggingViewModel()
        {
            var logFormatter = new TextFormatter();

            var flatFileTraceListener = new FlatFileTraceListener("log.hblog", "-------------------------",
                "-------------------------", logFormatter);

            var config = new LoggingConfiguration();
            config.AddLogSource("Hummingbird", SourceLevels.All, true).AddTraceListener(flatFileTraceListener);
            Logger = new LogWriter(config);
        }

        public LogWriter Logger { get; set; }

        public static LoggingViewModel Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new LoggingViewModel());
                }
            }
            set { _instance = value; }
        }
    }
}