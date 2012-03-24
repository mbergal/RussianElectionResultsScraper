using System;
using System.IO;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;
using RussianElectionResultsScraper.MigrationRunner;
using RussianElectionResultsScraper.Migrations;

namespace RussianElectionResultsScraper.Commands.Database
    {
    public class GenerateSchemaCommand : BaseConsoleCommand
        {
        public GenerateSchemaCommand()
            {
            this.IsCommand( "database:generate-schema", "Clean Database" );
            this.HasConnectionOption();
            this.HasProviderOption();
            }
    
        public override int Run(string[] args)
            {
            var runner = new RussianElectionResultsScraper.MigrationRunner.MigrationRunner( this._providerName );
            var m  = new StringWriter();
            runner.GenerateSchema( m );
            Console.Out.WriteLine( m.ToString() );
            return 0;
            }
        }
    }
