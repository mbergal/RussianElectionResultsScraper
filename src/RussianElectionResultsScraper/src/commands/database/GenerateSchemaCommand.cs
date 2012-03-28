using System;
using System.IO;

namespace RussianElectionResultsScraper.Commands.Database
    {
    public class GenerateSchemaCommand : BaseCommand
        {
        public GenerateSchemaCommand( string providerName )
            : base( providerName: providerName )
            {
            }
    
        public override int Execute()
            {
            var runner = new MigrationRunner.MigrationRunner( this._providerName );
            var m  = new StringWriter();
            runner.GenerateSchema( m );
            Console.Out.WriteLine( m.ToString() );
            return 0;
            }
        }
    }
