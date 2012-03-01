using System;
using System.Collections.Generic;
using NHibernate.Tool.hbm2ddl;

namespace RussianElectionResultsScraper
    {
    public class GenerateSchemaCommand : BaseConsoleCommand
        {
        public GenerateSchemaCommand()
            {
            this.IsCommand("generate-schema", "Generate Database Schema" );
            }
    
        public override int Run(string[] args)
            {
            var schemaExport = new SchemaExport( this.BuildElectionResultsDatabaseConfiguration() );
            schemaExport.Execute( Console.WriteLine, false, false );
            return 0;
            }

        }
    }
