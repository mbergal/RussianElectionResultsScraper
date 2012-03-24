using System;
using System.IO;
using FluentMigrator.Runner;

namespace RussianElectionResultsScraper.MigrationRunner
    {
    public class MigrationRunnerAnnouncer : IAnnouncer
        {
        private TextWriter _sqlWriter;
        private bool _quiet;
        private TextWriter _outWriter;

        public MigrationRunnerAnnouncer( TextWriter sqlWriter, bool quiet )
            {
            this._quiet = quiet;
            this._sqlWriter = sqlWriter;
            this._outWriter = quiet ? new StringWriter() : Console.Out;
            }

        public void Dispose()
            {
            }

        public void Heading( string message )
            {
            this._outWriter.WriteLine( message );
            }

        public void Say( string message )
            {
            this._outWriter.WriteLine( message );
            }

        public void Sql( string sql )
            {
            if ( this._sqlWriter != null )
                this._sqlWriter.WriteLine( sql );
            }

        public void ElapsedTime( TimeSpan timeSpan )
            {
            }

        public void Error( string message )
            {
            throw new Exception( message );
            }
        }

}