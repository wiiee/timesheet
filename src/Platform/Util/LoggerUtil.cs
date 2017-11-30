namespace Platform.Util
{
    using Enricher;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Setting;
    using System;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public static class LoggerUtil
    {
        private static ILoggerFactory _loggerFactory;
        private const string FILE_PATH = @"c:\log\log.txt";
        private const string MONGODB_PATH = @"mongodb://log:log@localhost/Log";
        private const string TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level}:{EventId} [{SourceContext}] {Message}{NewLine}{Exception}";

        static LoggerUtil()
        {
            var elasticSearchLogUri = Setting.Instance.Get("ElasticSearchLogUri");

            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .Enrich.With(new ThreadIdEnricher())
                  .Enrich.WithProperty("MachineName", Environment.MachineName)
               .CreateLogger();

            //Log.Logger = new LoggerConfiguration()
            //   .MinimumLevel.Debug()
            //   .Enrich.WithEnvironmentUserName()
            //   .Enrich.WithMachineName()
            //   .Enrich.WithProcessId()
            //   .Enrich.WithThreadId()
            //   .WriteTo.RollingFile(FILE_PATH, outputTemplate: TEMPLATE)
            //   .WriteTo.MongoDBCapped(MONGODB_PATH, collectionName: "Log")
            //   .CreateLogger();

            _loggerFactory = new LoggerFactory().AddSerilog();
        }

        public static ILogger CreateLogger<T>()
        {
            return _loggerFactory.CreateLogger(typeof(T).FullName);
        }
    }
}
