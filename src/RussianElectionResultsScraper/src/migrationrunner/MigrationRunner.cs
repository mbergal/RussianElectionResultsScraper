using System.IO;
using System.Reflection;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;
using RussianElectionResultsScraper.Migrations;

namespace RussianElectionResultsScraper.MigrationRunner
    {
    public class MigrationRunner
        {
        public MigrationRunner( string providerName )
            {
            this._providerName = providerName;
            this._migrationProcessorFactory = ProcessorFactory.GetFactory( this._providerName );
            }

        private readonly string                     _providerName;
        private readonly IMigrationProcessorFactory _migrationProcessorFactory;

        public void GenerateSchema( StringWriter stringWriter )
            {
            Assembly assembly = AssemblyLoaderFactory.GetAssemblyLoader( Assembly.GetAssembly(  typeof( Migration_0 ) ).FullName ).Load();

            var runnerContext = new MigrationRunnerContext( sqlWriter: stringWriter, quiet: true )
                                    {
                                    PreviewOnly = true
                                    };


            var fluentMigrationRunner = new FluentMigrator.Runner.MigrationRunner( 
                assembly, 
                runnerContext, 
                this._migrationProcessorFactory.Create( string.Empty, runnerContext.Announcer, new ProcessorOptions() {PreviewOnly = true}  ) );

            fluentMigrationRunner.MigrateUp();
            }

        public void MigrateUp( string connectionString )
            {
            Assembly assembly = AssemblyLoaderFactory.GetAssemblyLoader( Assembly.GetAssembly(  typeof( Migration_0 ) ).FullName ).Load();

            var runnerContext = new MigrationRunnerContext( quiet: true )
                                    {
                                    PreviewOnly = false,
                                    Connection = connectionString,
                                    };


            var fluentMigrationRunner = new FluentMigrator.Runner.MigrationRunner( 
                assembly, 
                runnerContext, 
                this._migrationProcessorFactory.Create( runnerContext.Connection, runnerContext.Announcer, new ProcessorOptions() {}  ) );

            fluentMigrationRunner.MigrateUp();
            }
        }
        
    }
