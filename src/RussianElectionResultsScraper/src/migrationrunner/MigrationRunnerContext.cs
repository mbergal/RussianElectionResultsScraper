using System.IO;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace RussianElectionResultsScraper.MigrationRunner
    {
    public class MigrationRunnerContext : IRunnerContext
        {
        public MigrationRunnerContext( TextWriter sqlWriter = null, bool quiet = false )
            {
            this.Announcer = new MigrationRunnerAnnouncer( sqlWriter, quiet );
            this.StopWatch = new StopWatch();
            }

        public string Database { get; set; }

        public string Connection { get; set; }

        public string Target { get; set; }

        public bool PreviewOnly { get; set; }

        public string Namespace { get; set; }

        public string Task { get; set; }

        public long Version { get; set; }

        public int Steps { get; set; }

        public string WorkingDirectory { get; set; }

        public string Profile { get; set; }

        public IAnnouncer Announcer { get; private set; }

        public IStopWatch StopWatch { get; private set; }

        public int Timeout { get; set; }

        public string ConnectionStringConfigPath { get; set; }
        }
    }